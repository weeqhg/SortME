using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

namespace WekenDev.СustomizationMenu
{

    public class CustomizationCamera : MonoBehaviour, IPointerUpHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private CinemachineFreeLook _freeLook;
        public void Init()
        {
            _freeLook.enabled = false;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            _freeLook.enabled = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _freeLook.enabled = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _freeLook.enabled = false;
        }
    }

}