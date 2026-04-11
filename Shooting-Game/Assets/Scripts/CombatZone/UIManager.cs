using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject startScreenPanel, gameHUDPanel, endScreenPanel;
    public Button easyButton, mediumButton, hardButton, startButton, celebrateButton;
    public TMP_Text timerText, scoreText, resultText, finalScoreText;
    float _sel = 90f;
    void Awake() { Instance = this; }
    void Start()
    {
        if (easyButton)      easyButton.onClick.AddListener(OnEasy);
        if (mediumButton)    mediumButton.onClick.AddListener(OnMedium);
        if (hardButton)      hardButton.onClick.AddListener(OnHard);
        if (startButton)     startButton.onClick.AddListener(OnStart);
        if (celebrateButton) celebrateButton.onClick.AddListener(OnCelebrate);
        Panels(true, false, false);
    }
    void OnEasy()      { _sel = 120f; Tint(0); }
    void OnMedium()    { _sel = 90f;  Tint(1); }
    void OnHard()      { _sel = 60f;  Tint(2); }
    void OnStart()     { if (GameManager.Instance) GameManager.Instance.StartGame(_sel); }
    void OnCelebrate() { if (CelebrationManager.Instance) CelebrationManager.Instance.Celebrate(); }
    public void ShowHUD() { Panels(false, true, false); }
    public void ShowStart() { Panels(true, false, false); }
    public void ShowEndScreen(bool won, int score)
    {
        Panels(false, false, true);
        if (resultText)     resultText.text     = won ? "MISSION COMPLETE!" : "TIME'S UP!";
        if (finalScoreText) finalScoreText.text = "Score: " + score;
    }
    public void UpdateTimer(float t)
    {
        if (!timerText) return;
        timerText.text  = Mathf.Max(0,(int)(t/60)).ToString("00")+":"+Mathf.Max(0,(int)(t%60)).ToString("00");
        timerText.color = t <= 10f ? Color.red : Color.white;
    }
    public void UpdateScore(int v) { if (scoreText) scoreText.text = "Score: " + v; }
    void Panels(bool a, bool b, bool c)
    {
        if (startScreenPanel) startScreenPanel.SetActive(a);
        if (gameHUDPanel)     gameHUDPanel.SetActive(b);
        if (endScreenPanel)   endScreenPanel.SetActive(c);
    }
    void Tint(int n)
    {
        Color on = new Color(1f, 0.6f, 0f), off = Color.white;
        if (easyButton)   easyButton.image.color   = n==0?on:off;
        if (mediumButton) mediumButton.image.color = n==1?on:off;
        if (hardButton)   hardButton.image.color   = n==2?on:off;
    }
}
