using UnityEngine;
using WekenDev.InputSystem;
using WekenDev.VoiceChat;

namespace WekenDev.Player
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private NetworkVoice _voicePlayer;
        private InputManager _inputManager;
        public AudioSource voiceAudioSource;

        public void Init()
        {
            _inputManager = InputManager.Instance;
            if(_inputManager == null) Debug.Log("Игрок не получил inputmanager");
            if (_inputManager != null) _inputManager.ChangeInputType(InputType.Player);
            _voicePlayer.Init();
        }

    }
}
