using UnityEngine;
using System;
using System.Collections.Generic;

namespace ResourceModule.Debug
{
    internal class ResourceModuleDebugger : MonoBehaviour
    {
        #region inspector
        public float visDestroyTimeout = 2f;
        #endregion inspector

        public static ResourceModuleDebugger Instance { get; private set; }

        public static void Init()
        {
            var go = new GameObject("_ResourceModuleDebugger");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ResourceModuleDebugger>();
        }

        class Category
        {
            public Type loaderType;
            public GameObject gameObject;
            public Dictionary<string, LoaderVisualizer> visualizers = new Dictionary<string, LoaderVisualizer>();
        }

        Dictionary<Type, Category> categories = new Dictionary<Type, Category>();

        void Update()
        {
            var pool = ResourceLoader._loadersPool;
            foreach (var kv in pool)
                UpdateVisualizers(GetCategory(kv.Key), kv.Value);
        }

        Category GetCategory(Type type)
        {
            Category category;
            if (!categories.TryGetValue(type, out category))
            {
                category = new Category();
                category.loaderType = type;
                category.gameObject = new GameObject(type.Name);
                category.gameObject.transform.SetParent(transform);
                categories.Add(type, category);
            }
            return category;
        }

        void UpdateVisualizers(Category category, Dictionary<string, ResourceLoader> dict)
        {
            foreach (var kv in dict)
            {
                LoaderVisualizer vis;
                category.visualizers.TryGetValue(kv.Key, out vis);
                if (null == vis)
                {
                    var go = new GameObject(kv.Value.Url);
                    go.transform.SetParent(category.gameObject.transform);
                    vis = go.AddComponent<LoaderVisualizer>();
                    category.visualizers[kv.Key] = vis;
                }
                vis.loader = kv.Value;
            }
        }
    }

}