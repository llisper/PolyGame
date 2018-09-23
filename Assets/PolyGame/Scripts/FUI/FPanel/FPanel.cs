using FairyGUI;
using System;

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

    public abstract class FPanel
    {
        protected GComponent component;

        public GComponent GComponent { get { return component;  } }

        public void Init(GComponent component)
        {
            this.component = component;
            OnInit();
        }

        protected virtual void OnInit() { }
    }
}
