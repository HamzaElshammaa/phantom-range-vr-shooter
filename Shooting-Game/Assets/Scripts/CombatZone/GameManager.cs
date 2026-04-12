using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int totalEnemies = 5;
    private int  _killed = 0;
    private bool _over   = false;

    void Awake()     { Instance = this; }
    void OnEnable()  { EnemyController.OnEnemyKilled += OnKill; }
    void OnDisable() { EnemyController.OnEnemyKilled -= OnKill; }

    public void StartGame(float seconds)
    {
        _killed = 0;
        _over   = false;
        ScoreManager.Instance?.Reset();
        UIManager.Instance?.BeginRound(seconds);
        TimerManager.Instance?.StartTimer(seconds);
        UIManager.Instance?.ShowHUD();
        CombatZoneDebugLog.Log("Game started", $"{seconds}s round, {totalEnemies} enemies");
    }

    void OnKill(int score)
    {
        if (_over) return;
        _killed++;
        UIManager.Instance?.SetHudAnnouncement($"Enemies left: {totalEnemies - _killed}");
        CombatZoneDebugLog.Log("Enemy eliminated", $"{_killed}/{totalEnemies} (+{score} pts)");
        if (_killed >= totalEnemies)
        {
            _over = true;
            TimerManager.Instance?.Stop();
            UIManager.Instance?.ShowEndScreen(true, ScoreManager.Instance?.GetScore() ?? 0);
        }
    }

    public void OnTimerEnd()
    {
        if (_over) return;
        _over = true;
        foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            e.HideForTimeUp();
        UIManager.Instance?.ShowEndScreen(false, ScoreManager.Instance?.GetScore() ?? 0);
    }

    /// <summary>Try Again: back to start panel, reset enemies and score. Timer already stopped.</summary>
    public void ResetToStartMenu()
    {
        _killed = 0;
        _over   = false;
        foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            e.ResetForNewRound();
        ScoreManager.Instance?.Reset();
        TimerManager.Instance?.Stop();
        UIManager.Instance?.ShowStart();
        CombatZoneDebugLog.Log("Session reset", "Start panel");
    }
}
