using UnityEngine;
using TMPro;

/// <summary>
/// Holds optional HUD text references. Timer and score are driven by TimerManager / ScoreManager via UIManager.
/// </summary>
public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
}
