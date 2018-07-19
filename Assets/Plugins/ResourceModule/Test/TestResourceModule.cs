using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceModule.Test
{
    public abstract class Tester : MonoBehaviour
    {
        public abstract Task Test();

        protected void Assert(bool cond)
        {
            if (!cond)
                throw new Exception("Assertion Failed!");
        }
    }

    public class TestResourceModule : MonoBehaviour
    {
        List<Type> testerTypes = new List<Type>();

        void Awake()
        {
            testerTypes.Add(typeof(TestWWWLoader));
            testerTypes.Add(typeof(TestBytesLoader));
            testerTypes.Add(typeof(TestAssetBundleLoader));
            testerTypes.Add(typeof(TestPrefabLoader));
        }

        void Start()
        {
            RunTests();    
        }

        async void RunTests()
        {
            await ResourceSystem.Init();
            ResourceSystem.ResMode = ResourceSystem.Mode.AssetBundle;
            AssetSystem.Instance.disposeDelay = 2f;
            ResLog.Log("=== ResourceSystem Initialized ===");

            foreach (Type type in testerTypes)
            {
                ResLog.LogFormat("=== Run {0} ===", type.Name);
                var go = new GameObject();
                var tester = (Tester)go.AddComponent(type);
                try
                {
                    await tester.Test();
                }
                catch (Exception e)
                {
                    ResLog.LogException(e);
                }
                Destroy(go);
                await Awaiters.NextFrame;
                ResLog.Log("=== Done ===");
            }
        }
    }
}
