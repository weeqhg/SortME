using UnityEngine;
using WekenDev.VoiceChat;
using WekenDev.Settings;
using WekenDev.MainMenu;
using WekenDev.GameMenu;
using WekenDev.Game;
using WekenDev.Spawn.Player;
using WekenDev.СustomizationMenu;

public class Bootstrap : MonoBehaviour
{
    [Header("Основные объекты")]
    [SerializeField] private GameObject _directionalLight;
    [SerializeField] private GameObject _spawnPrefab;
    [SerializeField] private GameObject _storagePrefab;

    [Header("Интерфейс")]
    [SerializeField] private GameObject _mainMenuCanvas;
    [SerializeField] private GameObject _settingMenuCanvas;
    [SerializeField] private GameObject _gameMenuCanvas;
    [SerializeField] private GameObject _customizationPrefab;
    [SerializeField] private GameObject _globalScorePrefab;
    [SerializeField] private GameObject _audioManagerPrefab;

    //Нужные менеджеры будем объявлять отсюда или сразу создаваться
    [SerializeField] private GameManager _gameManager;
    private IGameManager _gameManagerInterface;
    [SerializeField] private StartGame _startGame;

    private MainMenuManager _menuManager;
    private IMainMenu _mainMenu;

    private GameLobby _gameLobby;

    private SettingManager _settingManager;
    private ISettings _settings;

    private PlayerSpawner _playerSpawn;

    private GameMenuManager _gameMenuManager;
    private IGameMenuController _gameMenu;

    private СustomizationManager _сustomizationManager;
    private ICustomizationMenu _customInterface;

    private GlobalScoreRating _globalScoreRating;

    private StorageManager _storageManager;
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

        if (_customizationPrefab != null) _customizationPrefab = Instantiate(_customizationPrefab);
        else Debug.LogWarning("Внимание модуль Customization не установлен");

        if (_globalScorePrefab != null) _globalScorePrefab = Instantiate(_globalScorePrefab);
        else Debug.LogWarning("Внимание модуль GlobalScore не установлен");

        if (_audioManagerPrefab != null) _audioManagerPrefab = Instantiate(_audioManagerPrefab);
        else Debug.LogWarning("Внимание модуль AudioManager не установлен");

        if (_storagePrefab != null) _storagePrefab = Instantiate(_storagePrefab);
        else Debug.LogWarning("Внимание модуль Storage не установлен");
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

        if (_customizationPrefab != null) _сustomizationManager = _customizationPrefab.GetComponent<СustomizationManager>();
        if (_customizationPrefab != null) _customInterface = _customizationPrefab.GetComponent<ICustomizationMenu>();

        if (_gameManager != null) _gameManagerInterface = _gameManager.GetComponent<IGameManager>();

        if (_globalScorePrefab != null) _globalScoreRating = _globalScorePrefab.GetComponentInChildren<GlobalScoreRating>();

        if (_storagePrefab != null) _storageManager = _storagePrefab.GetComponentInChildren<StorageManager>();
    }

    private void StartInitialized()
    {
        Recorder.Instance?.Init();

        AudioManager.Instance?.Init();

        _settingManager?.Init();

        _playerSpawn?.Init();

        _menuManager?.Init(_settings, _customInterface, _gameManagerInterface);

        _gameMenuManager?.Init(_settings, _gameManagerInterface);

        _gameManager?.Init(_playerSpawn);

        _startGame?.Init(_gameLobby);

        _gameLobby?.Init(_gameMenu, _settings, _mainMenu, _gameManagerInterface);

        _сustomizationManager?.Init(_mainMenu, _gameManagerInterface);

        _globalScoreRating?.Init(_gameManagerInterface);

        _storageManager?.Init();

    }


}
