using System;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.СustomizationMenu
{
    public class СustomizationUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        [SerializeField] private Button _color;
        [SerializeField] private Button _body;
        [SerializeField] private Button _hats;
        [SerializeField] private Button _back;
        [SerializeField] private CanvasGroup _colorWheelGroup;
        [SerializeField] private CanvasGroup _bodyMenu;
        [SerializeField] private CanvasGroup _hatsMenu;
        public event Action OnBackToggle;
        public void Init()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _back.onClick.AddListener(() => OnBackToggle?.Invoke());
            _color.onClick.AddListener(ShowColorWheel);
            _body.onClick.AddListener(ShowClothMenu);
            _hats.onClick.AddListener(ShowHatsMenu);
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            HideAll();

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void ShowColorWheel()
        {
            HideAll();

            _colorWheelGroup.alpha = 1f;
            _colorWheelGroup.interactable = true;
            _colorWheelGroup.blocksRaycasts = true;
        }

        private void ShowClothMenu()
        {
            HideAll();

            _bodyMenu.alpha = 1f;
            _bodyMenu.interactable = true;
            _bodyMenu.blocksRaycasts = true;
        }
        private void ShowHatsMenu()
        {
            HideAll();

            _hatsMenu.alpha = 1f;
            _hatsMenu.interactable = true;
            _hatsMenu.blocksRaycasts = true;
        }

        private void HideAll()
        {
            _colorWheelGroup.alpha = 0f;
            _colorWheelGroup.interactable = false;
            _colorWheelGroup.blocksRaycasts = false;

            _bodyMenu.alpha = 0f;
            _bodyMenu.interactable = false;
            _bodyMenu.blocksRaycasts = false;

            _hatsMenu.alpha = 0f;
            _hatsMenu.interactable = false;
            _hatsMenu.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            _back.onClick.RemoveListener(() => OnBackToggle?.Invoke());
            _color.onClick.RemoveAllListeners();
            _body.onClick.RemoveAllListeners();
            _hats.onClick.RemoveAllListeners();
        }
    }
}
