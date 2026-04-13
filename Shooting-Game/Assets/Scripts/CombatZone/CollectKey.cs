using UnityEngine;

public class CollectKey : MonoBehaviour
{
    public int scoreValue = 100;

    public void Collect()
    {
        ScoreManager.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}