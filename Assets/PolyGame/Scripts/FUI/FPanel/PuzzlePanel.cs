using FairyGUI;
using System;

namespace UI
{
    public class PuzzlePanel : FPanel
    {
        Transition finishTexts;
        GButton hintBtn;
        GTextField hintCount;

        protected override void OnInit()
        {
            finishTexts = component.GetTransition("finishTexts");

            var backBtn = View.GetChild("backBtn") as GButton;
            backBtn.onClick.Add(OnBackClick);

            hintBtn = View.GetChild("hintBtn") as GButton;
            hintBtn.onClick.Add(OnHintClick);

            hintCount = hintBtn.GetChild("count") as GTextField;
            hintCount.text = PuzzleHintSystem.Instance.HintCount.ToString();

            GameEvent.Instance.Subscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
            GameEvent.Instance.Subscribe(GameEvent.PuzzleState, OnPuzzleState);
        }

        protected override void OnDispose()
        {
            GameEvent.Instance.Unsubscribe(GameEvent.PuzzleFinished, OnPuzzleFinished);
            GameEvent.Instance.Unsubscribe(GameEvent.PuzzleState, OnPuzzleState);
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

        void OnHintClick(EventContext eventContext)
        {
            if (PuzzleHintSystem.Instance.UseHint())
                hintCount.text = PuzzleHintSystem.Instance.HintCount.ToString();
        }

        void OnPuzzleFinished(int e, object[] p)
        {
            finishTexts.Play();
        }

        void OnPuzzleState(int e, object[] p)
        {
            hintBtn.visible = (((Type)p[0]) == typeof(PuzzleNormalState));
        }
    }
}
