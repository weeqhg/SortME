using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace WekenDev.Settings.Graphic
{
    public class ScreenResolutionSetting : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;

        private List<Resolution> _availableResolutions;
        public void Init()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            if (_dropdown != null)
            {
                InitResolutionDropdown();
            }
        }

        private void InitResolutionDropdown()
        {
            _dropdown.ClearOptions();

            _availableResolutions = new List<Resolution>(Screen.resolutions);

            // Сортируем разрешения по убыванию
            _availableResolutions.Sort((a, b) =>
                b.width != a.width ? b.width.CompareTo(a.width) : b.height.CompareTo(a.height));

            List<string> resolutionOptions = new List<string>();

            // Убираем дубликаты разрешений
            HashSet<string> uniqueResolutions = new HashSet<string>();

            foreach (var resolution in _availableResolutions)
            {
                string option = $"{resolution.width} × {resolution.height}";
                if (uniqueResolutions.Add(option)) // Add возвращает true если элемент новый
                {
                    resolutionOptions.Add(option);
                }
            }

            _dropdown.AddOptions(resolutionOptions);
            _dropdown.onValueChanged.AddListener(OnResolutionChanged);
            LoadSave();
        }

        private void OnResolutionChanged(int index)
        {
            if (_dropdown == null || index < 0 || index >= _availableResolutions.Count) return;

            Resolution selectedResolution = _availableResolutions[index];

            // Применяем новое разрешение с текущим режимом
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode);

            PlayerPrefs.SetInt("ScreenResolution", index);
        }

        private void LoadSave()
        {
            int index = PlayerPrefs.GetInt("ScreenResolution", 0);
            _dropdown.value = index;
            OnResolutionChanged(index);
        }

        private void OnDestroy()
        {
            if (_dropdown != null) _dropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        }
    }

}