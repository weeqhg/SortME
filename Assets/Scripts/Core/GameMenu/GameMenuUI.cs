using System;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.GameMenu.UI
{

    public class GameMenuUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _generalMenu;


        [SerializeField] private Button _return;
        [SerializeField] private Button _setting;
        [SerializeField] private Button _disconnect;


        public event Action OnReturnGame;
        public event Action OnSettingActiveUI;
        public event Action OnLeaveLobbyUI;
        public void Init()
        {
            _return.onClick.AddListener(ReturnGame);
            _setting.onClick.AddListener(ShowSetting);
            _disconnect.onClick.AddListener(LeaveLobby);
        }

        public void ShowGameMenu()
        {
            HideAll();

            _generalMenu.alpha = 1f;
            _generalMenu.interactable = true;
            _generalMenu.blocksRaycasts = true;
        }

        public void HideGeneralMenu()
        {
            HideAll();
        }

        private void ShowSetting()
        {
            HideAll();

            OnSettingActiveUI?.Invoke();
        }

        private void LeaveLobby()
        {
            HideAll();

            OnLeaveLobbyUI?.Invoke();
        }

        private void ReturnGame()
        {
            HideAll();

            OnReturnGame?.Invoke();
        }


        private void HideAll()
        {
            _generalMenu.alpha = 0f;
            _generalMenu.interactable = false;
            _generalMenu.blocksRaycasts = false;
        }
    }
}
