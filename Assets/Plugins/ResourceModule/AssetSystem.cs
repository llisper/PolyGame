using UnityEngine;
using System;
using System.Collections.Generic;

namespace ResourceModule
{
    public class AssetSystem : Singleton<AssetSystem>
    {
        #region inspector
        // how long before a non-referenced asset to be moved to waitToDispose list
        public float disposeDelay = 20f;
        // how many assets in waitToDispose list to be disposed in one frame
        public int disposePerFrame = 1;
        // tick interval, seconds
        public float tickInterval = 2f;
        #endregion inspector

        internal Dictionary<string, AssetHandle> assets = new Dictionary<string, AssetHandle>();
        internal List<AssetHandle> waitToDispose = new List<AssetHandle>();
        List<AssetHandle> tickAssets = new List<AssetHandle>();
        float tickTimer;

        public AssetHandle FindAsset(string path)
        {
            AssetHandle h;
            if (assets.TryGetValue(path, out h))
            {
                h.WaitForDispose = false;
                h.UpdateRefDropToZeroTime();
            }
            return h;
        }

        public T FindAsset<T>(string path) where T : AssetHandle
        {
            return (T)FindAsset(path);
        }

        public void AddAsset(AssetHandle h)
        {
            if (assets.ContainsKey(h.Path))
            {
                throw new ArgumentException(string.Format(
                    "Asset of path {0} already exists in AssetManager",
                    h.Path));
            }

            assets.Add(h.Path, h);
            if (h.RequireTick)
                tickAssets.Add(h);
            ResLog.Log("(AssetManager) add asset " + h.Path);
        }

        public void GarbageCollect()
        {
            while (CollectPass(true))
                DisposePass(true);
            DisposePass(true);
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        void Update()
        {
            CollectPass();
            DisposePass();
            Tick();
        }

        bool CollectPass(bool collectNow = false)
        {
            bool collectAny = false;
            float now = Time.unscaledTime;
            var etor = assets.GetEnumerator();
            while (etor.MoveNext())
            {
                var h = etor.Current.Value;
                if (!h.WaitForDispose)
                {
                    bool shouldDispose = (h.RefCount == 0) && 
                        (collectNow || now - h.TimeWhenRefDropToZero > disposeDelay);

                    if (h.AleadyDisposed || shouldDispose)
                    {
                        waitToDispose.Add(h);
                        h.WaitForDispose = true;
                        collectAny = true;
                        ResLog.Log("(AssetManager) collect " + h.Path);
                    }
                }
            }
            return collectAny;
        }

        void DisposePass(bool disposeAll = false)
        {
            int i = 0;
            for (int disposedCount = 0;
                 i < waitToDispose.Count && (disposeAll || disposedCount < disposePerFrame);
                 ++i)
            {
                var h = waitToDispose[i];
                if (h.WaitForDispose)
                {
                    try
                    {
                        h.Dispose();
                        assets.Remove(h.Path);
                        ResLog.Log("(AssetManager) dispose " + h.Path);
                    }
                    catch (Exception e)
                    {
                        ResLog.LogException(e);
                    }
                    ++disposedCount;
                }
            }
            waitToDispose.RemoveRange(0, i);
        }

        void Tick()
        {
            if ((tickTimer += Time.deltaTime) >= tickInterval)
            {
                tickTimer = 0f;
                for (int i = tickAssets.Count - 1; i >= 0; --i)
                {
                    var a = tickAssets[i];
                    if (a.AleadyDisposed)
                        tickAssets.RemoveAt(i);
                    else if (!a.WaitForDispose)
                        a.Tick();
                }
            }
        }
    }
}
