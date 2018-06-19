using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Battlehub.Wireframe;

public class Puzzle : MonoBehaviour
{
    public static void Start(string puzzleName)
    {
        GameScene.LoadScene("Puzzle", (c, n) => 
        {
            var go = new GameObject("Puzzle_" + puzzleName);
            var puzzle = go.AddComponent<Puzzle>();
            puzzle.Run(puzzleName);
        });
    }

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

    PolyGraphBehaviour puzzleObject;
    GameObject wireframeObject;
    DebrisMoveContainer debrisMoveContainer;
    Dictionary<GameObject, DebrisInfo> debrisMap = new Dictionary<GameObject, DebrisInfo>();

    Material objectMat;
    Material wireframeMat;
    Material finishedMat;
    Material selectedMat;

    int propColor;
    int propAlpha;
    float objectAlpha;
    float finishedAlpha;
    float wireframeAlpha;

    void Awake()
    {
        var go = new GameObject("DebrisMoveContainer");
        go.transform.SetParent(transform);
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

        if (null != wireframeMat)
            Destroy(wireframeMat);
        if (null != finishedMat)
            Destroy(finishedMat);
        if (null != selectedMat)
            Destroy(selectedMat);
    }

    void Update()
    {
        if (null != puzzleObject)
        {
            objectMat.SetColor(propColor, Color.Lerp(objectMat.GetColor(propColor), new Color(1f, 1f, 1f, objectAlpha), Time.deltaTime * fadeSpeed));
            wireframeMat.SetFloat(propAlpha, Mathf.Lerp(wireframeMat.GetFloat(propAlpha), wireframeAlpha, Time.deltaTime * fadeSpeed));
            finishedMat.SetColor(propColor, Color.Lerp(finishedMat.GetColor(propColor), new Color(1f, 1f, 1f, finishedAlpha), Time.deltaTime * fadeSpeed));
        }
    }

    void Run(string puzzleName)
    {
        this.puzzleName = puzzleName;
        Load();

        // TODO: if there is a save, load the save, or else start anew
        StartNew();

        PuzzleCamera.Instance.Init(puzzleObject.size);
    }

    void StartNew()
    {
        Scramble();
    }

    void Load()
    {
        var prefab = Resources.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = (GameObject)Instantiate(prefab);
        go.transform.SetParent(transform);

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            var child = go.transform.GetChild(i);
            Vector3 pos = child.localPosition;
            var info = new DebrisInfo() { index = i, position = pos };
            debrisMap.Add(child.gameObject, info);
            child.localPosition = ArrangeDepth(i, pos);
        }
        finished = new bool[go.transform.childCount];
        puzzleObject = go.GetComponent<PolyGraphBehaviour>();

        GenerateWireframe();
        InitMaterials();
    }

    void GenerateWireframe()
    {
        wireframeObject = new GameObject(puzzleName + "Wireframe", typeof(MeshFilter), typeof(MeshRenderer));
        wireframeObject.transform.SetParent(transform);

		var meshFilters = puzzleObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < combine.Length; ++i)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.name = puzzleName + "WireframeMesh";
        mesh.CombineMeshes(combine);
		Barycentric.CalculateBarycentric(mesh);
		wireframeObject.GetComponent<MeshFilter>().sharedMesh = mesh;

        var mat = new Material(Shader.Find("Battlehub/WireframeSimplify"));
        mat.name = puzzleName + "WireframeMaterial";
        var renderer = wireframeObject.GetComponent<MeshRenderer>();
        Utils.SetupMeshRenderer(renderer);
        renderer.sharedMaterial = mat;
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

        ShowWireframe(false);
        finishedMat.SetColor(propColor, new Color(1f, 1f, 1f, finishedAlpha));
        wireframeMat.SetFloat(propAlpha, wireframeAlpha);
        wireframeMat.SetColor(propColor, new Color32(200, 200, 200, 255));
        wireframeMat.SetFloat("_Thickness", 0.75f);
    }

    void ShowWireframe(bool show)
    {
        if (show)
        {
            objectAlpha = 0f;
            finishedAlpha = 1f;
            wireframeAlpha = 1f;
        }
        else
        {
            objectAlpha = 1f;
            finishedAlpha = 0.25f;
            wireframeAlpha = 0f;
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
            var child = puzzleObject.transform.GetChild(i);
            child.localPosition += (Vector3)(Random.insideUnitCircle * ScrambleRadius);
        }
    }

    void OnObjMove(Vector2 screenCurrent)
    {
        if (null == debrisMoveContainer.Target)
            return;

        Vector3 pos = debrisMoveContainer.transform.position;
        Vector3 newPos = PuzzleCamera.Main.ScreenToWorldPoint(screenCurrent);
        newPos.z = pos.z;
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
        debrisMoveContainer.transform.position = PuzzleCamera.Main.ScreenToWorldPoint(screenPos);
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
        debrisMoveContainer.Target = null;

        DebrisInfo di;
        if (!debrisMap.TryGetValue(target.gameObject, out di))
        {
            target.GetComponent<MeshRenderer>().sharedMaterial = objectMat;
            Debug.LogError(target + " is not found in debris map");
        }
        else
        {
            if (Vector2.Distance(target.localPosition, di.position) <= fitThreshold)
            {
                finished[di.index] = true;
                target.GetComponent<MeshRenderer>().sharedMaterial = finishedMat;
                target.GetComponent<MeshCollider>().enabled = false;
                StartCoroutine(FinishDebrisAnimation(target, di.position));
            }
        }
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
}
