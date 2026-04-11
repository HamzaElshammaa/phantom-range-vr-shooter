#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class CombatZoneSetup
{
    // ─────────────────────────────────────────────────────────────────
    //  STEP 1 – Write all game scripts to disk
    // ─────────────────────────────────────────────────────────────────
    [MenuItem("Tools/CombatZone/1 - Write Scripts")]
    public static void WriteScripts()
    {
        string dir = "Assets/Scripts/CombatZone";
        Directory.CreateDirectory(dir);

        File.WriteAllText(dir + "/GameManager.cs", @"using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int totalEnemies = 5;
    int killed; bool over;
    void Awake()    { Instance = this; }
    void OnEnable() { EnemyController.OnEnemyKilled += OnKill; }
    void OnDisable(){ EnemyController.OnEnemyKilled -= OnKill; }
    public void StartGame(float sec)
    {
        killed = 0; over = false;
        ScoreManager.Instance?.Reset();
        TimerManager.Instance?.StartTimer(sec);
        UIManager.Instance?.ShowHUD();
    }
    void OnKill(int s)
    {
        if (over) return;
        killed++;
        if (killed >= totalEnemies) { over = true; UIManager.Instance?.ShowEndScreen(true, ScoreManager.Instance?.GetScore() ?? 0); }
    }
    public void OnTimerEnd()
    {
        if (over) return;
        over = true;
        UIManager.Instance?.ShowEndScreen(false, ScoreManager.Instance?.GetScore() ?? 0);
    }
}");

        File.WriteAllText(dir + "/ScoreManager.cs", @"using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    int score;
    void Awake()    { Instance = this; }
    void OnEnable() { EnemyController.OnEnemyKilled += Add; }
    void OnDisable(){ EnemyController.OnEnemyKilled -= Add; }
    void Add(int pts) { score += pts; if (scoreText) scoreText.text = ""Score: "" + score; }
    public int GetScore() { return score; }
    public void Reset()   { score = 0; if (scoreText) scoreText.text = ""Score: 0""; }
}");

        File.WriteAllText(dir + "/TimerManager.cs", @"using UnityEngine;
using TMPro;
public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    public TextMeshProUGUI timerText;
    float remaining, startSec;
    bool running;
    float flickT;
    bool flickOn;
    void Awake() { Instance = this; }
    public void StartTimer(float sec)
    {
        remaining = sec; startSec = sec; running = true; flickT = 0; flickOn = false;
        Paint(Color.white);
    }
    public void StopTimer() { running = false; }
    public void ShowWin()   { running = false; Paint(new Color(0.1f,1f,0.2f)); }
    public void ShowLose()  { running = false; Paint(Color.red); }
    void Update()
    {
        if (!running) return;
        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0; running = false;
            Paint(Color.red);
            GameManager.Instance?.OnTimerEnd();
            return;
        }
        if (remaining <= startSec * 0.5f)
        {
            flickT += Time.deltaTime;
            if (flickT >= 0.3f) { flickT = 0; flickOn = !flickOn; }
            Paint(flickOn ? new Color(1f,0.9f,0f) : new Color(1f,0.4f,0f));
        }
        else Paint(Color.white);
    }
    void Paint(Color c)
    {
        if (!timerText) return;
        int m = Mathf.FloorToInt(remaining/60f);
        int s = Mathf.FloorToInt(remaining%60f);
        timerText.text  = m.ToString(""00"") + "":"" + s.ToString(""00"");
        timerText.color = c;
    }
}");

        File.WriteAllText(dir + "/UIManager.cs", @"using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header(""Panels"")]
    public GameObject startScreenPanel, gameHUDPanel, endScreenPanel;
    [Header(""Start Buttons"")]
    public Button easyButton, mediumButton, hardButton, startButton;
    [Header(""HUD"")]
    public TextMeshProUGUI timerText, scoreText;
    [Header(""End"")]
    public TextMeshProUGUI resultText, finalScoreText;
    public Button celebrateButton;
    float selTime = 600f;
    static readonly Color SEL   = new Color(0.1f,0.7f,0.1f);
    static readonly Color UNSEL = new Color(0.18f,0.18f,0.18f);
    void Awake() { Instance = this; }
    void Start()
    {
        ShowStartScreen();
        easyButton?.onClick.AddListener(()   => Pick(600f, easyButton));
        mediumButton?.onClick.AddListener(() => Pick(420f, mediumButton));
        hardButton?.onClick.AddListener(()   => Pick(240f, hardButton));
        startButton?.onClick.AddListener(()  => GameManager.Instance?.StartGame(selTime));
        celebrateButton?.onClick.AddListener(() => CelebrationManager.Instance?.Celebrate());
        Pick(600f, easyButton);
    }
    void Pick(float t, Button b)
    {
        selTime = t;
        Tint(easyButton,   b == easyButton);
        Tint(mediumButton, b == mediumButton);
        Tint(hardButton,   b == hardButton);
    }
    void Tint(Button b, bool on) { if (b) b.image.color = on ? SEL : UNSEL; }
    public void ShowStartScreen()
    {
        startScreenPanel?.SetActive(true);
        gameHUDPanel?.SetActive(false);
        endScreenPanel?.SetActive(false);
    }
    public void ShowHUD()
    {
        startScreenPanel?.SetActive(false);
        gameHUDPanel?.SetActive(true);
        endScreenPanel?.SetActive(false);
        if (TimerManager.Instance) TimerManager.Instance.timerText = timerText;
        if (ScoreManager.Instance) ScoreManager.Instance.scoreText = scoreText;
        if (scoreText) scoreText.text = ""Score: 0"";
    }
    public void ShowEndScreen(bool won, int score)
    {
        startScreenPanel?.SetActive(false);
        gameHUDPanel?.SetActive(false);
        endScreenPanel?.SetActive(true);
        if (resultText)    { resultText.text = won ? ""YOU WIN!"" : ""TIME'S UP!""; resultText.color = won ? new Color(0.1f,1f,0.2f) : Color.red; }
        if (finalScoreText)  finalScoreText.text = ""Score: "" + score + "" / 250"";
        if (won) TimerManager.Instance?.ShowWin();  else TimerManager.Instance?.ShowLose();
        if (celebrateButton) celebrateButton.gameObject.SetActive(won);
    }
}");

        File.WriteAllText(dir + "/EnemyController.cs", @"using UnityEngine;
using System.Collections;
public class EnemyController : MonoBehaviour
{
    public static System.Action<int> OnEnemyKilled;
    public int scoreValue = 50;
    bool dead;
    void Start() { tag = ""Enemy""; }
    public void TakeDamage()
    {
        if (dead) return;
        dead = true;
        OnEnemyKilled?.Invoke(scoreValue);
        StartCoroutine(Die());
    }
    IEnumerator Die()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            foreach (var m in r.materials) m.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        Destroy(gameObject);
    }
}");

        File.WriteAllText(dir + "/BulletController.cs", @"using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class BulletController : MonoBehaviour
{
    public float speed = 80f;
    public float life  = 5f;
    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, life);
    }
    void OnCollisionEnter(Collision c)
    {
        var e = c.gameObject.GetComponent<EnemyController>()
             ?? c.gameObject.GetComponentInParent<EnemyController>();
        e?.TakeDamage();
        Destroy(gameObject);
    }
}");

        File.WriteAllText(dir + "/GunController.cs", @"using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform  muzzlePoint;
    public float      fireRate = 0.25f;
    XRGrabInteractable grab;
    bool held; float nextFire;
    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        if (!grab) return;
        grab.selectEntered.AddListener(_ => held = true);
        grab.selectExited.AddListener(_  => held = false);
        grab.activated.AddListener(_ =>
        {
            if (!held || !bulletPrefab || !muzzlePoint) return;
            if (Time.time < nextFire) return;
            nextFire = Time.time + fireRate;
            Fire();
        });
    }
    void Fire()
    {
        var b = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
        foreach (var gc in GetComponentsInChildren<Collider>())
            foreach (var bc in b.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(gc, bc, true);
    }
}");

        File.WriteAllText(dir + "/CelebrationManager.cs", @"using UnityEngine;
public class CelebrationManager : MonoBehaviour
{
    public static CelebrationManager Instance;
    public ParticleSystem confettiEffect;
    void Awake() { Instance = this; }
    public void Celebrate()
    {
        if (confettiEffect) { confettiEffect.Stop(); confettiEffect.Play(); return; }
        Burst();
    }
    void Burst()
    {
        Color[] cols = { Color.red, Color.yellow, Color.cyan, Color.green, Color.magenta, Color.white };
        for (int i = 0; i < 50; i++)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.position   = transform.position + Vector3.up * 1.8f + Random.insideUnitSphere * 0.5f;
            p.transform.localScale = Vector3.one * 0.07f;
            Object.Destroy(p.GetComponent<Collider>());
            var mat = new Material(Shader.Find(""Standard""));
            mat.color = cols[Random.Range(0, cols.Length)];
            p.GetComponent<Renderer>().material = mat;
            var rb = p.AddComponent<Rigidbody>();
            rb.linearVelocity = new Vector3(Random.Range(-4f,4f), Random.Range(5f,11f), Random.Range(-4f,4f));
            Object.Destroy(p, 4f);
        }
    }
}");

        AssetDatabase.Refresh();
        Debug.Log("[CombatZone] All scripts written. Wait for compile, then run Step 2.");
    }

    // ─────────────────────────────────────────────────────────────────
    //  STEP 2 – Build UI + Bullet Prefab + Wire Everything
    // ─────────────────────────────────────────────────────────────────
    [MenuItem("Tools/CombatZone/2 - Build UI and Wire Scene")]
    public static void BuildUIAndWire()
    {
        // ── Delete old canvases ──────────────────────────────────────
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.name == "GameCanvas") Object.DestroyImmediate(c.gameObject);

        // ── Canvas (World Space, in front of player) ─────────────────
        var canvasGO = new GameObject("GameCanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        var crt = canvasGO.GetComponent<RectTransform>();
        crt.sizeDelta        = new Vector2(1200f, 860f);
        canvasGO.transform.position   = new Vector3(0f, 1.6f, 2f);
        canvasGO.transform.rotation   = Quaternion.Euler(0f, 180f, 0f);
        canvasGO.transform.localScale = Vector3.one * 0.002f;

        // ── START SCREEN ─────────────────────────────────────────────
        var startPanel = Panel(canvasGO, "StartScreenPanel", new Color(0.04f,0.04f,0.14f,0.95f));

        var title = TMP(startPanel,"TitleText","COMBAT ZONE",82,FontStyles.Bold,new Vector2(0,305),new Vector2(1100,120));
        title.color = new Color(1f,0.85f,0.1f);

        var instr = TMP(startPanel,"InstructionsText","Pick up an M1911  ·  Shoot all 5 enemies to win!\nGRIP = grab    TRIGGER = shoot",30,FontStyles.Normal,new Vector2(0,195),new Vector2(1060,90));
        instr.color = new Color(0.82f,0.82f,0.82f);

        var dlbl = TMP(startPanel,"DiffLabel","— Select Difficulty —",33,FontStyles.Bold,new Vector2(0,100),new Vector2(700,55));
        dlbl.color = Color.white;

        var easyBtn   = Btn(startPanel,"EasyButton",  "EASY\n10 min", new Vector2(-335,-15),new Vector2(285,100),new Color(0.07f,0.52f,0.07f),new Color(0.75f,1f,0.75f),28);
        var medBtn    = Btn(startPanel,"MediumButton","MEDIUM\n7 min",new Vector2(0,   -15),new Vector2(285,100),new Color(0.60f,0.42f,0f),   new Color(1f,0.95f,0.55f),28);
        var hardBtn   = Btn(startPanel,"HardButton",  "HARD\n4 min",  new Vector2(335, -15),new Vector2(285,100),new Color(0.58f,0.04f,0.04f),new Color(1f,0.6f,0.6f),  28);
        var startBtn  = Btn(startPanel,"StartButton", "▶  START GAME",new Vector2(0,-155), new Vector2(440,110),new Color(0.07f,0.36f,0.80f),Color.white,40);

        // ── HUD ───────────────────────────────────────────────────────
        var hudPanel = Panel(canvasGO,"GameHUDPanel",new Color(0,0,0,0));
        hudPanel.SetActive(false);

        var timerBG  = PanelSized(hudPanel,"TimerBG", new Color(0,0,0,0.65f),new Vector2(0,355),  new Vector2(310,72));
        var timerTxt = TMP(timerBG,"TimerText","10:00",50,FontStyles.Bold,Vector2.zero,new Vector2(290,62));
        timerTxt.color = Color.white;

        var scoreBG  = PanelSized(hudPanel,"ScoreBG", new Color(0,0,0,0.65f),new Vector2(-455,355),new Vector2(290,62));
        var scoreTxt = TMP(scoreBG,"ScoreText","Score: 0",36,FontStyles.Bold,Vector2.zero,new Vector2(270,52));
        scoreTxt.color = new Color(1f,0.92f,0.15f);

        // ── END SCREEN ────────────────────────────────────────────────
        var endPanel   = Panel(canvasGO,"EndScreenPanel",new Color(0.04f,0.04f,0.14f,0.96f));
        endPanel.SetActive(false);

        var resultTxt  = TMP(endPanel,"ResultText",  "YOU WIN!",    92,FontStyles.Bold,  new Vector2(0, 240),new Vector2(950,145));
        resultTxt.color = new Color(0.1f,1f,0.2f);

        var finalTxt   = TMP(endPanel,"FinalScoreText","Score: 0 / 250",50,FontStyles.Normal,new Vector2(0,70), new Vector2(750,85));
        finalTxt.color = Color.white;

        var celebBtn   = Btn(endPanel,"CelebrateButton","🎉  CELEBRATE!",new Vector2(0,-100),new Vector2(440,110),new Color(0.48f,0.07f,0.66f),Color.white,38);

        // ── Bullet Prefab ─────────────────────────────────────────────
        Directory.CreateDirectory("Assets/Prefabs");
        string bulletPath = "Assets/Prefabs/Bullet.prefab";

        // Build bullet in scene first
        var bGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bGO.name = "Bullet";
        bGO.transform.localScale = Vector3.one * 0.05f;
        bGO.tag = "Bullet";

        // Emissive yellow-orange material
        var bMat = new Material(Shader.Find("Standard"));
        bMat.color = new Color(1f, 0.55f, 0f);
        bMat.SetFloat("_Metallic", 0f);
        bMat.SetFloat("_Smoothness", 0.8f);
        bMat.EnableKeyword("_EMISSION");
        bMat.SetColor("_EmissionColor", new Color(2f, 0.9f, 0f));
        bGO.GetComponent<MeshRenderer>().material = bMat;

        // Physics
        var bRb = bGO.GetComponent<Rigidbody>() ?? bGO.AddComponent<Rigidbody>();
        bRb.useGravity = false;
        bRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // SphereCollider already on primitive – just set it
        var bCol = bGO.GetComponent<SphereCollider>();
        bCol.radius = 0.5f;

        // Add BulletController — script must have compiled first
        var bcType = System.Type.GetType("BulletController");
        if (bcType != null) bGO.AddComponent(bcType);
        else Debug.LogWarning("[CombatZone] BulletController not compiled yet — run Step 1 first and wait for compile.");

        // Save as prefab
        var bulletPrefabAsset = PrefabUtility.SaveAsPrefabAsset(bGO, bulletPath);
        Object.DestroyImmediate(bGO);
        Debug.Log("[CombatZone] Bullet prefab saved: " + bulletPath);

        // ── Wire UIManager ────────────────────────────────────────────
        var gmGO = GameObject.Find("GameManagers");
        if (gmGO == null) { Debug.LogWarning("[CombatZone] GameManagers not found!"); return; }

        var ui = gmGO.GetComponent<UIManager>();
        if (ui == null) { Debug.LogWarning("[CombatZone] UIManager not on GameManagers!"); return; }

        ui.startScreenPanel = startPanel;
        ui.gameHUDPanel     = hudPanel;
        ui.endScreenPanel   = endPanel;
        ui.easyButton       = easyBtn;
        ui.mediumButton     = medBtn;
        ui.hardButton       = hardBtn;
        ui.startButton      = startBtn;
        ui.timerText        = timerTxt;
        ui.scoreText        = scoreTxt;
        ui.resultText       = resultTxt;
        ui.finalScoreText   = finalTxt;
        ui.celebrateButton  = celebBtn;
        EditorUtility.SetDirty(gmGO);

        // ── Wire GunControllers ───────────────────────────────────────
        var guns = Object.FindObjectsByType<GunController>(FindObjectsSortMode.None);
        Debug.Log("[CombatZone] Found " + guns.Length + " GunController(s)");
        foreach (var gun in guns)
        {
            gun.bulletPrefab = bulletPrefabAsset;
            var mp = gun.transform.Find("MuzzlePoint");
            if (mp != null) gun.muzzlePoint = mp;
            else Debug.LogWarning("[CombatZone] No MuzzlePoint on " + gun.name);
            EditorUtility.SetDirty(gun.gameObject);
        }

        // ── Save scene ────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[CombatZone] Step 2 DONE — UI built, bullet prefab created, everything wired!");
        Selection.activeGameObject = canvasGO;
    }

    // ─── Helpers ──────────────────────────────────────────────────────
    static GameObject Panel(GameObject parent, string name, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = bg;
        return go;
    }

    static GameObject PanelSized(GameObject parent, string name, Color bg, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = bg;
        return go;
    }

    static TextMeshProUGUI TMP(GameObject parent, string name, string text, float fs, FontStyles style, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<CanvasRenderer>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fs; t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        return t;
    }

    static Button Btn(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color bg, Color tc, float fs)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = bg;
        var btn = go.AddComponent<Button>();
        var lgo = new GameObject("Label");
        lgo.transform.SetParent(go.transform, false);
        var lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(5,3); lrt.offsetMax = new Vector2(-5,-3);
        lgo.AddComponent<CanvasRenderer>();
        var t = lgo.AddComponent<TextMeshProUGUI>();
        t.text = label; t.fontSize = fs; t.color = tc;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.enableWordWrapping = false;
        return btn;
    }
}
#endif
