using UnityEngine;

public class PuzzleNormalState : PuzzleState
{
    public override void Start(params object[] p)
    {
        Data.objectAlpha = 1f;
        Data.wireframeAlpha = 0f;
        Data.objectMat.SetFloat(Data.propZWrite, 1f);
        Data.finishedAlpha = Data.Finished ? 1f : 0.25f;

        GameEvent.Instance.Subscribe(GameEvent.UseHint, OnUseHint);
    }

    public override void End()
    {
        GameEvent.Instance.Unsubscribe(GameEvent.UseHint, OnUseHint);
    }

    public override bool OnObjPicked(Transform objPicked)
    {
        if (null == objPicked)
            return false;

        Puzzle.DebrisInfo di;
        if (!Data.debrisMap.TryGetValue(objPicked.gameObject, out di) || Data.finished[di.index])
            return false;

        Next<PuzzleSolvingState>(objPicked, Data.puzzleObject.regions[di.index]);
        return true;
    }

    void OnUseHint(int e, object[] p)
    {
        for (int i = 0; i < Data.finished.Length; ++i)
        {
            if (!Data.finished[i])
            {
                Next<PuzzleHintState>(i);
                break;
            }
        }
    }
}
