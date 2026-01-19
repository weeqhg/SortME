using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using WekenDev.Player;
using System.Collections;
using System;

namespace WekenDev.Spawn.Player
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private NetworkManager _networkManager;
        private NetworkList<ulong> _playersId = new();
        private Dictionary<ulong, PlayerManager> _localPlayerManagers = new();
        public event Action<ulong, PlayerManager> OnFindPlayerManager;
        public event Action<ulong> OnDisconnectPlayer;
        public void Init()
        {
            _networkManager = NetworkManager.Singleton;

            // Подписываемся на события подключения/отключения
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback += OnClientConnected;
                _networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }

            _playersId.OnListChanged += OnListPlayerChanged;
        }

        public override void OnNetworkSpawn()
        {
            StartCoroutine(WaitForNetworkListSyncAndRegisterPlayers());
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            SpawnPlayer(clientId);
        }

        private void SpawnPlayer(ulong clientId)
        {
            Debug.Log("Спавним игрока");
            // Выбираем точку спавна
            Transform spawnPoint = GetSpawnPoint();

            // Создаем игрока
            GameObject player = Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation);

            // Делаем его сетевым объектом
            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            _playersId.Add(clientId);

            Debug.Log($"Spawned player for client {clientId} at {spawnPoint.position}");
        }

        private Transform GetSpawnPoint()
        {
            if (_spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return transform;
            }

            // Случайная точка
            return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];
        }

        private void OnListPlayerChanged(NetworkListEvent<ulong> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<ulong>.EventType.Add:
                    Debug.Log($"📨 Клиент получил нового игрока: {changeEvent.Value}");
                    RegisterPlayerOnClient(changeEvent.Value);
                    break;

                case NetworkListEvent<ulong>.EventType.Remove:
                    Debug.Log($"🗑️ Клиент получил удаление игрока: {changeEvent.Value}");
                    UnregisterPlayerOnClient(changeEvent.Value);
                    break;

                case NetworkListEvent<ulong>.EventType.Clear:
                    Debug.Log("🧹 Список игроков очищен");
                    _localPlayerManagers.Clear();
                    break;
            }
        }

        private IEnumerator WaitForNetworkListSyncAndRegisterPlayers()
        {
            Debug.Log("Жду синхронизации NetworkList...");

            // Ждем пока NetworkList получит данные
            int attempts = 0;
            while (_playersId.Count == 0 && attempts < 50) // 5 секунд максимум
            {
                attempts++;
                yield return new WaitForSeconds(0.1f);
            }

            if (_playersId.Count > 0)
            {
                Debug.Log($"✅ NetworkList синхронизирован! {_playersId.Count} игроков");
                FindAndRegisterAllExistingPlayers();
            }
            else
            {
                Debug.LogWarning("⚠️ NetworkList пуст даже после ожидания");
            }
        }

        private void FindAndRegisterAllExistingPlayers()
        {
            // Вариант 1: Через NetworkList (если он уже синхронизирован)
            if (_playersId.Count > 0)
            {
                Debug.Log($"В NetworkList {_playersId.Count} игроков");

                foreach (ulong playerId in _playersId)
                {
                    if (!_localPlayerManagers.ContainsKey(playerId))
                    {
                        RegisterPlayerOnClient(playerId);
                    }
                }
            }
        }

        private void RegisterPlayerOnClient(ulong clientId)
        {
            StartCoroutine(FindAndRegisterPlayer(clientId));
        }

        private IEnumerator FindAndRegisterPlayer(ulong clientId)
        {
            // Ждем пока игрок появится на этом клиенте
            NetworkObject playerObj = null;
            int attempts = 0;

            while (playerObj == null && attempts < 30) // 3 секунды максимум
            {
                playerObj = FindPlayerObjectByOwnerId(clientId);
                if (playerObj == null)
                {
                    attempts++;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            if (playerObj != null)
            {
                PlayerManager playerManager = playerObj.GetComponent<PlayerManager>();
                if (playerManager != null)
                {

                    playerManager.Init();
                    Debug.Log($"✅ PlayerManager {clientId} инициализирован с _recorder");

                    _localPlayerManagers[clientId] = playerManager;
                    OnFindPlayerManager?.Invoke(clientId, playerManager);
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Не удалось найти игрока {clientId} на клиенте");
            }
        }

        private NetworkObject FindPlayerObjectByOwnerId(ulong clientId)
        {
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if (networkObject.OwnerClientId == clientId && networkObject.IsPlayerObject)
                {
                    return networkObject;
                }
            }
            return null;
        }

        private void UnregisterPlayerOnClient(ulong clientId)
        {
            if (_localPlayerManagers.ContainsKey(clientId))
            {
                OnDisconnectPlayer?.Invoke(clientId);
                _localPlayerManagers.Remove(clientId);
                Debug.Log($"Игрок {clientId} удален с клиента");
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            _playersId.Remove(clientId);
        }




        // Метод для принудительного респавна
        private void RespawnPlayer(ulong clientId)
        {
            // Находим текущего игрока
            NetworkObject playerObject = _networkManager.SpawnManager.GetPlayerNetworkObject(clientId);

            if (playerObject != null)
            {
                playerObject.Despawn();
            }

            // Спавним нового
            SpawnPlayer(clientId);
        }

        public override void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= OnClientConnected;
                _networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            if (_playersId != null) _playersId.OnListChanged -= OnListPlayerChanged;
        }
    }
}