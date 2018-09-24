using UnityEngine;
using System.IO;
using FairyGUI;
using FairyGUI.Utils;
using ResourceModule;

namespace Experiments
{
    public class PuzzleItem : GComponent
    {
        Controller showAllCtrl;
        GLoader loader;

        Texture2D tex2d;
        PrefabResource snapshotPrefab;
        string graphName;

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            showAllCtrl = GetController("showAll");
            loader = GetChild("snapshot") as GLoader;
        }

        public override void Dispose()
        {
            ReleaseRes();
            base.Dispose();
        }

        public void Init(string graphName)
        {
            name = this.graphName = graphName;
            showAllCtrl.selectedIndex = 0;
            ReleaseRes();
            Load();
        }

        public void InitAsShowAll()
        {
            showAllCtrl.selectedIndex = 1;
            ReleaseRes();
        }

        public void Load()
        {
            string path = Paths.SnapshotSave(graphName);
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (null == tex2d)
                {
                    tex2d = new Texture2D(2, 2);
                    loader.texture = new NTexture(tex2d);
                }
                tex2d.LoadImage(bytes);
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
                    loader.texture = new NTexture(prefabObject.GetComponent<PuzzleSnapshotHolder>().texture);
                }
            }
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
