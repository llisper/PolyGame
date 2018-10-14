using System;
using System.Threading.Tasks;
using ResourceModule;
using Experiments;

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

    public static T Current<T>() where T : IScene
    {
        return (T)current;
    }

    static async Task Loading(Type sceneType)
    {
        string sceneName = sceneType.Name.Replace("Scene", "");
        if (string.IsNullOrEmpty(sceneName))
        {
            GameLog.LogErrorFormat("Failed to load scene {0}({1})", sceneName, sceneType.Name);
            onSceneLoaded = null;
            return;
        }

        await ScreenFader.AsyncFade(true);

        if (null != current)
            current.OnDestroy();
        current = null;
        // UI.Instance.ClosePanelsWhenSceneDestroy();

        await SceneLoader.AsyncLoad(sceneName);

        current = (IScene)Activator.CreateInstance(sceneType);
        current.Start();

        if (null != onSceneLoaded)
            onSceneLoaded();
        onSceneLoaded = null;

        await ScreenFader.AsyncFade(false);
    }
}
