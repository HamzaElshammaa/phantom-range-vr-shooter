using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    private float _remaining;
    private bool  _running;

    void Awake() { Instance = this; }

    public void StartTimer(float seconds)
    {
        _remaining = seconds;
        _running   = true;
    }

    void Update()
    {
        if (!_running) return;
        _remaining -= Time.deltaTime;
        UIManager.Instance?.UpdateTimer(_remaining);
        if (_remaining <= 0f)
        {
            _running = false;
            GameManager.Instance?.OnTimerEnd();
        }
    }

    public void Stop() { _running = false; }
}
