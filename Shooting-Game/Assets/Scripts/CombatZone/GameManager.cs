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
        TimerManager.Instance?.StartTimer(seconds);
        UIManager.Instance?.ShowHUD();
    }

    void OnKill(int score)
    {
        if (_over) return;
        _killed++;
        if (_killed >= totalEnemies)
        {
            _over = true;
            UIManager.Instance?.ShowEndScreen(true, ScoreManager.Instance?.GetScore() ?? 0);
        }
    }

    public void OnTimerEnd()
    {
        if (_over) return;
        _over = true;
        UIManager.Instance?.ShowEndScreen(false, ScoreManager.Instance?.GetScore() ?? 0);
    }
}
