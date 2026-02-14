using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float screenEdge = 5f;
    [SerializeField] private float minY = -6f;
    [SerializeField] private float maxY = -1f;

    public bool IsGrabbed { get; private set; }

    private float swimSpeed;
    private int swimDirection;
    private float baseY;
    private float timeOffset;
    private SpriteRenderer spriteRenderer;

    public void Initialize(Color color, float y)
    {
        baseY = y;
        timeOffset = Random.Range(0f, 10f);
        swimSpeed = Random.Range(0.6f, 1.5f);
        swimDirection = Random.value > 0.5f ? 1 : -1;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = color;

        // Face swim direction
        FlipFish(swimDirection);
    }

    private void Update()
    {
        if (IsGrabbed) return;

        float time = Time.time + timeOffset;

        // Horizontal swimming
        Vector3 pos = transform.position;
        pos.x += swimDirection * swimSpeed * Time.deltaTime;

        // Wrap around screen edges - reappear on the other side at a new depth
        if (pos.x > screenEdge)
        {
            pos.x = -screenEdge;
            baseY = Random.Range(minY, maxY);
            swimSpeed = Random.Range(0.6f, 1.5f);
        }
        else if (pos.x < -screenEdge)
        {
            pos.x = screenEdge;
            baseY = Random.Range(minY, maxY);
            swimSpeed = Random.Range(0.6f, 1.5f);
        }

        // Gentle vertical bobbing
        pos.y = baseY + Mathf.Sin(time * 2f) * 0.1f;

        transform.position = pos;
    }

    private void FlipFish(int direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    public void Grab()
    {
        IsGrabbed = true;
        // Fish hangs vertically from the hook (head up, tail down)
        transform.rotation = Quaternion.Euler(0, 0, -90f);
    }

    public void Release()
    {
        IsGrabbed = false;
        transform.rotation = Quaternion.identity;
        baseY = transform.position.y;
    }
}
