using UnityEngine;
using System;
using System.Collections.Generic;

namespace ResourceModule
{
    public abstract class AssetHandle
    {
        int refCount;
        internal List<AssetHandle> dependencies = new List<AssetHandle>();

        public string Path { get; private set; }
        public bool WaitForDispose { get; set; }
        public bool AleadyDisposed { get; private set; }
        public float TimeWhenRefDropToZero { get; private set; }
        public virtual bool RequireTick { get { return false; } }

        public int RefCount
        {
            get { return refCount; }
            internal set
            {
                int origin = refCount;
                refCount = value;
                if (refCount < 0)
                {
                    refCount = 0;
                    ResLog.LogErrorFormat("{0} refCount < 0 !", this);
                }

                if (origin > 0 && refCount == 0)
                    UpdateRefDropToZeroTime();
            }
        }

        public AssetHandle(string path)
        {
            Path = path;
        }

        public void AddDependency(AssetHandle handle)
        {
            if (dependencies.Contains(handle))
            {
                throw new ApplicationException(string.Format(
                    "{0} already has a reference to {1}",
                    this, handle));
            }

            ++handle.RefCount;
            dependencies.Add(handle);
        }

        public void Dispose()
        {
            if (!AleadyDisposed)
            {
                AleadyDisposed = true;
                for (int i = 0; i < dependencies.Count; ++i)
                    --dependencies[i].RefCount;
                dependencies.Clear();
                OnDispose();
            }
        }

        public void UpdateRefDropToZeroTime()
        {
            TimeWhenRefDropToZero = Time.unscaledTime;
        }

        protected abstract void OnDispose();

        public virtual void Tick() { }

        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, Path);
        }
    }
}
