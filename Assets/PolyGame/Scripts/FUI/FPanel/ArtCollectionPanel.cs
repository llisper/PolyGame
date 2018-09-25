using System.Collections.Generic;
using FairyGUI;

namespace Experiments
{
    public class ArtCollectionPanel : FPanel
    {
        protected override void OnInit()
        {
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
