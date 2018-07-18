using UnityEngine;

namespace ResourceModule.Debug
{
    internal class LoaderVisualizer : MonoBehaviour
    {
        #region inspector
        public ResourceLoader loader;
        #endregion inspector

        int progress;
        float destroyTimer;

        void Update()
        {
            int p = (int)(loader.Progress * 1000);
            if (p != progress)
            {
                progress = p;
                name = string.Format("{0:P1} {1}", loader.Progress, loader.Url);
            }

            if (loader.IsComplete && loader.RefCount <= 0)
            {
                float timeout = ResourceModuleDebugger.Instance.visDestroyTimeout;
                if ((destroyTimer += Time.deltaTime) >= timeout)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                destroyTimer = 0f;
            }
        }
    }
}
