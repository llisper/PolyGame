using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule.Test
{
    class TestAssetBundleLoader : Tester
    {
        List<string> paths = new List<string>();

        public override async Task Test()
        {
            foreach (string ab in AssetBundleLoader.Manifest.GetAllAssetBundles())
                paths.Add(ab);

            var handle = await AssetBundleLoader.AsyncLoad("Prefabs/CompleteLevelArt.prefab.ab");
            ++handle.RefCount;
            await Awaiters.Seconds(5f);
            --handle.RefCount;
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Async Load Finished");

            handle = AssetBundleLoader.Load("Prefabs/CompleteLevelArt.prefab.ab");
            ++handle.RefCount;
            await Awaiters.Seconds(5f);
            --handle.RefCount;
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Load Finished");

            List<AssetHandle> handles = new List<AssetHandle>();
            foreach (string ab in AssetBundleLoader.Manifest.GetAllAssetBundles())
            {
                var h = await AssetBundleLoader.AsyncLoad(ab);
                ++h.RefCount;
                handles.Add(h);
            }
            ResLog.Log("Async Load All");
            await Awaiters.Seconds(5f);
            ResLog.Log("Disposing");
            handles.ForEach(h => --h.RefCount);
            handles.Clear();
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Async Load All Finished");

            foreach (string ab in AssetBundleLoader.Manifest.GetAllAssetBundles())
            {
                var h = AssetBundleLoader.Load(ab);
                ++h.RefCount;
                handles.Add(h);
            }
            ResLog.Log("Load All");
            await Awaiters.Seconds(5f);
            ResLog.Log("Disposing");
            handles.ForEach(h => --h.RefCount);
            handles.Clear();
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Load All Finished");
        }
    }
}
