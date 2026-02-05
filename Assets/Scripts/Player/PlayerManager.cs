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
        private PlayerNetwork playerNetwork;

#if UNITY_EDITOR
        private void Start()
        {
            // Этот метод полностью удаляется из билда
            if (Application.isEditor)
            {
                Init();
                InputManager.Instance.ChangeInputType(InputType.Player);
                Debug.Log("Тестовый код в редакторе");
            }
        }
#endif
        public void Init()
        {
            playerNetwork = GetComponent<PlayerNetwork>();
            playerNetwork.Init();
            if (Recorder.Instance != null) _voicePlayer.Init();
        }

    }
}
