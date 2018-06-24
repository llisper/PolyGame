using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameScene
{
    public interface IScene
    {
        void Start();
        void OnDestroy();
    }

    public delegate void OnSceneLoaded(Scene current, Scene next);
    static OnSceneLoaded onSceneLoaded;

    static IScene current;
    static bool sceneChangedFlag;

    public static void Init()
    {
        SceneManager.activeSceneChanged += ActiveSceneChanged;
        LoadScene<MenuScene>();
    }

    public static void LoadScene<T>(OnSceneLoaded onSceneLoaded = null) where T : IScene
    {
        GameScene.onSceneLoaded = onSceneLoaded;
        Game.Instance.StartCoroutine(Loading(typeof(T)));
    }

    static IEnumerator Loading(Type sceneType)
    {
        string sceneName = sceneType.Name.Replace("Scene", "");
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogErrorFormat("Failed to load scene {0}({1})", sceneName, sceneType.Name);
            onSceneLoaded = null;
            yield break;
        }

        yield return ScreenOverlay.Fade(false);

        if (null != current)
            current.OnDestroy();
        current = null;
        UI.Instance.ClosePanelsWhenSceneDestroy();

        sceneChangedFlag = false;
        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        while (asyncOp.isDone)
            yield return null;

        current = (IScene)Activator.CreateInstance(sceneType);
        current.Start();

        while (!sceneChangedFlag)
            yield return null;
        yield return ScreenOverlay.Fade(true);
    }

    static void ActiveSceneChanged(Scene current, Scene next)
    {
        if (null != onSceneLoaded)
            onSceneLoaded(current, next);
        onSceneLoaded = null;
        sceneChangedFlag = true;
    }
}
