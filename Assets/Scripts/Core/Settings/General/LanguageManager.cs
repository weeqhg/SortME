using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

namespace WekenDev.Settings.General
{
    public class LanguageManager : MonoBehaviour
    {
        [Header("Language Dropdown")]
        [SerializeField] private TMP_Dropdown languageDropdown;

        public void Init()
        {
            if (languageDropdown != null)
            {
                LoadSavedLanguage();
                languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        private void OnDropdownValueChanged(int selectedIndex)
        {
            string languageCode = selectedIndex switch
            {
                0 => "ru", // Russian
                1 => "en", // English
                _ => "en"
            };

            ChangeLanguage(languageCode);
        }

        private void ChangeLanguage(string languageCode)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(languageCode);
            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;

                PlayerPrefs.SetString("SelectedLanguage", languageCode);
                PlayerPrefs.Save();
            }
        }

        private void LoadSavedLanguage()
        {
            string savedLang = PlayerPrefs.GetString("SelectedLanguage", "ru");

            // Устанавливаем правильное значение в dropdown
            int dropdownIndex = savedLang switch
            {
                "ru" => 0,
                "en" => 1,
                _ => 0
            };

            if (languageDropdown != null)
            {
                languageDropdown.value = dropdownIndex;
                OnDropdownValueChanged(dropdownIndex);
            }
        }

        private void OnDestroy()
        {
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            }
        }
    }
}