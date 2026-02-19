using System.Globalization;
using UnityEngine;

public class TutorColorPlayer : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _body;
    [SerializeField] private Material _originalMaterial;
    private Material _clonedMaterial;


    public void Init()
    {
        _clonedMaterial = new(_originalMaterial);

        _body.material = _clonedMaterial;




        LoadAndSendColor();


    }

    private void LoadAndSendColor()
    {
        string savedColorHex = PlayerPrefs.GetString("PlayerColor", "FFFFFF");

        if (ColorUtility.TryParseHtmlString("#" + savedColorHex, out Color savedColor))
        {
            _clonedMaterial.color = savedColor;
        }
    }





}
