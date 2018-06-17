using UnityEngine.SceneManagement;
using System.Collections;

public class GameScene
{
    public delegate void OnSceneLoaded(Scene current, Scene next);
    static OnSceneLoaded onSceneLoaded;

    static GameScene()
    {
        SceneManager.activeSceneChanged += ActiveSceneChanged;
    }

    public static void LoadScene(string name, OnSceneLoaded onSceneLoaded)
    {
        GameScene.onSceneLoaded = onSceneLoaded;
        Game.Instance.StartCoroutine(Loading(name));
    }

    static IEnumerator Loading(string name)
    {
        var asyncOp = SceneManager.LoadSceneAsync(name);
        while (asyncOp.isDone)
            yield return null;
    }

    static void ActiveSceneChanged(Scene current, Scene next)
    {
        if (null != onSceneLoaded)
            onSceneLoaded(current, next);
        onSceneLoaded = null;
    }
}
