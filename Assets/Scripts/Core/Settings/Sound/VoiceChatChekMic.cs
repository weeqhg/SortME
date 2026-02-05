using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WekenDev.VoiceChat;

namespace WekenDev.Settings.Sound
{
    public class VoiceChatChekMic : MonoBehaviour, IPointerUpHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Color _deactive;
        [SerializeField] private Color _active;

        public void Init()
        {
            _button.image.color = _deactive;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Recorder.Instance?.SetCheckMicrophone(true);
            _button.image.color = _active;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Recorder.Instance?.SetCheckMicrophone(false);
            _button.image.color = _deactive;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Recorder.Instance?.SetCheckMicrophone(false);
            _button.image.color = _deactive;
        }

        private void OnDisable()
        {
            Recorder.Instance?.SetCheckMicrophone(false);
            _button.image.color = _deactive;
        }

        // На случай уничтожения объекта
        private void OnDestroy()
        {
            Recorder.Instance?.SetCheckMicrophone(false);
            _button.image.color = _deactive;
        }
    }
}
