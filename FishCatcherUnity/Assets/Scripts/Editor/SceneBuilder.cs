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

    // Cached TMP font asset for Press Start 2P
    private static TMP_FontAsset _pixelFont;

    // Cached button sprites
    private static Sprite _btnGreen;
    private static Sprite _btnBlue;
    private static Sprite _btnYellow;
    private static Sprite _btnRed;

    [MenuItem("Window/Fish Catcher/Build Scenes")]
    public static void ShowWindow()
    {
        GetWindow<SceneBuilder>("Scene Builder");
    }

    [MenuItem("Window/Fish Catcher/Build All Now")]
    public static void BuildAllDirect()
    {
        FishSpriteGenerator.ClearCache();
        BackgroundGenerator.ClearCache();
        ConfigureSpriteImports();
        CreatePixelFontAsset();
        LoadButtonSprites();
        BuildFishPrefab();
        BuildHomeScreenScene();
        BuildGameScene();
        Debug.Log("All scenes built successfully!");
    }

    /// <summary>
    /// Ensures all PNGs in Assets/Sprites are imported as Sprite type.
    /// Unity defaults new textures to "Default" which breaks LoadAssetAtPath&lt;Sprite&gt;.
    /// </summary>
    private static void ConfigureSpriteImports()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
        bool changed = false;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                changed = true;
            }
        }

        if (changed)
            AssetDatabase.Refresh();
    }

    /// <summary>
    /// Creates a TMP font asset from Press Start 2P TTF if it doesn't exist yet.
    /// </summary>
    private static void CreatePixelFontAsset()
    {
        // Try loading existing asset first
        _pixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P SDF.asset");
        if (_pixelFont != null) return;

        // Load the TTF
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P-Regular.ttf");
        if (sourceFont == null)
        {
            Debug.LogWarning("Press Start 2P font not found at Assets/Fonts/PressStart2P-Regular.ttf");
            return;
        }

        // Create TMP font asset
        _pixelFont = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (_pixelFont != null)
        {
            _pixelFont.name = "PressStart2P SDF";
            EnsureDirectory("Assets/Fonts");
            AssetDatabase.CreateAsset(_pixelFont, "Assets/Fonts/PressStart2P SDF.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Created TMP font asset: PressStart2P SDF");
        }
    }

    /// <summary>
    /// Maps old color hints to the correct button sprite asset.
    /// Green = primary actions, Blue = secondary, Yellow = home/neutral, Red = danger.
    /// </summary>
    private static Sprite PickButtonSprite(Color? hint)
    {
        if (hint == null) return _btnBlue ?? CreateRoundedRectSprite(64, 64, 16);

        Color c = hint.Value;
        // Green-ish (play, resume, play again)
        if (c.g > 0.6f && c.r < 0.5f) return _btnGreen ?? CreateRoundedRectSprite(64, 64, 16);
        // Blue-ish (restart, pause)
        if (c.b > 0.6f && c.g < 0.6f) return _btnBlue ?? CreateRoundedRectSprite(64, 64, 16);
        // Grey/neutral (home)
        if (c.r > 0.3f && c.r < 0.6f && c.g > 0.3f && c.g < 0.6f) return _btnYellow ?? CreateRoundedRectSprite(64, 64, 16);
        // Red
        if (c.r > 0.7f && c.g < 0.5f) return _btnRed ?? CreateRoundedRectSprite(64, 64, 16);

        return _btnBlue ?? CreateRoundedRectSprite(64, 64, 16);
    }

    private static void LoadButtonSprites()
    {
        _btnGreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Buttons/Button_Green.png");
        _btnBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Buttons/Button_Blue.png");
        _btnYellow = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Buttons/Button_Yellow.png");
        _btnRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Buttons/Button_Red.png");

        if (_btnGreen == null)
            Debug.LogWarning("Button sprites not found in Assets/Sprites/Buttons/. Run Build All again after Unity imports them.");
    }

    private static void ApplyPixelFont(TextMeshProUGUI tmp)
    {
        if (_pixelFont != null)
            tmp.font = _pixelFont;
    }

    private static void ApplyPixelFont(TextMeshPro tmp)
    {
        if (_pixelFont != null)
            tmp.font = _pixelFont;
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
        cam.backgroundColor = new Color(0.02f, 0.05f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0, 0, -10);
        camObj.tag = "MainCamera";

        // Underwater pixel art background
        BuildUnderwaterBackground(0f);

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
        titleTMP.fontSize = 72;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;
        ApplyPixelFont(titleTMP);
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
        subTMP.fontSize = 28;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(1f, 0.9f, 0.5f);
        ApplyPixelFont(subTMP);
        RectTransform subRT = subtitleObj.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.5f, 1f);
        subRT.anchorMax = new Vector2(0.5f, 1f);
        subRT.pivot = new Vector2(0.5f, 1f);
        subRT.anchoredPosition = new Vector2(0, -400);
        subRT.sizeDelta = new Vector2(600, 60);

        // ── Play button (center-lower for thumb zone, large touch target) ──
        GameObject playBtn = CreateButton(safeArea.transform, "PlayButton", "PLAY",
            Vector2.zero, new Vector2(560, 150), 42,
            new Color(0.2f, 0.75f, 0.35f));
        RectTransform playRT = playBtn.GetComponent<RectTransform>();
        playRT.anchorMin = new Vector2(0.5f, 0.5f);
        playRT.anchorMax = new Vector2(0.5f, 0.5f);
        playRT.anchoredPosition = new Vector2(0, -100);
        Button playButton = playBtn.GetComponent<Button>();

        // ── How To Play button (below play, yellow) ──
        GameObject howToPlayBtn = CreateButton(safeArea.transform, "HowToPlayButton", "HOW TO PLAY",
            Vector2.zero, new Vector2(560, 150), 32,
            new Color(0.85f, 0.7f, 0.1f));
        RectTransform htpRT = howToPlayBtn.GetComponent<RectTransform>();
        htpRT.anchorMin = new Vector2(0.5f, 0.5f);
        htpRT.anchorMax = new Vector2(0.5f, 0.5f);
        htpRT.anchoredPosition = new Vector2(0, -280);

        // ── Introduction Panel (overlay) ──
        GameObject introPanel = CreatePopupPanel(safeArea.transform, "IntroPanel", new Vector2(900, 1200));
        introPanel.SetActive(false);

        CreateTMPText(introPanel.transform, "IntroTitle", "HOW TO PLAY",
            new Vector2(0, 480), 48, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.5f));
        CreateTMPText(introPanel.transform, "IntroText",
            "Hold to cast your\nline into the sea\n\nCatch a fish and\nreel it back to\nyour boat to score\n\nCatch 10 fish\nbefore time runs\nout to win!\n\n+2 seconds for\neach fish caught",
            new Vector2(0, -30), 28, TextAlignmentOptions.Center);
        // Make intro text taller to fit all lines
        GameObject introTextObj = introPanel.transform.Find("IntroText").gameObject;
        introTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 800);

        GameObject backBtn = CreateButton(introPanel.transform, "BackButton", "BACK",
            new Vector2(0, -500), new Vector2(400, 120), 36,
            new Color(0.25f, 0.5f, 0.85f));

        // ── Version label (bottom, non-interactive) ──
        GameObject versionObj = new GameObject("VersionLabel");
        versionObj.transform.SetParent(safeArea.transform, false);
        TextMeshProUGUI verTMP = versionObj.AddComponent<TextMeshProUGUI>();
        verTMP.text = "v1.2.0";
        verTMP.fontSize = 24;
        verTMP.alignment = TextAlignmentOptions.Center;
        verTMP.color = new Color(1, 1, 1, 0.35f);
        ApplyPixelFont(verTMP);
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

        // Load fish sprite assets for decorative fish
        string[] homeGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Fish" });
        SerializedProperty homeSprites = so.FindProperty("fishSprites");
        homeSprites.arraySize = homeGuids.Length;
        for (int i = 0; i < homeGuids.Length; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(homeGuids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            homeSprites.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
        }

        so.FindProperty("introPanel").objectReferenceValue = introPanel;
        so.ApplyModifiedProperties();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(playButton.onClick,
            new UnityEngine.Events.UnityAction(hs.OnPlayPressed));

        // Wire HOW TO PLAY and BACK button events
        Button htpButton = howToPlayBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(htpButton.onClick,
            new UnityEngine.Events.UnityAction(hs.OnIntroPressed));
        Button backButton = backBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backButton.onClick,
            new UnityEngine.Events.UnityAction(hs.OnBackToHome));

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

        // Camera - shifted up so line starts from top
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.02f, 0.05f, 0.15f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0, 2f, -10);
        camObj.tag = "MainCamera";

        // Underwater pixel art background
        BuildUnderwaterBackground(2f);

        // ── Fish Spawner ──
        GameObject spawnerObj = new GameObject("FishSpawner");
        spawnerObj.transform.position = Vector3.zero;
        FishSpawner spawner = spawnerObj.AddComponent<FishSpawner>();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FishPrefab.prefab");
        if (prefab != null)
        {
            SerializedObject spawnerSo = new SerializedObject(spawner);
            spawnerSo.FindProperty("fishPrefab").objectReferenceValue = prefab;

            // Load all fish sprite assets
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Fish" });
            SerializedProperty spritesArray = spawnerSo.FindProperty("fishSprites");
            spritesArray.arraySize = spriteGuids.Length;
            for (int i = 0; i < spriteGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                spritesArray.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
            }

            spawnerSo.ApplyModifiedProperties();
        }

        // ── Fishing Boat ──
        GameObject boatObj = new GameObject("FishingBoat");
        boatObj.transform.position = new Vector3(0, 8.5f, 0);

        // Boat sprite
        Sprite boatSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/boat.png");
        SpriteRenderer boatSr = boatObj.AddComponent<SpriteRenderer>();
        if (boatSprite != null)
        {
            boatSr.sprite = boatSprite;
            // Scale boat to reasonable size (tune based on actual sprite dimensions)
            float boatScale = 3f / Mathf.Max(boatSprite.bounds.size.x, boatSprite.bounds.size.y);
            boatObj.transform.localScale = Vector3.one * boatScale;
        }
        boatSr.sortingOrder = 7;

        FishingBoat boat = boatObj.AddComponent<FishingBoat>();

        // Rod tip (child of boat, starts at anchor offset)
        GameObject rodTip = new GameObject("RodTip");
        rodTip.transform.SetParent(boatObj.transform);
        rodTip.transform.localPosition = new Vector3(-1f, -0.5f, 0);

        // Fishing line
        LineRenderer fishingLine = boatObj.AddComponent<LineRenderer>();
        fishingLine.positionCount = 2;
        fishingLine.startWidth = 0.06f;
        fishingLine.endWidth = 0.06f;
        fishingLine.material = new Material(Shader.Find("Sprites/Default"));
        fishingLine.startColor = new Color(0.85f, 0.85f, 0.9f);
        fishingLine.endColor = new Color(0.85f, 0.85f, 0.9f);
        fishingLine.useWorldSpace = false;
        fishingLine.sortingOrder = 6;

        // Wire FishingBoat references
        SerializedObject boatSo = new SerializedObject(boat);
        boatSo.FindProperty("rodTip").objectReferenceValue = rodTip.transform;
        boatSo.FindProperty("fishingLine").objectReferenceValue = fishingLine;
        boatSo.FindProperty("boatRenderer").objectReferenceValue = boatSr;
        boatSo.ApplyModifiedProperties();

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
        ApplyPixelFont(scoreTMP);
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
        ApplyPixelFont(fishTMP);
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
        ApplyPixelFont(timerTMP);
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
        ApplyPixelFont(timeTMP);
        RectTransform timeRT = timeLabel.GetComponent<RectTransform>();
        timeRT.anchorMin = new Vector2(1, 0);
        timeRT.anchorMax = new Vector2(1, 0);
        timeRT.pivot = new Vector2(1, 0);
        timeRT.anchoredPosition = new Vector2(-30, 90);
        timeRT.sizeDelta = new Vector2(150, 30);

        // Pause button - top-right, minimum touch target 120px
        GameObject pauseBtn = CreateButton(safeArea.transform, "PauseButton", "| |",
            Vector2.zero, new Vector2(120, 120), 36,
            new Color(0.2f, 0.3f, 0.5f, 0.6f));
        RectTransform pauseRT = pauseBtn.GetComponent<RectTransform>();
        pauseRT.anchorMin = new Vector2(1f, 1f);
        pauseRT.anchorMax = new Vector2(1f, 1f);
        pauseRT.pivot = new Vector2(1f, 1f);
        pauseRT.anchoredPosition = new Vector2(-20, -10);

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
        gmSo.FindProperty("boat").objectReferenceValue = boat;
        gmSo.FindProperty("fishSpawner").objectReferenceValue = spawner;
        gmSo.ApplyModifiedProperties();

        // Wire FishingBoat's GameManager reference
        boatSo = new SerializedObject(boat);
        boatSo.FindProperty("gameManager").objectReferenceValue = gm;
        boatSo.ApplyModifiedProperties();

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

    // ─── UNDERWATER BACKGROUND ─────────────────────────────────

    private static void BuildUnderwaterBackground(float cameraY)
    {
        float bottom = cameraY - 8f;
        float top = cameraY + 8f;

        GameObject bg = new GameObject("UnderwaterBackground");

        // Background image sprite
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UnderwaterBackground.png");
        if (bgSprite != null)
        {
            GameObject bgImg = new GameObject("BackgroundImage");
            bgImg.transform.SetParent(bg.transform);
            bgImg.transform.position = new Vector3(0, cameraY, 0);
            SpriteRenderer bgSr = bgImg.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;

            // Scale to fill the camera view (orthoSize=8 => 16 units tall)
            float spriteH = bgSprite.bounds.size.y;
            float spriteW = bgSprite.bounds.size.x;
            float scaleY = 16f / spriteH;
            // Use aspect ratio to get width, ensure it covers screen width too
            float targetW = 16f * (9f / 16f); // ~9 units for 9:16 portrait
            float scaleX = Mathf.Max(scaleY, targetW / spriteW * scaleY / scaleY);
            // Just use uniform scale based on height to fill vertically
            float uniformScale = 16f / spriteH;
            bgImg.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }

        // Bubble animator
        GameObject bubbleMgr = new GameObject("BubbleManager");
        bubbleMgr.transform.SetParent(bg.transform);
        BubbleAnimator ba = bubbleMgr.AddComponent<BubbleAnimator>();

        SerializedObject baSo = new SerializedObject(ba);
        baSo.FindProperty("spawnMinX").floatValue = -4f;
        baSo.FindProperty("spawnMaxX").floatValue = 4f;
        baSo.FindProperty("spawnMinY").floatValue = bottom;
        baSo.FindProperty("spawnMaxY").floatValue = bottom + 3f;
        baSo.FindProperty("despawnY").floatValue = top + 1f;
        baSo.ApplyModifiedProperties();
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
        ApplyPixelFont(tmp);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(600, 80);

        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 size, int fontSize,
        Color? bgColor = null, Color? textColor = null)
    {
        Color fg = textColor ?? Color.white;

        // Pick button sprite based on color hint
        Sprite btnSprite = PickButtonSprite(bgColor);

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = Color.white; // Don't tint - sprite has its own color
        img.sprite = btnSprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
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
        ApplyPixelFont(tmp);

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
