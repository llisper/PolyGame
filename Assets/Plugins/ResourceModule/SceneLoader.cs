using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ResourceModule
{
    internal class SceneResource : AssetHandle
    {
        public static SceneResource Current;

        public SceneResource(string name) : base(name)
        {
            ++RefCount;
        }

        protected override void OnDispose()
        {
            --RefCount;
        }
    }

    public class SceneLoader : ResourceLoader
    {
        public static void Load(string name)
        {
            string path = PathRouter.ScenesFolder + name;
            using (var timer = LoadTimer.Start<SceneLoader>(path))
            {
                if (null == SceneResource.Current || SceneResource.Current.Path != path)
                {
                    SceneResource sceneResource = new SceneResource(path);
                    try
                    {
                        if (Application.isEditor && IsDev)
                        {
                            BuildSettingsCheck(path);
                            SceneManager.LoadScene(path);
                        }
                        else
                        {
                            string bundleName = path + ".unity" + PathRouter.AssetBundleSuffix;
                            var assetBundleHandle = AssetBundleLoader.Load(bundleName);
                            SceneManager.LoadScene(path);
                            sceneResource.AddDependency(assetBundleHandle);
                        }

                        if (null != SceneResource.Current)
                        {
                            SceneResource.Current.Dispose();
                            AssetSystem.Instance.GarbageCollect();
                        }

                        SceneResource.Current = sceneResource;
                        AssetSystem.Instance.AddAsset(sceneResource);
                    }
                    catch (Exception e)
                    {
                        sceneResource.Dispose();
                        throw new ApplicationException("Error when loading Scene:" + path, e);
                    }
                }
            }
        }

        public static async Task AsyncLoad(string name, OnProgress onProgress = null)
        {
            string path = PathRouter.ScenesFolder + name;
            if (null == SceneResource.Current || SceneResource.Current.Path != path)
            {
                var loader = AutoNew<SceneLoader>(path, onProgress);
                await loader.task;
                loader.Release();
            }
        }

        protected override async Task AsyncRun()
        {
            SceneResource sceneResource = new SceneResource(Url);
            try
            {
                if (Application.isEditor && IsDev)
                {
                    BuildSettingsCheck(Url);
                    await LoadSceneAsync();
                }
                else
                {
                    string bundleName = Url + ".unity" + PathRouter.AssetBundleSuffix;
                    var assetBundleHandle = await AssetBundleLoader.AsyncLoad(
                        bundleName, v => Progress = v * 0.5f);

                    await LoadSceneAsync(0.5f);
                    sceneResource.AddDependency(assetBundleHandle);
                }

                if (null != SceneResource.Current)
                {
                    SceneResource.Current.Dispose();
                    AssetSystem.Instance.GarbageCollect();
                }

                SceneResource.Current = sceneResource;
                AssetSystem.Instance.AddAsset(sceneResource);
                Finish(sceneResource);
            }
            catch (Exception e)
            {
                Finish(null);
                sceneResource.Dispose();
                throw new ApplicationException("Error when loading Scene:" + Url, e);
            }
        }

        async Task LoadSceneAsync(float progressRange = 1f)
        {
            float p = Progress;
            var asyncOp = SceneManager.LoadSceneAsync(Url);
            while (!asyncOp.isDone)
            {
                Progress = p + asyncOp.progress * progressRange;
                await Awaiters.NextFrame;
            }
            Progress = p + progressRange;
        }

        static void BuildSettingsCheck(string path)
        {
            #if UNITY_EDITOR
            foreach (var s in UnityEditor.EditorBuildSettings.scenes)
            {
                if (s.path.Contains(path))
                    return;
            }

            throw new ApplicationException(string.Format(
                "Load scene: {0} is not in Build Settings",
                path));
            #endif
        }
    }
}
