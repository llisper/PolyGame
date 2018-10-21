using UnityEngine;
using System.Collections;
using Lean.Touch;

public class PuzzleSolvingState : PuzzleState
{
    public override void Start(params object[] p)
    {
        Data.objectAlpha = 0f;
        Data.wireframeAlpha = 1f;
        Data.objectMat.SetFloat(Data.propZWrite, 0f);
        Data.finishedAlpha = 1f;

        var objPicked = (Transform)p[0];

        Vector3 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition;
        Data.debrisMoveContainer.transform.position = (Vector2)PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
        Data.debrisMoveContainer.Target = objPicked;
        objPicked.GetComponent<MeshRenderer>().sharedMaterial = Data.selectedMat;

        if (p.Length > 1)
            Data.wireframeObject.SetColor(Color.black, Data.puzzleObject, (Region)p[1]);
    }

    public override void OnObjMove(Transform objPicked, Vector2 screenCurrent)
    {
        if (null == Data.debrisMoveContainer.Target)
            return;

        Vector3 pos = Data.debrisMoveContainer.transform.position;
        Vector2 newPos = PuzzleCamera.Main.ScreenToWorldPoint(screenCurrent);
        pos = Vector3.Lerp(pos, newPos, Time.deltaTime * moveSpeed);
        Data.debrisMoveContainer.transform.position = pos;
    }

    public override void OnObjReleased(Transform objPicked)
    {
        var target = Data.debrisMoveContainer.Target;
        var targetRenderer = target.GetComponent<MeshRenderer>();
        var targetCollider = target.GetComponent<Collider>();

        targetRenderer.sharedMaterial = Data.objectMat;
        Data.debrisMoveContainer.Target = null;

        bool match = false;
        Puzzle.DebrisInfo di;
        if (!Data.debrisMap.TryGetValue(target.gameObject, out di))
        {
            GameLog.LogError(target + " is not found in debris map");
        }
        else
        {
            Data.wireframeObject.ResetColors();

            if (Vector2.Distance(target.localPosition, di.position) <= PuzzleVars.fitThreshold)
            {
                Data.finished[di.index] = true;
                ++(Data.finishCount);
                Data.history.Add(di.index);
                Data.needToSave = true;
                targetRenderer.sharedMaterial = Data.finishedMat;
                targetCollider.enabled = false;
                var finishPosition = (Vector3)di.position;
                finishPosition.z = Config.Instance.zorder.debrisFinished;
                Data.StartCoroutine(FinishDebrisAnimation(target, finishPosition));
                match = true;
            }

            var bounds = Data.playgroundBounds;
            var b = targetRenderer.bounds;
            b.center = new Vector3(b.center.x, b.center.y, 0f);
            if (!bounds.Contains(b.min) || !bounds.Contains(b.max))
            {
                Vector3 center = b.center;
                Vector3 offset = center - target.position;
                offset.z = 0;

                center.x = Mathf.Clamp(b.center.x, bounds.min.x + b.extents.x, bounds.max.x - b.extents.x);
                center.y = Mathf.Clamp(b.center.y, bounds.min.y + b.extents.y, bounds.max.y - b.extents.y);
                Vector3 pos = center - offset;
                pos.z = target.position.z;

                Puzzle.OutofBoundDebris obd = new Puzzle.OutofBoundDebris();
                obd.target = target;
                obd.inboundPos = pos;
                targetCollider.enabled = false;
                Data.outOfBounds.Add(obd);
            }
        }

        if (!match)
            Next<PuzzleNormalState>();
    }

    IEnumerator FinishDebrisAnimation(Transform xform, Vector3 position)
    {
        if (Data.Finished)
            LeanTouch.Instance.enabled = false;
    
        while (Vector3.Distance(xform.localPosition, position) > 0.1f)
        {
            xform.localPosition = Vector3.Lerp(xform.localPosition, position, Time.deltaTime * finishDebrisMoveSpeed);
            yield return null;
        }
        xform.localPosition = position;

        if (Data.Finished)
            Next<PuzzleFinishingState>();
        else
            Next<PuzzleNormalState>();
    }
}
