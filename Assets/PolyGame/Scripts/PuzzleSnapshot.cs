using UnityEngine;
using System;
using System.IO;

[ExecuteInEditMode]
public class PuzzleSnapshot : MonoBehaviour
{
    Camera camera;
    RenderTexture renderTexture;
    Material originMat;
    Material greyscaleMat;
    PolyGraphBehaviour puzzleObject;

    string puzzleName;

    void Awake()
    {
        var size = Config.SnapshotSize;
        renderTexture = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGBHalf);
        renderTexture.name = "PuzzleSnapshotRT";
        renderTexture.Create();

        var camGo = (GameObject)Instantiate(Resources.Load(Prefabs.PuzzleCamera), transform, true);
        camera = camGo.GetComponent<Camera>();
        camera.cullingMask = (1 << Layers.Snapshot);
        camera.targetTexture = renderTexture;
        camGo.SetActive(false);
    }

    void OnDestroy()
    {
        camera.targetTexture = null;
        Utils.Destroy(renderTexture);
        if (null != originMat)
            Utils.Destroy(originMat);
        if (null != greyscaleMat)
            Utils.Destroy(greyscaleMat);
    }

    public void Init(string puzzleName, bool[] finished = null)
    {
        this.puzzleName = puzzleName;
        var prefab = Resources.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = (GameObject)Instantiate(prefab, transform);
        InternalInit(go.GetComponent<PolyGraphBehaviour>(), finished);
    }

    public void Init(PolyGraphBehaviour puzzleObject, Material mat, bool[] finished = null)
    {
        puzzleName = puzzleObject.name;
        InternalInit(Instantiate(puzzleObject, transform), finished);
    }

    public void OnFinish(int index)
    {
        var child = puzzleObject.transform.GetChild(index);
        var renderer = child.GetComponent<MeshRenderer>();
        if (null != renderer)
            renderer.sharedMaterial = originMat;
    }

    public static string SavePath(string puzzleName)
    {
        return string.Format("{0}/{1}/Snapshot.jpg", Paths.Saves, puzzleName);
    }

    [ContextMenu("Save")]
    public void Save()
    {
        if (null != puzzleObject)
        {
            var currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture; 
            camera.gameObject.SetActive(true);
            camera.Render();

            try
            {
                var size = Config.SnapshotSize;
                Texture2D tex2d = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
                tex2d.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);

                byte[] bytes = tex2d.EncodeToJPG();
                string path = SavePath(puzzleName);
                File.WriteAllBytes(path, bytes);
                Debug.Log("Save snapshot to " + path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                RenderTexture.active = currentRT;
                camera.gameObject.SetActive(false);
            }
        }
    }

    void InternalInit(PolyGraphBehaviour puzzleObject, bool[] finished)
    {
        this.puzzleObject = puzzleObject;
        var renderer = puzzleObject.GetComponentInChildren<MeshRenderer>();
        if (null != renderer)
            InitMaterial(renderer.sharedMaterial);

        for (int i = 0; i < puzzleObject.transform.childCount; ++i)
        {
            var child = puzzleObject.transform.GetChild(i);
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
        PuzzleCamera.SetupCamera(camera, puzzleObject.size);
    }

    void InitMaterial(Material mat)
    {
        if (null != greyscaleMat)
        {
            Utils.Destroy(greyscaleMat);
            greyscaleMat = null;
        }

        if (null != mat)
        {
            originMat = Instantiate(mat);
            originMat.name = mat.name;

            greyscaleMat = Instantiate(mat);
            greyscaleMat.name = mat.name + "Greyscale";
            greyscaleMat.EnableKeyword(ShaderFeatures._GREYSCALE);
        }
    }
}

public static class PuzzleSnapshotOneOff
{
    public static void Take(string puzzleName, bool[] finished = null)
    {
        var go = new GameObject("PuzzleSnapshot");
        var snapshot = go.AddComponent<PuzzleSnapshot>();
        snapshot.Init(puzzleName, finished);
        snapshot.Save();
        Utils.Destroy(go);
    }

    public static void Take(PolyGraphBehaviour puzzleObject, Material mat, bool[] finished = null)
    {
        var go = new GameObject("PuzzleSnapshot");
        var snapshot = go.AddComponent<PuzzleSnapshot>();
        snapshot.Init(puzzleObject, mat, finished);
        snapshot.Save();
        Utils.Destroy(go);
    }
}