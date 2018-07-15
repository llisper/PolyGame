using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class MenuPanel : Panel
{
    public Dropdown dropdown;
    public RawImage snapshot;

    Texture2D tex2d;

    void Awake()
    {
        List<string> options = new List<string>();
        foreach (var g in ArtCollection.Instance.groups)
            options.AddRange(g.items.ConvertAll(v => v.name));

        dropdown.AddOptions(options);
        LoadSnapshot();
    }

    void OnDestroy()
    {
        if (null != tex2d)        
            Destroy(tex2d);
    }

    void LoadSnapshot()
    {
        string text = dropdown.captionText.text;
        snapshot.enabled = false;
        if (!string.IsNullOrEmpty(text))
        {
            string path = Paths.SnapshotSave(text);
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (null == tex2d)
                {
                    tex2d = new Texture2D(2, 2);
                    snapshot.texture = tex2d;
                }
                tex2d.LoadImage(bytes);
                snapshot.enabled = true;
            }
            else
            {
                var prefab = (GameObject)Resources.Load(string.Format(
                    "{0}/{1}/{1}_{2}",
                    Paths.Artworks,
                    text,
                    Path.GetFileNameWithoutExtension(PuzzleSnapshot.FileName)));

                if (null != prefab)
                {
                    snapshot.texture = prefab.GetComponent<PuzzleSnapshotHolder>().texture;
                    snapshot.enabled = true;
                }
            }
        }
    }

    public void OnValueChanged(int index)
    {
        LoadSnapshot();
    }

    public void OnStartClicked()
    {
        string text = dropdown.captionText.text;
        if (!string.IsNullOrEmpty(text))
            Puzzle.Start(text);
    }
}
