using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FairyGUI;

namespace Experiments
{
    public static class FPackage
    {
        public const string Main = "Main";
    }

    public enum UILayer
    {
        Background,
        Base,
        Menu,
        Overlay,
        Count,
    }

    public class FUI
    {
        public static FUI Instance;
        public static Vector2Int designResolution = new Vector2Int(750, 1334);

        GComponent[] layers;
        Dictionary<Type, FPanelInfo> pinfoByType = new Dictionary<Type, FPanelInfo>();
        Dictionary<Type, FPanel> panels = new Dictionary<Type, FPanel>();

        #region initialization
        public static async Task Init()
        {
            Instance = new FUI();
            Instance.InitLayers();
            Instance.InitExtensions();
            Instance.RegisterPanels();
            UIPackage.AddPackage("UI/Main");
        }

        FUI()
        {
            GRoot.inst.SetContentScaleFactor(designResolution.x, designResolution.y);
            var stageCamera = GameObject.Find(StageCamera.Name);
            if (null != stageCamera)
                GameObject.DontDestroyOnLoad(stageCamera);
        }

        void InitLayers()
        {
            layers = new GComponent[(int)UILayer.Count];
            for (int i = 0; i < (int)UILayer.Count; ++i)
            {
                var gcom = new GComponent();
                gcom.gameObjectName = gcom.name = ((UILayer)i).ToString();
                GRoot.inst.AddChild(gcom);
                layers[i] = gcom;
            }
        }

        void InitExtensions()
        {
            UIObjectFactory.SetPackageItemExtension("ui://Main/PuzzleItem", typeof(PuzzleItem));
            UIObjectFactory.SetPackageItemExtension("ui://Main/PuzzleGroupView", typeof(PuzzleGroupView));
        }

        void RegisterPanels()
        {
            RegisterPanel<Background>(FPackage.Main, UILayer.Background);
            RegisterPanel<PuzzlePanel>(FPackage.Main, UILayer.Base);
            RegisterPanel<ArtCollectionPanel>(FPackage.Main, UILayer.Base);
            RegisterPanel<ShowAllPanel>(FPackage.Main, UILayer.Base);
            RegisterPanel<MyWorksPanel>(FPackage.Main, UILayer.Base);
            RegisterPanel<MenuPanel>(FPackage.Main, UILayer.Menu);
            RegisterPanel<ScreenFader>(FPackage.Main, UILayer.Overlay);
        }

        void RegisterPanel<T>(string package, UILayer layer) where T : FPanel
        {
            var pinfo = new FPanelInfo(typeof(T), package, layer);
            pinfoByType.Add(pinfo.type, pinfo);
        }
        #endregion initialization

        public T OpenPanel<T>() where T : FPanel
        {
            return (T)OpenPanel(typeof(T));
        }

        public FPanel OpenPanel(Type type)
        {
            FPanel panel = GetPanel(type);
            if (null == panel)
            {
                var pinfo = pinfoByType[type];
                var component = UIPackage.CreateObject(pinfo.package, pinfo.type.Name) as GComponent;
                layers[(int)pinfo.layer].AddChild(component);

                panel = Activator.CreateInstance(type) as FPanel;
                panel.Init(component);
                panels.Add(type, panel);
            }
            panel.Visible = true;
            return panel;
        }

        public T GetPanel<T>() where T : FPanel
        {
            return (T)GetPanel(typeof(T));
        }

        public FPanel GetPanel(Type type)
        {
            FPanel panel;
            panels.TryGetValue(type, out panel);
            return panel;
        }

        public void ClosePanel<T>(bool destroy = false) where T : FPanel
        {
            ClosePanel(typeof(T), destroy);
        }

        public void ClosePanel(Type type, bool destroy = false)
        {
            FPanel panel = GetPanel(type);
            if (null != panel)
            {
                if (destroy)
                {
                    panel.Dispose();
                    panels.Remove(type);
                }
                else
                {
                    panel.Visible = false;
                }
            }
        }
    }
}
