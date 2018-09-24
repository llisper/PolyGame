using UnityEngine;
using FairyGUI;

namespace Experiments
{
    public class PuzzleGroupViewPanel : FPanel
    {
        protected override void OnInit()
        {
            var puzzleGroupView = View.GetChild("view") as PuzzleGroupView;
            var groups = ArtCollection.Instance.groups;
            puzzleGroupView.Init(groups[0]);
        }
    }
}
