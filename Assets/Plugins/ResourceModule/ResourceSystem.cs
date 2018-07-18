using UnityEngine;
using ResourceModule.Debug;
using System.Threading.Tasks;

namespace ResourceModule
{
    public class ResLog : LogDefine<ResLog> { }

    public class ResourceSystem : Singleton<ResourceSystem>
    {
        #region inspector
        public enum Mode
        {
            Dev,
            AssetBundle,
        }

        public Mode mode = Mode.AssetBundle;
        #endregion inspector

        protected override async Task AsyncInit()
        {
            if (Application.isEditor)
            {
                mode = Mode.Dev;
                ResourceModuleDebugger.Init();
            }

            PathRouter.Init();
        }
    }
}
