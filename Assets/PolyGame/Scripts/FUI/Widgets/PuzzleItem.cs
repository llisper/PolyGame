using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FairyGUI;
using FairyGUI.Utils;
using ResourceModule;

namespace UI
{
    public class PuzzleItem : GComponent
    {
        Controller showAllCtrl;
        GLoader loader;
        List<GImage> masks = new List<GImage>();

        Texture2D tex2d;
        PrefabResource snapshotPrefab;
        string graphName;
        ArtCollection.Group group;

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            showAllCtrl = GetController("showAll");
            loader = GetChild("snapshot") as GLoader;
            InitMasks();
        }

        public override void Dispose()
        {
            graphName = null;
            group = null;
            ReleaseRes();
            base.Dispose();
        }

        public void Start()
        {
            if (showAllCtrl.selectedIndex == 1)
            {
                GameScene.Current<MenuScene>().ShowPage<ShowAllPanel>(p =>
                {
                    p.Init(group);
                });
            }
            else
            {
                Puzzle.Start(graphName);
            }
        }

        public void Init(string graphName)
        {
            name = this.graphName = graphName;
            showAllCtrl.selectedIndex = 0;
            ReleaseRes();
            Load();
        }

        public void InitAsShowAll(ArtCollection.Group group)
        {
            this.group = group;
            showAllCtrl.selectedIndex = 1;
            ReleaseRes();
        }

        public void Load()
        {
            if (showAllCtrl.selectedIndex == 1)
                return;

            string path = Paths.SnapshotSave(graphName);
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (null == tex2d)
                    tex2d = new Texture2D(2, 2);
                tex2d.LoadImage(bytes);
                loader.texture = new NTexture(tex2d);
            }
            else
            {
                if (null == snapshotPrefab)
                {
                    snapshotPrefab = PrefabLoader.Load(string.Format(
                        "{0}/{1}/{1}_{2}",
                        Paths.Artworks,
                        graphName,
                        Paths.Snapshot));

                    if (null != snapshotPrefab)
                        snapshotPrefab.KeepReference();
                }

                if (null != snapshotPrefab)
                {
                    var prefabObject = (GameObject)snapshotPrefab.Object;
                    var holder = prefabObject.GetComponent<PuzzleSnapshotHolder>();
                    if (null != holder && null != holder.texture)
                        loader.texture = new NTexture(holder.texture);
                    else
                        GameLog.LogError("PuzzleSnapshotHolder missing or texture missing: " + graphName);
                }
            }
        }

        public void SetMask(int index)
        {
            for (int i = 0; i < masks.Count; ++i)
            {
                var image = masks[i];
                // NOTE: mask choosing logic has bugs, disable it for now
                image.visible = false;
                //if (i == index)
                //{
                //    image.visible = true;
                //    mask = image.displayObject;
                //}
                //else
                //{
                //    image.visible = false;
                //}
            }
        }

        void InitMasks()
        {
            for (int i = 0; i < 6; ++i)
                masks.Add(GetChild("mask" + i) as GImage);
        }

        void ReleaseRes()
        {
            if (null != tex2d)
            {
                GameObject.Destroy(tex2d);
                tex2d = null;
            }
            if (null != snapshotPrefab)
            {
                snapshotPrefab.Release();
                snapshotPrefab = null;
            }
        }
    }
}
