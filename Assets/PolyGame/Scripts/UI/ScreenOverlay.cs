using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;

public class ScreenOverlay : Panel
{
    public float defaultDuration = 0.5f;
    public Ease defaultEase = Ease.OutSine;

    static ScreenOverlay Instance;

    CanvasGroup canvasGroup;
    float toAlpha;
    bool isTweening;

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

    public static async Task AsyncFade(bool fadeIn, float? duration = null, Ease? easeType = null)
    {
        if (null != Instance)
        {
            if (Instance.isTweening)
                await Awaiters.Until(() => !Instance.isTweening);

            Instance.gameObject.SetActive(true);
            await Instance.Fading(
                fadeIn,
                duration.HasValue ? duration.Value : Instance.defaultDuration,
                easeType.HasValue ? easeType.Value : Instance.defaultEase);
        }
    }

    IEnumerator Fading(bool fadeIn, float duration, Ease easeType)
    {
        isTweening = true;
        toAlpha = fadeIn ? 0f : 1f;
        yield return canvasGroup
            .DOFade(toAlpha, duration)
            .SetEase(easeType)
            .WaitForCompletion();

        if (toAlpha == 0f)
            gameObject.SetActive(false);
        isTweening = false;
    }
}
