using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#pragma warning disable 649
namespace ResourceModule.Test
{
    class LoadPrefabTester : MonoBehaviour
    {
        #region inspector
        public Dropdown dropdown;
        public Text instanceCount;
        public Text progress;
        public ModeSelector modeSelector;
        #endregion inspector

        class ResourceInfo
        {
            public string path;
            public List<GameObject> gameObjects = new List<GameObject>();

            public ResourceInfo(string path) { this.path = path; }
        }

        bool initialized;
        bool loading;
        List<ResourceInfo> resourceInfo = new List<ResourceInfo>();

        void Start()
        {
            Init();
        }

        async void Init()
        {
            await ResourceSystem.Init();
            modeSelector.Init();
            AddOptions();
            UpdateInstanceCount();
            initialized = true;
        }

        void AddOptions()
        {
            List<string> options = new List<string>()
            {
                "Prefabs/CompleteDustTrail",
                "Prefabs/CompleteLevelArt",
                "Prefabs/CompletePumpJack",
                "Prefabs/CompleteShell",
                "Prefabs/CompleteShellExplosion",
                "Prefabs/CompleteTank",
                "Prefabs/CompleteTankExplosion",
                "Prefabs/DustTrail",
                "Prefabs/LevelArt",
                "Prefabs/MaterialTest",
                "Prefabs/PumpJack",
                "Prefabs/ShellExplosion",
                "Prefabs/TankExplosion",
            };
            foreach (var opt in options)
                resourceInfo.Add(new ResourceInfo(opt));
            dropdown.AddOptions(options);
        }

        void UpdateInstanceCount()
        {
            var r = resourceInfo[dropdown.value];
            instanceCount.text = string.Format("instances: " + r.gameObjects.Count);
        }

        async void AsyncAdd()
        {
            loading = true;
            int index = dropdown.value;
            var r = resourceInfo[index];
            var handle = await PrefabLoader.AsyncLoad(
                r.path,
                v => progress.text = string.Format("Loading {0:P2}", v));
            progress.text = string.Empty;
            r.gameObjects.Add(handle.Instantiate<GameObject>());
            UpdateInstanceCount();
            loading = false;
        }

        void SyncAdd()
        {
            int index = dropdown.value;
            var r = resourceInfo[index];
            var handle = PrefabLoader.Load(r.path);
            r.gameObjects.Add(handle.Instantiate<GameObject>());
            UpdateInstanceCount();
        }

        void Remove()
        {
            int index = dropdown.value;
            var r = resourceInfo[index];
            if (r.gameObjects.Count > 0)
            {
                GameObject.Destroy(r.gameObjects[0]);
                r.gameObjects.RemoveAt(0);
            }
            UpdateInstanceCount();
        }

        public void OnSelectionChanged()
        {
            if (!initialized || loading) return;
            UpdateInstanceCount();
        }
        
        public void OnAddInstance()
        {
            if (!initialized || loading) return;
            if (modeSelector.syncMode == "Async")
                AsyncAdd();
            else
                SyncAdd();
        }

        public void OnRemoveInstance()
        {
            if (!initialized || loading) return;
            Remove();
        }

        public void OnGCNow()
        {
            if (!initialized || loading) return;
            AssetSystem.Instance.GarbageCollect();
        }
    }
}
#pragma warning restore 649
