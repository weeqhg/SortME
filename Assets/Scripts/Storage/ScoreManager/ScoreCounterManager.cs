using UnityEngine;

public class ScoreCounterManager : MonoBehaviour
{
    private IScoreManager _score;
    private ScoreCounterUI _scoreCounterUI;
    private ScoreCounterNetwork _scoreCounterNetwork;

    public void Init(IScoreManager scoreManager)
    {
        _score = scoreManager;
        _scoreCounterUI = GetComponent<ScoreCounterUI>();
        _scoreCounterNetwork = GetComponent<ScoreCounterNetwork>();
        _scoreCounterUI.Init();
        _scoreCounterNetwork.Init();
    }

    public int CountScoreForOrder(int durability, float timeRemaining, float maxTime, bool isBox)
    {
        float dur = Mathf.Clamp(durability, 0, 100) / 100f;
        float timeRatio = 0f;

        if (maxTime > 0f)
        {
            timeRatio = Mathf.Clamp01(timeRemaining / maxTime);
        }

        const float weightDurability = 0.6f;
        const float weightTime = 0.4f;
        const float boxModifierValue = 0.2f;
        float boxModifier = isBox ? boxModifierValue : -boxModifierValue;

        float combined = dur * weightDurability + timeRatio * weightTime + boxModifier;

        int score = Mathf.Clamp(Mathf.RoundToInt(combined * 5f), 0, 5);

        if (_scoreCounterNetwork != null)
        {
            _scoreCounterNetwork.ServerShowReviewForScore(score);
        }
        else if (_scoreCounterUI != null)
        {
            // fallback: локальный показ
            _scoreCounterUI.ShowReviewForScore(score);
        }

        Debug.Log($"ScoreCounter: durability={durability}, timeRemaining={timeRemaining}, maxTime={maxTime}, computedScore={score}");

        if (_score == null)
        {
            Debug.LogWarning("ScoreCounterManager: _globalScore не инициализирован. Невозможно добавить очки.");
        }
        else
        {
            int delta;
            if (score == 0) delta = -1;
            else if (score >= 1 && score <= 3) delta = 0;
            else delta = 1;

            Debug.Log($"ScoreCounter: mappedDelta={delta} for score={score}");
            _score.AddScoreValue(delta);
        }

        return score;
    }
}
