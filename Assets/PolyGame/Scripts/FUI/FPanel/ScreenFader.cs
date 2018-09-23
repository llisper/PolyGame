using System;
using FairyGUI;

namespace Experiments
{
    public class FPanelInfo
    {
        public Type type;
        public UILayer layer;
        public string package;

        public FPanelInfo (Type type, string package, UILayer layer)
        {
            this.type = type;
            this.layer = layer;
            this.package = package;
        }
    }

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
