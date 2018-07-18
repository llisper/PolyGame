using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ResourceModule.Hotfix
{
    public class HotfixLog : LogDefine<HotfixLog> { }

    public class HotfixSystem : Singleton<HotfixSystem>
    {
        public static int RetryTimes = 3;

        string platform;
        Version version;
        FileManager fileManager;

        Version newVersion;
        FileManifest newFileManifest;

        enum VersionCheckResult
        {
            AppUpdate,
            ResUpdate,
            Latest,
        }

        protected override async Task AsyncInit()
        {
            if (ResourceSystem.Instance.mode == ResourceSystem.Mode.Dev)
            {
                HotfixLog.Log("Skip hotfix process in Dev mode");
                return;
            }

            HotfixLog.Log("SandboxExpireCheck");
            SandboxExpireCheck();
            HotfixLog.Log("InitVersion");
            InitVersion();

            HotfixLog.Log("NewVersionCheck");
            var verCheckRet = await NewVersionCheck();
            HotfixLog.Log("NewVersionCheck result: " + verCheckRet);
            if (verCheckRet == VersionCheckResult.AppUpdate)
            {
                // TODO: notify user and direct to app store
                // IMPORTANT: In most cases termination of application under iOS should be left at the user discretion. Consult Apple Technical Page qa1561 for further details.
                HotfixLog.LogFormat(
                    "App update required, {0} -> {1}",
                    version,
                    newVersion);

                if (Application.isEditor)
                    throw new Exception();
                else
                    Application.Quit();
            }

            HotfixLog.Log("ScatterUpdate");
            await ScatterUpdate();

            HotfixLog.Log("ApplyNewVersion");
            ApplyNewVersion();

            HotfixLog.Log("ClearUnusedFiles");
            Instance.fileManager.ClearUnusedFiles();
            
            HotfixLog.Log("Done");
        }

        public HotfixSystem()
        {
            var p = Application.platform;
            if (p == RuntimePlatform.WindowsEditor || p == RuntimePlatform.Android)
                platform = "Android";
            else if (p == RuntimePlatform.OSXEditor || p == RuntimePlatform.IPhonePlayer)
                platform = "iOS";
            HotfixLog.Log("Platform: " + platform);
        }

        void InitVersion()
        {
            version = Version.Create();
            HotfixLog.LogFormat("Version(local): {0} cdn: {1}", version.Name, version.Cdn);

            fileManager = FileManager.Create();
            HotfixLog.LogFormat(
                "Manifest(local), files: {0}, sandbox files: {1}",
                fileManager.manifest.files.Count,
                fileManager.sandboxFiles.Count);
        }

        void SandboxExpireCheck()
        {
            if (!Directory.Exists(PathRouter.SandboxPath))
            {
                Directory.CreateDirectory(PathRouter.SandboxPath);
                return;
            }

            string sanboxVersionFile = PathRouter.SandboxPath + PathRouter.Version;
            bool exists = File.Exists(sanboxVersionFile);
            if (exists)
            {
                Version sandboxVersion = Version.Create(File.ReadAllBytes(sanboxVersionFile));
                string inAppVersionFile = PathRouter.StreamingPath + PathRouter.Version;
                Version inAppVersion;
                if (Application.platform == RuntimePlatform.Android)
                    inAppVersion = Version.Create(AndroidPlugin.GetAssetBytes(inAppVersionFile));
                else
                    inAppVersion = Version.Create(File.ReadAllBytes(inAppVersionFile));

                if (inAppVersion > sandboxVersion)
                {
                    Directory.Delete(PathRouter.SandboxPath, true);
                    HotfixLog.LogFormat(
                        "Clear sandbox files, version(inApp:{0}) > version(sandbox:{1})",
                        inAppVersion, sandboxVersion);
                }
            }
        }

        void ApplyNewVersion()
        {
            newVersion.WriteToFile();
            version = newVersion;
            if (null != newFileManifest)
            {
                newFileManifest.WriteToFile();
                fileManager.manifest = newFileManifest;
            }
        }

        async Task<VersionCheckResult> NewVersionCheck()
        {
            byte[] data = await Download(PathRouter.Version);
            newVersion = Version.Create(data);
            HotfixLog.LogFormat("Version(new): {0} cdn: {1}", newVersion.Name, newVersion.Cdn);

            var result = VersionCheckResult.Latest;
            if (version > newVersion)
            {
                throw new ApplicationException(string.Format(
                    "version({0}) > new version({1})",
                    version, newVersion));
            }
            else if (version < newVersion)
            {
                result = version.Major < newVersion.Major 
                    ? VersionCheckResult.AppUpdate 
                    : VersionCheckResult.ResUpdate;
            }

            return result;
        }

        async Task ScatterUpdate()
        {
            byte[] data = await Download(PathRouter.FileManifest);
            data = FileLoader.RemoveBOM(data);
            newFileManifest = JsonUtility.FromJson<FileManifest>(Encoding.UTF8.GetString(data));
            newFileManifest.BuildQuery();
            HotfixLog.LogFormat("Manifest(new), files: {0}", newFileManifest.files.Count);

            var updateFiles = fileManager.CalculateUpdateFiles(newFileManifest);
            foreach (string name in updateFiles)
            {
                HotfixLog.Log("Update file: " + name);
                string file = PathRouter.SandboxPath + name;
                string downloadFile = file + ".download";
                Directory.CreateDirectory(Path.GetDirectoryName(file));

                try
                {
                    await DownloadToFile(name, downloadFile);
                    File.Delete(file);
                    File.Move(downloadFile, file);
                    fileManager.AddFile(name);
                }
                catch (Exception e)
                {
                    File.Delete(downloadFile);
                    throw e;
                }
            }
        }

        async Task<byte[]> Download(string url)
        {
            url = version.Cdn + platform + '/' + url;
            int retry = 0;
            string error = null;
            do
            {
                using (var request = UnityWebRequest.Get(url))
                {
                    await request.SendWebRequest();
                    if (request.isNetworkError || request.isHttpError)
                        error = request.error;
                    else
                        return request.downloadHandler.data;
                }
            } while (++retry < RetryTimes);
            throw new DownloadException(url, error);
        }

        async Task DownloadToFile(string url, string path)
        {
            url = version.Cdn + platform + '/' + url;
            int retry = 0;
            string error = null;
            do
            {
                using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
                {
                    request.downloadHandler = new DownloadHandlerFile(path);
                    await request.SendWebRequest();
                    if (request.isNetworkError || request.isHttpError)
                        error = request.error;
                    else
                        return;
                }
            } while (++retry < RetryTimes);
            throw new DownloadException(url, error);
        }
    }

    public class DownloadException : ApplicationException
    {
        public DownloadException(string url, string error)
            : base(string.Format("Error while requesting {0} => {1}", url, error))
        { }
    }
}
