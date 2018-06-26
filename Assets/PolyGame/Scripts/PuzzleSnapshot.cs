using UnityEngine;

[ExecuteInEditMode]
public class PuzzleSnapshot : MonoBehaviour
{
    Material originMat;
    Material greyscaleMat;
    PolyGraphBehaviour puzzleObject;

    void Awake()
    {
        // create camera        
        // create rendertexture
    }

    void OnDestroy()
    {
        if (null != originMat)
            Destroy(originMat);
        if (null != greyscaleMat)
            Destroy(greyscaleMat);
    }

    public void Init(string puzzleName, bool[] finished = null)
    {
        var prefab = Resources.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, puzzleName));
        var go = (GameObject)Instantiate(prefab, transform);
        InternalInit(go.GetComponent<PolyGraphBehaviour>(), finished);
    }

    public void Init(PolyGraphBehaviour puzzleObject, Material mat, bool[] finished = null)
    {
        InternalInit(Instantiate(puzzleObject, transform), finished);
    }

    public void OnFinish(int index)
    {
        var child = puzzleObject.transform.GetChild(index);
        var renderer = child.GetComponent<MeshRenderer>();
        if (null != renderer)
            renderer.sharedMaterial = originMat;
    }

    public void Save()
    {
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
    }

    void InitMaterial(Material mat)
    {
        if (null != greyscaleMat)
        {
            Destroy(greyscaleMat);
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