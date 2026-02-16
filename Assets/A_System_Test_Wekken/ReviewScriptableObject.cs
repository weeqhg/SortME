using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewReview", menuName = "Custom Data/Review")]
public class ReviewScriptableObject : ScriptableObject
{
    [Tooltip("Текст отзыва (Localized)")]
    public LocalizedString localizedText;
}
