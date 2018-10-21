using UnityEngine;

public class PuzzleHintState : PuzzleState
{
    int index;
    Transform targetObj;
    Region targetRegion;
    
    MeshCollider[] meshColliders;

    public override void Start(params object[] p)
    {
        Data.objectAlpha = 0f;
        Data.wireframeAlpha = 1f;
        Data.objectMat.SetFloat(Data.propZWrite, 0f);
        Data.finishedAlpha = 1f;

        index = (int)p[0];
        targetObj = Data.puzzleObject.transform.GetChild(index);
        targetRegion = Data.puzzleObject.regions[index];

        targetObj.GetComponent<MeshRenderer>().sharedMaterial = Data.selectedMat;
        Data.wireframeObject.SetColor(Color.black, Data.puzzleObject, targetRegion, true);
        SetMeshColliders(false);
    }

    public override void End()
    {
        SetMeshColliders(true);
        index = -1;
        targetObj = null;
        targetRegion = null;
    }

    public override bool OnObjPicked(Transform objPicked)
    {
        if (objPicked == targetObj)
        {
            Next<PuzzleSolvingState>(targetObj);
            return true;
        }
        return false;
    }

    void SetMeshColliders(bool enable)
    {
        if (null == meshColliders)
            meshColliders = Data.puzzleObject.GetComponentsInChildren<MeshCollider>();

        for (int i = 0; i < meshColliders.Length; ++i)
        {
            if (i != index && !Data.finished[i])
                meshColliders[i].enabled = enable;
        }
    }
}
