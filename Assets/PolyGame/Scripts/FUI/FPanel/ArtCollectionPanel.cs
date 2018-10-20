using System.Collections.Generic;
using FairyGUI;

namespace UI
{
    public class ArtCollectionPanel : FPanel
    {
        List<PuzzleGroupView> groupViews = new List<PuzzleGroupView>();

        protected override void OnInit()
        {
            var scrollview = component.GetChild("scrollview") as GList;
            var groups = ArtCollection.Instance.groups;
            for (int i = 0; i < groups.Count; ++i)
            {
                var groupView = scrollview.AddItemFromPool() as PuzzleGroupView;
                groupView.Init(groups[i]);
                groupViews.Add(groupView);
            }
        }
    }
}
