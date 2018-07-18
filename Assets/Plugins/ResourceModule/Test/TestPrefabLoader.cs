using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceModule.Test
{
    class TestPrefabLoader : Tester
    {
        public override async Task Test()
        {
            string folder = Application.dataPath + "/Resources/Prefabs";
            foreach (string file in Directory.EnumerateFiles(folder, "*.prefab", SearchOption.AllDirectories))
            {
                string path = PathRouter.NormalizePath(file)
                    .Replace(folder, "Prefabs")
                    .Replace(".prefab", "");
                ResLog.Log(path);
            }

            await TestAsyncLoad();
            await TestLoad();
        }

        async Task TestAsyncLoad()
        {
            List<GameObject> instances = new List<GameObject>();
            var prefabResource = await PrefabLoader.AsyncLoad("Prefabs/CompleteLevelArt");
            instances.Add(prefabResource.Instantiate<GameObject>());
            instances.Add(prefabResource.Instantiate<GameObject>());
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Instantiate Finish");
            await Awaiters.Seconds(5f);

            instances.ForEach(go => GameObject.Destroy(go));
            instances.Clear();
            ResLog.Log("Destroy Instances");
            await Awaiters.Seconds(3f);
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Garbage Collect");
            await Awaiters.Seconds(5f);

            ResLog.Log("TestAsyncLoad Finish");
        }

        async Task TestLoad()
        {
            List<GameObject> instances = new List<GameObject>();
            var prefabResource = PrefabLoader.Load("Prefabs/CompleteLevelArt");
            instances.Add(prefabResource.Instantiate<GameObject>());
            instances.Add(prefabResource.Instantiate<GameObject>());
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Instantiate Finish");
            await Awaiters.Seconds(5f);

            instances.ForEach(go => GameObject.Destroy(go));
            instances.Clear();
            ResLog.Log("Destroy Instances");
            await Awaiters.Seconds(3f);
            AssetSystem.Instance.GarbageCollect();
            ResLog.Log("Garbage Collect");
            await Awaiters.Seconds(5f);

            ResLog.Log("TestLoad Finish");
        }
    }
}
