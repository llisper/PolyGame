using UnityEngine;
using ResourceModule;

public class PuzzleInitState : PuzzleState
{
    public override void Start(params object[] p)
    {
        LoadPuzzleObject();
        LoadWireframe();
        Data.LoadProgress();
        InitMaterials();
        ApplyProgress();
        PuzzleCamera.Instance.Init(Data.puzzleObject.size);
        InitPlaygroundBounds();
        Scramble();
        LoadBackgroundQuad();

        Next<PuzzleNormalState>();
    }

    void LoadPuzzleObject()
    {
        var prefab = PrefabLoader.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, Data.puzzleName));
        var go = prefab.Instantiate<GameObject>(Data.transform);
        go.name = Data.puzzleName;

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            var child = go.transform.GetChild(i);
            Vector3 pos = child.localPosition;
            var info = new Puzzle.DebrisInfo() { index = i, position = pos };
            Data.debrisMap.Add(child.gameObject, info);
            child.localPosition = ArrangeDepth(i, pos);
        }
        Data.puzzleObject = go.GetComponent<PolyGraph>();
    }

    void LoadWireframe()
    {
        var prefab = PrefabLoader.Load(string.Format("{0}/{1}/{1}_{2}", Paths.Artworks, Data.puzzleName, Paths.Wireframe));
        var go = prefab.Instantiate<GameObject>(Data.transform);
        go.transform.position = new Vector3(0f, 0f, Config.Instance.zorder.wireframe);
        go.name = prefab.Name;
        Data.wireframeObject = go.GetComponent<PuzzleWireframe>();
    }

    void LoadBackgroundQuad()
    {
        var go = PuzzleBackground.Create(Data.puzzleObject, Data.playgroundBounds);
        go.layer = Layers.Debris;
        go.transform.SetParent(Data.transform, true);
    }

    void InitMaterials()
    {
        Data.objectMat = Data.puzzleObject.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        Data.finishedMat = GameObject.Instantiate(Data.objectMat);
        Data.finishedMat.name = Data.objectMat.name + "Finished";
        Data.finishedMat.renderQueue = Data.objectMat.renderQueue - 100;
        Data.selectedMat = GameObject.Instantiate(Data.objectMat);
        Data.selectedMat.name = Data.objectMat.name + "Selected";

        Data.propColor = Shader.PropertyToID("_Color");
        Data.propAlpha = Shader.PropertyToID("_Alpha");
        Data.propZWrite = Shader.PropertyToID("_ZWrite");

        Data.objectAlpha = 1f;
        Data.wireframeAlpha = 0f;
        Data.finishedAlpha = Data.Finished ? 1f : 0.25f;

        Data.objectMat.SetFloat(Data.propZWrite, 1f);
        Data.selectedMat.SetFloat(Data.propZWrite, 1f);
        Data.finishedMat.SetFloat(Data.propZWrite, 1f);
        Data.finishedMat.SetColor(Data.propColor, new Color(1f, 1f, 1f, Data.finishedAlpha));
        Data.wireframeObject.Renderer.GetPropertyBlock(Data.materialPropertyBlock);
        Data.materialPropertyBlock.SetFloat(Data.propAlpha, Data.wireframeAlpha);
        Data.wireframeObject.Renderer.SetPropertyBlock(Data.materialPropertyBlock);
    }

    Vector3 ArrangeDepth(int i, Vector3 pos)
    {
        pos.z = (i + 1) * Config.Instance.zorder.debrisStart;
        return pos;
    }

    void Scramble()
    {
        float radius = Mathf.Min(scrambleRadius, Data.playgroundBounds.extents.x, Data.playgroundBounds.extents.y);
        Vector3 min = Data.playgroundBounds.min + new Vector3(radius, radius);
        Vector3 max = Data.playgroundBounds.max - new Vector3(radius, radius);
        for (int i = 0; i < Data.puzzleObject.transform.childCount; ++i)
        {
            if (i >= Data.finished.Length || !Data.finished[i])
            {
                var child = Data.puzzleObject.transform.GetChild(i);
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
        for (int i = 0; i < Data.finished.Length; ++i)
        {
            if (Data.finished[i])
            {
                var child = Data.puzzleObject.transform.GetChild(i);
                if (null == child)
                {
                    GameLog.LogErrorFormat("Missing child({0}) when applying progress", i);
                    continue;
                }

                var pos = child.localPosition;
                pos.z = Config.Instance.zorder.debrisFinished;
                child.localPosition = pos;
                child.GetComponent<MeshRenderer>().sharedMaterial = Data.finishedMat;
                child.GetComponent<Collider>().enabled = false;
            }
        }
    }

    void InitPlaygroundBounds()
    {
        var cam = PuzzleCamera.Main;
        Vector2 pos = Data.puzzleObject.size;
        pos = pos / 2f;
        Data.playgroundBounds = Utils.CalculateBounds(pos, cam.aspect, cam.orthographicSize);
    }
}
