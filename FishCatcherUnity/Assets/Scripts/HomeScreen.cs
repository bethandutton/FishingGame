using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HomeScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private RectTransform titleTransform;

    [Header("Panels")]
    [SerializeField] private GameObject introPanel;

    [Header("Decorative Fish")]
    [SerializeField] private GameObject fishPrefab;
    [SerializeField] private Transform fishContainer;
    [SerializeField] private int decorativeFishCount = 8;
    [SerializeField] private Sprite[] fishSprites;

    private float[] fishSpeeds;
    private float[] fishBaseY;
    private Transform[] fishTransforms;

    private void Start()
    {
        // Find and wire Play button at runtime
        Button playButton = FindAnyObjectByType<Button>();
        if (playButton == null)
        {
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

    public void OnIntroPressed()
    {
        if (introPanel != null)
            introPanel.SetActive(true);
    }

    public void OnBackToHome()
    {
        if (introPanel != null)
            introPanel.SetActive(false);
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

            // Assign a random fish sprite
            SpriteRenderer sr = fish.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && fishSprites != null && fishSprites.Length > 0)
            {
                sr.sprite = fishSprites[Random.Range(0, fishSprites.Length)];
                sr.color = Color.white;
            }

            fishTransforms[i] = fish.transform;
            fishBaseY[i] = y;
            float speed = Random.Range(0.5f, 1.3f);
            fishSpeeds[i] = Random.value > 0.5f ? speed : -speed;

            // Face swim direction (flip sprite when going left)
            if (sr != null)
                sr.flipX = (fishSpeeds[i] < 0);
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

            // Wrap around edges
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
