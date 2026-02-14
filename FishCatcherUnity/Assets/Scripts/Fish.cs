using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float leftBound = -4.5f;
    [SerializeField] private float rightBound = 4.5f;

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

        // Reverse at screen edges (fish swim across the whole screen)
        if (pos.x > rightBound)
        {
            swimDirection = -1;
            FlipFish(-1);
        }
        else if (pos.x < leftBound)
        {
            swimDirection = 1;
            FlipFish(1);
        }

        // Vertical bobbing
        pos.y = baseY + Mathf.Sin(time * 2f) * 0.08f;

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
        // Wiggle animation
        StartCoroutine(WiggleAnimation());
    }

    public void Release()
    {
        IsGrabbed = false;
        baseY = transform.position.y;
    }

    private System.Collections.IEnumerator WiggleAnimation()
    {
        float duration = 0.1f;
        transform.rotation = Quaternion.Euler(0, 0, 12f);
        yield return new WaitForSeconds(duration);
        transform.rotation = Quaternion.Euler(0, 0, -12f);
        yield return new WaitForSeconds(duration);
        transform.rotation = Quaternion.identity;
    }
}
