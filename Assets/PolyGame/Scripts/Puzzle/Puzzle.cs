using UnityEngine;
using System.Collections.Generic;

public partial class Puzzle : MonoBehaviour
{
    public static void Start(string puzzleName)
    {
        GameScene.LoadScene<PuzzleScene>(() => 
        {
            var go = new GameObject("Puzzle_" + puzzleName);
            var puzzle = go.AddComponent<Puzzle>();
            puzzle.Run(puzzleName);
        }).WrapErrors();
    }

    public static Puzzle Current;

    PuzzleStateMachine stateMachine;

    float scrambleRadius { get { return Config.Instance.puzzle.scrambleRadius; } }
    float moveSpeed { get { return Config.Instance.puzzle.moveSpeed; } }
    float fadeSpeed { get { return Config.Instance.puzzle.fadeSpeed; } }
    float finishDebrisMoveSpeed { get { return Config.Instance.puzzle.finishDebrisMoveSpeed; } }
    
    public string puzzleName;
    public bool[] finished;
    public int finishCount;
    public List<int> history;
    public bool isMovingDebris;

    public class DebrisInfo
    {
        public int index;
        public Vector2 position;
    }

    public struct OutofBoundDebris
    {
        public Transform target;
        public Vector3 inboundPos;
    }

    public Bounds playgroundBounds;
    public PolyGraph puzzleObject;
    public PuzzleWireframe wireframeObject;
    public DebrisMoveContainer debrisMoveContainer;
    public Dictionary<GameObject, DebrisInfo> debrisMap = new Dictionary<GameObject, DebrisInfo>();
    public List<OutofBoundDebris> outOfBounds = new List<OutofBoundDebris>();
    public MaterialPropertyBlock materialPropertyBlock;

    public Material objectMat;
    public Material finishedMat;
    public Material selectedMat;

    public int propColor;
    public int propAlpha;
    public int propZWrite;

    public float objectAlpha;
    public float finishedAlpha;
    public float wireframeAlpha;

    public bool Finished { get { return finishCount >= finished.Length; } }

    void Awake()
    {
        Current = this;
        var go = new GameObject("DebrisMoveContainer");
        go.transform.SetParent(transform, true);
        debrisMoveContainer = go.AddComponent<DebrisMoveContainer>();
        materialPropertyBlock = new MaterialPropertyBlock();

        PuzzleTouch.onObjPicked += OnObjPicked;
        PuzzleTouch.onObjMove += OnObjMove;
        PuzzleTouch.onObjReleased += OnObjReleased;
    }

    void OnDestroy()
    {
        PuzzleTouch.onObjPicked -= OnObjPicked;
        PuzzleTouch.onObjMove -= OnObjMove;
        PuzzleTouch.onObjReleased -= OnObjReleased;

        if (null != objectMat)
        {
            objectMat.SetColor(propColor, Color.white);
            objectMat.SetFloat(propZWrite, 1f);
        }
        if (null != finishedMat)
            Destroy(finishedMat);
        if (null != selectedMat)
            Destroy(selectedMat);
        Current = null;
    }

    void Update()
    {
        if (null != puzzleObject)
        {
            objectMat.SetColor(propColor, Color.Lerp(objectMat.GetColor(propColor), new Color(1f, 1f, 1f, objectAlpha), Time.deltaTime * fadeSpeed));
            finishedMat.SetColor(propColor, Color.Lerp(finishedMat.GetColor(propColor), new Color(1f, 1f, 1f, finishedAlpha), Time.deltaTime * fadeSpeed));

            wireframeObject.Renderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat(propAlpha, Mathf.Lerp(materialPropertyBlock.GetFloat(propAlpha), wireframeAlpha, Time.deltaTime * fadeSpeed));
            wireframeObject.Renderer.SetPropertyBlock(materialPropertyBlock);
        }

        if (outOfBounds.Count > 0)
        {
            for (int i = outOfBounds.Count - 1; i >= 0; --i)
            {
                var obd = outOfBounds[i];
                var pos = obd.target.position;
                pos = Vector3.Lerp(pos, obd.inboundPos, Time.deltaTime * moveSpeed);
                if (Vector3.Distance(pos, obd.inboundPos) < 0.1f)
                {
                    pos = obd.inboundPos;
                    obd.target.GetComponent<Collider>().enabled = true;
                    outOfBounds.RemoveAt(i);
                }
                obd.target.position = pos;
            }
        }

        UpdaetProgress();
        stateMachine.Update();
    }

    void Run(string puzzleName)
    {
        this.puzzleName = puzzleName;
        stateMachine = new PuzzleStateMachine(this);
        stateMachine.Add<PuzzleInitState>(true);
        stateMachine.Add<PuzzleNormalState>();
        stateMachine.Add<PuzzleSolvingState>();
        stateMachine.Add<PuzzleFinishingState>();
        stateMachine.Start();
    }

    void OnObjMove(Transform objPicked, Vector2 screenCurrent)
    {
        var state = stateMachine.Current;
        if (null != state)
            state.OnObjMove(objPicked, screenCurrent);
    }

    bool OnObjPicked(Transform objPicked)
    {
        var state = stateMachine.Current;
        if (null != state)
            return state.OnObjPicked(objPicked);
        return false;
    }

    void OnObjReleased(Transform objPicked)
    {
        var state = stateMachine.Current;
        if (null != state)
            state.OnObjReleased(objPicked);
    }

    #region debug
    void OnDrawGizmos()
    {
        var bounds = playgroundBounds;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bounds.min, bounds.min + new Vector3(0f, bounds.size.y, 0f));
        Gizmos.DrawLine(bounds.min + new Vector3(0f, bounds.size.y, 0f), bounds.max);
        Gizmos.DrawLine(bounds.max, bounds.max - new Vector3(0f, bounds.size.y, 0f));
        Gizmos.DrawLine(bounds.max - new Vector3(0f, bounds.size.y, 0f), bounds.min);
    }
    #endregion debug
}
