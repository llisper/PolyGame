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

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public IEnumerator Init()
    {
        var seq = DOTween.Sequence();
        yield return seq
            .Append(background.DOColor(Color.white, 0.5f).SetDelay(0.5f))
            .Append(title.DOFade(1f, 0.25f).SetDelay(0.25f))
            .Insert(1f, title.transform.DOLocalMoveY(150f, 0.25f))
            .WaitForCompletion();
    }

    public IEnumerator Finish()
    {
        yield return canvasGroup.DOFade(0f, 0.5f).WaitForCompletion();
        Destroy(gameObject);
    }
}