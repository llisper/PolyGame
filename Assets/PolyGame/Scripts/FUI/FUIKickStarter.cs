using UnityEngine;
using FairyGUI;

namespace Experiments
{
    public class FUIKickStarter : MonoBehaviour
    {
        ScreenFader screenFader;
        bool fade = true;

        void Start()
        {
            FUI.Init();

            //screenFader = new ScreenFader();
            //screenFader.Init(UIPackage.CreateObject("Main", "ScreenFader") as GComponent);
            //GRoot.inst.AddChild(screenFader.GComponent);

            screenFader = FUI.Instance.OpenPanel<ScreenFader>();

            Stage.inst.onKeyDown.Add(OnKeyDown);
        }

        void OnKeyDown(EventContext context)
        {
            if (context.inputEvent.keyCode == KeyCode.Space)
            {
                screenFader.Fade(fade);
                fade = !fade;
            }
        }
    }
}
