using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class GlobalSoreUI : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent _scoreText;
    [SerializeField] private Image _positivScore;
    [SerializeField] private Image _negativScore;

    [Header("Настройки анимации")]
    [SerializeField] private float scoreAnimationDuration = 0.5f;
    [SerializeField] private float sliderAnimationDuration = 0.8f;
    [SerializeField] private Ease scoreEase = Ease.OutBack;
    [SerializeField] private Ease sliderEase = Ease.OutQuad;

    [Header("Цвета")]
    [SerializeField] private Color positiveColor = new Color(0.2f, 0.6f, 1f); // Синий
    [SerializeField] private Color negativeColor = new Color(1f, 0.3f, 0.3f); // Красный
    [SerializeField] private Color zeroColor = Color.white;

    [Header("Эффекты")]
    [SerializeField] private bool usePulseEffect = true;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.2f;

    private int _displayedScore = 0;
    private int _targetScore = 0;

    private Tween _scoreTween;
    private Tween _positiveSliderTween;
    private Tween _negativeSliderTween;

    private IGlobalScoreManager _globalScore;
    public void Init(IGlobalScoreManager globalScore)
    {
        _globalScore = globalScore;

        if (_globalScore != null)
        {
            _targetScore = _globalScore.CurrentScore;
            _displayedScore = _targetScore;

            UpdateScoreText(_targetScore);
            UpdateSlidersWithAnimation(_targetScore);

            _globalScore.OnScoreChanged += OnScoreChanged;
            Debug.Log("GlobalScoreManager найден");
        }
        else
        {
            Debug.LogError("GlobalScoreManager не найден!");
        }
    }
    private void OnScoreChanged(int newScore)
    {
        Debug.Log($"Получено новое значение счета: {newScore}");
        _targetScore = newScore;
        AnimateScoreChange(newScore);
        UpdateSlidersWithAnimation(newScore);
    }

    #region Animation
    void AnimateScoreChange(int newScore)
    {
        if (newScore == _displayedScore)
        {
            UpdateScoreText(newScore);
            return;
        }


        // Сохраняем старое значение для эффектов
        int oldScore = _displayedScore;

        // Останавливаем предыдущую анимацию если есть
        if (_scoreTween != null && _scoreTween.IsActive())
        {
            _scoreTween.Kill();
        }

        // Анимация числового значения
        _scoreTween = DOTween.To(
            () => _displayedScore, // Начальное значение
            x =>
            {
                _displayedScore = x;
                UpdateScoreText(x);
            },
            newScore,         // Конечное значение
            scoreAnimationDuration // Длительность
        )
        .SetEase(scoreEase)
        .OnStart(() =>
        {
            // Эффект при начале анимации
            if (usePulseEffect && Mathf.Abs(newScore - oldScore) >= 5)
            {
                PulseScoreText();
            }
        })
        .OnComplete(() =>
        {
            // Гарантируем точное значение в конце
            _displayedScore = newScore;
            UpdateScoreText(newScore);
        });

        // Визуальные эффекты для больших изменений
        if (Mathf.Abs(newScore - oldScore) >= 20)
        {
            PlayBigChangeEffect(oldScore, newScore);
        }
    }

    void UpdateScoreText(int score)
    {
        if (_scoreText != null)
        {
            // Форматируем со знаком
            string formattedScore = FormatScoreWithSign(score);
            _scoreText.StringReference.Arguments = new object[] { formattedScore };
            _scoreText.RefreshString();

            // Меняем цвет текста
            UpdateTextColor(score);
        }
    }

    string FormatScoreWithSign(int score)
    {
        if (score > 0) return $"+{score}";
        else return score.ToString();
    }

    void UpdateTextColor(int score)
    {
        var textComponent = _scoreText.GetComponent<Text>();
        if (textComponent != null)
        {
            if (score > 0)
                textComponent.DOColor(positiveColor, 0.3f);
            else if (score < 0)
                textComponent.DOColor(negativeColor, 0.3f);
            else
                textComponent.DOColor(zeroColor, 0.3f);
        }
    }

    void PulseScoreText()
    {
        if (_scoreText != null)
        {
            Transform textTransform = _scoreText.transform;
            Sequence pulseSequence = DOTween.Sequence();

            pulseSequence.Append(textTransform.DOScale(pulseScale, pulseDuration / 2f));
            pulseSequence.Append(textTransform.DOScale(1f, pulseDuration / 2f));
            pulseSequence.SetEase(Ease.OutQuad);
        }
    }

    void PlayBigChangeEffect(int oldScore, int newScore)
    {
        if (_scoreText != null)
        {
            Transform textTransform = _scoreText.transform;

            // Эффект "прыжка" для больших изменений
            Sequence bigChangeSequence = DOTween.Sequence();

            bigChangeSequence.Append(textTransform.DOScale(1.3f, 0.15f));
            bigChangeSequence.Append(textTransform.DOScale(0.9f, 0.1f));
            bigChangeSequence.Append(textTransform.DOScale(1.1f, 0.1f));
            bigChangeSequence.Append(textTransform.DOScale(1f, 0.05f));

            // Эффект "тряски" если изменение очень большое
            if (Mathf.Abs(newScore - oldScore) >= 50)
            {
                bigChangeSequence.Join(textTransform.DOShakePosition(0.3f, strength: 10f, vibrato: 20));
            }
        }
    }

    // Анимация слайдеров
    void UpdateSlidersWithAnimation(int score)
    {
        // Ограничиваем значение
        score = Mathf.Clamp(score, -100, 100);

        // Останавливаем предыдущие анимации
        if (_positiveSliderTween != null && _positiveSliderTween.IsActive())
            _positiveSliderTween.Kill();

        if (_negativeSliderTween != null && _negativeSliderTween.IsActive())
            _negativeSliderTween.Kill();

        if (score > 0)
        {
            // Анимация синего слайдера (положительные значения)
            _positiveSliderTween = _positivScore.DOFillAmount(
                score / 100f,
                sliderAnimationDuration
            )
            .SetEase(sliderEase);

            // Анимация цвета
            _positivScore.DOColor(
                Color.Lerp(Color.cyan, positiveColor, score / 100f),
                sliderAnimationDuration
            );

            // Скрываем красный слайдер с анимацией
            _negativeSliderTween = _negativScore.DOFillAmount(0f, sliderAnimationDuration / 2f);
        }
        else if (score < 0)
        {
            // Анимация красного слайдера (отрицательные значения)
            _negativeSliderTween = _negativScore.DOFillAmount(
                Mathf.Abs(score) / 100f,
                sliderAnimationDuration
            )
            .SetEase(sliderEase);

            // Анимация цвета
            _negativScore.DOColor(
                Color.Lerp(Color.yellow, negativeColor, Mathf.Abs(score) / 100f),
                sliderAnimationDuration
            );

            // Скрываем синий слайдер с анимацией
            _positiveSliderTween = _positivScore.DOFillAmount(0f, sliderAnimationDuration / 2f);
        }
        else
        {
            // Анимация к нулю (оба слайдера скрываются)
            _positiveSliderTween = _positivScore.DOFillAmount(0f, sliderAnimationDuration / 2f);
            _negativeSliderTween = _negativScore.DOFillAmount(0f, sliderAnimationDuration / 2f);

            // Возвращаем цвета к исходным
            _positivScore.DOColor(positiveColor, 0.3f);
            _negativScore.DOColor(negativeColor, 0.3f);
        }
    }
    #endregion

    private void OnDestroy()
    {
        _globalScore.OnScoreChanged -= OnScoreChanged;
        DOTween.Kill(_scoreText?.transform);
        DOTween.Kill(_positivScore);
        DOTween.Kill(_negativScore);
    }
}
