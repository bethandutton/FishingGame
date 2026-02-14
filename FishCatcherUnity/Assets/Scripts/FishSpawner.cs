using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int numFish = 15;
    [SerializeField] private int minFishCount = 5;
    [SerializeField] private GameObject fishPrefab;

    [Header("Spawn Area (world units)")]
    [SerializeField] private float spawnWidth = 8f;
    [SerializeField] private float spawnHeight = 3f;
    [SerializeField] private float spawnOriginX = -4f;
    [SerializeField] private float spawnOriginY = -3f;

    private static readonly Color[] FishColors = new Color[]
    {
        new Color(1.0f, 0.3f, 0.3f),   // Red
        new Color(1.0f, 0.6f, 0.2f),   // Orange
        new Color(1.0f, 0.9f, 0.2f),   // Yellow
        new Color(0.3f, 0.8f, 0.3f),   // Green
        new Color(0.3f, 0.5f, 1.0f),   // Blue
        new Color(0.6f, 0.3f, 0.8f),   // Purple
    };

    private int fishIdCounter;

    private void Start()
    {
        SpawnInitialFish();
    }

    public void SpawnInitialFish()
    {
        for (int i = 0; i < numFish; i++)
            SpawnFish(i);
    }

    private void SpawnFish(int index)
    {
        if (fishPrefab == null) return;

        int row = index / 5;
        int col = index % 5;
        float baseX = spawnOriginX + col * (spawnWidth / 5f) + Random.Range(-0.3f, 0.3f);
        float baseY = spawnOriginY - row * (spawnHeight / 3f) + Random.Range(-0.15f, 0.15f);

        GameObject fishObj = Instantiate(fishPrefab, new Vector3(baseX, baseY, 0), Quaternion.identity, transform);
        fishObj.name = $"Fish_{fishIdCounter++}";

        Fish fish = fishObj.GetComponent<Fish>();
        if (fish != null)
            fish.Initialize(FishColors[index % FishColors.Length], baseY);
    }

    public void ResetFish()
    {
        // Remove all existing fish
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        fishIdCounter = 0;
        // Wait a frame for destroys, then spawn
        Invoke(nameof(SpawnInitialFish), 0.05f);
    }

    private void Update()
    {
        // Respawn if too few remain
        int validCount = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                validCount++;
        }

        if (validCount < minFishCount)
            SpawnFish(fishIdCounter);
    }
}
