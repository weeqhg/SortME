using UnityEngine;
using UnityEngine.UI;
using System;

namespace WekenDev.MainMenu.UI
{

    public class MainMenuUI : MonoBehaviour
    {
        [Header("Version")]
        [SerializeField] private Text version;
        [Header("CanvasGroup")]
        [SerializeField] private CanvasGroup _mainMenu;
        [SerializeField] private CanvasGroup _lobbyMenu;
        [SerializeField] private CanvasGroup _joinMenu;
        [SerializeField] private CanvasGroup _authorMenu;

        [Header("Buttons")]
        [SerializeField] private Button _createGame;
        [SerializeField] private Button _joinLobby;
        [SerializeField] private Button _customizeMenu;
        [SerializeField] private Button _joinGame;
        [SerializeField] private Button _settingGame;
        [SerializeField] private Button _authorGame;
        [SerializeField] private Button _quitGame;

        public event Action OnSettingsActiveUI;
        public event Action OnСustomizationMenu;

        public void Init()
        {
            ShowMainMenu();

            version.text = "v" + Application.version;

            _createGame.onClick.AddListener(ShowWaitStartRoom);
            _joinLobby.onClick.AddListener(ShowJoinRoom);
            _customizeMenu.onClick.AddListener(ShowCustomize);
            _joinGame.onClick.AddListener(ShowWaitStartRoom);
            _authorGame.onClick.AddListener(ShowAuthor);
            _settingGame.onClick.AddListener(ShowSetting);
            _quitGame.onClick.AddListener(QuitGame);
        }

        public void ShowMainMenu()
        {
            HideAll();

            _mainMenu.alpha = 1f;
            _mainMenu.interactable = true;
            _mainMenu.blocksRaycasts = true;
        }

        private void ShowWaitStartRoom()
        {
            HideAll();

            _lobbyMenu.alpha = 1f;
            _lobbyMenu.interactable = true;
            _lobbyMenu.blocksRaycasts = true;
        }

        private void ShowJoinRoom()
        {
            HideAll();

            _joinMenu.alpha = 1f;
            _joinMenu.interactable = true;
            _joinMenu.blocksRaycasts = true;
        }

        private void ShowCustomize()
        {
            HideAll();

            OnСustomizationMenu?.Invoke();
        }


        private void ShowAuthor()
        {
            HideAll();

            _authorMenu.alpha = 1f;
            _authorMenu.interactable = true;
            _authorMenu.blocksRaycasts = true;
        }

        private void ShowSetting()
        {
            HideAll();

            OnSettingsActiveUI?.Invoke();
        }

        private void HideAll()
        {
            _mainMenu.alpha = 0f;
            _mainMenu.interactable = false;
            _mainMenu.blocksRaycasts = false;

            _joinMenu.alpha = 0f;
            _joinMenu.interactable = false;
            _joinMenu.blocksRaycasts = false;

            _lobbyMenu.alpha = 0f;
            _lobbyMenu.interactable = false;
            _lobbyMenu.blocksRaycasts = false;

            _authorMenu.alpha = 0f;
            _authorMenu.interactable = false;
            _authorMenu.blocksRaycasts = false;
        }

        private void QuitGame()
        {
            AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
            Debug.Log("Выход из игры...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
