using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule.Test
{
    class TestWWWLoader : Tester
    {
        List<string> urls = new List<string>();

        public override async Task Test()
        {
            foreach (string ab in AssetBundleLoader.Manifest.GetAllAssetBundles())
            {
                string relpath = PathRouter.ABFolder + '/' + ab;
                string fullpath;
                Assert(PathLocation.NotFound != PathRouter.GetFullPath(relpath, true, out fullpath));
                urls.Add(fullpath);
            }

            ResLog.Log("ResourceSystem.Init");
            await TestLoadOne(urls[0]);
            ResLog.Log("TestLoadOne finished");
            await TestLoadMutiple();
            ResLog.Log("TestLoadMutiple finished");
        }

        async Task TestLoadOne(string url)
        {
            byte[] wwwBytes = await WWWLoader.AsyncLoad(url);
            Assert(wwwBytes.Length > 0);
        }

        async Task TestLoadMutiple()
        {
            List<Task> tasks = new List<Task>();
            foreach (string url in urls)
                tasks.Add(TestLoadOne(url));
            await Task.WhenAll(tasks);
        }
    }
}
