using UnityEngine;
using System;
using System.IO;
using ResourceModule;

[ExecuteInEditMode]
public class PuzzleSnapshot : MonoBehaviour
{
    Camera ssCamera;
    RenderTexture renderTexture;
    Material originMat;
    Material greyscaleMat;
    PolyGraph graph;
    GameObject backgroundObject;

    string puzzleName;

    void Awake()
    {
        var size = Config.Instance.snapshotSize;
        renderTexture = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 8;
        renderTexture.name = "PuzzleSnapshotRT";
        renderTexture.Create();

        var camPrefab = PrefabLoader.Load(Prefabs.PuzzleCamera);
        var camGo = camPrefab.Instantiate<GameObject>(transform, true);
        ssCamera = camGo.GetComponent<Camera>();
        ssCamera.cullingMask = (1 << Layers.Snapshot);
        ssCamera.targetTexture = renderTexture;
        ssCamera.backgroundColor = Color.white;
        camGo.SetActive(false);
    }

    void OnDestroy()
    {
        ssCamera.targetTexture = null;
        Utils.Destroy(renderTexture);
        Clear();
    }

    public void Init(string puzzleName, bool[] finished = null)
    {
        this.puzzleName = puzzleName;
        var prefab = PrefabLoader.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = prefab.Instantiate<GameObject>(transform);
        InternalInit(go.GetComponent<PolyGraph>(), finished);
    }

    public void Init(PolyGraph puzzleObject, bool[] finished = null)
    {
        puzzleName = puzzleObject.name;
        InternalInit(Instantiate(puzzleObject, transform), finished);
    }

    public void OnFinish(int index)
    {
        var child = graph.transform.GetChild(index);
        var renderer = child.GetComponent<MeshRenderer>();
        if (null != renderer)
            renderer.sharedMaterial = originMat;
    }

    public void Take(string path = null, bool destroy = true)
    {
        if (null != graph)
        {
            var currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture; 
            ssCamera.Render();

            try
            {
                var size = Config.Instance.snapshotSize;
                Texture2D tex2d = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                tex2d.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);

                byte[] bytes = tex2d.EncodeToPNG();
                path = null != path ? path : Paths.SnapshotSave(puzzleName);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, bytes);
                GameLog.Log("Save snapshot to " + path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                RenderTexture.active = currentRT;
                if (destroy)
                    Utils.Destroy(gameObject);
            }
        }
    }

    void InternalInit(PolyGraph graph, bool[] finished)
    {
        Clear();

        this.graph = graph;
        graph.transform.localPosition = new Vector3(0f, 0f, Config.Instance.zorder.debrisStart);
        var renderer = graph.GetComponentInChildren<MeshRenderer>();
        if (null != renderer)
            InitMaterial(renderer.sharedMaterial);

        for (int i = 0; i < graph.transform.childCount; ++i)
        {
            var child = graph.transform.GetChild(i);
            child.gameObject.layer = Layers.Snapshot;
            renderer = child.GetComponent<MeshRenderer>();
            if (null != renderer)
            {
                if (null == finished || i >= finished.Length || !finished[i])
                    renderer.sharedMaterial = greyscaleMat;
                else
                    renderer.sharedMaterial = originMat;
            }
        }
        PuzzleCamera.SetupCamera(ssCamera, graph.size, Config.Instance.camera.sizeExtendScale);
        InitBackground();
    }

    void InitMaterial(Material mat)
    {
        if (null != mat)
        {
            originMat = Instantiate(mat);
            originMat.name = mat.name;

            greyscaleMat = Instantiate(mat);
            greyscaleMat.name = mat.name + "Greyscale";
            greyscaleMat.EnableKeyword(ShaderFeatures._GREYSCALE);
        }
    }

    void InitBackground()
    {
        Vector2 pos = graph.size;
        pos = pos / 2f;
        var bounds = Utils.CalculateBounds(pos, ssCamera.aspect, ssCamera.orthographicSize);
        backgroundObject = PuzzleBackground.Create(graph, bounds);
        backgroundObject.layer = Layers.Snapshot;
        backgroundObject.transform.SetParent(transform, true);
        var renderer = backgroundObject.GetComponent<MeshRenderer>();
        var mat = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
        mat.EnableKeyword(ShaderFeatures._USE_CIRCLE_ALPHA);
        if (!Application.isPlaying)
            mat.SetColor("_Color", new Color32(140, 140, 140, 255));
    }

    void Clear()
    {
        if (null != graph)
        {
            Utils.Destroy(graph);
            graph = null;
        }

        if (null != backgroundObject)
        {
            var renderer = backgroundObject.GetComponent<MeshRenderer>();
            if (Application.isPlaying)
                Utils.Destroy(renderer.material);
            else
                Utils.Destroy(renderer.sharedMaterial);
            Utils.Destroy(backgroundObject);
            backgroundObject = null;
        }

        if (null != originMat)
        {
            Utils.Destroy(originMat);
            originMat = null;
        }

        if (null != greyscaleMat)
        {
            Utils.Destroy(greyscaleMat);
            greyscaleMat = null;
        }
    }
}

public static class PuzzleSnapshotOneOff
{
    public static void Take(string puzzleName, bool[] finished = null, string savePath = null)
    {
        var go = new GameObject("PuzzleSnapshot");
        var snapshot = go.AddComponent<PuzzleSnapshot>();
        snapshot.Init(puzzleName, finished);
        snapshot.Take(savePath);
    }

    public static void Take(PolyGraph puzzleObject, bool[] finished = null, string savePath = null)
    {
        var go = new GameObject("PuzzleSnapshot");
        var snapshot = go.AddComponent<PuzzleSnapshot>();
        snapshot.Init(puzzleObject, finished);
        snapshot.Take(savePath);
    }
}