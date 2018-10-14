using System.Threading.Tasks;
using FairyGUI;

namespace UI
{
    public class ScreenFader : FPanel
    {
        static ScreenFader instance;
        public static ScreenFader Instance
        {
            get
            {
                if (null == instance)
                    instance = FUI.Instance.OpenPanel<ScreenFader>();
                return instance;
            }
        }


        public static async Task AsyncFade(bool fadeOut)
        {
            Instance.Fade(fadeOut);
            while (!Instance.animationFinished)
                await Awaiters.NextFrame;
        }
        
        Controller controller;
        bool animationFinished = true;

        protected override void OnInit()
        {
            controller = component.GetController("fading");
            var bg = component.GetChild("bg");
            bg.OnGearStop.Add(OnGearStop);
        }

        public void Fade(bool fadeOut, bool immediate = false)
        {
            component.visible = true;
            int index = fadeOut ? 1 : 0;
            if (!immediate)
            {
                animationFinished = false;
                controller.selectedIndex = index;
            }
            else
            {
                GearBase.disableAllTweenEffect = true;
                controller.selectedIndex = index;
                GearBase.disableAllTweenEffect = false;
            }
        }

        void OnGearStop(EventContext eventContext)
        {
            if (controller.selectedIndex == 0)
                component.visible = false;
            animationFinished = true;
        }
    }
}
