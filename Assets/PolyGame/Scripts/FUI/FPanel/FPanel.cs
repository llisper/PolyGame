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

        public GComponent View { get { return component;  } }

        public void Init(GComponent component)
        {
            this.component = component;
            OnInit();
        }

        public void Close(bool destroy = false)
        {
            FUI.Instance.ClosePanel(GetType(), destroy);
        }

        public void Dispose()
        {
            OnDispose();
            component.Dispose();
        }

        public bool Visible
        {
            get { return component.visible;  }
            set
            {
                if (component.visible != value)
                {
                    component.visible = value;
                    if (value)
                        OnVisible();
                    else
                        OnInvisible();
                }
            }
        }

        protected virtual void OnInit() { }
        protected virtual void OnDispose() { }
        protected virtual void OnVisible() { }
        protected virtual void OnInvisible() { }
    }
}
