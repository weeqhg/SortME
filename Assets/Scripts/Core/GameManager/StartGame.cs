using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using WekenDev.MainMenu;

public class StartGame : NetworkBehaviour
{

    private GameLobby _gameLobby;

    public enum Scene
    {
        LobbyScene,
        GameScene
    }

    public void Init(GameLobby gameLobby)
    {
        _gameLobby = gameLobby;
        
        if (_gameLobby != null) _gameLobby.OnStartGame += HandleStartGame;
    }

    private void HandleStartGame()
    {
        StartGameServerRpc();
    }
    
    [ServerRpc]
    private void StartGameServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            Scene.GameScene.ToString(),
            LoadSceneMode.Single
        );
    }

    public override void OnNetworkDespawn()
    {
        if (_gameLobby != null) _gameLobby.OnStartGame -= HandleStartGame;
    }
}
