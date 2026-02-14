#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Editor utility to build the game scenes programmatically.
/// UI follows iOS best practices: safe area, 1080x1920 reference,
/// minimum 120px touch targets, thumb zone layout, generous padding.
/// </summary>
public class SceneBuilder : EditorWindow
{
    // Reference resolution for all canvases
    const int REF_W = 1080;
    const int REF_H = 1920;

    [MenuItem("Window/Fish Catcher/Build Scenes")]
    public static void ShowWindow()
    {
        GetWindow<SceneBuilder>("Scene Builder");
    }

    [MenuItem("Window/Fish Catcher/Build All Now")]
    public static void BuildAllDirect()
    {
        ClawSpriteGenerator.ClearCache();
        FishSpriteGenerator.ClearCache();
        BuildFishPrefab();
        BuildHomeScreenScene();
        BuildGameScene();
        Debug.Log("All scenes built successfully!");
    }

    private void OnGUI()
    {
        GUILayout.Label("Fish Catcher Scene Builder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Build Fish Prefab"))
            BuildFishPrefab();

        GUILayout.Space(5);

        if (GUILayout.Button("Build Home Screen Scene"))
            BuildHomeScreenScene();

        GUILayout.Space(5);

        if (GUILayout.Button("Build Game Scene"))
            BuildGameScene();

        GUILayout.Space(10);

