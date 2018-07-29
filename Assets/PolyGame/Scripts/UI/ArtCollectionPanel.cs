using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ResourceModule;

public class ArtCollectionPanel : Panel
{
    public ScrollRect scrollRect;
    public Texture2D[] masks;

    List<PuzzleGroupView> groupViews;

    public override bool Persistent { get { return true; } }

    void Start()
    {
        var prefab = PrefabLoader.Load(Prefabs.PuzzleGroupView);
        var groups = ArtCollection.Instance.groups;
        groupViews = new List<PuzzleGroupView>();
        for (int i = 0; i < groups.Count; ++i)
        {
            var go = prefab.Instantiate<GameObject>(transform);
            go.transform.SetParent(scrollRect.content.transform);
            var view = go.GetComponent<PuzzleGroupView>();
            view.Init(groups[i]);
            groupViews.Add(view); 
        }
    }
}
