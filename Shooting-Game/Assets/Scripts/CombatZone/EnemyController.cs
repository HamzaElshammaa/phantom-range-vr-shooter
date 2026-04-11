using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static event System.Action<int> OnEnemyKilled;
    public int scoreValue = 50;

public void Die()
    {
        OnEnemyKilled?.Invoke(scoreValue);
        gameObject.SetActive(false);
    }
}
