using FairyGUI;

namespace Experiments
{
    public class ScreenFader : FPanel
    {
        Controller controller;

        protected override void OnInit()
        {
            controller = component.GetController("fading");
            var bg = component.GetChild("bg");
            bg.OnGearStop.Add(OnGearStop);
        }

        public void Fade(bool fadeOut)
        {
            component.visible = true;
            controller.selectedIndex = fadeOut ? 1 : 0;
        }

        void OnGearStop(EventContext eventContext)
        {
            if (controller.selectedIndex == 0)
                component.visible = false;
        }
    }
}
