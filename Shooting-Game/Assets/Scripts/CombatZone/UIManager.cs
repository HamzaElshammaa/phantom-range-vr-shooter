using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject startScreenPanel, gameHUDPanel, endScreenPanel;
    public Button easyButton, mediumButton, hardButton, startButton, celebrateButton;
    public TMP_Text timerText, scoreText, resultText, finalScoreText;
    [Tooltip("Start panel line that shows selected round length (e.g. DifficultyLabel).")]
    public TMP_Text startScreenRoundInfo;

    float _sel = 420f;
    int _difficultyIndex = 1;
    float _roundTotalSeconds = 1f;

    static readonly Color TimerColorNormal = Color.white;
    static readonly Color TimerColorWarn = new Color(1f, 0.92f, 0.2f);
    static readonly Color TimerColorCrit = new Color(1f, 0.25f, 0.2f);

    void Awake() { Instance = this; }

    void Start()
    {
        if (easyButton)      easyButton.onClick.AddListener(OnEasy);
        if (mediumButton)    mediumButton.onClick.AddListener(OnMedium);
        if (hardButton)      hardButton.onClick.AddListener(OnHard);
        if (startButton)     startButton.onClick.AddListener(OnStart);
        Panels(true, false, false);
        Tint(_difficultyIndex);
        RefreshStartScreenRoundInfo();
        WireEndScreenButtonForMenu();
        CombatZoneDebugLog.Log("UI init", "Start screen visible");
        var audioMgr = CombatZoneAudioManager.Instance;
        if (audioMgr == null)
            audioMgr = Object.FindFirstObjectByType<CombatZoneAudioManager>();
        audioMgr?.BeginBackgroundMusicAfterUiReady();
    }

    void WireEndScreenButtonForMenu()
    {
        if (!celebrateButton) return;
        celebrateButton.onClick.RemoveAllListeners();
        celebrateButton.onClick.AddListener(OnEndScreenPrimaryButton);
    }

    void OnEndScreenPrimaryButton()
    {
        if (_lastEndScreenWasWin)
            OnCelebrate();
        else
            OnTryAgain();
    }

    bool _lastEndScreenWasWin;

    void OnEasy()
    {
        _sel = 600f;
        _difficultyIndex = 0;
        Tint(0);
        RefreshStartScreenRoundInfo();
        CombatZoneDebugLog.Log("Difficulty selected", "Easy — 10:00");
    }

    void OnMedium()
    {
        _sel = 420f;
        _difficultyIndex = 1;
        Tint(1);
        RefreshStartScreenRoundInfo();
        CombatZoneDebugLog.Log("Difficulty selected", "Medium — 7:00");
    }

    void OnHard()
    {
        _sel = 240f;
        _difficultyIndex = 2;
        Tint(2);
        RefreshStartScreenRoundInfo();
        CombatZoneDebugLog.Log("Difficulty selected", "Hard — 4:00");
    }

    void OnStart()
    {
        if (!GameManager.Instance) return;
        CombatZoneDebugLog.Log("Start pressed", $"{FormatMmSs(_sel)} round");
        GameManager.Instance.StartGame(_sel);
    }

    void OnCelebrate()
    {
        if (CelebrationManager.Instance)
            CelebrationManager.Instance.Celebrate();
    }

    void OnTryAgain()
    {
        GameManager.Instance?.ResetToStartMenu();
    }

    public void BeginRound(float totalSeconds)
    {
        _roundTotalSeconds = Mathf.Max(1f, totalSeconds);
        EnsureHudMissionBanner();
        int n = GameManager.Instance != null ? GameManager.Instance.totalEnemies : 5;
        SetHudAnnouncement($"Eliminate {n} enemies before the timer reaches 0:00.");
    }

    public void ShowHUD()
    {
        Panels(false, true, false);
    }

    public void ShowStart()
    {
        Panels(true, false, false);
        if (timerText)
        {
            timerText.text = "--:--";
            timerText.color = TimerColorNormal;
        }
    }

    public void ShowEndScreen(bool won, int score)
    {
        _lastEndScreenWasWin = won;
        Panels(false, false, true);

        if (resultText)
        {
            resultText.text = won ? "YOU WIN!" : "YOU LOSE";
            resultText.color = won ? new Color(0.15f, 1f, 0.25f) : new Color(1f, 0.35f, 0.25f);
        }
        if (finalScoreText)
            finalScoreText.text = "Score: " + score;

        if (celebrateButton)
        {
            celebrateButton.onClick.RemoveAllListeners();
            celebrateButton.onClick.AddListener(OnEndScreenPrimaryButton);
            var label = celebrateButton.GetComponentInChildren<TMP_Text>(true);
            if (label)
                label.text = won ? "CELEBRATE!" : "TRY AGAIN";
            celebrateButton.gameObject.SetActive(true);
        }

        if (won)
            CombatZoneAudioManager.Instance?.PlayVictory();
        else
            CombatZoneAudioManager.Instance?.PlaySadTrombone();

        CombatZoneDebugLog.Log(won ? "End — win" : "End — lose", $"score={score}");
    }

    public void UpdateTimer(float t)
    {
        if (!timerText) return;
        int sec = Mathf.Max(0, Mathf.CeilToInt(t));
        int m = sec / 60;
        int s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";

        float total = Mathf.Max(_roundTotalSeconds, 1f);
        float ratio = t / total;
        if (ratio <= 0.25f)
            timerText.color = TimerColorCrit;
        else if (ratio <= 0.5f)
            timerText.color = TimerColorWarn;
        else
            timerText.color = TimerColorNormal;
    }

    public void UpdateScore(int v)
    {
        if (scoreText) scoreText.text = "Score: " + v;
        CombatZoneDebugLog.Log("Score updated", v.ToString());
    }

    TMP_Text _hudMissionBanner;

    void EnsureHudMissionBanner()
    {
        if (_hudMissionBanner != null) return;
        if (!gameHUDPanel) return;
        var t = gameHUDPanel.transform.Find("HUDMissionBanner");
        if (t != null)
        {
            _hudMissionBanner = t.GetComponent<TMP_Text>();
            return;
        }
        var go = new GameObject("HUDMissionBanner");
        go.transform.SetParent(gameHUDPanel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -40f);
        rt.sizeDelta = new Vector2(920f, 72f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 26f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.75f);
        tmp.enableWordWrapping = true;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        _hudMissionBanner = tmp;
    }

    public void SetHudAnnouncement(string msg)
    {
        EnsureHudMissionBanner();
        if (_hudMissionBanner) _hudMissionBanner.text = msg;
    }

    void Panels(bool a, bool b, bool c)
    {
        if (startScreenPanel) startScreenPanel.SetActive(a);
        if (gameHUDPanel)     gameHUDPanel.SetActive(b);
        if (endScreenPanel)   endScreenPanel.SetActive(c);
    }

    void Tint(int n)
    {
        Color on = new Color(1f, 0.6f, 0f), off = Color.white;
        if (easyButton)   easyButton.image.color   = n == 0 ? on : off;
        if (mediumButton) mediumButton.image.color = n == 1 ? on : off;
        if (hardButton)   hardButton.image.color   = n == 2 ? on : off;
    }

    void RefreshStartScreenRoundInfo()
    {
        if (!startScreenRoundInfo) return;
        string name = _difficultyIndex == 0 ? "Easy" : (_difficultyIndex == 1 ? "Medium" : "Hard");
        startScreenRoundInfo.text = $"- Select Difficulty ({name}) -\nRound length: {FormatMmSs(_sel)}";
    }

    static string FormatMmSs(float seconds)
    {
        int sec = Mathf.Max(0, Mathf.RoundToInt(seconds));
        return $"{sec / 60:00}:{sec % 60:00}";
    }
}
