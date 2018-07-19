using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResourceModule
{
    public class PrefabResource : AssetHandle
    {
        int handleRefCount;
        internal UnityEngine.Object prefabObject;
        List<UnityEngine.Object> instantiatedObjects = new List<UnityEngine.Object>();

        public PrefabResource(string path) : base(path) { }

        public string Name { get { return prefabObject.name; } }
        public UnityEngine.Object Object { get { return prefabObject; } }
        public override bool RequireTick { get { return true; } }

        public void KeepReference()
        {
            ++handleRefCount;
            ++RefCount;
        }

        public void Release()
        {
            if (handleRefCount == 0)
            {
                ResLog.LogErrorFormat("{0} handleRefCount < 0 !", this);
                return;
            }
            --handleRefCount;
            --RefCount;
        }

        public override void Tick()
        {
            for (int i = instantiatedObjects.Count - 1; i >= 0; --i)
            {
                if (null == instantiatedObjects[i])
                {
                    instantiatedObjects.RemoveAt(i);
                    --RefCount;
                }
            }
        }

        protected override void OnDispose() { }

        #region Instantiate Methods
        public T Instantiate<T>(Vector3 position, Quaternion rotation) where T : UnityEngine.Object
        {
            return (T)Instantiate(position, rotation);
        }
        public T Instantiate<T>(Transform parent, bool worldPositionStays) where T : UnityEngine.Object
        {
            return (T)Instantiate(parent, worldPositionStays);
        }
        public T Instantiate<T>(Transform parent) where T : UnityEngine.Object
        {
            return (T)Instantiate(parent);
        }
        public T Instantiate<T>(Vector3 position, Quaternion rotation, Transform parent) where T : UnityEngine.Object
        {
            return (T)Instantiate(position, rotation, parent);
        }
        public T Instantiate<T>() where T : UnityEngine.Object
        {
            return (T)Instantiate();
        }
        public UnityEngine.Object Instantiate(Vector3 position, Quaternion rotation, Transform parent)
        {
            AliveCheck();
            var obj = UnityEngine.Object.Instantiate(prefabObject, position, rotation, parent);
            return OnInstantiateFinish(obj);
        }
        public UnityEngine.Object Instantiate(Transform parent)
        {
            AliveCheck();
            var obj = UnityEngine.Object.Instantiate(prefabObject, parent);
            return OnInstantiateFinish(obj);
        }
        public UnityEngine.Object Instantiate()
        {
            AliveCheck();
            var obj = UnityEngine.Object.Instantiate(prefabObject);
            return OnInstantiateFinish(obj);
        }
        public UnityEngine.Object Instantiate(Vector3 position, Quaternion rotation)
        {
            AliveCheck();
            var obj = UnityEngine.Object.Instantiate(prefabObject, position, rotation);
            return OnInstantiateFinish(obj);
        }
        public UnityEngine.Object Instantiate(Transform parent, bool instantiateInWorldSpace)
        {
            AliveCheck();
            var obj = UnityEngine.Object.Instantiate(prefabObject, parent, instantiateInWorldSpace);
            return OnInstantiateFinish(obj);
        }

        void AliveCheck()
        {
            if (AleadyDisposed)
            {
                throw new ApplicationException("Reference to disposed prefab resource: " + Path);
            }
            else if (WaitForDispose)
            {
                WaitForDispose = false;
                UpdateRefDropToZeroTime();
            }
        }
        
        UnityEngine.Object OnInstantiateFinish(UnityEngine.Object obj)
        {
            instantiatedObjects.Add(obj);
            ++RefCount;
            return obj;
        }
        #endregion Instantiate Methods
    }

    public class PrefabLoader : ResourceLoader
    {
        public static PrefabResource Load(string path)
        {
            using (var timer = LoadTimer.Start<PrefabLoader>(path))
            {
                var res = AssetSystem.Instance.FindAsset<PrefabResource>(path);
                if (null == res)
                {
                    res = new PrefabResource(path);
                    try
                    {
                        if (IsDev)
                        {
                            #if UNITY_EDITOR
                            string assetPath = string.Format("{0}/{1}.prefab", PathRouter.Res, path);
                            res.prefabObject = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                            if (null == res.prefabObject)
                            {
                                throw new ApplicationException(string.Format(
                                    "UnityEditor.AssetDatabase.LoadAssetAtPath(\"{0}\") => null", assetPath));
                            }
                            #else
                            throw new ApplicationException("Load prefab in IsDev is not allowed unless in Editor");
                            #endif
                        }
                        else
                        {
                            string bundleName = path + ".prefab" + PathRouter.AssetBundleSuffix;
                            var assetBundleHandle = AssetBundleLoader.Load(bundleName);

                            res.prefabObject = assetBundleHandle.Bundle.LoadAsset(
                                Path.GetFileNameWithoutExtension(path));
                            if (null == res.prefabObject)
                            {
                                throw new ApplicationException(string.Format(
                                    "AssetBundle.LoadAsset({0}) => null, Bundle:{1}",
                                    Path.GetFileNameWithoutExtension(path),
                                    assetBundleHandle.Path));
                            }
                            res.AddDependency(assetBundleHandle);
                        }
                        AssetSystem.Instance.AddAsset(res);
                    }
                    catch (Exception e)
                    {
                        res.Dispose();
                        throw new ApplicationException("Error when loading Prefab:" + path, e);
                    }
                }
                return res;
            }
        }

        public static async Task<PrefabResource> AsyncLoad(string path, OnProgress onProgress = null)
        {
            var res = AssetSystem.Instance.FindAsset<PrefabResource>(path);
            if (null == res)
            {
                var loader = AutoNew<PrefabLoader>(path, onProgress);
                await loader.task;
                res = (PrefabResource)loader.ResultObject;
                loader.Release();
            }
            return res;
        }

        protected override async Task AsyncRun()
        {
            PrefabResource prefabResource = new PrefabResource(Url);
            try
            {
                if (IsDev)
                {
                    #if UNITY_EDITOR
                    string assetPath = string.Format("{0}/{1}.prefab", PathRouter.Res, Url);
                    prefabResource.prefabObject = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (null == prefabResource.prefabObject)
                    {
                        throw new ApplicationException(string.Format(
                            "UnityEditor.AssetDatabase.LoadAssetAtPath(\"{0}\") => null", assetPath));
                    }
                    Progress = 1f;
                    #else
                    throw new ApplicationException("IsDev is not allowed unless in Editor");
                    #endif
                }
                else
                {
                    string bundleName = Url + ".prefab" + PathRouter.AssetBundleSuffix;
                    var assetBundleHandle = await AssetBundleLoader.AsyncLoad(
                        bundleName, v => Progress = v);

                    prefabResource.prefabObject = assetBundleHandle.Bundle.LoadAsset(
                        Path.GetFileNameWithoutExtension(Url));
                    if (null == prefabResource.prefabObject)
                    {
                        throw new ApplicationException(string.Format(
                            "AssetBundle.LoadAsset({0}) => null, Bundle:{1}",
                            Path.GetFileNameWithoutExtension(Url),
                            assetBundleHandle.Path));
                    }
                    prefabResource.AddDependency(assetBundleHandle);
                }
                AssetSystem.Instance.AddAsset(prefabResource);
                Finish(prefabResource);
            }
            catch (Exception e)
            {
                Finish(null);
                prefabResource.Dispose();
                throw new ApplicationException("Error when loading Prefab:" + Url, e);
            }
        }
    }
}
