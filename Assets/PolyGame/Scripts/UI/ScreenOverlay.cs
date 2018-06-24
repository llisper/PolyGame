using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenOverlay : Panel
{
    public float fadeSpeed = 15f;

    static ScreenOverlay Instance;

    CanvasGroup canvasGroup;
    float toAlpha;
    Coroutine fadingRoutine;

    void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        gameObject.SetActive(false);    
    }

    public override bool Persistent { get { return true; } }

    public static Coroutine Fade(bool fadeIn)
    {
        if (null != Instance)
        {
            Instance.gameObject.SetActive(true);
            Instance.Finish();
            Instance.fadingRoutine = Instance.StartCoroutine(Instance.Fading(fadeIn));
            return Instance.fadingRoutine;
        }
        return null;
    }

    IEnumerator Fading(bool fadeIn)
    {
        toAlpha = fadeIn ? 0f : 1f;
        float alpha = canvasGroup.alpha;
        while (Mathf.Abs(toAlpha - alpha) > 0.001f)
        {
            alpha = Mathf.Lerp(alpha, toAlpha, Time.deltaTime * fadeSpeed);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = toAlpha;
        fadingRoutine = null;

        if (toAlpha == 0f)
            gameObject.SetActive(false);
    }

    void Finish()
    {
        if (null != fadingRoutine)
        {
            StopCoroutine(fadingRoutine);
            canvasGroup.alpha = toAlpha;
            fadingRoutine = null;
        }
    }
}
