using UnityEngine;

public class PuzzleNormalState : PuzzleState
{
    public override void Start(params object[] p)
    {
        Data.objectAlpha = 1f;
        Data.wireframeAlpha = 0f;
        Data.objectMat.SetFloat(Data.propZWrite, 1f);
        Data.finishedAlpha = Data.Finished ? 1f : 0.25f;
    }

    public override bool OnObjPicked(Transform objPicked)
    {
        if (null == objPicked)
            return false;

        Puzzle.DebrisInfo di;
        if (!Data.debrisMap.TryGetValue(objPicked.gameObject, out di) || Data.finished[di.index])
            return false;

        Data.wireframeObject.SetColor(Color.black, Data.puzzleObject, Data.puzzleObject.regions[di.index]);

        Vector3 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition;
        Data.debrisMoveContainer.transform.position = (Vector2)PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
        Data.debrisMoveContainer.Target = objPicked;
        objPicked.GetComponent<MeshRenderer>().sharedMaterial = Data.selectedMat;

        //ShowWireframe(true);
        Next<PuzzleSolvingState>();
        return true;
    }
}
