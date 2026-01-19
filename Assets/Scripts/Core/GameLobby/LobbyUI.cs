using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.MainMenu.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _codeLobby;
        [SerializeField] private Button _copyCode;

        private void Start()
        {
            _copyCode.onClick.AddListener(CopyCode);
        }

        public void ChangeJoinCode(string code)
        {
            _codeLobby.text = code;
        }

        private void CopyCode()
        {
            GUIUtility.systemCopyBuffer = _codeLobby.text;
        }

    }
}
