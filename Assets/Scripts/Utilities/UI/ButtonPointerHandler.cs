using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonPointerHandler : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance?.PlayAudioUI(TypeUiAudio.Button);
    }

}
