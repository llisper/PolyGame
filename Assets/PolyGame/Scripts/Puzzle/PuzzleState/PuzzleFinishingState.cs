using UnityEngine;
using DG.Tweening;
using UI;

public class PuzzleFinishingState : PuzzleState
{
    float flashDuration = 2f;
    Ease flashEase = Ease.OutCubic;
    float cameraDuration = 3f;

    public override void Start(params object[] p)
    {
        ScreenFader.Instance.Fade(true, true);
        ScreenFader.Instance.Fade(false);
        DOTween.Sequence()
               .Append(ResetCamera())
               .AppendCallback(() => Data.finishedMat.SetColor("_Highlight", Color.white))
               .Append(Data.finishedMat.DOColor(Color.black, "_Highlight", 1f).SetEase(Ease.OutSine))
               .AppendCallback(() => GameEvent.Instance.Fire(GameEvent.PuzzleFinished));
    }

    Sequence ResetCamera()
    {
        var initialPos = PuzzleCamera.Instance.InitialPosition;
        var initialSize = PuzzleCamera.Instance.InitialOrthoSize;
        var cam = PuzzleCamera.Main;

        return DOTween.Sequence()
                      .Append(cam.transform.DOMove(initialPos, cameraDuration))
                      .Insert(0f, cam.DOOrthoSize(initialSize, cameraDuration));

    }
}
