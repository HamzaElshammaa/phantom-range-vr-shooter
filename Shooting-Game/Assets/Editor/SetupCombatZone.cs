using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class SetupCombatZone : EditorWindow
{
    [MenuItem("Tools/Setup CombatZone NOW")]
    static void Run()
    {
        // ── 1. GAME MANAGERS ────────────────────────────────────────
        GameObject mgr = GameObject.Find("GameManagers") ?? new GameObject("GameManagers");
        if (!mgr.GetComponent<GameManager>()) mgr.AddComponent<GameManager>();
        ScoreManager sm = mgr.GetComponent<ScoreManager>() ?? mgr.AddComponent<ScoreManager>();
        TimerManager tm = mgr.GetComponent<TimerManager>() ?? mgr.AddComponent<TimerManager>();
        UIManager um = mgr.GetComponent<UIManager>() ?? mgr.AddComponent<UIManager>();
        CelebrationManager cm = mgr.GetComponent<CelebrationManager>() ?? mgr.AddComponent<CelebrationManager>();

        // ── 2. CANVAS ───────────────────────────────────────────────
        GameObject canvasGO = GameObject.Find("GameCanvas") ?? new GameObject("GameCanvas");
        Canvas canvas = canvasGO.GetComponent<Canvas>() ?? canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (!canvasGO.GetComponent<CanvasScaler>()) canvasGO.AddComponent<CanvasScaler>();
        if (!canvasGO.GetComponent<GraphicRaycaster>()) canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.transform.position = new Vector3(0f, 1.6f, 3f);
        canvasGO.transform.localScale = Vector3.one * 0.002f;
        RectTransform cr = canvasGO.GetComponent<RectTransform>();
        cr.sizeDelta = new Vector2(800, 600);

        // ── 3. PANELS ───────────────────────────────────────────────
        GameObject startPanel = MakePanel(canvasGO, "StartScreenPanel", new Color(0.05f, 0.05f, 0.1f, 0.95f));
        GameObject hudPanel   = MakePanel(canvasGO, "GameHUDPanel",     new Color(0, 0, 0, 0f));
        GameObject endPanel   = MakePanel(canvasGO, "EndScreenPanel",   new Color(0.05f, 0.05f, 0.1f, 0.95f));

        // ── START SCREEN ─────────────────────────────────────────────
        MakeText(startPanel,  "TitleText",        "Pick up your weapon",                    58, new Vector2(0, 200),  new Vector2(760, 80));
        MakeText(startPanel,  "InstructionsText", "Shoot all enemies before time runs out", 28, new Vector2(0, 130),  new Vector2(760, 50));
        Button easyBtn   = MakeButton(startPanel, "EasyButton",   "Easy\n10 min",   new Vector2(-240, 20), new Vector2(180, 80), new Color(0.15f, 0.6f, 0.15f));
        Button medBtn    = MakeButton(startPanel, "MediumButton", "Medium\n7 min",  new Vector2(0,    20), new Vector2(180, 80), new Color(0.7f,  0.5f, 0.05f));
        Button hardBtn   = MakeButton(startPanel, "HardButton",   "Hard\n4 min",    new Vector2(240,  20), new Vector2(180, 80), new Color(0.7f,  0.1f, 0.1f));
        Button startBtn  = MakeButton(startPanel, "StartButton",  "START",          new Vector2(0, -100), new Vector2(260, 75), new Color(0.1f,  0.45f, 0.85f));

        // ── HUD ─────────────────────────────────────────────────────
        TextMeshProUGUI timerTxt = MakeText(hudPanel, "TimerText", "10:00", 80, new Vector2(0, 230), new Vector2(400, 110));
        timerTxt.color = Color.white;
        TextMeshProUGUI scoreTxt = MakeText(hudPanel, "ScoreText", "Score: 0", 42, new Vector2(0, 145), new Vector2(400, 65));
        scoreTxt.color = Color.white;
        hudPanel.SetActive(false);

        // ── END SCREEN ───────────────────────────────────────────────
        TextMeshProUGUI resultTxt    = MakeText(endPanel, "ResultText",    "You Win!",        80, new Vector2(0,  130), new Vector2(720, 110));
        TextMeshProUGUI finalScoreTxt = MakeText(endPanel, "FinalScoreText","Final Score: 0", 48, new Vector2(0,  20),  new Vector2(600, 70));
        Button celebBtn = MakeButton(endPanel, "CelebrateButton", "Celebrate! 🎉", new Vector2(0, -90), new Vector2(300, 80), new Color(0.85f, 0.35f, 0.05f));
        endPanel.SetActive(false);

        // ── 4. WIRE UI MANAGER ──────────────────────────────────────
        um.startScreenPanel = startPanel;
        um.gameHUDPanel     = hudPanel;
        um.endScreenPanel   = endPanel;
        um.easyButton       = easyBtn;
        um.mediumButton     = medBtn;
        um.hardButton       = hardBtn;
        um.startButton      = startBtn;
        um.timerText        = timerTxt;
        um.scoreText        = scoreTxt;
        um.resultText       = resultTxt;
        um.finalScoreText   = finalScoreTxt;
        um.celebrateButton  = celebBtn;

        // ── 5. BULLET PREFAB ────────────────────────────────────────
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        string bulletPath = "Assets/Prefabs/Bullet.prefab";
        GameObject bulletGO = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath) != null
            ? null
            : GameObject.CreatePrimitive(PrimitiveType.Sphere);

        GameObject bulletPrefabAsset;
        if (bulletGO != null)
        {
            bulletGO.name = "Bullet";
            bulletGO.transform.localScale = Vector3.one * 0.06f;
            bulletGO.transform.position   = new Vector3(0, -200, 0);
            bulletGO.tag = "Bullet";
            Rigidbody brb = bulletGO.GetComponent<Rigidbody>() ?? bulletGO.AddComponent<Rigidbody>();
            brb.useGravity = false;
            brb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            if (!bulletGO.GetComponent<BulletController>()) bulletGO.AddComponent<BulletController>();
            AssetDatabase.Refresh();
            bulletPrefabAsset = PrefabUtility.SaveAsPrefabAsset(bulletGO, bulletPath);
            Object.DestroyImmediate(bulletGO);
        }
        else
        {
            bulletPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPath);
        }

        // ── 6. ENEMIES ──────────────────────────────────────────────
        string[] enemies = { "Enemy 1", "Enemy 2", "Enemy 3", "Enemy 4", "Enemy 5" };
        foreach (string eName in enemies)
        {
            GameObject e = GameObject.Find(eName);
            if (e == null) continue;
            e.tag = "Enemy";
            if (!e.GetComponent<EnemyController>()) e.AddComponent<EnemyController>();
            if (!e.GetComponent<BoxCollider>())
            {
                BoxCollider bc = e.AddComponent<BoxCollider>();
                bc.isTrigger = false;
                bc.center    = new Vector3(0, 0.9f, 0);
                bc.size      = new Vector3(0.6f, 1.8f, 0.5f);
            }
        }

        // ── 7. GUNS ─────────────────────────────────────────────────
        string[] guns = { "M1911", "M1911 (1)", "M1911 (2)" };
        foreach (string gName in guns)
        {
            GameObject g = GameObject.Find(gName);
            if (g == null) continue;
            GunShooter gc = g.GetComponent<GunShooter>() ?? g.AddComponent<GunShooter>();
            // bulletPrefab removed — GunShooter uses raycasting
            Transform muzzle = g.transform.Find("MuzzlePoint");
            if (muzzle == null)
            {
                GameObject mp = new GameObject("MuzzlePoint");
                mp.transform.SetParent(g.transform, false);
                mp.transform.localPosition = new Vector3(0, 0, 0.25f);
                muzzle = mp.transform;
            }
            gc.muzzlePoint = muzzle;
        }

        // ── 8. CONFETTI ─────────────────────────────────────────────
        GameObject confetti = GameObject.Find("Confetti");
        if (confetti == null)
        {
            confetti = new GameObject("Confetti");
            confetti.transform.position = new Vector3(0, 3f, 0);
            ParticleSystem ps = confetti.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.cyan);
            main.maxParticles = 200;
            var em = ps.emission;
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 200) });
            cm.confettiSystem = confetti.GetComponent<ParticleSystem>();
        }

        // ── 9. SAVE ─────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        EditorUtility.DisplayDialog("CombatZone Ready!",
            "All done!\n\n" +
            "✅ GameManagers (GM + Score + Timer + UI + Celebration)\n" +
            "✅ World-Space Canvas @ (0, 1.6, 3)\n" +
            "✅ StartScreen | HUD | EndScreen panels\n" +
            "✅ Bullet prefab  →  Assets/Prefabs/Bullet.prefab\n" +
            "✅ Enemy 1-5  →  EnemyController + BoxCollider\n" +
            "✅ M1911 guns  →  GunController + MuzzlePoint\n" +
            "✅ Confetti particle system\n\n" +
            "Press PLAY to test!", "Let's go!");
    }

    static GameObject MakePanel(GameObject parent, string n, Color bg)
    {
        Transform t = parent.transform.Find(n);
        if (t != null) return t.gameObject;
        GameObject p = new GameObject(n);
        p.transform.SetParent(parent.transform, false);
        RectTransform rt = p.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        p.AddComponent<Image>().color = bg;
        return p;
    }

    static TextMeshProUGUI MakeText(GameObject parent, string n, string txt, int size, Vector2 pos, Vector2 sz)
    {
        Transform t = parent.transform.Find(n);
        if (t != null) return t.GetComponent<TextMeshProUGUI>();
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = sz;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = txt; tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    static Button MakeButton(GameObject parent, string n, string label, Vector2 pos, Vector2 sz, Color col)
    {
        Transform t = parent.transform.Find(n);
        if (t != null) return t.GetComponent<Button>();
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = sz;
        go.AddComponent<Image>().color = col;
        Button btn = go.AddComponent<Button>();
        GameObject lgo = new GameObject("Label");
        lgo.transform.SetParent(go.transform, false);
        RectTransform lrt = lgo.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = lgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return btn;
    }
}
