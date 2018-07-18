using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceModule.Test
{
    class LoadSceneTester : MonoBehaviour
    {
        #region inspector
        public Dropdown dropdown;
        public Text current;
        public Text progress;
        public ModeSelector modeSelector;
        #endregion inspector

        bool initialized;
        bool loading;

        void Start()
        {
            Init();
            DontDestroyOnLoad(gameObject);
        }

        async void Init()
        {
            await ResourceSystem.Init();
            modeSelector.Init();
            AddOptions();
            initialized = true;
        }

        void AddOptions()
        {
            List<string> options = new List<string>()
            {
                "CityScene",
            };
            dropdown.AddOptions(options);
        }

        void Load()
        {
            SceneLoader.Load(dropdown.captionText.text);
            current.text = SceneResource.Current.Path;
        }

        async void AsyncLoad()
        {
            loading = true;
            try
            {
                await SceneLoader.AsyncLoad(
                    dropdown.captionText.text,
                    v => progress.text = string.Format("Loading {0:P2}", v));
                current.text = SceneResource.Current.Path;
            }
            catch (Exception e)
            {
                ResLog.LogException(e);
            }
            loading = false;
        }

        public void OnLoad()
        {
            if (!initialized || loading)
                return;

            if (modeSelector.syncMode == "Async")
                AsyncLoad();
            else
                Load();
        }
    }
}
