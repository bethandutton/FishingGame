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
/// Use Window > Fish Catcher > Build Scenes to generate both scenes.
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("Window/Fish Catcher/Build Scenes")]
    public static void ShowWindow()
    {
        GetWindow<SceneBuilder>("Scene Builder");
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

    private static void BuildFishPrefab()
    {
        GameObject fish = new GameObject("FishPrefab");

        // SpriteRenderer for the fish body
        SpriteRenderer sr = fish.AddComponent<SpriteRenderer>();
        sr.sprite = FishSpriteGenerator.GetFishSprite();
        sr.sortingOrder = 5;

        // CircleCollider2D for grab detection
        CircleCollider2D col = fish.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;
        col.isTrigger = true;

        // Fish script
        fish.AddComponent<Fish>();
        fish.AddComponent<ProceduralSetup>();

        // Save prefab
        string path = "Assets/Prefabs/FishPrefab.prefab";
        EnsureDirectory("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(fish, path);
        DestroyImmediate(fish);

        Debug.Log("Fish prefab created at " + path);
    }

    private static void BuildHomeScreenScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.529f, 0.808f, 0.922f); // sky blue
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0, 0, -10);
        camObj.tag = "MainCamera";

        // Water background
        GameObject water = new GameObject("Water");
        SpriteRenderer waterSr = water.AddComponent<SpriteRenderer>();
        waterSr.color = new Color(0.2f, 0.5f, 0.7f);
        waterSr.drawMode = SpriteDrawMode.Tiled;
        waterSr.sortingOrder = -1;
        water.transform.position = new Vector3(0, -5f, 0);
        water.transform.localScale = new Vector3(12f, 8f, 1f);

        // Fish container
        GameObject fishContainer = new GameObject("FishContainer");

        // Canvas for UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(720, 1280);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Title
        GameObject titleObj = CreateTMPText(canvasObj.transform, "TitleLabel", "FISH\nCATCHER",
            new Vector2(0, 200), 64, TextAlignmentOptions.Center);
        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.color = Color.white;
        titleTMP.enableWordWrapping = false;

        // Subtitle
        CreateTMPText(canvasObj.transform, "Subtitle", "Catch them all!",
            new Vector2(0, 80), 24, TextAlignmentOptions.Center,
            new Color(1f, 0.9f, 0.5f));

        // Play button
        GameObject playBtn = CreateButton(canvasObj.transform, "PlayButton", "PLAY",
            new Vector2(0, -50), new Vector2(280, 70), 28);

        // Version label
        CreateTMPText(canvasObj.transform, "VersionLabel", "v1.2.0",
            new Vector2(0, -550), 14, TextAlignmentOptions.Center,
            new Color(1, 1, 1, 0.5f));

        // HomeScreen script
        GameObject homeManager = new GameObject("HomeScreenManager");
        HomeScreen hs = homeManager.AddComponent<HomeScreen>();

        // Wire up references via SerializedObject
        SerializedObject so = new SerializedObject(hs);
        so.FindProperty("titleLabel").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("titleTransform").objectReferenceValue = titleObj.GetComponent<RectTransform>();
        so.FindProperty("fishContainer").objectReferenceValue = fishContainer.transform;

        // Set fish prefab if it exists
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FishPrefab.prefab");
        if (prefab != null)
            so.FindProperty("fishPrefab").objectReferenceValue = prefab;

        so.ApplyModifiedProperties();

        // Wire play button click
        Button btn = playBtn.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick,
            new UnityEngine.Events.UnityAction(hs.OnPlayPressed));

        // EventSystem (required for UI interaction)
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        string scenePath = "Assets/Scenes/HomeScreen.unity";
        EnsureDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Home Screen scene created at " + scenePath);
    }

    private static void BuildGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.15f, 0.25f, 0.4f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObj.transform.position = new Vector3(0, 0, -10);
        camObj.tag = "MainCamera";

        // Water area background
        GameObject water = new GameObject("WaterArea");
        SpriteRenderer waterSr = water.AddComponent<SpriteRenderer>();
        waterSr.color = new Color(0.1f, 0.35f, 0.55f);
        waterSr.sortingOrder = -1;
        water.transform.position = new Vector3(0, -5f, 0);
        water.transform.localScale = new Vector3(12f, 8f, 1f);

        // DropZone (bucket)
        GameObject bucket = new GameObject("DropZone");
        bucket.transform.position = new Vector3(4f, 4f, 0);
        SpriteRenderer bucketSr = bucket.AddComponent<SpriteRenderer>();
        bucketSr.sprite = ClawSpriteGenerator.GetBucketSprite();
        bucketSr.sortingOrder = 3;
        BoxCollider2D bucketCol = bucket.AddComponent<BoxCollider2D>();
        bucketCol.size = new Vector2(1.5f, 1.5f);
        bucketCol.isTrigger = true;
        DropZone dropZone = bucket.AddComponent<DropZone>();

        // "DROP HERE" label
        GameObject dropLabel = new GameObject("DropLabel");
        dropLabel.transform.SetParent(bucket.transform);
        dropLabel.transform.localPosition = new Vector3(0, 1.2f, 0);
        TextMeshPro dropTMP = dropLabel.AddComponent<TextMeshPro>();
        dropTMP.text = "DROP HERE";
        dropTMP.fontSize = 4;
        dropTMP.alignment = TextAlignmentOptions.Center;
        dropTMP.sortingOrder = 10;

        // Fish Spawner
        GameObject spawnerObj = new GameObject("FishSpawner");
        spawnerObj.transform.position = new Vector3(0, -2f, 0);
        FishSpawner spawner = spawnerObj.AddComponent<FishSpawner>();

        // Set fish prefab reference
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FishPrefab.prefab");
        if (prefab != null)
        {
            SerializedObject spawnerSo = new SerializedObject(spawner);
            spawnerSo.FindProperty("fishPrefab").objectReferenceValue = prefab;
            spawnerSo.ApplyModifiedProperties();
        }

        // Claw
        GameObject clawObj = new GameObject("Claw");
        clawObj.transform.position = new Vector3(0, 6f, 0);
        Claw claw = clawObj.AddComponent<Claw>();

        // Claw head
        GameObject clawHead = new GameObject("ClawHead");
        clawHead.transform.SetParent(clawObj.transform);
        clawHead.transform.localPosition = Vector3.zero;

        // Claw base sprite
        GameObject clawBase = new GameObject("ClawBase");
        clawBase.transform.SetParent(clawHead.transform);
        clawBase.transform.localPosition = Vector3.zero;
        SpriteRenderer baseSr = clawBase.AddComponent<SpriteRenderer>();
        baseSr.sprite = ClawSpriteGenerator.GetClawBaseSprite();
        baseSr.sortingOrder = 8;

        // Claw left arm
        GameObject clawLeft = new GameObject("ClawLeft");
        clawLeft.transform.SetParent(clawHead.transform);
        clawLeft.transform.localPosition = new Vector3(-0.25f, -0.4f, 0);
        SpriteRenderer leftSr = clawLeft.AddComponent<SpriteRenderer>();
        leftSr.sprite = ClawSpriteGenerator.GetClawArmSprite();
        leftSr.sortingOrder = 7;

        // Claw right arm
        GameObject clawRight = new GameObject("ClawRight");
        clawRight.transform.SetParent(clawHead.transform);
        clawRight.transform.localPosition = new Vector3(0.25f, -0.4f, 0);
        SpriteRenderer rightSr = clawRight.AddComponent<SpriteRenderer>();
        rightSr.sprite = ClawSpriteGenerator.GetClawArmSprite();
        rightSr.sortingOrder = 7;

        // Claw grab area (for collision detection)
        GameObject grabArea = new GameObject("GrabArea");
        grabArea.transform.SetParent(clawHead.transform);
        grabArea.transform.localPosition = new Vector3(0, -0.6f, 0);
        CircleCollider2D grabCol = grabArea.AddComponent<CircleCollider2D>();
        grabCol.radius = 0.4f;
        grabCol.isTrigger = true;

        // Rope (LineRenderer)
        LineRenderer rope = clawObj.AddComponent<LineRenderer>();
        rope.positionCount = 2;
        rope.startWidth = 0.06f;
        rope.endWidth = 0.06f;
        rope.material = new Material(Shader.Find("Sprites/Default"));
        rope.startColor = new Color(0.8f, 0.7f, 0.2f);
        rope.endColor = new Color(0.8f, 0.7f, 0.2f);
        rope.useWorldSpace = false;
        rope.sortingOrder = 6;

        // Wire claw references
        SerializedObject clawSo = new SerializedObject(claw);
        clawSo.FindProperty("clawHead").objectReferenceValue = clawHead.transform;
        clawSo.FindProperty("rope").objectReferenceValue = rope;
        clawSo.FindProperty("clawLeft").objectReferenceValue = clawLeft.transform;
        clawSo.FindProperty("clawRight").objectReferenceValue = clawRight.transform;
        clawSo.ApplyModifiedProperties();

        // UI Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Top bar
        GameObject topBar = new GameObject("TopBar");
        topBar.transform.SetParent(canvasObj.transform, false);
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.1f, 0.15f, 0.25f);
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.pivot = new Vector2(0.5f, 1);
        topBarRT.sizeDelta = new Vector2(0, 80);

        // Score label
        GameObject scoreObj = CreateTMPText(canvasObj.transform, "ScoreLabel", "Fish: 0 / 10",
            new Vector2(-170, 590), 28, TextAlignmentOptions.Left);

        // Timer label
        GameObject timerObj = CreateTMPText(canvasObj.transform, "TimerLabel", "Time: 30",
            new Vector2(170, 590), 28, TextAlignmentOptions.Right);

        // Pause button (overlays timer area)
        GameObject pauseBtn = CreateButton(canvasObj.transform, "PauseButton", "||",
            new Vector2(300, 590), new Vector2(60, 50), 20);

        // Game Over Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Vector2(500, 400));
        gameOverPanel.SetActive(false);
        CreateTMPText(gameOverPanel.transform, "GameOverLabel", "GAME OVER",
            new Vector2(0, 130), 42, TextAlignmentOptions.Center);
        GameObject resultObj = CreateTMPText(gameOverPanel.transform, "ResultLabel", "You caught 0 fish!",
            new Vector2(0, 40), 28, TextAlignmentOptions.Center);
        GameObject winLoseObj = CreateTMPText(gameOverPanel.transform, "WinLoseLabel", "TRY AGAIN!",
            new Vector2(0, -30), 32, TextAlignmentOptions.Center);
        GameObject restartBtn = CreateButton(gameOverPanel.transform, "RestartButton", "PLAY AGAIN",
            new Vector2(0, -110), new Vector2(250, 50), 24);
        GameObject homeBtn = CreateButton(gameOverPanel.transform, "HomeButton", "HOME",
            new Vector2(0, -170), new Vector2(250, 50), 24);

        // Pause Panel
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Vector2(500, 360));
        pausePanel.SetActive(false);
        CreateTMPText(pausePanel.transform, "PausedLabel", "PAUSED",
            new Vector2(0, 120), 48, TextAlignmentOptions.Center);
        GameObject resumeBtn = CreateButton(pausePanel.transform, "ResumeButton", "RESUME",
            new Vector2(0, 30), new Vector2(250, 60), 28);
        GameObject restartPauseBtn = CreateButton(pausePanel.transform, "RestartPauseButton", "RESTART",
            new Vector2(0, -50), new Vector2(250, 60), 28);
        GameObject homePauseBtn = CreateButton(pausePanel.transform, "HomePauseButton", "HOME",
            new Vector2(0, -130), new Vector2(250, 60), 28);

        // GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // Wire GameManager references
        SerializedObject gmSo = new SerializedObject(gm);
        gmSo.FindProperty("scoreLabel").objectReferenceValue = scoreObj.GetComponent<TextMeshProUGUI>();
        gmSo.FindProperty("timerLabel").objectReferenceValue = timerObj.GetComponent<TextMeshProUGUI>();
        gmSo.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        gmSo.FindProperty("resultLabel").objectReferenceValue = resultObj.GetComponent<TextMeshProUGUI>();
        gmSo.FindProperty("winLoseLabel").objectReferenceValue = winLoseObj.GetComponent<TextMeshProUGUI>();
        gmSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        gmSo.FindProperty("claw").objectReferenceValue = claw;
        gmSo.FindProperty("fishSpawner").objectReferenceValue = spawner;
        gmSo.ApplyModifiedProperties();

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

        // EventSystem (required for UI interaction)
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        string scenePath = "Assets/Scenes/Game.unity";
        EnsureDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Game scene created at " + scenePath);
    }

    // --- Helper methods ---

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
        tmp.enableWordWrapping = false;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(400, 60);

        return obj;
    }

    private static GameObject CreateButton(Transform parent, string name, string text,
        Vector2 anchoredPos, Vector2 size, int fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.5f, 0.8f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.5f, 0.8f);
        colors.highlightedColor = new Color(0.4f, 0.6f, 0.9f);
        colors.pressedColor = new Color(0.2f, 0.4f, 0.7f);
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
        tmp.color = Color.white;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        return obj;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.1f, 0.15f, 0.25f, 0.95f);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        return obj;
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
