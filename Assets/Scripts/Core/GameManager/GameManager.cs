using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using WekenDev.Player;
using WekenDev.Spawn.Player;

namespace WekenDev.Game
{
    public class GameManager : MonoBehaviour
    {
        private PlayerSpawner _playerSpawn;
        private PlayerManager _myPlayerManager;
        private Dictionary<ulong, PlayerManager> _otherPlayerManagers = new();
        public event Action<ulong, AudioSource> OnNewAudioSourcePlayer;
        public event Action<ulong> OnDisconnectPlayer;
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
