using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ResourceModule;

public class MyWorksPanel : Panel
{
    public ScrollRect scrollRect;

    static int[] maskIndex = new int[] { 3, 5, 0, 2 };
    PuzzleItem reloadItem;
    HashSet<string> items = new HashSet<string>();

    public override bool Persistent { get { return true; } }

    void Awake()
    {
        UpdateItems();
        UpdateMasks();
    }

    void OnEnable()
    {
        bool modified = UpdateItems();
        if (null != reloadItem)
        {
            reloadItem.Load();
            reloadItem.transform.SetSiblingIndex(0);
            reloadItem = null;
            modified = true;
        }

        if (modified)
            UpdateMasks();
    }

    void OnItemClicked(GameObject go)
    {
        reloadItem = go.GetComponent<PuzzleItem>();
        Puzzle.Start(go.name);
    }

    bool UpdateItems()
    {
        string[] savePaths = Directory
            .EnumerateDirectories(Paths.Saves)
            .Where(p => !items.Contains(Path.GetFileName(p)))
            .OrderBy(p => File.GetLastAccessTimeUtc(p))
            .ToArray();

        var prefab = PrefabLoader.Load(Prefabs.PuzzleItem);
        for (int i = 0; i < savePaths.Length; ++i)
        {
            string name = Path.GetFileName(savePaths[i]);
            items.Add(name);

            var go = prefab.Instantiate<GameObject>(scrollRect.content.transform);
            go.transform.SetSiblingIndex(0);

            var ev = go.GetComponent<ItemEvents>();
            ev.onClicked += OnItemClicked;

            var item = go.GetComponent<PuzzleItem>();
            item.Init(name, null);
        }

        return savePaths.Length > 0;
    }

    void UpdateMasks()
    {
        int count = scrollRect.content.transform.childCount;
        for (int i = 0; i < count; ++i)
        {
            var t = scrollRect.content.transform.GetChild(i);
            var item = t.GetComponent<PuzzleItem>();
            item.SetMask(SelectMaterial(i));
        }
    }

    Material SelectMaterial(int i)
    {
        int index = maskIndex[i % maskIndex.Length];
        return SnapshotMasks.Instance.materials[index];
    }
}
