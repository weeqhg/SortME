using UnityEngine;
using TMPro;
using System.Collections.Generic;


namespace WekenDev.Settings.Graphic
{
    public class WindowModeSetting : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private List<FullScreenMode> _displayModes = new List<FullScreenMode>
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.Windowed
    };
        public void Init()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            if (_dropdown != null)
            {
                _dropdown.onValueChanged.AddListener(OnDisplayModeChanged);
                LoadSave();
            }


        }

        private void OnDisplayModeChanged(int index)
        {
            if (index < 0 || index >= _displayModes.Count) return;

            FullScreenMode newMode = _displayModes[index];
            PlayerPrefs.SetInt("FullScreenMode", index);

            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, newMode);
        }

        private void LoadSave()
        {
            int index = PlayerPrefs.GetInt("FullScreenMode", 0);
            _dropdown.value = index;
            OnDisplayModeChanged(index);
        }

        private void OnDestroy()
        {
            if (_dropdown != null) _dropdown.onValueChanged.RemoveListener(OnDisplayModeChanged);
        }
    }
}
