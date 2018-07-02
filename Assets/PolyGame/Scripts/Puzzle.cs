using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Puzzle : MonoBehaviour
{
    public static void Start(string puzzleName)
    {
        GameScene.LoadScene<PuzzleScene>((c, n) => 
        {
            var go = new GameObject("Puzzle_" + puzzleName);
            var puzzle = go.AddComponent<Puzzle>();
            puzzle.Run(puzzleName);
        });
    }

    public static Puzzle Current;

    const float ScrambleRadius = 50f;
    const float moveSpeed = 30f;
    const float fadeSpeed = 15f;
    const float fitThreshold = 50f;
    const float finishDebrisMoveSpeed = 10f;
    
    string puzzleName;
    bool[] finished;
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
    GameObject wireframeObject;
    DebrisMoveContainer debrisMoveContainer;
    Dictionary<GameObject, DebrisInfo> debrisMap = new Dictionary<GameObject, DebrisInfo>();
    List<OutofBoundDebris> outOfBounds = new List<OutofBoundDebris>();

    Material objectMat;
    Material wireframeMat;
    Material finishedMat;
    Material selectedMat;

    int propColor;
    int propAlpha;
    int propZWrite;

    float objectAlpha;
    float finishedAlpha;
    float wireframeAlpha;

    public string PuzzleName { get { return puzzleName; } }
    public Bounds PlaygroundBounds { get { return playgroundBounds; } }
    public bool[] FinishedFlags { get { return finished; } }

    void Awake()
    {
        Current = this;
        var go = new GameObject("DebrisMoveContainer");
        go.transform.SetParent(transform, true);
        debrisMoveContainer = go.AddComponent<DebrisMoveContainer>();

        PuzzleTouch.onObjPicked += OnObjPicked;
        PuzzleTouch.onObjMove += OnObjMove;
        PuzzleTouch.onObjReleased += OnObjReleased;
    }

    void OnDestroy()
    {
        PuzzleTouch.onObjPicked -= OnObjPicked;
        PuzzleTouch.onObjMove -= OnObjMove;
        PuzzleTouch.onObjReleased -= OnObjReleased;

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
            wireframeMat.SetFloat(propAlpha, Mathf.Lerp(wireframeMat.GetFloat(propAlpha), wireframeAlpha, Time.deltaTime * fadeSpeed));
            finishedMat.SetColor(propColor, Color.Lerp(finishedMat.GetColor(propColor), new Color(1f, 1f, 1f, finishedAlpha), Time.deltaTime * fadeSpeed));
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
                    obd.target.GetComponent<MeshCollider>().enabled = true;
                    outOfBounds.RemoveAt(i);
                }
                obd.target.position = pos;
            }
        }
    }

    void Run(string puzzleName)
    {
        this.puzzleName = puzzleName;
        LoadPuzzleObject();
        LoadWireframe();
        InitMaterials();
        ApplyProgress();
        Scramble();

        PuzzleCamera.Instance.Init(puzzleObject.size);
        var cam = PuzzleCamera.Main;
        Vector2 orthoSize = new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize);
        Vector2 pos = puzzleObject.size;
        pos = pos / 2f;
        playgroundBounds = new Bounds(pos, orthoSize * 2);

    }

    void LoadPuzzleObject()
    {
        var prefab = Resources.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = (GameObject)Instantiate(prefab, transform);
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
        var prefab = Resources.Load(string.Format("{0}/{1}/{1}Wireframe", Paths.Artworks, puzzleName));
        wireframeObject = (GameObject)Instantiate(prefab, transform);
        wireframeObject.name = prefab.name;
    }

    void InitMaterials()
    {
        objectMat = puzzleObject.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        wireframeMat = wireframeObject.GetComponent<MeshRenderer>().sharedMaterial;
        finishedMat = Instantiate(objectMat);
        finishedMat.name = objectMat.name + "Finished";
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
        wireframeMat.SetFloat(propAlpha, wireframeAlpha);
        wireframeMat.SetFloat("_Thickness", 0.75f);
    }

    void ShowWireframe(bool show)
    {
        if (show)
        {
            objectAlpha = 0f;
            finishedAlpha = 1f;
            wireframeAlpha = 1f;
            objectMat.SetFloat(propZWrite, 0f);
        }
        else
        {
            objectAlpha = 1f;
            finishedAlpha = 0.25f;
            wireframeAlpha = 0f;
            objectMat.SetFloat(propZWrite, 1f);
        }
    }

    Vector3 ArrangeDepth(int i, Vector3 pos)
    {
        pos.z = -i * 0.1f;
        return pos;
    }

    void Scramble()
    {
        for (int i = 0; i < puzzleObject.transform.childCount; ++i)
        {
            if (i >= finished.Length || !finished[i])
            {
                var child = puzzleObject.transform.GetChild(i);
                child.localPosition += (Vector3)(Random.insideUnitCircle * ScrambleRadius);
            }
        }
    }

    void ApplyProgress()
    {
        LoadProgress();
        for (int i = 0; i < finished.Length; ++i)
        {
            if (finished[i])
            {
                var child = puzzleObject.transform.GetChild(i);
                if (null == child)
                {
                    Debug.LogErrorFormat("Missing child({0}) when applying progress", i);
                    continue;
                }

                child.GetComponent<MeshRenderer>().sharedMaterial = finishedMat;
                child.GetComponent<MeshCollider>().enabled = false;
            }
        }
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

        Vector3 screenPos = PuzzleTouch.Instance.MainFinger.ScreenPosition;
        debrisMoveContainer.transform.position = (Vector2)PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
        debrisMoveContainer.Target = objPicked;
        objPicked.GetComponent<MeshRenderer>().sharedMaterial = selectedMat;

        ShowWireframe(true);
        isMovingDebris = true;
        Debug.Log("Pick " + objPicked);
        return true;
    }

    void OnObjReleased(Transform objPicked)
    {
        var target = debrisMoveContainer.Target;
        var targetRenderer = target.GetComponent<MeshRenderer>();
        var targetCollider = target.GetComponent<MeshCollider>();

        targetRenderer.sharedMaterial = objectMat;
        debrisMoveContainer.Target = null;

        DebrisInfo di;
        if (!debrisMap.TryGetValue(target.gameObject, out di))
        {
            Debug.LogError(target + " is not found in debris map");
        }
        else
        {
            if (Vector2.Distance(target.localPosition, di.position) <= fitThreshold)
            {
                finished[di.index] = true;
                targetRenderer.sharedMaterial = finishedMat;
                targetCollider.enabled = false;
                StartCoroutine(FinishDebrisAnimation(target, di.position));
                return;
            }

            var bounds = playgroundBounds;
            var b = targetRenderer.bounds;
            if (!bounds.Contains(b.min) || !bounds.Contains(b.max))
            {
                Vector3 center = b.center;
                Vector3 offset = center - target.position;
                offset.z = 0;

                center.x = Mathf.Clamp(b.center.x, bounds.min.x + b.extents.x, bounds.max.x - b.extents.x);
                center.y = Mathf.Clamp(b.center.y, bounds.min.y + b.extents.y, bounds.max.y - b.extents.y);
                Vector3 pos = center - offset;
                pos.z = transform.position.z;

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

    IEnumerator FinishDebrisAnimation(Transform xform, Vector3 position)
    {
        while (Vector3.Distance(xform.localPosition, position) > 0.1f)
        {
            xform.localPosition = Vector3.Lerp(xform.localPosition, position, Time.deltaTime * finishDebrisMoveSpeed);
            yield return null;
        }
        xform.localPosition = position;
        ShowWireframe(false);
        isMovingDebris = false;
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
}
