using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.MainMenu.UI
{


    public class JoinUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputCode;
        [SerializeField] private Button _joinLobby;
        private string currentCode;

        public event Action<string> OnJoinLobby;


        private void Start()
        {
            _inputCode.onEndEdit.AddListener(ChangedCode);
            _joinLobby.onClick.AddListener(JoinLobby);
        }

        private void ChangedCode(string code)
        {
            currentCode = code;
        }

        private void JoinLobby()
        {
            OnJoinLobby?.Invoke(currentCode);
        }

    }
}