        if (GUILayout.Button("Build All"))
        {
            BuildFishPrefab();
            BuildHomeScreenScene();
            BuildGameScene();
        }
    }

    // ─── FISH PREFAB ────────────────────────────────────────────

    private static void BuildFishPrefab()
    {
        GameObject fish = new GameObject("FishPrefab");

        SpriteRenderer sr = fish.AddComponent<SpriteRenderer>();
        sr.sprite = FishSpriteGenerator.GetFishSprite();
        sr.sortingOrder = 5;

        CircleCollider2D col = fish.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        Rigidbody2D rb = fish.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        fish.AddComponent<Fish>();
        fish.AddComponent<ProceduralSetup>();

        string path = "Assets/Prefabs/FishPrefab.prefab";
        EnsureDirectory("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(fish, path);
        DestroyImmediate(fish);

        Debug.Log("Fish prefab created at " + path);
    }

    // ─── HOME SCREEN ────────────────────────────────────────────

    private static void BuildHomeScreenScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.18f, 0.45f, 0.65f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0, 0, -10);
        camObj.tag = "MainCamera";

        // Water background
        GameObject water = new GameObject("Water");
        SpriteRenderer waterSr = water.AddComponent<SpriteRenderer>();
        waterSr.color = new Color(0.12f, 0.32f, 0.52f);
        waterSr.drawMode = SpriteDrawMode.Tiled;
        waterSr.sortingOrder = -1;
        water.transform.position = new Vector3(0, -4f, 0);
        water.transform.localScale = new Vector3(14f, 12f, 1f);

        // Fish container
        GameObject fishContainer = new GameObject("FishContainer");

        // Canvas (1080x1920, match height)
        GameObject canvasObj = CreateCanvas();

        // Safe Area Panel - all UI goes inside this
        GameObject safeArea = CreateSafeAreaPanel(canvasObj.transform);

        // ── Title (upper area, non-interactive) ──
        GameObject titleObj = new GameObject("TitleLabel");
        titleObj.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "FISH\nCATCHER";
        titleTMP.fontSize = 120;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -60);
        titleRT.sizeDelta = new Vector2(800, 320);

        // ── Subtitle ──
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI subTMP = subtitleObj.AddComponent<TextMeshProUGUI>();
        subTMP.text = "Catch them all!";
        subTMP.fontSize = 40;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(1f, 0.9f, 0.5f);
        RectTransform subRT = subtitleObj.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.5f, 1f);
        subRT.anchorMax = new Vector2(0.5f, 1f);
        subRT.pivot = new Vector2(0.5f, 1f);
        subRT.anchoredPosition = new Vector2(0, -400);
        subRT.sizeDelta = new Vector2(600, 60);

        // ── Play button (center-lower for thumb zone, large touch target) ──
        GameObject playBtn = CreateButton(safeArea.transform, "PlayButton", "PLAY",
            Vector2.zero, new Vector2(560, 150), 56,
            new Color(0.2f, 0.75f, 0.35f));
        RectTransform playRT = playBtn.GetComponent<RectTransform>();
        playRT.anchorMin = new Vector2(0.5f, 0.5f);
        playRT.anchorMax = new Vector2(0.5f, 0.5f);
        playRT.anchoredPosition = new Vector2(0, -100);
        Button playButton = playBtn.GetComponent<Button>();

        // ── Version label (bottom, non-interactive) ──
        GameObject versionObj = new GameObject("VersionLabel");
        versionObj.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI verTMP = versionObj.AddComponent<TextMeshProUGUI>();
        verTMP.text = "v1.2.0";
        verTMP.fontSize = 24;
        verTMP.alignment = TextAlignmentOptions.Center;
        verTMP.color = new Color(1, 1, 1, 0.35f);
        RectTransform verRT = versionObj.GetComponent<RectTransform>();
        verRT.anchorMin = new Vector2(0.5f, 0f);
        verRT.anchorMax = new Vector2(0.5f, 0f);
        verRT.pivot = new Vector2(0.5f, 0f);
        verRT.anchoredPosition = new Vector2(0, 30);
        verRT.sizeDelta = new Vector2(300, 40);

        // HomeScreen script
        GameObject homeManager = new GameObject("HomeScreenManager");
        HomeScreen hs = homeManager.AddComponent<HomeScreen>();

        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("titleLabel").objectReferenceValue = titleTMP;
        so.FindProperty("titleTransform").objectReferenceValue = titleRT;
        so.FindProperty("fishContainer").objectReferenceValue = fishContainer.transform;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FishPrefab.prefab");
        if (prefab != null)
            so.FindProperty("fishPrefab").objectReferenceValue = prefab;
        so.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(playButton.onClick,
            new UnityEngine.Events.UnityAction(hs.OnPlayPressed));

        // EventSystem
        CreateEventSystem();

        string scenePath = "Assets/Scenes/HomeScreen.unity";
        EnsureDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Home Screen scene created at " + scenePath);
    }

    // ─── GAME SCENE ─────────────────────────────────────────────

    private static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera - shifted up so hook at y=8 is visible
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.12f, 0.22f, 0.38f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0, 2f, -10);
        camObj.tag = "MainCamera";

        // Water background
        GameObject water = new GameObject("WaterArea");
        SpriteRenderer waterSr = water.AddComponent<SpriteRenderer>();
        waterSr.color = new Color(0.08f, 0.28f, 0.48f);
        waterSr.sortingOrder = -1;
        water.transform.position = new Vector3(0, -2f, 0);
        water.transform.localScale = new Vector3(14f, 16f, 1f);

        // ── DropZone (bucket) ──
        GameObject bucket = new GameObject("DropZone");
        bucket.transform.position = new Vector3(2.5f, 5f, 0);
        bucket.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        SpriteRenderer bucketSr = bucket.AddComponent<SpriteRenderer>();
        bucketSr.sprite = ClawSpriteGenerator.GetBucketSprite();
        bucketSr.sortingOrder = 3;
        BoxCollider2D bucketCol = bucket.AddComponent<BoxCollider2D>();
        bucketCol.size = new Vector2(2.5f, 2f);
        bucketCol.isTrigger = true;
        Rigidbody2D bucketRb = bucket.AddComponent<Rigidbody2D>();
        bucketRb.bodyType = RigidbodyType2D.Kinematic;
        bucketRb.gravityScale = 0f;
        DropZone dropZone = bucket.AddComponent<DropZone>();

        // "DROP" label
        GameObject dropLabel = new GameObject("DropLabel");
        dropLabel.transform.SetParent(bucket.transform);
        dropLabel.transform.localPosition = new Vector3(0, 1.5f, 0);
        TextMeshPro dropTMP = dropLabel.AddComponent<TextMeshPro>();
        dropTMP.text = "DROP";
        dropTMP.fontSize = 4;
        dropTMP.alignment = TextAlignmentOptions.Center;
        dropTMP.color = new Color(1f, 0.9f, 0.5f);
        dropTMP.sortingOrder = 10;

        // ── Fish Spawner ──
        GameObject spawnerObj = new GameObject("FishSpawner");
        spawnerObj.transform.position = Vector3.zero;
        FishSpawner spawner = spawnerObj.AddComponent<FishSpawner>();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FishPrefab.prefab");
        if (prefab != null)
        {
            SerializedObject spawnerSo = new SerializedObject(spawner);
            spawnerSo.FindProperty("fishPrefab").objectReferenceValue = prefab;
            spawnerSo.ApplyModifiedProperties();
        }

        // ── Hook (replaces claw) ──
        GameObject clawObj = new GameObject("Claw");
        clawObj.transform.position = new Vector3(0, 8f, 0);
        Claw claw = clawObj.AddComponent<Claw>();

        // Hook head
        GameObject clawHead = new GameObject("ClawHead");
        clawHead.transform.SetParent(clawObj.transform);
        clawHead.transform.localPosition = Vector3.zero;
        clawHead.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // Hook sprite
        GameObject hookSprite = new GameObject("HookSprite");
        hookSprite.transform.SetParent(clawHead.transform);
        hookSprite.transform.localPosition = Vector3.zero;
        SpriteRenderer hookSr = hookSprite.AddComponent<SpriteRenderer>();
        hookSr.sprite = ClawSpriteGenerator.GetHookSprite();
        hookSr.sortingOrder = 8;

        // Placeholder left/right (Claw script references)
        GameObject clawLeft = new GameObject("ClawLeft");
        clawLeft.transform.SetParent(clawHead.transform);
        clawLeft.transform.localPosition = Vector3.zero;

        GameObject clawRight = new GameObject("ClawRight");
        clawRight.transform.SetParent(clawHead.transform);
        clawRight.transform.localPosition = Vector3.zero;

        // Rope (fishing line)
        LineRenderer rope = clawObj.AddComponent<LineRenderer>();
        rope.positionCount = 2;
        rope.startWidth = 0.04f;
        rope.endWidth = 0.04f;
        rope.material = new Material(Shader.Find("Sprites/Default"));
        rope.startColor = new Color(0.7f, 0.7f, 0.7f);
        rope.endColor = new Color(0.7f, 0.7f, 0.7f);
        rope.useWorldSpace = false;
        rope.sortingOrder = 6;

        // Wire claw references
        SerializedObject clawSo = new SerializedObject(claw);
        clawSo.FindProperty("clawHead").objectReferenceValue = clawHead.transform;
        clawSo.FindProperty("rope").objectReferenceValue = rope;
        clawSo.FindProperty("clawLeft").objectReferenceValue = clawLeft.transform;
        clawSo.FindProperty("clawRight").objectReferenceValue = clawRight.transform;
        clawSo.ApplyModifiedProperties();

        // ── UI Canvas (1080x1920, match height) ──
        GameObject canvasObj = CreateCanvas();
        GameObject safeArea = CreateSafeAreaPanel(canvasObj.transform);

        // Score label - top-left, non-interactive, inside safe area
        GameObject scoreObj = new GameObject("ScoreLabel");
        scoreObj.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI scoreTMP = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTMP.text = "0";
        scoreTMP.fontSize = 42;
        scoreTMP.alignment = TextAlignmentOptions.Left;
        scoreTMP.color = Color.white;
        scoreTMP.fontStyle = FontStyles.Bold;
        RectTransform scoreRT = scoreObj.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0, 1);
        scoreRT.anchorMax = new Vector2(0, 1);
        scoreRT.pivot = new Vector2(0, 1);
        scoreRT.anchoredPosition = new Vector2(30, -20);
        scoreRT.sizeDelta = new Vector2(200, 60);

        // Fish icon label next to score
        GameObject fishIcon = new GameObject("FishIcon");
        fishIcon.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI fishTMP = fishIcon.AddComponent<TextMeshProUGUI>();
        fishTMP.text = "FISH";
        fishTMP.fontSize = 24;
        fishTMP.alignment = TextAlignmentOptions.Left;
        fishTMP.color = new Color(1, 1, 1, 0.6f);
        RectTransform fishRT = fishIcon.GetComponent<RectTransform>();
        fishRT.anchorMin = new Vector2(0, 1);
        fishRT.anchorMax = new Vector2(0, 1);
        fishRT.pivot = new Vector2(0, 1);
        fishRT.anchoredPosition = new Vector2(30, -80);
        fishRT.sizeDelta = new Vector2(150, 30);

        // Timer label - bottom-right
        GameObject timerObj = new GameObject("TimerLabel");
        timerObj.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI timerTMP = timerObj.AddComponent<TextMeshProUGUI>();
        timerTMP.text = "10s";
        timerTMP.fontSize = 48;
        timerTMP.alignment = TextAlignmentOptions.Right;
        timerTMP.color = Color.white;
        timerTMP.fontStyle = FontStyles.Bold;
        RectTransform timerRT = timerObj.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(1, 0);
        timerRT.anchorMax = new Vector2(1, 0);
        timerRT.pivot = new Vector2(1, 0);
        timerRT.anchoredPosition = new Vector2(-30, 20);
        timerRT.sizeDelta = new Vector2(200, 70);

        // Time sub-label
        GameObject timeLabel = new GameObject("TimeIcon");
        timeLabel.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI timeTMP = timeLabel.AddComponent<TextMeshProUGUI>();
        timeTMP.text = "TIME";
        timeTMP.fontSize = 24;
        timeTMP.alignment = TextAlignmentOptions.Right;
        timeTMP.color = new Color(1, 1, 1, 0.5f);
        RectTransform timeRT = timeLabel.GetComponent<RectTransform>();
        timeRT.anchorMin = new Vector2(1, 0);
        timeRT.anchorMax = new Vector2(1, 0);
        timeRT.pivot = new Vector2(1, 0);
        timeRT.anchoredPosition = new Vector2(-30, 90);
        timeRT.sizeDelta = new Vector2(150, 30);

        // Pause button - top-center, minimum touch target 120px
        GameObject pauseBtn = CreateButton(safeArea.transform, "PauseButton", "| |",
            Vector2.zero, new Vector2(120, 120), 36,
            new Color(0.2f, 0.3f, 0.5f, 0.6f));
        RectTransform pauseRT = pauseBtn.GetComponent<RectTransform>();
        pauseRT.anchorMin = new Vector2(0.5f, 1f);
        pauseRT.anchorMax = new Vector2(0.5f, 1f);
        pauseRT.pivot = new Vector2(0.5f, 1f);
        pauseRT.anchoredPosition = new Vector2(0, -10);

        // ── Game Over Panel (centered popup with CanvasGroup) ──
        GameObject gameOverPanel = CreatePopupPanel(safeArea.transform, "GameOverPanel", new Vector2(800, 650));
        gameOverPanel.SetActive(false);

        CreateTMPText(gameOverPanel.transform, "GameOverLabel", "GAME OVER",
            new Vector2(0, 220), 60, TextAlignmentOptions.Center);
        GameObject resultObj = CreateTMPText(gameOverPanel.transform, "ResultLabel", "You caught 0 fish!",
            new Vector2(0, 110), 38, TextAlignmentOptions.Center);
        GameObject winLoseObj = CreateTMPText(gameOverPanel.transform, "WinLoseLabel", "TRY AGAIN!",
            new Vector2(0, 30), 48, TextAlignmentOptions.Center);
        GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -100), new Vector2(480, 120), 40,
            new Color(0.2f, 0.75f, 0.35f));
        GameObject homeBtn = CreateButton(gameOverPanel.transform, "HomeButton", "HOME",
            new Vector2(0, -240), new Vector2(480, 120), 40,
            new Color(0.4f, 0.45f, 0.55f));

        // ── Pause Panel (centered popup with CanvasGroup) ──
        GameObject pausePanel = CreatePopupPanel(safeArea.transform, "PausePanel", new Vector2(800, 620));
        pausePanel.SetActive(false);

        CreateTMPText(pausePanel.transform, "PausedLabel", "PAUSED",
            new Vector2(0, 210), 64, TextAlignmentOptions.Center);
        GameObject resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 60), new Vector2(480, 120), 40,
            new Color(0.2f, 0.75f, 0.35f));
        GameObject restartPauseBtn = CreateButton(pausePanel.transform, "RestartPauseButton", "RESTART",
            new Vector2(0, -80), new Vector2(480, 120), 40,
            new Color(0.25f, 0.5f, 0.85f));
        GameObject homePauseBtn = CreateButton(pausePanel.transform, "HomePauseButton", "HOME",
            new Vector2(0, -220), new Vector2(480, 120), 40,
            new Color(0.4f, 0.45f, 0.55f));

        // ── GameManager ──
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        SerializedObject gmSo = new SerializedObject(gm);
        gmSo.FindProperty("scoreLabel").objectReferenceValue = scoreTMP;
        gmSo.FindProperty("timerLabel").objectReferenceValue = timerTMP;
        gmSo.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        gmSo.FindProperty("resultLabel").objectReferenceValue = resultObj.GetComponent<TextMeshProUGUI>();
        gmSo.FindProperty("winLoseLabel").objectReferenceValue = winLoseObj.GetComponent<TextMeshProUGUI>();
        gmSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        gmSo.FindProperty("claw").objectReferenceValue = claw;
        gmSo.FindProperty("fishSpawner").objectReferenceValue = spawner;
        gmSo.ApplyModifiedProperties();

        // Wire Claw's GameManager reference
        clawSo = new SerializedObject(claw);
        clawSo.FindProperty("gameManager").objectReferenceValue = gm;
        clawSo.ApplyModifiedProperties();

        // Wire DropZone references
        SerializedObject dzSo = new SerializedObject(dropZone);
        dzSo.FindProperty("claw").objectReferenceValue = claw;
        dzSo.FindProperty("gameManager").objectReferenceValue = gm;
        dzSo.ApplyModifiedProperties();

        // Wire button onClick events
        WireButton(pauseBtn, gm, "OnPausePressed");
        WireButton(restartBtn, gm, "OnRestartPressed");
        WireButton(homeBtn, gm, "OnHomePressed");
        WireButton(resumeBtn, gm, "OnResumePressed");
        WireButton(restartPauseBtn, gm, "OnRestartFromPause");
        WireButton(homePauseBtn, gm, "OnHomePressed");

        // EventSystem
        CreateEventSystem();

        string scenePath = "Assets/Scenes/Game.unity";
        EnsureDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Game scene created at " + scenePath);
    }

    // ─── SHARED HELPERS ─────────────────────────────────────────

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_W, REF_H);
        scaler.matchWidthOrHeight = 1f; // Match height for portrait consistency

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private static GameObject CreateSafeAreaPanel(Transform canvasTransform)
    {
        GameObject safeArea = new GameObject("SafeAreaPanel");
        safeArea.transform.SetParent(canvasTransform, false);

        RectTransform rt = safeArea.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        safeArea.AddComponent<SafeAreaPanel>();

        return safeArea;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text,
        Vector2 anchoredPos, int fontSize, TextAlignmentOptions alignment, Color? color = null)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color ?? Color.white;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(600, 80);

        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 size, int fontSize,
        Color? bgColor = null, Color? textColor = null)
    {
        Color bg = bgColor ?? new Color(0.25f, 0.5f, 0.85f);
        Color fg = textColor ?? Color.white;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = bg;
        img.sprite = CreateRoundedRectSprite(64, 64, 16);
        img.type = Image.Type.Sliced;

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bg;
        colors.highlightedColor = new Color(
            Mathf.Min(bg.r + 0.1f, 1f),
            Mathf.Min(bg.g + 0.1f, 1f),
            Mathf.Min(bg.b + 0.1f, 1f));
        colors.pressedColor = new Color(bg.r * 0.75f, bg.g * 0.75f, bg.b * 0.75f);
        btn.colors = colors;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = fg;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        return obj;
    }

    private static GameObject CreatePopupPanel(Transform parent, string name, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.08f, 0.12f, 0.22f, 0.95f);
        img.sprite = CreateRoundedRectSprite(128, 128, 24);
        img.type = Image.Type.Sliced;

        // CanvasGroup for smooth fade transitions
        CanvasGroup cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        return obj;
    }

    private static Sprite CreateRoundedRectSprite(int w, int h, int radius)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color white = Color.white;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inside = true;
                if (x < radius && y < radius)
                    inside = (x - radius) * (x - radius) + (y - radius) * (y - radius) <= radius * radius;
                else if (x > w - radius && y < radius)
                    inside = (x - (w - radius)) * (x - (w - radius)) + (y - radius) * (y - radius) <= radius * radius;
                else if (x < radius && y > h - radius)
                    inside = (x - radius) * (x - radius) + (y - (h - radius)) * (y - (h - radius)) <= radius * radius;
                else if (x > w - radius && y > h - radius)
                    inside = (x - (w - radius)) * (x - (w - radius)) + (y - (h - radius)) * (y - (h - radius)) <= radius * radius;

                tex.SetPixel(x, y, inside ? white : clear);
            }
        }
        tex.Apply();

        Vector4 border = new Vector4(radius, radius, radius, radius);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        sprite.name = "RoundedRect";
        return sprite;
    }

    private static void WireButton(GameObject buttonObj, Object target, string methodName)
    {
        Button btn = buttonObj.GetComponent<Button>();
        if (btn == null) return;

        var method = target.GetType().GetMethod(methodName);
        if (method == null)
        {
            Debug.LogWarning($"Method {methodName} not found on {target.GetType().Name}");
            return;
        }

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick,
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, method) as UnityEngine.Events.UnityAction);
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
