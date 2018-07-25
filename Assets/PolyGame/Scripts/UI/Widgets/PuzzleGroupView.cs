using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ResourceModule;

public class PuzzleGroupView : MonoBehaviour
{
    public int itemLimitHalf = 5;
    public ScrollRect scrollRect;
    public GameObject seeAll;
    public Text category;

    List<PuzzleItem> puzzleItems;
    PuzzleItem reloadItem;
    Material[] maskMats;

    public void Init(ArtCollection.Group group, Material[] maskMats)
    {
        this.maskMats = maskMats;
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
            go.transform.SetSiblingIndex(i + 2);
            var ev = go.GetComponent<ItemEvents>();
            ev.onClicked += OnItemClicked;

            var item = go.GetComponent<PuzzleItem>();
            item.Init(items[i].name, SelectMaterial(i, showCount, items.Count));
            puzzleItems.Add(item);

        }

        category.text = I18n.Get(group.name);
        seeAll.SetActive(showCount < items.Count);
    }

    void Awake()
    {
        var ev = seeAll.GetComponent<ItemEvents>();
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

    Material SelectMaterial(int i, int showCount, int itemCount)
    {
        if (i == 0)
            return maskMats[0];
        if (i == 1)
            return maskMats[3];

        if (i % 2 == 0)
        {
            if (i == (itemLimitHalf - 1) * 2)
                return maskMats[2];
            else
                return maskMats[1];
        }
        else
        {
            if (i == showCount - 1 && showCount == itemCount)
                return maskMats[5];
            else
                return maskMats[4];
        }
    }
}
