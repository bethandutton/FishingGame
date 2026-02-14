using UnityEngine;

/// <summary>
/// Spawns and animates a pool of pixel art bubbles that float upward.
/// Attach to a single manager GameObject in the scene.
/// </summary>
public class BubbleAnimator : MonoBehaviour
{
    private const int POOL_SIZE = 12;
    private const float SPAWN_INTERVAL_MIN = 0.8f;
    private const float SPAWN_INTERVAL_MAX = 1.5f;
    private const float RISE_SPEED_MIN = 0.3f;
    private const float RISE_SPEED_MAX = 0.8f;
    private const float WOBBLE_AMPLITUDE = 0.15f;
    private const float WOBBLE_FREQUENCY = 2f;

    [SerializeField] private float spawnMinX = -4f;
    [SerializeField] private float spawnMaxX = 4f;
    [SerializeField] private float spawnMinY = -6f;
    [SerializeField] private float spawnMaxY = -3f;
    [SerializeField] private float despawnY = 10f;

    private GameObject[] bubblePool;
    private float[] riseSpeed;
    private float[] baseX;
    private float[] timeOffset;
    private float spawnTimer;
    private float nextSpawnTime;

    private void Start()
    {
        bubblePool = new GameObject[POOL_SIZE];
        riseSpeed = new float[POOL_SIZE];
        baseX = new float[POOL_SIZE];
        timeOffset = new float[POOL_SIZE];

        Sprite bubbleSprite = BackgroundGenerator.GetBubbleSprite();

        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject bubble = new GameObject($"Bubble_{i}");
            bubble.transform.SetParent(transform);
            SpriteRenderer sr = bubble.AddComponent<SpriteRenderer>();
            sr.sprite = bubbleSprite;
            sr.sortingOrder = -3;
            bubble.SetActive(false);
            bubblePool[i] = bubble;
        }

        nextSpawnTime = Random.Range(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX);
    }

    private void Update()
    {
        for (int i = 0; i < POOL_SIZE; i++)
        {
            if (!bubblePool[i].activeSelf) continue;

            Vector3 pos = bubblePool[i].transform.position;
            pos.y += riseSpeed[i] * Time.deltaTime;
            pos.x = baseX[i] + Mathf.Sin((Time.time + timeOffset[i]) * WOBBLE_FREQUENCY) * WOBBLE_AMPLITUDE;
            bubblePool[i].transform.position = pos;

            if (pos.y > despawnY)
                bubblePool[i].SetActive(false);
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= nextSpawnTime)
        {
            spawnTimer = 0f;
            nextSpawnTime = Random.Range(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX);
            SpawnBubble();
        }
    }

    private void SpawnBubble()
    {
        for (int i = 0; i < POOL_SIZE; i++)
        {
            if (bubblePool[i].activeSelf) continue;

            float x = Random.Range(spawnMinX, spawnMaxX);
            float y = Random.Range(spawnMinY, spawnMaxY);
            bubblePool[i].transform.position = new Vector3(x, y, 0);

            float scale = Random.Range(0.5f, 1.2f);
            bubblePool[i].transform.localScale = Vector3.one * scale;

            riseSpeed[i] = Random.Range(RISE_SPEED_MIN, RISE_SPEED_MAX);
            baseX[i] = x;
            timeOffset[i] = Random.Range(0f, 10f);
            bubblePool[i].SetActive(true);
            return;
        }
    }
}
