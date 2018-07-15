using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArtCollectionPanel : Panel
{
    public ScrollRect scrollRect;

    List<PuzzleGroupView> groupViews;

    void Start()
    {
        var prefab = Resources.Load<GameObject>(Prefabs.PuzzleGroupView);
        var groups = ArtCollection.Instance.groups;
        groupViews = new List<PuzzleGroupView>();
        for (int i = 0; i < groups.Count; ++i)
        {
            var go = Instantiate(prefab, transform);
            go.transform.SetParent(scrollRect.content.transform);
            var view = go.GetComponent<PuzzleGroupView>();
            view.Init(groups[i]);
            groupViews.Add(view); 
        }
    }

    public override bool Persistent { get { return true; } }
}
