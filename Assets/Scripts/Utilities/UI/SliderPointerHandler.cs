using UnityEngine;
using UnityEngine.EventSystems;

public class SliderPointerHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Slider);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Mute);
    }
}
