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
        fishTransforms = new Transform[decorativeFishCount];

        for (int i = 0; i < decorativeFishCount; i++)
        {
            float x = Random.Range(-4f, 4f);
            float y = Random.Range(-5f, -2f);

            GameObject fish = Instantiate(fishPrefab, new Vector3(x, y, 0), Quaternion.identity, fishContainer);
            fish.transform.localScale = Vector3.one * 0.8f;

            SpriteRenderer sr = fish.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.color = FishColors[i % FishColors.Length];

            fishTransforms[i] = fish.transform;
            fishSpeeds[i] = Random.Range(0.5f, 1.3f) * (Random.value > 0.5f ? 1 : -1);
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

            // Reverse at edges
            if (pos.x > 5f)
            {
                fishSpeeds[i] = -Mathf.Abs(fishSpeeds[i]);
                Vector3 s = fishTransforms[i].localScale;
                s.x = -Mathf.Abs(s.x);
                fishTransforms[i].localScale = s;
            }
            else if (pos.x < -5f)
            {
                fishSpeeds[i] = Mathf.Abs(fishSpeeds[i]);
                Vector3 s = fishTransforms[i].localScale;
                s.x = Mathf.Abs(s.x);
                fishTransforms[i].localScale = s;
            }

            // Bobbing
            pos.y += Mathf.Sin(Time.time * 2f + i) * 0.002f;
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
