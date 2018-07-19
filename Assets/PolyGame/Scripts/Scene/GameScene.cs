using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Threading.Tasks;
using ResourceModule;

public class GameScene
{
    public interface IScene
    {
        void Start();
        void OnDestroy();
    }

    public delegate void OnSceneLoaded();
    static OnSceneLoaded onSceneLoaded;

    static IScene current;

    public static async Task Init()
    {
        await LoadScene<MenuScene>();
    }

    public static async Task LoadScene<T>(OnSceneLoaded onSceneLoaded = null) where T : IScene
    {
        GameScene.onSceneLoaded = onSceneLoaded;
        await Loading(typeof(T));
    }

    static async Task Loading(Type sceneType)
    {
        string sceneName = sceneType.Name.Replace("Scene", "");
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogErrorFormat("Failed to load scene {0}({1})", sceneName, sceneType.Name);
            onSceneLoaded = null;
            return;
        }

        await ScreenOverlay.AsyncFade(false);

        if (null != current)
            current.OnDestroy();
        current = null;
        UI.Instance.ClosePanelsWhenSceneDestroy();

        await SceneLoader.AsyncLoad(sceneName);

        current = (IScene)Activator.CreateInstance(sceneType);
        current.Start();

        if (null != onSceneLoaded)
            onSceneLoaded();
        onSceneLoaded = null;

        await ScreenOverlay.AsyncFade(true);
    }
}
