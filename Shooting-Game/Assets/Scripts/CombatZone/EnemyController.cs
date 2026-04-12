using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static event System.Action<int> OnEnemyKilled;
    public int scoreValue = 50;

    bool _dead;

    public void Die()
    {
        if (_dead) return;
        _dead = true;
        OnEnemyKilled?.Invoke(scoreValue);
        CombatZoneDebugLog.Log("Enemy died", $"{name} (+{scoreValue})");
        gameObject.SetActive(false);
    }

    /// <summary>Used when time runs out — hide living enemies without scoring.</summary>
    public void HideForTimeUp()
    {
        if (_dead) return;
        gameObject.SetActive(false);
    }

    /// <summary>Reset for a new round from the start panel (Try Again / new game).</summary>
    public void ResetForNewRound()
    {
        _dead = false;
        gameObject.SetActive(true);
    }
}
