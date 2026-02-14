using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int numFish = 15;
    [SerializeField] private int minFishCount = 5;
    [SerializeField] private GameObject fishPrefab;

    [Header("Fish Sprites")]
    [SerializeField] private Sprite[] fishSprites;

    [Header("Spawn Area (world units)")]
    [SerializeField] private float spawnWidth = 6f;
    [SerializeField] private float spawnHeight = 5f;
    [SerializeField] private float spawnOriginX = -3f;
    [SerializeField] private float spawnOriginY = -1f;
    [SerializeField] private float fishScale = 0.35f;

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
        fishObj.transform.localScale = Vector3.one * fishScale;
        fishObj.name = $"Fish_{fishIdCounter++}";

        Fish fish = fishObj.GetComponent<Fish>();
        if (fish != null)
        {
            Sprite sprite = GetRandomFishSprite();
            fish.Initialize(sprite, baseY);
        }
    }

    private Sprite GetRandomFishSprite()
    {
        if (fishSprites != null && fishSprites.Length > 0)
            return fishSprites[Random.Range(0, fishSprites.Length)];
        return null;
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
