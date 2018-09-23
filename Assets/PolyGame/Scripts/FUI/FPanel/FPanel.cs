using FairyGUI;

namespace Experiments
{
    public abstract class FPanel
    {
        protected GComponent component;

        public GComponent GComponent { get { return component;  } }

        public void Init(GComponent component)
        {
            this.component = component;
            OnInit();
        }

        protected abstract void OnInit();
    }
}
