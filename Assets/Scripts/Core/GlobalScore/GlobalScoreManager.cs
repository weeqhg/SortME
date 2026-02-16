using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;

public interface IGlobalScoreManager
{
    void StartAutoUpdate();
    void StopAutoUpdate();

    event Action<int> OnScoreChanged;
    int CurrentScore { get; }
    void AddScoreValue(int value);
}
public class GlobalScoreManager : MonoBehaviour, IGlobalScoreManager
{
    [Header("Warehouse Account")]
    [SerializeField] private string warehouseUsername = "MyWarehouse";
    [SerializeField] private string warehousePassword = "MyPassword123";
    [SerializeField] private string leaderboardId = "123";

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 3f;

    public event Action<int> OnScoreChanged;

    private static GlobalScoreManager _instance;
    private int _currentScore = 0;
    private bool _isUpdating = false;
    private Coroutine _updateCoroutine;

    private async void Awake()
    {
        await Initialize();
    }

    public int CurrentScore => _currentScore;
    public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;

    private async Task Initialize()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await SignInToWarehouse();
            Debug.Log("GlobalScoreManager инициализирован");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка инициализации: {e.Message}");
        }
    }

    private async Task SignInToWarehouse()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(warehouseUsername, warehousePassword);
                Debug.Log("Успешный вход в склад!");
            }
        }
        catch (RequestFailedException)
        {
            await CreateWarehouse();
        }
    }

    private async Task CreateWarehouse()
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(warehouseUsername, warehousePassword);
            Debug.Log("Склад создан!");
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Ошибка создания склада: {e.Message}");
        }
    }

    public void StartAutoUpdate()
    {
        StopAutoUpdate();
        _updateCoroutine = StartCoroutine(AutoUpdateCoroutine());
    }

    public void StopAutoUpdate()
    {
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    private IEnumerator AutoUpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (!_isUpdating && AuthenticationService.Instance.IsSignedIn)
            {
                _ = LoadWarehouseScore();
            }
        }
    }

    public async Task LoadWarehouseScore()
    {
        if (!AuthenticationService.Instance.IsSignedIn || _isUpdating)
        {
            Debug.Log("Не авторизован или уже обновляется");
            return;
        }

        _isUpdating = true;

        try
        {
            var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
            int serverScore = (int)playerScore.Score;

            if (serverScore != _currentScore)
            {
                Debug.Log($"Счет синхронизирован: {_currentScore} -> {serverScore}");
                _currentScore = serverScore;
                OnScoreChanged?.Invoke(_currentScore);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Склад пока не в рейтинге или ошибка: " + e.Message);
            if (_currentScore != 0)
            {
                _currentScore = 0;
                OnScoreChanged?.Invoke(0);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    public void AddScoreValue(int points)
    {
        _ = AddScore(points);
    }
    public async Task AddScore(int points)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            return;
        }

        if (_isUpdating)
        {
            Debug.Log("Подождите, идет обновление...");
            return;
        }

        _isUpdating = true;

        try
        {
            var options = new AddPlayerScoreOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "added", $"{points} points" },
                    { "device", SystemInfo.deviceName },
                    { "timestamp", DateTime.Now.ToString("HH:mm:ss") }
                }
            };

            var result = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                leaderboardId,
                points,
                options
            );

            int serverScore = (int)result.Score;

            if (serverScore != _currentScore)
            {
                _currentScore = serverScore;
                OnScoreChanged?.Invoke(_currentScore);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка добавления счета: {e.Message}");
        }
        finally
        {
            _isUpdating = false;
        }
    }

    public void Logout()
    {
        AuthenticationService.Instance.SignOut();
        _currentScore = 0;
        StopAutoUpdate();
        Debug.Log("Выход из склада");
    }

    private void OnDestroy()
    {
        StopAutoUpdate();
        if (_instance == this)
        {
            _instance = null;
        }
    }
}