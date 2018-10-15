using FairyGUI;

namespace UI
{
    public class PuzzlePanel : FPanel
    {
        Transition finishTexts;

        protected override void OnInit()
        {
            finishTexts = component.GetTransition("finishTexts");

            var backBtn = View.GetChild("backBtn") as GButton;
            backBtn.onClick.Add(OnBackClick);

            GameEvent.Instance.Subscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
        }

        protected override void OnDispose()
        {
            GameEvent.Instance.Unsubscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
        }

        protected override void OnInvisible()
        {
            finishTexts.PlayReverse();
            finishTexts.Stop(true, false);
        }

        void OnBackClick(EventContext eventContext)
        {
            GameScene.LoadScene<MenuScene>().WrapErrors();
        }

        void OnPuzzleFinished(int e, object[] p)
        {
            finishTexts.Play();
        }
    }
}
