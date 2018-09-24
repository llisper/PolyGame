using UnityEngine;
using System.Collections.Generic;
using FairyGUI;

namespace Experiments
{
    public class ArtCollectionPanel : FPanel
    {
        protected override void OnInit()
        {
            //var puzzleGroupView = View.GetChild("view") as PuzzleGroupView;
            //var groups = ArtCollection.Instance.groups;
            //puzzleGroupView.Init(groups[0]);

            var scrollview = component.GetChild("scrollview") as GList;
            var groups = ArtCollection.Instance.groups;
            for (int i = 0; i < groups.Count; ++i)
            {
                var groupView = scrollview.AddItemFromPool() as PuzzleGroupView;
                groupView.Init(groups[i]);
            }
        }
    }
}
