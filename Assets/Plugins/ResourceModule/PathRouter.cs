using UnityEngine;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ResourceModule
{
    public enum PathLocation
    {
        InApp,
        Sandbox,
        NotFound,
    }

    public class PathRouter
    {
        public const string AssetBundleSuffix = ".ab";
        public const string PrefabsFolder = "Prefabs/";
        public const string ScenesFolder = "Scenes/";
        public const string ABFolder = "AssetBundles";
        public const string ABManifest = "AssetBundles/AssetBundles";
        public const string Version = "Version.json";
        public const string FileManifest = "FileManifest.json";
        public const string Sandbox = "Sandbox";
        public const string Res = "Assets/Res";
        public const string StartScene = "Assets/Scenes/Start.unity";

        public static string ProductPath = Application.dataPath + "/../Product/";

        public static string FileProtocol
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return "file:///";
                }
                else
                {
                    return "file://";
                }
            }
        }

        public static string StreamingPath;
        public static string StreamingPathWithProtocol;

        public static string PersistentPath;
        public static string PersistentPathWithProtocol;

        public static string SandboxPath;
        public static string SandboxPathWithProtocol;

        public static void Init()
        {
            PersistentPath = Application.persistentDataPath + '/';
            PersistentPathWithProtocol = FileProtocol + PersistentPath;

            SandboxPath = PersistentPath + Sandbox + '/';
            SandboxPathWithProtocol = FileProtocol + SandboxPath;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    {
                        StreamingPath = Application.streamingAssetsPath + '/';
                        StreamingPathWithProtocol = FileProtocol + StreamingPath;
                    }
                    break;
                case RuntimePlatform.Android:
                    {
                        StreamingPath = Application.dataPath + "!/assets/";
                        StreamingPathWithProtocol = Application.streamingAssetsPath + '/';
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    {
                        StreamingPath = Application.streamingAssetsPath + '/';
                        StreamingPathWithProtocol = Uri.EscapeUriString(FileProtocol + StreamingPath);
                    }
                    break;
                default:
                    throw new ApplicationException("Unsupported platform: " + Application.platform);
            }

            ResLog.Log("DataPath: " + Application.dataPath);
            ResLog.Log("PersistentPath: " + PersistentPath);
            ResLog.Log("PersistentPathWithProtocol: " + PersistentPathWithProtocol);
            ResLog.Log("SandboxPath: " + SandboxPath);
            ResLog.Log("SandboxPathWithProtocol: " + SandboxPathWithProtocol);
            ResLog.Log("StreamingPath: " + StreamingPath);
            ResLog.Log("StreamingPathWithProtocol: " + StreamingPathWithProtocol);
        }

        public static string NoPrefix(string path)
        {
            // remove "Assets/"
            return path.Remove(0, 7);
        }

        public static string NormalizePath(string path)
        {
            return Regex.Replace(path.Replace('\\', '/'), @"/+", "/");
        }

        public static PathLocation Exists(string path)
        {
            string ppath = SandboxPath + path;
            if (File.Exists(ppath))
                return PathLocation.Sandbox;

            bool exists = false;
            string spath = StreamingPath + path;
            if (!Application.isEditor && Application.platform == RuntimePlatform.Android)
                exists = AndroidPlugin.IsAssetExists(path);
            else
                exists = File.Exists(spath);

            return exists ? PathLocation.InApp : PathLocation.NotFound;
        }

        public static PathLocation GetFullPath(string path, bool withFileProtocol, out string fullpath)
        {
            string ppath = SandboxPath + path;
            if (File.Exists(ppath))
            {
                fullpath = withFileProtocol ? SandboxPathWithProtocol + path : ppath;
                return PathLocation.Sandbox;
            }

            bool exists = false;
            string spath = StreamingPath + path;
            if (!Application.isEditor && Application.platform == RuntimePlatform.Android)
                exists = AndroidPlugin.IsAssetExists(path);
            else
                exists = File.Exists(spath);

            if (exists)
            {
                fullpath = withFileProtocol ? StreamingPathWithProtocol + path : spath;
                return PathLocation.InApp;
            }

            fullpath = null;
            return PathLocation.NotFound;
        }

        public static string BuildOutput(string buildTarget)
        {
            return Application.dataPath + "/../Build/Output/" + buildTarget;
        }
    }
}
