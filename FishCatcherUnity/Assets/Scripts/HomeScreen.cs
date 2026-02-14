using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HomeScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private RectTransform titleTransform;

    [Header("Decorative Fish")]
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private Transform fishContainer;
    [SerializeField] private int decorativeFishCount = 8;

    private float[] fishSpeeds;
    private float[] fishBaseY;
    private Transform[] fishTransforms;

    private static readonly Color[] FishColors = new Color[]
    {
        new Color(1.0f, 0.3f, 0.3f),
        new Color(1.0f, 0.6f, 0.2f),
        new Color(1.0f, 0.9f, 0.2f),
        new Color(0.3f, 0.8f, 0.3f),
        new Color(0.3f, 0.5f, 1.0f),
        new Color(0.6f, 0.3f, 0.8f),
    };

    private void Start()
    {
        // Find and wire Play button at runtime
        Button playButton = FindAnyObjectByType<Button>();
        if (playButton == null)
        {
            // Search by name if multiple buttons exist
            GameObject btnObj = GameObject.Find("PlayButton");
            if (btnObj != null) playButton = btnObj.GetComponent<Button>();
        }
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayPressed);

        SpawnDecorativeFish();
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene("Game");
    }

    private void SpawnDecorativeFish()
    {
        if (fishPrefab == null || fishContainer == null) return;

        fishSpeeds = new float[decorativeFishCount];
        fishBaseY = new float[decorativeFishCount];
        fishTransforms = new Transform[decorativeFishCount];

        for (int i = 0; i < decorativeFishCount; i++)
        {
            float x = Random.Range(-6f, 6f);
            float y = Random.Range(-7f, -3f);

            GameObject fish = Instantiate(fishPrefab, new Vector3(x, y, 0), Quaternion.identity, fishContainer);
            fish.transform.localScale = Vector3.one * 0.4f;

            // Disable the Fish script so it doesn't interfere
            Fish fishScript = fish.GetComponent<Fish>();
            if (fishScript != null) fishScript.enabled = false;

            SpriteRenderer sr = fish.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.color = FishColors[i % FishColors.Length];

            fishTransforms[i] = fish.transform;
            fishBaseY[i] = y;
            float speed = Random.Range(0.5f, 1.3f);
            fishSpeeds[i] = Random.value > 0.5f ? speed : -speed;

            // Face swim direction
            Vector3 s = fish.transform.localScale;
            s.x = Mathf.Abs(s.x) * (fishSpeeds[i] > 0 ? 1 : -1);
            fish.transform.localScale = s;
        }
    }

    private void Update()
    {
        // Animate decorative fish
        if (fishTransforms == null) return;

        for (int i = 0; i < fishTransforms.Length; i++)
        {
            if (fishTransforms[i] == null) continue;

            Vector3 pos = fishTransforms[i].position;
            pos.x += fishSpeeds[i] * Time.deltaTime;

            // Wrap around edges - reappear on the other side at a new depth
            if (pos.x > 6f)
            {
                pos.x = -6f;
                fishBaseY[i] = Random.Range(-7f, -3f);
            }
            else if (pos.x < -6f)
            {
                pos.x = 6f;
                fishBaseY[i] = Random.Range(-7f, -3f);
            }

            // Gentle bobbing
            pos.y = fishBaseY[i] + Mathf.Sin(Time.time * 2f + i) * 0.1f;
            fishTransforms[i].position = pos;
        }

        // Pulse title
        if (titleTransform != null)
        {
            float scale = 1f + Mathf.Sin(Time.time * 4f) * 0.025f;
            titleTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
