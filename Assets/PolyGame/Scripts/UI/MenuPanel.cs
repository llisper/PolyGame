using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class MenuPanel : Panel
{
    public List<string> options;
    public Dropdown dropdown;
    public RawImage snapshot;

    Texture2D tex2d;

    void Awake()
    {
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
                // NOTE: can't load the whole puzzle prefab just to find out the snapshot texture reference
                var prefab = (GameObject)Resources.Load(string.Format("{0}/{1}/{1}", Paths.Artworks, text));
                if (null != prefab)
                {
                    snapshot.texture = prefab.GetComponent<PolyGraph>().initialSnapshot;
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
