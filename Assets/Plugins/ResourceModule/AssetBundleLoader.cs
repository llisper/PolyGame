using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule
{
    internal class AssetBundleHandle : AssetHandle
    {
        public AssetBundle Bundle { get; set; }

        public AssetBundleHandle(string path) : base(path) { } 

        protected override void OnDispose()
        {
            if (null != Bundle)
                Bundle.Unload(true);
        }
    }

    internal class AssetBundleLoader : ResourceLoader
    {
        public static AssetBundleHandle Load(string path)
        {
            path = path.ToLower();
            using (var timer = LoadTimer.Start<AssetBundleLoader>(path))
            {
                var handle = AssetSystem.Instance.FindAsset<AssetBundleHandle>(path);
                if (null == handle)
                {
                    handle = new AssetBundleHandle(path);
                    try
                    {
                        LoadDependencies(handle, path);

                        string loadUrl = GetLoadUrl(path);
                        var bundle = AssetBundle.LoadFromFile(loadUrl);
                        if (null == bundle)
                        {
                            throw new ApplicationException(string.Format(
                                "Failed to load AssetBundle:{0}",
                                path));
                        }

                        handle.Bundle = bundle;
                        AssetSystem.Instance.AddAsset(handle);
                    }
                    catch (Exception e)
                    {
                        handle.Dispose();
                        throw new ApplicationException("Error when loading " + path, e);
                    }
                }
                return handle;
            }
        }

        public static async Task<AssetBundleHandle> AsyncLoad(string path, OnProgress onProgress = null)
        {
            path = path.ToLower();
            var handle = AssetSystem.Instance.FindAsset<AssetBundleHandle>(path);
            if (null == handle)
            {
                var loader = AutoNew<AssetBundleLoader>(path, onProgress);
                await loader.task;
                handle = (AssetBundleHandle)loader.ResultObject;
                loader.Release();
            }
            return handle;
        }

        protected override async Task AsyncRun()
        {
            var assetBundleHandle = new AssetBundleHandle(Url);
            try
            {
                await AsyncLoadDependencies(assetBundleHandle, Url);
                Progress = 0.5f;

                string loadUrl = GetLoadUrl(Url);
                var request = AssetBundle.LoadFromFileAsync(loadUrl);
                await request;

                var bundle = request.assetBundle;
                if (null == bundle)
                {
                    throw new ApplicationException(string.Format(
                        "Failed to load AssetBundle:{0}",
                        loadUrl));
                }
                Progress = 1f;

                assetBundleHandle.Bundle = bundle;
                AssetSystem.Instance.AddAsset(assetBundleHandle);
                Finish(assetBundleHandle);
            }
            catch (Exception e)
            {
                Finish(null);
                assetBundleHandle.Dispose();
                throw new ApplicationException("Error when loading AssetBundle:" + Url, e);
            }
        }

        static void LoadDependencies(AssetBundleHandle handle, string abUrl)
        {
            var manifest = Manifest;
            string[] deps = manifest.GetAllDependencies(abUrl);
            for (int i = 0; i < deps.Length; ++i)
            {
                string d = deps[i];
                var depHandle = AssetSystem.Instance.FindAsset(d);
                if (null == depHandle)
                    depHandle = Load(d);
                handle.AddDependency(depHandle);
            }
        }

        static async Task AsyncLoadDependencies(AssetBundleHandle handle, string abUrl)
        {
            var manifest = Manifest;
            string[] deps = manifest.GetAllDependencies(abUrl);
            var depLoadTasks = new List<Task<AssetBundleHandle>>();
            for (int i = 0; i < deps.Length; ++i)
            {
                string d = deps[i];
                var depHandle = AssetSystem.Instance.FindAsset(d);
                if (null != depHandle)
                    handle.AddDependency(depHandle);
                else
                    depLoadTasks.Add(AsyncLoad(d));
            }

            while (depLoadTasks.Count > 0)
            {
                var task = await Task.WhenAny(depLoadTasks);
                handle.AddDependency(task.Result);
                depLoadTasks.Remove(task);
            }
        }

        static string GetLoadUrl(string url)
        {
            url = PathRouter.ABFolder + '/' + url;
            string fullurl;
            var loc = PathRouter.GetFullPath(url, true, out fullurl);
            if (loc == PathLocation.NotFound)
                throw new FileNotFoundException("AssetBundleLoader", url);

            if (fullurl.StartsWith(PathRouter.FileProtocol))
                fullurl = fullurl.Substring(PathRouter.FileProtocol.Length);
            return fullurl;
        }

        #region AssetBundleManifest 
        static AssetBundleManifest _manifest;

        internal static AssetBundleManifest Manifest
        {
            get
            {
                PreloadManifest();
                return _manifest;
            }
        }

        internal static void PreloadManifest()
        {
            if (null == _manifest)
            {
                byte[] bytes = BytesLoader.Load(PathRouter.ABManifest);
                var ab = AssetBundle.LoadFromMemory(bytes);
                _manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }
        #endregion AssetBundleManifest 
    }
}
