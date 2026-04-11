using UnityEngine;
using TMPro;
using System.Reflection;

public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    private GameManager gm;
    private ScoreManager sm;
    private FieldInfo tf;

    void Start()
    {
        gm = GameManager.Instance;
        sm = ScoreManager.Instance;
        if (gm != null)
            tf = typeof(GameManager).GetField("timeRemaining",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void Update()
    {
        if (timerText != null && gm != null)
        {
            float t = tf != null ? (float)tf.GetValue(gm) : Time.timeSinceLevelLoad;
            timerText.SetText("{0:00}:{1:00}", Mathf.FloorToInt(t / 60f), Mathf.FloorToInt(t % 60f));
        }
        if (scoreText != null && sm != null)
        {
            scoreText.SetText("Score  {0}", sm.GetScore());
        }
    }
}
