using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : Panel
{
    Text text;
    float timer;
    int frameCount;

    void Awake()
    {
        text = GetComponent<Text>();    
        frameCount = Time.frameCount;
    }

    void Update()
    {
        if ((timer += Time.deltaTime) > 1f)
        {
            float fps = (Time.frameCount - frameCount) / timer;
            frameCount = Time.frameCount;
            timer = 0f;
            text.text = string.Format("fps: {0:f2}", fps);
        }
    }

    public override bool Persistent { get { return true; } }
}
