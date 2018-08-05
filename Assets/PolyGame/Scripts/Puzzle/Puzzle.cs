using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ResourceModule;

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

    float scrambleRadius { get { return Config.Instance.puzzle.scrambleRadius; } }
    float moveSpeed { get { return Config.Instance.puzzle.moveSpeed; } }
    float fadeSpeed { get { return Config.Instance.puzzle.fadeSpeed; } }
    float finishDebrisMoveSpeed { get { return Config.Instance.puzzle.finishDebrisMoveSpeed; } }
    
    string puzzleName;
    bool[] finished;
    int finishCount;
    List<int> history;
    bool isMovingDebris;

    class DebrisInfo
    {
        public int index;
        public Vector2 position;
    }

    struct OutofBoundDebris
    {
        public Transform target;
        public Vector3 inboundPos;
    }

    Bounds playgroundBounds;
    PolyGraph puzzleObject;
    PuzzleWireframe wireframeObject;
    DebrisMoveContainer debrisMoveContainer;
    Dictionary<GameObject, DebrisInfo> debrisMap = new Dictionary<GameObject, DebrisInfo>();
    List<OutofBoundDebris> outOfBounds = new List<OutofBoundDebris>();
    MaterialPropertyBlock materialPropertyBlock;

    Material objectMat;
    Material finishedMat;
    Material selectedMat;

    int propColor;
    int propAlpha;
    int propZWrite;

    float objectAlpha;
    float finishedAlpha;
    float wireframeAlpha;

    public string PuzzleName { get { return puzzleName; } }
    public PolyGraph PuzzleObject { get { return puzzleObject; } }
    public Bounds PlaygroundBounds { get { return playgroundBounds; } }
    public bool[] FinishedFlags { get { return finished; } }
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
    }

    void Run(string puzzleName)
    {
        this.puzzleName = puzzleName;
        LoadPuzzleObject();
        LoadWireframe();
        LoadProgress();
        InitMaterials();
        ApplyProgress();
        PuzzleCamera.Instance.Init(puzzleObject.size);
        InitPlaygroundBounds();
        Scramble();
        LoadBackgroundQuad();
    }

    void LoadPuzzleObject()
    {
        var prefab = PrefabLoader.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = prefab.Instantiate<GameObject>(transform);
        go.name = puzzleName;

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            var child = go.transform.GetChild(i);
            Vector3 pos = child.localPosition;
            var info = new DebrisInfo() { index = i, position = pos };
            debrisMap.Add(child.gameObject, info);
            child.localPosition = ArrangeDepth(i, pos);
        }
        puzzleObject = go.GetComponent<PolyGraph>();
    }

    void LoadWireframe()
    {
        var prefab = PrefabLoader.Load(string.Format("{0}/{1}/{1}_{2}", Paths.Artworks, puzzleName, Paths.Wireframe));
        var go = prefab.Instantiate<GameObject>(transform);
        go.transform.position = new Vector3(0f, 0f, Config.Instance.zorder.wireframe);
        go.name = prefab.Name;
        wireframeObject = go.GetComponent<PuzzleWireframe>();
    }

    void LoadBackgroundQuad()
    {
        var go = PuzzleBackground.Create(puzzleObject, playgroundBounds);
        go.transform.SetParent(transform, true);
    }

    void InitMaterials()
    {
        objectMat = puzzleObject.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        finishedMat = Instantiate(objectMat);
        finishedMat.name = objectMat.name + "Finished";
        finishedMat.renderQueue = objectMat.renderQueue - 100;
        selectedMat = Instantiate(objectMat);
        selectedMat.name = objectMat.name + "Selected";

        propColor = Shader.PropertyToID("_Color");
        propAlpha = Shader.PropertyToID("_Alpha");
        propZWrite = Shader.PropertyToID("_ZWrite");

        ShowWireframe(false);

        objectMat.SetFloat(propZWrite, 1f);
        selectedMat.SetFloat(propZWrite, 1f);
        finishedMat.SetFloat(propZWrite, 1f);
        finishedMat.SetColor(propColor, new Color(1f, 1f, 1f, finishedAlpha));
        wireframeObject.Renderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat(propAlpha, wireframeAlpha);
        wireframeObject.Renderer.SetPropertyBlock(materialPropertyBlock);
    }

    void ShowWireframe(bool show)
    {
        if (show)
        {
            objectAlpha = 0f;
            wireframeAlpha = 1f;
            objectMat.SetFloat(propZWrite, 0f);
        }
        else
        {
            objectAlpha = 1f;
            wireframeAlpha = 0f;
            objectMat.SetFloat(propZWrite, 1f);
        }
        finishedAlpha = (Finished || show) ? 1f : 0.25f;
    }

    Vector3 ArrangeDepth(int i, Vector3 pos)
    {
        pos.z = (i + 1) * Config.Instance.zorder.debrisStart;
        return pos;
    }

    void Scramble()
    {
        float radius = Mathf.Min(scrambleRadius, playgroundBounds.extents.x, playgroundBounds.extents.y);
        Vector3 min = playgroundBounds.min + new Vector3(radius, radius);
        Vector3 max = playgroundBounds.max - new Vector3(radius, radius);
        for (int i = 0; i < puzzleObject.transform.childCount; ++i)
        {
            if (i >= finished.Length || !finished[i])
            {
                var child = puzzleObject.transform.GetChild(i);
                var ext = child.GetComponent<MeshRenderer>().bounds.extents;
                var startPos = child.position;
                startPos.x = Mathf.Clamp(startPos.x, min.x, max.x);
                startPos.y = Mathf.Clamp(startPos.y, min.y, max.y);
                float r = Mathf.Max(0, Mathf.Min(radius - ext.x, radius - ext.y));
                child.position = startPos + (Vector3)(UnityEngine.Random.insideUnitCircle * r);
            }
        }
    }

    void ApplyProgress()
    {
        for (int i = 0; i < finished.Length; ++i)
        {
            if (finished[i])
            {
                var child = puzzleObject.transform.GetChild(i);
                if (null == child)
                {
                    GameLog.LogErrorFormat("Missing child({0}) when applying progress", i);
                    continue;
                }

                var pos = child.localPosition;
                pos.z = Config.Instance.zorder.debrisFinished;
                child.localPosition = pos;
                child.GetComponent<MeshRenderer>().sharedMaterial = finishedMat;
                child.GetComponent<Collider>().enabled = false;
            }
        }
    }

    void InitPlaygroundBounds()
    {
        var cam = PuzzleCamera.Main;
        Vector2 pos = puzzleObject.size;
        pos = pos / 2f;
        playgroundBounds = Utils.CalculateBounds(pos, cam.aspect, cam.orthographicSize);
    }

    void OnObjMove(Transform objPicked, Vector2 screenCurrent)
    {
        if (null == debrisMoveContainer.Target)
            return;

        Vector3 pos = debrisMoveContainer.transform.position;
        Vector2 newPos = PuzzleCamera.Main.ScreenToWorldPoint(screenCurrent);
        pos = Vector3.Lerp(pos, newPos, Time.deltaTime * moveSpeed);
        debrisMoveContainer.transform.position = pos;
    }

    bool OnObjPicked(Transform objPicked)
    {
        if (isMovingDebris || null == objPicked)
            return false;

        DebrisInfo di;
        if (!debrisMap.TryGetValue(objPicked.gameObject, out di) || finished[di.index])
            return false;

        wireframeObject.SetColor(Color.black, puzzleObject, puzzleObject.regions[di.index]);

        Vector3 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition;
        debrisMoveContainer.transform.position = (Vector2)PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
        debrisMoveContainer.Target = objPicked;
        objPicked.GetComponent<MeshRenderer>().sharedMaterial = selectedMat;

        ShowWireframe(true);
        isMovingDebris = true;
        return true;
    }

    void OnObjReleased(Transform objPicked)
    {
        var target = debrisMoveContainer.Target;
        var targetRenderer = target.GetComponent<MeshRenderer>();
        var targetCollider = target.GetComponent<Collider>();

        targetRenderer.sharedMaterial = objectMat;
        debrisMoveContainer.Target = null;

        DebrisInfo di;
        if (!debrisMap.TryGetValue(target.gameObject, out di))
        {
            GameLog.LogError(target + " is not found in debris map");
        }
        else
        {
            wireframeObject.ResetColors();

            if (Vector2.Distance(target.localPosition, di.position) <= PuzzleVars.fitThreshold)
            {
                finished[di.index] = true;
                ++finishCount;
                history.Add(di.index);
                needToSave = true;
                targetRenderer.sharedMaterial = finishedMat;
                targetCollider.enabled = false;
                var finishPosition = (Vector3)di.position;
                finishPosition.z = Config.Instance.zorder.debrisFinished;
                StartCoroutine(FinishDebrisAnimation(target, finishPosition));
                return;
            }

            var bounds = playgroundBounds;
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

                OutofBoundDebris obd = new OutofBoundDebris();
                obd.target = target;
                obd.inboundPos = pos;
                targetCollider.enabled = false;
                outOfBounds.Add(obd);
            }
        }
        ShowWireframe(false);
        isMovingDebris = false;
    }

    #region debug
    [ContextMenu("Debug Switch Wireframe")]
    void DebugSwitchWireframe()
    {
        ShowWireframe(objectAlpha == 1f);
    }

    void OnDrawGizmos()
    {
        var bounds = playgroundBounds;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bounds.min, bounds.min + new Vector3(0f, bounds.size.y, 0f));
        Gizmos.DrawLine(bounds.min + new Vector3(0f, bounds.size.y, 0f), bounds.max);
        Gizmos.DrawLine(bounds.max, bounds.max - new Vector3(0f, bounds.size.y, 0f));
        Gizmos.DrawLine(bounds.max - new Vector3(0f, bounds.size.y, 0f), bounds.min);
    }

    [ContextMenu("Leave One Unfinished")]
    void LeaveOneUnfinished()
    {
        if (null != finished)
        {
            int i = Array.IndexOf(finished, false);
            if (i >= 0)
            {
                for (int j = i + 1; j < finished.Length; ++j)
                {
                    if (!finished[j])
                    {
                        finished[j] = true;
                        history.Add(j);
                    }
                }
            }
        }
    }
    #endregion debug
}
