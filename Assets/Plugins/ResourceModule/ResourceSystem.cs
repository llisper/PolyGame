using UnityEngine;
using System;
using System.Threading.Tasks;
using ResourceModule.Debug;

namespace ResourceModule
{
    public class ResLog : LogDefine<ResLog> { }

    public class ResourceSystem : Singleton<ResourceSystem>
    {
        public enum Mode
        {
            Dev,
            AssetBundle,
        }

        public Mode mode = Mode.AssetBundle;

        public static Mode ResMode
        {
            get 
            {
                #if UNITY_EDITOR
                return (Mode)UnityEditor.EditorPrefs.GetInt("ResMode", (int)Mode.Dev);
                #else
                return Mode.AssetBundle;
                #endif
            }
            set
            {
                #if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetInt("ResMode", (int)value);
                #else
                ResLog.LogError("Setting ResMode in release is forbidden!");
                #endif
            }
        }

        protected override async Task AsyncInit()
        {
            #if UNITY_EDITOR
            ResourceModuleDebugger.Init();
            #endif
            PathRouter.Init();
        }
    }
}
