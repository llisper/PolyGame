using FairyGUI;

namespace UI
{
    public class PuzzlePanel : FPanel
    {
        protected override void OnInit()
        {
            var backBtn = View.GetChild("backBtn") as GButton;
            backBtn.onClick.Add(OnBackClick);

            GameEvent.Instance.Subscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
        }

        protected override void OnDispose()
        {
            GameEvent.Instance.Unsubscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
        }

        void OnBackClick(EventContext eventContext)
        {
            GameScene.LoadScene<MenuScene>().WrapErrors();
        }

        void OnPuzzleFinished(int e, object[] p)
        {
            var transition = component.GetTransition("finishTexts");
            transition.Play();
        }
    }
}
