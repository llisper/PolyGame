using UnityEngine;
using System;
using System.Collections.Generic;

public enum UILayer
{
    Base,
    Overlay,
    Count,
}

public class UI : MonoBehaviour
{
    public static UI Instance;
    public Canvas Canvas;

    Transform[] layers;
    Dictionary<Type, Panel> panels = new Dictionary<Type, Panel>();

    public static void Init()
    {
        Instantiate(Resources.Load("UI/UI"));
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        layers = new Transform[Canvas.transform.childCount];
        for (int i = 0; i < Canvas.transform.childCount; ++i)
            layers[i] = Canvas.transform.GetChild(i);
    }

    void Start()
    {
        OpenPanel<ScreenOverlay>(UILayer.Overlay);
        OpenPanel<FpsCounter>(UILayer.Overlay);
    }

    public Panel OpenPanel<T>(UILayer layer = UILayer.Base) where T : Panel
    {
        Type type = typeof(T);
        Panel panel;
        if (panels.TryGetValue(type, out panel))
            return panel;

        string path = "UI/" + type.Name;
        var prefab = Resources.Load(path);
        if (null == prefab)
        {
            Debug.LogError("Failed to load UI: " + path);
            return null;
        }

        var go = (GameObject)Instantiate(prefab, GetLayer(layer));
        panel = go.GetComponent<Panel>();
        panels.Add(type, panel);
        return panel;
    }

    public void ClosePanel<T>() where T : Panel
    {
        ClosePanel(typeof(T));
    }

    public void ClosePanel(Type type)
    {
        Panel panel;
        if (panels.TryGetValue(type, out panel))
        {
            Destroy(panel.gameObject);
            panels.Remove(type);
        }
    }

    public void ClosePanelsWhenSceneDestroy()
    {
        var removeList = new List<Panel>();
        foreach (var kv in panels)
        {
            if (!kv.Value.Persistent)
                removeList.Add(kv.Value);
        }

        for (int i = 0; i < removeList.Count; ++i)
        {
            var p = removeList[i];
            panels.Remove(p.GetType());
            Destroy(p.gameObject);
        }
    }

    Transform GetLayer(UILayer layer)
    {
        int i = (int)layer;
        if (i >= 0 && i < layers.Length)
        {
            return layers[i];
        }
        else
        {
            Debug.LogError("Failed to GetLayer: " + layer);
            return Canvas.transform;
        }
    }
}
