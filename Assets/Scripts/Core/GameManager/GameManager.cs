using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WekenDev.InputSystem;
using WekenDev.Player;
using WekenDev.Spawn.Player;

public interface IGameManager
{
    bool GetPlayerOnScene();
    event Action<ulong, AudioSource> OnNewAudioSourcePlayer;
    event Action<ulong> OnDisconnectPlayer;
    void SwitchCurrentState(GameState gameState);
    GameState GetCurrentState();
    event Action OnChangeNewGameState;
}

public enum GameState { MainMenu, Playing, Paused }

namespace WekenDev.Game
{
    public class GameManager : MonoBehaviour, IGameManager
    {
        private GameState _currentState = GameState.MainMenu;
        private PlayerSpawner _playerSpawn;
        private PlayerManager _myPlayerManager;
        private Dictionary<ulong, PlayerManager> _otherPlayerManagers = new();
        public event Action<ulong, AudioSource> OnNewAudioSourcePlayer;
        public event Action<ulong> OnDisconnectPlayer;
        public event Action OnChangeNewGameState;
        public bool IsHaveLocalPlayer => _myPlayerManager != null;

        public void Init(PlayerSpawner playerSpawner)
        {
            _playerSpawn = playerSpawner;

            if (_playerSpawn != null)
            {
                _playerSpawn.OnFindPlayerManager += HandleNewPlayerManager;
                _playerSpawn.OnDisconnectPlayer += HandleDeletePlayerManager;
            }
        }

        public bool GetPlayerOnScene()
        {
            return _myPlayerManager != null;
        }

        public void SwitchCurrentState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.MainMenu:
                    _currentState = GameState.MainMenu;
                    InputManager.Instance.ChangeInputType(InputType.UI);
                    break;

                case GameState.Playing:
                    _currentState = GameState.Playing;
                    InputManager.Instance.ChangeInputType(InputType.Player);
                    break;

                case GameState.Paused:
                    _currentState = GameState.Paused;
                    InputManager.Instance.ChangeInputType(InputType.UI);
                    break;
            }

            OnChangeNewGameState?.Invoke();
        }

        public GameState GetCurrentState()
        {
            return _currentState;
        }

        private void HandleNewPlayerManager(ulong clientId, PlayerManager playerManager)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                _myPlayerManager = playerManager;
            }
            else
            {
                _otherPlayerManagers[clientId] = playerManager;
                GetAudioSource(clientId, playerManager);
            }
        }

        private void GetAudioSource(ulong clientId, PlayerManager playerManager)
        {
            AudioSource audioSource = playerManager.voiceAudioSource;
            OnNewAudioSourcePlayer?.Invoke(clientId, audioSource);
        }

        private void HandleDeletePlayerManager(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                _myPlayerManager = null;
            }
            else
            {
                _otherPlayerManagers.Remove(clientId);
                OnDisconnectPlayer?.Invoke(clientId);
            }
        }

        private void OnDestroy()
        {
            if (_playerSpawn != null) _playerSpawn.OnFindPlayerManager -= HandleNewPlayerManager;
            if (_playerSpawn != null) _playerSpawn.OnDisconnectPlayer -= HandleDeletePlayerManager;
        }


    }
}
