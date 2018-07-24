using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class LoadingUI : MonoBehaviour
{
    public Image background;
    public Text title;
    public Text progress;

    CanvasGroup canvasGroup;
    bool progressActive;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnDestroy()
    {
        GameEvent.Instance.Unsubscribe(GameEvent.UpdateProgress, OnUpdateProgress);
    }

    public IEnumerator Init()
    {
        GameEvent.Instance.Subscribe(GameEvent.UpdateProgress, OnUpdateProgress);

        var seq = DOTween.Sequence();
        yield return seq
            .Append(background.DOColor(Color.white, 0.5f).SetDelay(0.5f).SetEase(Ease.OutSine))
            .Append(title.DOFade(1f, 0.25f).SetDelay(0.25f).SetEase(Ease.InCubic))
            .Insert(1.25f, title.transform.DOLocalMoveY(150f, 0.25f).SetEase(Ease.OutCubic))
            .WaitForCompletion();
    }

    public IEnumerator Finish()
    {
        yield return canvasGroup
            .DOFade(0f, 1f)
            .SetDelay(0.5f)
            .SetEase(Ease.InOutSine)
            .WaitForCompletion();
        Destroy(gameObject);
    }

    void OnUpdateProgress(int e, object[] p)
    {
        // int level = (int)p[0]; NOTE: ignore for now
        string name = (string)p[1];
        float perc = (float)p[2];

        if (!progressActive)
        { 
            progress.DOFade(1f, 0.15f);
            progressActive = true;
        }
        
        if (perc <= 0f)
            progress.text = name;
        else
            progress.text = string.Format("{0}...{1:P2}", name, perc);
    }
}