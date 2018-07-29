using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ResourceModule;

public class PuzzleGroupView : MonoBehaviour
{
    public int itemLimitHalf = 5;
    public ScrollRect scrollRect;
    public Image all;
    public Text category;

    List<PuzzleItem> puzzleItems;
    PuzzleItem reloadItem;

    public void Init(ArtCollection.Group group)
    {
        var items = group.items;
        int showCount = items.Count;
        int itemLimit = itemLimitHalf * 2;
        if (items.Count > itemLimit)
            showCount = itemLimit - 1;

        var prefab = PrefabLoader.Load(Prefabs.PuzzleItem);
        puzzleItems = new List<PuzzleItem>();
        for (int i = 0; i < showCount; ++i)
        {
            var go = prefab.Instantiate<GameObject>(scrollRect.content.transform);
            go.transform.SetSiblingIndex(i);
            var ev = go.GetComponent<ItemEvents>();
            ev.onClicked += OnItemClicked;

            var item = go.GetComponent<PuzzleItem>();
            item.Init(items[i].name, SelectMaterial(i, items.Count));
            puzzleItems.Add(item);

        }

        if (showCount < items.Count)
        {
            all.material = SelectMaterial(showCount, items.Count);
            all.gameObject.SetActive(true);
        }
        else
        {
            all.gameObject.SetActive(false);
        }
        category.text = I18n.Get(group.name);
    }

    void Awake()
    {
        var ev = all.GetComponent<ItemEvents>();
        ev.onClicked += OnSeeAllClicked;
    }

    void OnEnable()
    {
        if (null != reloadItem)
        {
            reloadItem.Load();
            reloadItem = null;
        }
    }

    void OnSeeAllClicked(GameObject go)
    {
        GameLog.Verbose("OnSeeAllClicked");
    }

    void OnItemClicked(GameObject go)
    {
        reloadItem = go.GetComponent<PuzzleItem>();
        Puzzle.Start(go.name);
    }

    Material SelectMaterial(int i, int itemCount)
    {
        int half = itemLimitHalf;
        int step = Mathf.Min(1, i / half) * 3;
        i = i % half;

        var mats = SnapshotMasks.Instance.materials;
        if (i == 0)
            return mats[0 + step];
        else if (i < half - 1)
            return mats[1 + step];
        else
            return mats[2 + step];
    }
}
