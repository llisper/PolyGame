using UnityEngine;
using UnityEngine.UI;
using System.IO;
using ResourceModule;

public class PuzzleItem : MonoBehaviour
{
    RawImage rawImage;
    Texture2D tex2d;
    PrefabResource snapshotPrefab;
    string graphName;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    void OnDestroy()
    {
        if (null != tex2d)
            GameObject.Destroy(tex2d);
        if (null != snapshotPrefab)
            snapshotPrefab.Release();
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
            if (null == snapshotPrefab)
            {
                snapshotPrefab = PrefabLoader.Load(string.Format(
                    "{0}/{1}/{1}_{2}",
                    Paths.Artworks,
                    graphName,
                    Paths.Snapshot));

                if (null != snapshotPrefab)
                    snapshotPrefab.KeepReference();
            }

            if (null != snapshotPrefab)
            {
                var prefabObject = (GameObject)snapshotPrefab.Object;
                rawImage.texture = prefabObject.GetComponent<PuzzleSnapshotHolder>().texture;
            }
        }
    }
}
