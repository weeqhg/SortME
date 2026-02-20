using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ScoreCounterUI : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _reviewText;
    [SerializeField] private float _showDuration = 3f;
    [SerializeField] private float _fadeDuration = 0.25f;

    [Header("Stars (images)")]
    [SerializeField] private Image[] _ratingStars = new Image[5];
    [SerializeField] private Sprite _starOn;
    [SerializeField] private Sprite _starOff;

    [Header("Reviews")]
    [Tooltip("Отзывы для плохой оценки (0..2)")]
    [SerializeField] private List<ReviewScriptableObject> _badReviews;
    [Tooltip("Отзывы для нейтральной оценки (3)")]
    [SerializeField] private List<ReviewScriptableObject> _neutralReviews;
    [Tooltip("Отзывы для положительной оценки (4..5)")]
    [SerializeField] private List<ReviewScriptableObject> _goodReviews;

    [Header("Typing")]
    [Tooltip("Задержка между символами при анимации печатания (в секундах)")]
    [SerializeField] private float _typingCharDelay = 0.03f;

    private Coroutine _hideRoutine;
    private AsyncOperationHandle<string> _currentHandle;
    private bool _hasHandle;

    private string _pendingReviewText;
    private Coroutine _typingCoroutine;

    public int GetCountBad() => _badReviews.Count;
    public int GetCountNeutral() => _neutralReviews.Count;
    public int GetCountGood() => _goodReviews.Count;
    public void Init()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        if (_reviewText != null)
            _reviewText.text = "";

        ResetStarsVisual();

        LoadReviewsFromResources();
    }

    private void LoadReviewsFromResources()
    {
        // Ожидаемые папки в Resources:
        // Resources/Reviews/Bad
        // Resources/Reviews/Neutral
        // Resources/Reviews/Good
        // Если под-папки отсутствуют, пытаемся загрузить все из Resources/Reviews и распределить по имени.

        // Загрузка по под-папкам, только если соответствующий список пуст
        if (_badReviews == null) _badReviews = new List<ReviewScriptableObject>();
        if (_neutralReviews == null) _neutralReviews = new List<ReviewScriptableObject>();
        if (_goodReviews == null) _goodReviews = new List<ReviewScriptableObject>();

        if (_badReviews.Count == 0)
        {
            var bad = Resources.LoadAll<ReviewScriptableObject>("Reviews/Bad");
            if (bad != null && bad.Length > 0)
                _badReviews = new List<ReviewScriptableObject>(bad);
        }

        if (_neutralReviews.Count == 0)
        {
            var neutral = Resources.LoadAll<ReviewScriptableObject>("Reviews/Neutral");
            if (neutral != null && neutral.Length > 0)
                _neutralReviews = new List<ReviewScriptableObject>(neutral);
        }

        if (_goodReviews.Count == 0)
        {
            var good = Resources.LoadAll<ReviewScriptableObject>("Reviews/Good");
            if (good != null && good.Length > 0)
                _goodReviews = new List<ReviewScriptableObject>(good);
        }

        if (_badReviews.Count == 0) Debug.LogWarning("ScoreCounterUI: пул плохих отзывов пуст (Resources/Reviews/Bad или Reviews)");
        if (_neutralReviews.Count == 0) Debug.LogWarning("ScoreCounterUI: пул нейтральных отзывов пуст (Resources/Reviews/Neutral или Reviews)");
        if (_goodReviews.Count == 0) Debug.LogWarning("ScoreCounterUI: пул позитивных отзывов пуст (Resources/Reviews/Good или Reviews)");
    }

    /// <summary>
    /// Локальный вызов: показать отзыв по звёздам (0..5) — существующая логика (оставлена).
    /// </summary>
    public void ShowReviewForScore(int stars)
    {
        int clamped = Mathf.Clamp(stars, 0, 5);
        List<ReviewScriptableObject> pool = GetPoolForScore(clamped);
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("ScoreCounterUI: Нет отзывов в соответствующем пуле");
            return;
        }

        var review = pool[Random.Range(0, pool.Count)];
        if (review == null)
        {
            Debug.LogWarning("ScoreCounterUI: выбрано пустое значение отзыва");
            return;
        }

        UpdateStarsVisual(clamped);

        if (_hasHandle)
        {
            AddressablesReleaseSafe();
        }

        _currentHandle = review.localizedText.GetLocalizedStringAsync();
        _hasHandle = true;
        _currentHandle.Completed += handle =>
        {
            _hasHandle = false;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _pendingReviewText = handle.Result;
                ShowPanel();
            }
            else
            {
                Debug.LogWarning($"ScoreCounterUI: не удалось загрузить review text ({review.name})");
            }

            AddressablesReleaseSafe();
        };
    }

    /// <summary>
    /// Клиентская функция: сервер присылает индекс. Клиент берёт отзыв из своего пула по индексу и показывает его.
    /// </summary>
    public void ShowReviewByIndex(int stars, int index)
    {
        List<ReviewScriptableObject> pool = GetPoolForScore(stars);
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("ScoreCounterUI: Нет отзывов в соответствующем пуле (ShowReviewByIndex)");
            return;
        }

        if (index < 0 || index >= pool.Count)
        {
            Debug.LogWarning($"ScoreCounterUI: получен некорректный индекс отзыва {index}, пул имеет размер {pool.Count}. Попытка зафиксировать.");
            index = Mathf.Clamp(index, 0, pool.Count - 1);
        }

        var review = pool[index];
        if (review == null)
        {
            Debug.LogWarning("ScoreCounterUI: по индексу получено null-отзыв");
            return;
        }

        UpdateStarsVisual(stars);

        if (_hasHandle)
        {
            AddressablesReleaseSafe();
        }

        _currentHandle = review.localizedText.GetLocalizedStringAsync();
        _hasHandle = true;
        _currentHandle.Completed += handle =>
        {
            _hasHandle = false;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _pendingReviewText = handle.Result;
                ShowPanel();
            }
            else
            {
                Debug.LogWarning($"ScoreCounterUI: не удалось загрузить review text ({review.name})");
            }

            AddressablesReleaseSafe();
        };
    }

    private List<ReviewScriptableObject> GetPoolForScore(int stars)
    {
        if (stars <= 0) return _badReviews;
        if (stars >= 1 && stars <= 3) return _neutralReviews;
        return _goodReviews;
    }

    private void ShowPanel()
    {
        if (_canvasGroup == null) return;

        // Остановим корутину скрытия, если она идёт
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);

        // Отменяем возможные старые твины и ставим начальный scale = 0
        _canvasGroup.transform.DOKill();
        _canvasGroup.transform.localScale = Vector3.zero;

        // Показываем панель
        _canvasGroup.alpha = 1f;

        // Сбрасываем текст перед анимацией печатания
        if (_reviewText != null)
            _reviewText.text = "";

        // Появление: выпрыгивающий эффект
        float popDuration = 0.35f;
        _canvasGroup.transform.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // После завершения появления запускаем анимацию печатающего текста
                StartTyping();
            });

        // Запускаем таймер скрытия
        _hideRoutine = StartCoroutine(HideAfterDelay(_showDuration));
    }

    private void StartTyping()
    {
        if (_reviewText == null) return;

        // Остановим предыдущую корутину печати, если есть
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        string textToType = _pendingReviewText ?? string.Empty;
        _typingCoroutine = StartCoroutine(TypeTextRoutine(textToType));
    }

    private IEnumerator TypeTextRoutine(string fullText)
    {
        if (_reviewText == null)
        {
            _typingCoroutine = null;
            yield break;
        }

        _reviewText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            _reviewText.text += fullText[i];
            yield return new WaitForSeconds(_typingCharDelay);
        }

        _typingCoroutine = null;
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_canvasGroup == null)
        {
            _hideRoutine = null;
            yield break;
        }

        float t = 0f;
        float start = _canvasGroup.alpha;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, 0f, t / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;

        if (_reviewText != null)
            _reviewText.text = "";

        _hideRoutine = null;
    }
    private void UpdateStarsVisual(int filled)
    {
        if (_ratingStars == null || _ratingStars.Length == 0) return;

        for (int i = 0; i < _ratingStars.Length; i++)
        {
            if (_ratingStars[i] == null) continue;
            if (_starOn != null && _starOff != null)
            {
                _ratingStars[i].sprite = i < filled ? _starOn : _starOff;
                _ratingStars[i].color = Color.white;
            }
            else
            {
                _ratingStars[i].color = i < filled ? Color.yellow : new Color(1f, 1f, 1f, 0.25f);
            }
        }
    }
    private void ResetStarsVisual()
    {
        if (_ratingStars == null || _ratingStars.Length == 0) return;
        for (int i = 0; i < _ratingStars.Length; i++)
        {
            if (_ratingStars[i] == null) continue;
            if (_starOff != null)
                _ratingStars[i].sprite = _starOff;
            else
                _ratingStars[i].color = new Color(1f, 1f, 1f, 0.25f);
        }
    }
    private void OnDisable()
    {
        AddressablesReleaseSafe();
    }

    private void AddressablesReleaseSafe()
    {
        if (_hasHandle)
        {
            try
            {
                UnityEngine.AddressableAssets.Addressables.Release(_currentHandle);
            }
            catch { /* ignore */ }
            _hasHandle = false;
        }
    }
}
