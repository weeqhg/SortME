using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Сервер выбирает индекс отзыва и рассылает всем клиентам.
/// На клиентах локальный ScoreCounterUI берёт отзыв по индексу из своего массива.
/// </summary>
public class ScoreCounterNetwork : NetworkBehaviour
{
    [Header("Reviews (server-side pools)")]
    [Tooltip("Отзывы для плохой оценки (0..2)")]
    [SerializeField] private List<ReviewScriptableObject> _badReviews;
    [Tooltip("Отзывы для нейтральной оценки (3)")]
    [SerializeField] private List<ReviewScriptableObject> _neutralReviews;
    [Tooltip("Отзывы для положительной оценки (4..5)")]
    [SerializeField] private List<ReviewScriptableObject> _goodReviews;

    private ScoreCounterUI _scoreUI;
    public void Init()
    {
        _scoreUI = GetComponent<ScoreCounterUI>();
    }
    /// <summary>
    /// Вызывается на сервере, выбирает индекс в нужном пуле и шлёт всем клиентам.
    /// </summary>
    public void ServerShowReviewForScore(int stars)
    {
        if (!IsServer) return;

        int clamped = Mathf.Clamp(stars, 0, 5);
        int pool = GetPoolForScore(clamped);

        if (pool == 0)
        {
            Debug.LogWarning("ScoreCounterNetwork: соответствующий пул пуст, отправляем -1");
            ShowReviewClientRpc(clamped, -1);
            return;
        }

        int index = Random.Range(0, pool);
        ShowReviewClientRpc(clamped, index);
    }

    private int GetPoolForScore(int stars)
    {
        if (stars <= 0) return _scoreUI.GetCountBad();
        if (stars >= 1 && stars <= 3) return _scoreUI.GetCountNeutral();
        return _scoreUI.GetCountGood();
    }

    [ClientRpc]
    private void ShowReviewClientRpc(int stars, int reviewIndex)
    {
        if (_scoreUI != null)
        {
            if (reviewIndex < 0)
            {
                // Некорректный индекс — покажем просто рейтинг (без текста) или локально выбрать случайный
                _scoreUI.ShowReviewForScore(stars);
            }
            else
            {
                _scoreUI.ShowReviewByIndex(stars, reviewIndex);
            }
        }
        else
        {
            Debug.LogWarning("ScoreCounterNetwork: не найден ScoreCounterUI на клиенте");
        }
    }
}