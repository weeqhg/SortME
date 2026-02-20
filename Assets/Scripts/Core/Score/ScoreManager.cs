using System;
using UnityEngine;

public interface IScoreManager
{
    event Action<int> OnScoreChanged;
    int CurrentScore { get; }
    void AddScoreValue(int value);
}
public class ScoreManager : MonoBehaviour, IScoreManager
{
    public event Action<int> OnScoreChanged;

    private int _currentScore = 0;
    public int CurrentScore => _currentScore;
   
    public void Init()
    {
        _currentScore = PlayerPrefs.GetInt("Score", 0);

        OnScoreChanged?.Invoke(_currentScore);
    }

    public void AddScoreValue(int points)
    {
        _currentScore += points;

        OnScoreChanged?.Invoke(_currentScore);

        PlayerPrefs.SetInt("Score", _currentScore);
    }
    
}