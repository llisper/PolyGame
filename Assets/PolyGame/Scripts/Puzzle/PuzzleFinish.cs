using UnityEngine;
using System.Collections;
using Lean.Touch;
using DG.Tweening;
using UI;

public partial class Puzzle : MonoBehaviour
{
    IEnumerator FinishDebrisAnimation(Transform xform, Vector3 position)
    {
        if (Finished)
            LeanTouch.Instance.enabled = false;
    
        while (Vector3.Distance(xform.localPosition, position) > 0.1f)
        {
            xform.localPosition = Vector3.Lerp(xform.localPosition, position, Time.deltaTime * finishDebrisMoveSpeed);
            yield return null;
        }
        xform.localPosition = position;
        ShowWireframe(false);
        isMovingDebris = false;

        if (Finished)
            AllFinishedAnimation();
    }

    public float flashDuration = 2f;
    public Ease flashEase = Ease.OutCubic;
    public float cameraDuration = 3f;

    void AllFinishedAnimation()
    {
        ScreenFader.Instance.Fade(true, true);
        ScreenFader.Instance.Fade(false);
        DOTween.Sequence()
               .Append(ResetCamera())
               .AppendCallback(() => finishedMat.SetColor("_Highlight", Color.white))
               .Append(finishedMat.DOColor(Color.black, "_Highlight", 1f).SetEase(Ease.OutSine))
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