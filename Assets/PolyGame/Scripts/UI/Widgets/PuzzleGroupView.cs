using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleGroupView : MonoBehaviour
{
    public int itemsLimit = 10;
    public ScrollRect scrollRect;
    public GameObject seeAll;
    public Text category;

    List<PuzzleItem> puzzleItems;

    public void Init(ArtCollection.Group group)
    {
        var items = group.items;
        int showCount = items.Count;
        if (items.Count > itemsLimit)
            showCount = itemsLimit - 1;

        var prefab = Resources.Load<GameObject>(Prefabs.PuzzleItem);
        puzzleItems = new List<PuzzleItem>();
        for (int i = 0; i < showCount; ++i)
        {
            var go = Instantiate(prefab, scrollRect.content.transform);
            go.transform.SetSiblingIndex(i + 2);
            var ev = go.GetComponent<ItemEvents>();
            ev.onClicked += OnItemClicked;

            var item = go.GetComponent<PuzzleItem>();
            item.Init(items[i].name);
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

    void Start()
    {
        ConfigLoader.LoadAll();
        Init(ArtCollection.Instance.groups[0]);
    }

    void OnSeeAllClicked(GameObject go)
    {
        Debug.Log("OnSeeAllClicked");
    }

    void OnItemClicked(GameObject go)
    {
        Debug.Log("OnItemClicked: " + go);
    }
}
