using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace ResourceModule.Test
{
    class ModeSelector : MonoBehaviour
    {
        #region inspector
        public ToggleGroup modeGroup;
        public ToggleGroup syncGroup;
        #endregion inspector

        [NonSerialized]
        public string syncMode;

        public void Init()
        {
            SetMode();
            SetSyncMode();
        }

        void Awake()
        {
            foreach (var toggle in modeGroup.GetComponentsInChildren<Toggle>())
                toggle.onValueChanged.AddListener(OnModeChanged);
            foreach (var toggle in syncGroup.GetComponentsInChildren<Toggle>())
                toggle.onValueChanged.AddListener(OnSyncModeChanged);
        }

        void SetMode()
        {
            string n = modeGroup.ActiveToggles().First().name;
            ResourceSystem.Instance.mode = (ResourceSystem.Mode)Enum.Parse(typeof(ResourceSystem.Mode), n);
            ResLog.Log("Current mode: " + n);
        }

        void SetSyncMode()
        {
            syncMode = syncGroup.ActiveToggles().First().name;
            ResLog.Log("Current sync mode: " + syncMode);
        }

        public void OnModeChanged(bool arg)
        {
            SetMode();
        }

        public void OnSyncModeChanged(bool arg)
        {
            SetSyncMode();
        }
    }
}
