using UnityEngine;
using DG.Tweening;

public class SkyBoxRotation : MonoBehaviour
{
    [SerializeField] private float _rotationDuration = 100f;
    
    void Start()
    {
        RenderSettings.skybox.DOFloat(360f, "_Rotation", _rotationDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }
}
