using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using ResourceModule;

public class ArtCollectionPanel : Panel
{
    public Image background;
    public float colorDuration;
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
        PlayBackgroundColor();
    }

    void OnDestroy()
    {
        for (int i = 0; i < maskMats.Length; ++i)
            Destroy(maskMats[i]);
    }

    void PlayBackgroundColor()
    {
        string[] colorWheel = new string[]
        {
            "cfdb00",
            "8fc31f",
            "22ac38",
            "009944",
            "009b6b",
            "009e96",
            "00a0c1",
            "00a0e9",
            "0086d1",
            "0068b7",
            "00479d",
            "1d2088",
            "601986",
            "920783",
            "be0081",
            "e4007f",
            "e5006a",
            "e5004f",
            "e60033",
            "e60012",
            "eb6100",
            "f39800",
            "fcc800",
            "fff100",
        };

        var seq = DOTween.Sequence();
        for (int i = 0; i <= colorWheel.Length; ++i)
        {
            Color next = Utils.ColorFromString(colorWheel[i % colorWheel.Length]);
            seq.Append(background.DOColor(next, colorDuration));
        }
        seq.SetLoops(-1, LoopType.Restart);
    }
}
