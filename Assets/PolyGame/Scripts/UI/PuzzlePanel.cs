using UnityEngine.UI;
using DG.Tweening;

public class PuzzlePanel : Panel
{
    public Text[] finishedTitles;

    void Awake()
    {
        GameEvent.Instance.Subscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
    }

    void OnDestroy()
    {
        GameEvent.Instance.Unsubscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
    }

    public void OnBackClicked()
    {
        GameScene.LoadScene<MenuScene>().WrapErrors();
    }

    void OnPuzzleFinished(int e, object[] p)
    {
        var seq = DOTween.Sequence();
        for (int i = 0; i < finishedTitles.Length; ++i)
            seq.Insert(i, finishedTitles[i].DOFade(1f, 1f).SetEase(Ease.InOutSine));
    }
}
