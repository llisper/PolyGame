using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UI.Init();
        GameScene.Init();
	}

    #region fps counter
    GUIStyle style;
    float fps;
    float fpsTimer;
    int fpsCounter;

    void OnGUI()
    {
        if (null == style)
        {
            style = new GUIStyle();
            style.fontSize = 15;
            style.normal.textColor = Color.green;
            fpsCounter = Time.frameCount;
        }

        if ((fpsTimer += Time.deltaTime) > 1f)
        {
            fps = (Time.frameCount - fpsCounter) / fpsTimer;
            fpsCounter = Time.frameCount;
            fpsTimer = 0f;
        }
        GUI.Label(new Rect(Screen.width - 75, 10, 75, 30), string.Format("fps: {0:f2}", fps), style);
    }
    #endregion fps counter
}
