using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PuzzleItem : MonoBehaviour
{
    RawImage rawImage;
    Texture2D tex2d;
    string graphName;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    void OnDestroy()
    {
        if (null != tex2d)
            GameObject.Destroy(tex2d);
    }

    public void Init(string graphName)
    {
        this.graphName = graphName;
        name = graphName;
        Load();
    }

    public void Load()
    {
        string path = Paths.SnapshotSave(graphName);
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (null == tex2d)
            {
                tex2d = new Texture2D(2, 2);
                rawImage.texture = tex2d;
            }
            tex2d.LoadImage(bytes);
        }
        else
        {
            var prefab = (GameObject)Resources.Load(string.Format(
                "{0}/{1}/{1}_{2}",
                Paths.Artworks,
                graphName,
                Paths.Snapshot));

            if (null != prefab)
                rawImage.texture = prefab.GetComponent<PuzzleSnapshotHolder>().texture;
        }
    }
}
