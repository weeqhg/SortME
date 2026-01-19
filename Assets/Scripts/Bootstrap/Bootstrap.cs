using UnityEngine;
using WekenDev.VoiceChat;
using WekenDev.Settings;
using WekenDev.MainMenu;
using WekenDev.GameMenu;
using WekenDev.Game;
using WekenDev.Spawn.Player;

public class Bootstrap : MonoBehaviour
{
    [Header("Основные объекты")]
    [SerializeField] private GameObject _directionalLight;
    [SerializeField] private GameObject _spawnPrefab;

    [Header("Интерфейс")]
    [SerializeField] private GameObject _mainMenuCanvas;
    [SerializeField] private GameObject _settingMenuCanvas;
    [SerializeField] private GameObject _gameMenuCanvas;

    //Нужные менеджеры будем объявлять отсюда или сразу создаваться
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private StartGame _startGame;

    private MainMenuManager _menuManager;
    private IMainMenu _mainMenu;

    private GameLobby _gameLobby;

    private SettingManager _settingManager;
    private ISettings _settings;

    private PlayerSpawner _playerSpawn;

    private GameMenuManager _gameMenuManager;
    private IGameMenuController _gameMenu;

    private void Start()
    {
        CreateObject();
        InitializeComponents();
        StartInitialized();
    }

    private void CreateObject()
    {
        if (_directionalLight != null) _directionalLight = Instantiate(_directionalLight);

        if (_mainMenuCanvas != null) _mainMenuCanvas = Instantiate(_mainMenuCanvas);
        else Debug.LogWarning("Внимание модуль главного меню не установлен");

        if (_settingMenuCanvas != null) _settingMenuCanvas = Instantiate(_settingMenuCanvas);
        else Debug.LogWarning("Внимание модуль для настроек игры не установлен");

        if (_spawnPrefab != null) _spawnPrefab = Instantiate(_spawnPrefab);

        if (_gameMenuCanvas != null) _gameMenuCanvas = Instantiate(_gameMenuCanvas);
        else Debug.LogWarning("Внимание модуль игрового меню не установлен");

        if (_gameManager != null) _gameManager = Instantiate(_gameManager);
        else Debug.LogWarning("Внимание модуль Game Manager не установлен");

        if (_startGame != null) _startGame = Instantiate(_startGame);
    }


    private void InitializeComponents()
    {
        if (_mainMenuCanvas != null) _menuManager = _mainMenuCanvas.GetComponent<MainMenuManager>();
        if (_mainMenuCanvas != null) _mainMenu = _mainMenuCanvas.GetComponent<IMainMenu>();

        if (_mainMenuCanvas != null) _gameLobby = _mainMenuCanvas.GetComponent<GameLobby>();
        if (_startGame != null) _startGame = _startGame.GetComponent<StartGame>();

        if (_settingMenuCanvas != null) _settingManager = _settingMenuCanvas.GetComponent<SettingManager>();
        if (_settingMenuCanvas != null) _settings = _settingMenuCanvas.GetComponent<ISettings>();

        if (_spawnPrefab != null) _playerSpawn = _spawnPrefab.GetComponent<PlayerSpawner>();

        if (_gameMenuCanvas != null) _gameMenuManager = _gameMenuCanvas.GetComponent<GameMenuManager>();
        if (_gameMenuCanvas != null) _gameMenu = _gameMenuCanvas.GetComponent<IGameMenuController>();
    }

    private void StartInitialized()
    {
        Recorder.Instance?.Init();

        _settingManager?.Init();

        _playerSpawn?.Init();

        _menuManager?.Init(_settings, _gameManager);

        _gameMenuManager?.Init(_settings, _gameManager);

        _gameManager?.Init(_playerSpawn);

        _startGame?.Init(_gameLobby);

        _gameLobby?.Init(_gameMenu, _settings, _mainMenu);
    }


}
