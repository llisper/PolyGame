using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule.Test
{
    class TestBytesLoader : Tester
    {
        List<string> paths = new List<string>();

        public override async Task Test()
        {
            foreach (string ab in AssetBundleLoader.Manifest.GetAllAssetBundles())
                paths.Add(PathRouter.ABFolder + '/' + ab);

            DeleteSandboxFiles();
            await InAppTest();
            await SandboxTest();
            DeleteSandboxFiles();
        }

        void DeleteSandboxFiles()
        {
            string path = PathRouter.SandboxPath + PathRouter.ABFolder;
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        async Task InAppTest()
        {
            foreach (string p in paths)
            {
                byte[] bytes = BytesLoader.Load(p);
                Assert(bytes.Length > 0);
            }
            ResLog.Log("InApp: Load Finished");

            foreach (string p in paths)
            {
                byte[] bytes = await BytesLoader.AsyncLoad(p);
                Assert(bytes.Length > 0);
            }
            ResLog.Log("InApp: AsyncLoad Finished");
        }

        async Task SandboxTest()
        {
            string p = paths[0];
            byte[] bytes = BytesLoader.Load(p);
            int originalLength = bytes.Length;
            int newLength = originalLength * 2;

            string ppath = PathRouter.SandboxPath + p;
            Directory.CreateDirectory(Path.GetDirectoryName(ppath));
            File.WriteAllBytes(ppath, new byte[newLength]);
            ResLog.LogFormat("Write {0} bytes to {1}", newLength, ppath);

            bytes = BytesLoader.Load(p);
            Assert(bytes.Length > 0);
            ResLog.LogFormat("length:{0} original:{1}", bytes.Length, originalLength);
            Assert(bytes.Length == newLength);
            ResLog.Log("Sandbox: Load Finished");
            bytes = await BytesLoader.AsyncLoad(p);
            Assert(bytes.Length > 0);
            Assert(bytes.Length == newLength);
            ResLog.LogFormat("length:{0} original:{1}", bytes.Length, originalLength);
            ResLog.Log("Sandbox: AsyncLoad Finished");
        }
    }
}
