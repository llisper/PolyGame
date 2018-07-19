using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule
{
    public abstract class ResourceLoader
    {
        internal static Dictionary<Type, Dictionary<string, ResourceLoader>> _loadersPool = 
            new Dictionary<Type, Dictionary<string, ResourceLoader>>();

        public delegate void OnProgress(float value);

        protected Task task;
        protected event OnProgress onProgress;
        protected float progress;
        LoadTimer timer;

        public string Url { get; protected set; }
        public int RefCount { get; protected set; }
        public bool IsComplete { get; protected set; }
        public object ResultObject { get; protected set; }
        public bool HasError { get; protected set; }

        public static bool IsDev
        {
            get
            {
                if (Application.isPlaying)
                {
                    return ResourceSystem.ResMode == ResourceSystem.Mode.Dev && 
                        Application.isEditor;
                }
                else
                {
                    return true;
                }
            }
        }

        public float Progress
        {
            get { return progress; }
            protected set
            {
                progress = value;
                onProgress?.Invoke(progress);
            }
        }

        public void Release()
        {
            if (--RefCount <= 0)
            {
                if (RefCount < 0)
                    ResLog.LogWarningFormat("{0} RefCount({1}) < 0", ToString(), RefCount);

                var typeDict = GetTypeDict(GetType());
                typeDict.Remove(Url);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, Url);
        }

        protected abstract Task AsyncRun();

        protected static T AutoNew<T>(string url, OnProgress onProgress)
            where T : ResourceLoader, new()
        {
            var typeDict = GetTypeDict(typeof(T));
            ResourceLoader loader;
            if (!typeDict.TryGetValue(url, out loader))
            {
                loader = new T();
                loader.Url = url;
                loader.timer = LoadTimer.Start<T>(url);
                typeDict.Add(url, loader);
            }
            ++loader.RefCount;
            if (null != onProgress)
                loader.onProgress += onProgress;
            if (null == loader.task)
                loader.task = loader.AsyncRun();
            return (T)loader;
        }

        protected static Dictionary<string, ResourceLoader> GetTypeDict(Type type)
        {
            Dictionary<string, ResourceLoader> typesDict;
            if (!_loadersPool.TryGetValue(type, out typesDict))
                typesDict = _loadersPool[type] = new Dictionary<string, ResourceLoader>();
            return typesDict;
        }

        protected void Finish(object result, bool hasError = false)
        {
            IsComplete = true;
            ResultObject = result;
            HasError = hasError;
            timer.Stop();
        }
    }
}
