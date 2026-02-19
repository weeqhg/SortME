using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WekenDev.MainMenu.UI;


namespace WekenDev.MainMenu
{
    public class GameLobby : MonoBehaviour
    {
        [SerializeField] private Button _createRoom;
        [SerializeField] private Button _tutorial;
        [SerializeField] private string _tutorialScene;
        [SerializeField] private JoinUI _joinUI;


        [Header("Loading UI")]
        [Tooltip("Image с типом Filled (Fill Method - Horizontal/Vertical)")]
        [SerializeField] private Image _loadingFill;
        [Tooltip("Опциональная контейнер-панель для показа индикатора загрузки")]
        [SerializeField] private CanvasGroup _loadingContainer;
        [Tooltip("Длительность одного цикла заполнения (сек)")]
        [SerializeField] private float _loadingCycleDuration = 1.0f;


        private int maxPlayers = 4;
        private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
        private bool _isLeaving = false;
        private Lobby joinedLobby;
        private IGameMenuController _gameMenu;
        private ISettings _settings;
        private IMainMenu _mainMenu;
        private IGameManager _gameManager;

        public event Action OnStartGame;

        private Coroutine _loadingRoutine;
        public enum Scene
        {
            LobbyScene,
            GameScene
        }

        public void Init(IGameMenuController gameMenu, ISettings settings, IMainMenu mainMenu, IGameManager gameManager)
        {
            InitializeAuth();

            _settings = settings;
            _mainMenu = mainMenu;
            _gameManager = gameManager;
            _gameMenu = gameMenu;
            if (_gameMenu != null) _gameMenu.OnLeaveGame += LeaveLobbyAndRelay;

            if (_createRoom != null) _createRoom.onClick.AddListener(CreateLobby);

            if (_joinUI != null) _joinUI.OnJoinLobby += JoinLobby;

            if (_tutorial != null) _tutorial.onClick.AddListener(StartTutorial);

            if (NetworkManager.Singleton != null) NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        public void StartGame()
        {
            OnStartGame?.Invoke();
        }

        private void StartTutorial()
        {
            StartLoadingAnimation();

            SceneManager.LoadScene(_tutorialScene);
        }

        private async void InitializeAuth()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        // Создание лобби (хост)
        private async void CreateLobby()
        {
            try
            {
                _gameManager?.SwitchCurrentState(GameState.Playing);

                // Запускаем индикатор загрузки
                StartLoadingAnimation();

                // 1. Создаем лобби
                joinedLobby = await LobbyService.Instance.CreateLobbyAsync(
                    "0000",
                    maxPlayers,
                    new CreateLobbyOptions { IsPrivate = false }
                );

                string lobbyCode = joinedLobby.LobbyCode; // ← Вот он!
                Debug.Log($"КОД ЛОББИ ДЛЯ ПРИСОЕДИНЕНИЯ: {lobbyCode}");

                _gameMenu.ChangeJoinCode(lobbyCode);

                // 2. Создаем Relay
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // 3. Сохраняем код в лобби
                await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
                });

                // 4. Настраиваем сеть
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                // 5. Запускаем хост
                NetworkManager.Singleton.StartHost();

                _mainMenu?.Hide();
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка: {e.Message}");
            }
            finally
            {
                // Останавливаем индикатор загрузки в любом случае
                StopLoadingAnimation();
            }
        }

