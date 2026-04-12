using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    private float _remaining;
    private bool  _running;

    public float RemainingSeconds => _remaining;
    public bool  IsRunning        => _running;

    void Awake() { Instance = this; }

    public void StartTimer(float seconds)
    {
        _remaining = seconds;
        _running   = true;
        UIManager.Instance?.UpdateTimer(_remaining);
        CombatZoneDebugLog.Log("Timer started", $"{seconds}s remaining");
    }

    void Update()
    {
        if (!_running) return;
        _remaining -= Time.deltaTime;
        UIManager.Instance?.UpdateTimer(_remaining);
        if (_remaining <= 0f)
        {
            _running = false;
            CombatZoneDebugLog.Log("Timer expired");
            GameManager.Instance?.OnTimerEnd();
        }
    }

    public void Stop()
    {
        _running = false;
        CombatZoneDebugLog.Log("Timer stopped", $"{_remaining:F1}s left");
    }
}
