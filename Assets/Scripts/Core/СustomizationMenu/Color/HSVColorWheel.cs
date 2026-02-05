using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HSVColorWheel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Image _wheelImage;
    [SerializeField] private RectTransform _hueSelector;
    [SerializeField] private Slider _brightnessSlider;
    [SerializeField] private Image _currentColorDisplay;
    [SerializeField] private Material _bodyPlayer;

    private float _hue = 0f;        // 0-1
    private float _saturation = 1f; // 0-1  
    private float _brightness = 1f; // 0-1

    private Texture2D _wheelTexture;
    private const int TEXTURE_SIZE = 256;

    public void Init()
    {
        GenerateHSVWheelTexture();
        _brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        LoadColor();
    }
    private void LoadColor()
    {
        string hexColor = PlayerPrefs.GetString("PlayerColor", "FFFFFF");
        string fullHex = "#" + hexColor; // Добавляем # здесь

        if (ColorUtility.TryParseHtmlString(fullHex, out Color color))
        {
            _bodyPlayer.color = color;
            Debug.Log($"Загружен цвет: {color}");
        }
    }

    private void GenerateHSVWheelTexture()
    {
        _wheelTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE);

        Vector2 center = new Vector2(TEXTURE_SIZE / 2, TEXTURE_SIZE / 2);
        float radius = TEXTURE_SIZE / 2;

        for (int y = 0; y < TEXTURE_SIZE; y++)
        {
            for (int x = 0; x < TEXTURE_SIZE; x++)
            {
                Vector2 point = new Vector2(x, y);
                Vector2 direction = point - center;
                float distance = direction.magnitude;

                if (distance <= radius)
                {
                    // Hue - угол
                    float angle = Mathf.Atan2(direction.y, direction.x);
                    float hue = (angle + Mathf.PI) / (2 * Mathf.PI);

                    // Saturation - расстояние от центра
                    float saturation = distance / radius;

                    // Используем максимальную яркость для круга
                    Color color = Color.HSVToRGB(hue, saturation, _brightness);
                    _wheelTexture.SetPixel(x, y, color);
                }
                else
                {
                    _wheelTexture.SetPixel(x, y, Color.clear);
                }
            }
        }

        _wheelTexture.Apply();
        _wheelImage.sprite = Sprite.Create(_wheelTexture,
            new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE), Vector2.one * 0.5f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SelectColorFromScreen(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        SelectColorFromScreen(eventData.position);
    }

    private void SelectColorFromScreen(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _wheelImage.rectTransform,
            screenPosition,
            null,
            out Vector2 localPoint
        );

        float radius = _wheelImage.rectTransform.rect.width / 2;
        Vector2 center = Vector2.zero;

        Vector2 direction = localPoint - center;
        float distance = direction.magnitude;

        if (distance <= radius)
        {
            // Обновляем селектор
            _hueSelector.anchoredPosition = localPoint;

            // Вычисляем HSV
            _hue = (Mathf.Atan2(direction.y, direction.x) + Mathf.PI) / (2 * Mathf.PI);
            _saturation = Mathf.Clamp01(distance / radius);

            UpdateColorDisplay();
        }
    }

    private void OnBrightnessChanged(float value)
    {
        _brightness = value;
        GenerateHSVWheelTexture(); // Перегенерируем с новой яркостью
        UpdateColorDisplay();
    }

    private void UpdateColorDisplay()
    {
        Color color = Color.HSVToRGB(_hue, _saturation, _brightness);
        _currentColorDisplay.color = color;

        // Сохраняем
        SaveColor(color);

        // Применяем к игроку
        ApplyColorToPlayer(color);
    }

    private void SaveColor(Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color); // Без #
        PlayerPrefs.SetString("PlayerColor", hexColor);
        PlayerPrefs.Save();
        Debug.Log($"Сохранён цвет: {hexColor}");
    }

    private void ApplyColorToPlayer(Color color)
    {
        // Здесь логика применения цвета к игроку
        _bodyPlayer.color = color;
    }
}
