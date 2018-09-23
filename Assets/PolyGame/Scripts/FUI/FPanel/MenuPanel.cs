using FairyGUI;

namespace Experiments
{
    public class MenuPanel : FPanel
    {
        protected override void OnInit()
        {
            var btn = component.GetChild("gallery") as GButton;
            btn.onClick.Add(TestStartPuzzle);
        }

        void TestStartPuzzle(EventContext eventContext)
        {
            Puzzle.Start("animal004");
        }
    }
}
