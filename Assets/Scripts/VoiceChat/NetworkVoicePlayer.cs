using Unity.Netcode;
using UnityEngine;

namespace WekenDev.VoiceChat
{
    public class NetworkVoice : NetworkBehaviour
    {
        private Speaker _localSpeaker;
        private Recorder _recorder;

        public void Init()
        {
            _recorder = Recorder.Instance;

            _localSpeaker = GetComponent<Speaker>();

            if (_recorder == null) Debug.Log($"Не найден модуль Recoder");

            _localSpeaker.Init();

            if (_recorder != null) _recorder.OnSendDataToNetwork += SendVoiceToServer;
        }

        // Отправка голоса на сервер
        private void SendVoiceToServer(byte[] voiceData)
        {
            if (!IsOwner)
            {
                Debug.LogWarning($"Not owner, cannot send voice. Owner: {OwnerClientId}, Local: {NetworkManager.LocalClientId}");
                return;
            }

            if (!IsSpawned)
            {
                Debug.LogWarning("NetworkVoice not spawned yet, cannot send");
                return;
            }

            if (voiceData == null || voiceData.Length == 0)
            {
                Debug.LogWarning("Attempted to send empty voice data");
                return;
            }

            if (IsServer)
            {
                // Если мы и сервер, и клиент (host), отправляем сразу на клиентов
                ReceiveVoiceDataClientRpc(voiceData, OwnerClientId);
            }
            else if (IsClient)
            {
                // Обычный клиент отправляет на сервер
                SendVoiceDataServerRpc(voiceData);
            }
        }

        // RPC для отправки данных на сервер
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void SendVoiceDataServerRpc(byte[] voiceData)
        {
            ReceiveVoiceDataClientRpc(voiceData, OwnerClientId);
        }

        // RPC для получения данных всеми клиентами
        [ClientRpc]
        private void ReceiveVoiceDataClientRpc(byte[] voiceData, ulong senderId)
        {
            // Не воспроизводим свой собственный голос
            if (NetworkManager.Singleton.LocalClientId == senderId)
            {
                return;
            }

            // Передаем данные в Speaker
            if (_localSpeaker != null) _localSpeaker.AddVoiceData(voiceData);
        }

        public override void OnNetworkDespawn()
        {
            if (_recorder != null) _recorder.OnSendDataToNetwork -= SendVoiceToServer;
        }
    }

}