using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ResourceModule;

public class ArtCollectionPanel : Panel
{
    public ScrollRect scrollRect;
    public Texture2D[] masks;

    List<PuzzleGroupView> groupViews;
    Material[] maskMats;

    public override bool Persistent { get { return true; } }

    void Awake()
    {
        maskMats = new Material[masks.Length];
        for (int i = 0; i < maskMats.Length; ++i)
        {
            var mat = new Material(Shader.Find("PolyGame/SnapshotImage"));
            mat.name = "SnapshotMask" + i;
            mat.SetTexture("_Mask", masks[i]);
            maskMats[i] = mat;
        }
    }

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
            view.Init(groups[i], maskMats);
            groupViews.Add(view); 
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < maskMats.Length; ++i)
            Destroy(maskMats[i]);
    }
}
