using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int _score;

    void Awake() { Instance = this; }

    void OnEnable()  { EnemyController.OnEnemyKilled += AddScore; }
    void OnDisable() { EnemyController.OnEnemyKilled -= AddScore; }

    public void AddScore(int val)
    {
        _score += val;
        UIManager.Instance?.UpdateScore(_score);
    }

    public int  GetScore() => _score;
    public void Reset()    { _score = 0; UIManager.Instance?.UpdateScore(0); }
}
