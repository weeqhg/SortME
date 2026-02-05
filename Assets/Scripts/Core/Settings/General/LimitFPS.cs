using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.Settings.General
{
    public class LimitFPS : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        private bool _isShowFps = false;
        public void Init()
        {
            Application.targetFrameRate = 60;
            _toggle.onValueChanged.AddListener(OnToggleFps);
            LoadSave();
        }

        private void OnToggleFps(bool enabled)
        {
            _isShowFps = enabled;
            PlayerPrefs.SetInt("FPS", enabled ? 1 : 0);
        }

        private void LoadSave()
        {
            int enabled = PlayerPrefs.GetInt("FPS", 0);
            if (enabled == 1) _isShowFps = true;
            else _isShowFps = false;

            _toggle.isOn = _isShowFps;
        }

        private void OnGUI()
        {
            if (!_isShowFps) return;
            // Показать FPS в углу экрана
            GUI.Label(new Rect(10, 10, 100, 20),
                      $"FPS: {1.0f / Time.deltaTime:F1}");
        }
    }
}
