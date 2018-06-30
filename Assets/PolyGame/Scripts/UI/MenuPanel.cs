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
            string path = PuzzleSnapshot.SavePath(text);
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
