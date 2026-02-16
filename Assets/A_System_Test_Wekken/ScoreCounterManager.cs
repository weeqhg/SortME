using UnityEngine;

public class ScoreCounterManager : MonoBehaviour
{
    private IGlobalScoreManager _globalScore;
    private ScoreCounterUI _scoreCounterUI;
    private ScoreCounterNetwork _scoreCounterNetwork;

    public void Init(IGlobalScoreManager globalScore)
    {
        _globalScore = globalScore;
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

        const float weightDurability = 0.65f;
        const float weightTime = 0.35f;
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

        if (_globalScore == null)
        {
            Debug.LogWarning("ScoreCounterManager: _globalScore не инициализирован. Невозможно добавить очки.");
        }
        else
        {
            int delta;
            if (score <= 1) delta = -1;
            else if (score == 2 || score == 3) delta = 0;
            else delta = 1;

            Debug.Log($"ScoreCounter: mappedDelta={delta} for score={score}");
            _globalScore.AddScoreValue(delta);
        }

        return score;
    }
}