        // Присоединение по коду (клиент)
        private async void JoinLobby(string lobbyCode)
        {
            _gameMenu.ChangeJoinCode(lobbyCode);


            try
            {
                // Показываем индикатор на время подключения
                StartLoadingAnimation();

                // 1. Находим лобби
                joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

                // 2. Получаем Relay код
                string relayCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

                // 3. Подключаемся к Relay
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);

                // 4. Настраиваем сеть
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                // 5. Запускаем клиент
                NetworkManager.Singleton.StartClient();

                _gameManager.SwitchCurrentState(GameState.Playing);

                _mainMenu?.Hide();

                Debug.Log($"Присоединились к лобби!");

            }
            catch (Exception e)
            {
                _mainMenu?.Show();
                Debug.Log($"Ошибка: {e.Message}");
            }
            finally
            {
                StopLoadingAnimation();
            }
        }

        // Выход из лобби
        private async void LeaveLobbyAndRelay()
        {
            if (_isLeaving) return;
            _isLeaving = true;

            try
            {
                // Останавливаем сеть
                if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                    // Ждем полной остановки (важно!)
                    await Task.Delay(500); // Короткая задержка для очистки
                    Debug.Log("✅ Сетевое соединение остановлено");
                }

                // 2. ПОТОМ выходим из лобби (если оно еще существует)
                if (joinedLobby != null)
                {
                    try
                    {
                        if (IsLobbyHost())
                        {
                            Debug.Log("🗑️ Хост удаляет лобби...");
                            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                            Debug.Log("✅ Лобби удалено!");
                        }
                        else
                        {
                            Debug.Log("👤 Клиент выходит из лобби...");
                            await LobbyService.Instance.RemovePlayerAsync(
                                joinedLobby.Id,
                                AuthenticationService.Instance.PlayerId
                            );
                            Debug.Log("✅ Вышли из лобби");
                        }
                    }
                    catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.LobbyNotFound)
                    {
                        // ✅ Лобби уже удалено хостом - это нормально, не ошибка!
                        Debug.Log("ℹ️ Лобби уже удалено (возможно хостом) - продолжаем выход");
                    }
                    catch (LobbyServiceException ex)
                    {
                        // Другие ошибки лобби (логируем, но продолжаем)
                        Debug.LogWarning($"⚠️ Ошибка лобби: {ex.Message} (продолжаем выход)");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"⚠️ Неожиданная ошибка: {ex.Message} (продолжаем выход)");
                    }
                }
                else
                {
                    Debug.Log("ℹ️ Лобби уже null, пропускаем выход");
                }

                _gameManager.SwitchCurrentState(GameState.MainMenu);

                _mainMenu?.Show();
                _gameMenu?.HideMenu();
                _settings?.Hide();

                // 3. Очищаем ссылку
                joinedLobby = null;

                Debug.Log("🏠 Возвращаемся в главное меню");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Ошибка выхода: {e.Message}");
                // Все равно очищаем
                joinedLobby = null;

                // Пытаемся остановить NetworkManager в любом случае
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }
            finally
            {
                // Снимаем флаг
                _isLeaving = false;

                SceneManager.LoadScene("GameScene");
            }
        }

        // ВЫХОД ИЗ ЛОББИ (без остановки Relay)
        private async Task LeaveLobbyOnly()
        {
            try
            {
                if (joinedLobby != null)
                {
                    if (IsLobbyHost())
                    {
                        Debug.Log("🗑️ Хост удаляет лобби (Relay остается)...");
                        await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                        Debug.Log("✅ Лобби удалено!");
                    }
                    else
                    {
                        Debug.Log("👤 Клиент выходит из лобби...");
                        await LobbyService.Instance.RemovePlayerAsync(
                            joinedLobby.Id,
                            AuthenticationService.Instance.PlayerId
                        );
                        Debug.Log("✅ Вышли из лобби");
                    }
                }
            }
            catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.Log("ℹ️ Лобби уже удалено");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"⚠️ Ошибка при выходе из лобби: {ex.Message}");
            }
            finally
            {
                joinedLobby = null; // Очищаем ссылку на лобби
            }
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                Debug.Log("// Сервер отключился - выходим");
                // Сервер отключился - выходим
                if (!_isLeaving) LeaveLobbyAndRelay();
            }
        }

        private bool IsLobbyHost()
        {
            return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
        }


        private void StartLoadingAnimation()
        {
            if (_loadingFill == null && _loadingContainer == null) return;

            if (_loadingContainer != null) _loadingContainer.alpha = 1f;

            if (_loadingFill != null) _loadingFill.fillAmount = 0f;

            if (_loadingRoutine != null) StopCoroutine(_loadingRoutine);
            _loadingRoutine = StartCoroutine(LoadingFillRoutine());
        }

        private void StopLoadingAnimation()
        {
            if (_loadingRoutine != null)
            {
                StopCoroutine(_loadingRoutine);
                _loadingRoutine = null;
            }

            if (_loadingFill != null) _loadingFill.fillAmount = 0f;
            if (_loadingContainer != null) _loadingContainer.alpha = 0f;
        }

        private System.Collections.IEnumerator LoadingFillRoutine()
        {
            if (_loadingFill == null)
            {
                // просто показываем контейнер без анимации
                yield break;
            }

            float cycle = Mathf.Max(0.01f, _loadingCycleDuration);

            while (true)
            {
                float t = 0f;
                while (t < cycle)
                {
                    t += Time.deltaTime;
                    _loadingFill.fillAmount = Mathf.Clamp01(t / cycle);
                    yield return null;
                }

                // сброс для повторного цикла (можно изменить на обратную анимацию)
                _loadingFill.fillAmount = 0f;
                yield return null;
            }
        }

        private void OnDestroy()
        {
            if (_gameMenu != null) _gameMenu.OnLeaveGame -= LeaveLobbyAndRelay;
            if (_joinUI != null) _joinUI.OnJoinLobby -= JoinLobby;

            // исправленная отписка: от того же колбэка, который мы подписываем в Init
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
        }
    }
}