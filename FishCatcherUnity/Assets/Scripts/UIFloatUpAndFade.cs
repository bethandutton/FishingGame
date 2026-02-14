using UnityEngine;
using TMPro;

/// <summary>
/// Floats a UI element upward while fading it out, then destroys it.
/// Attach to a RectTransform with a TextMeshProUGUI component.
/// </summary>
public class UIFloatUpAndFade : MonoBehaviour
{
    private const float DURATION = 1f;
    private const float FLOAT_DISTANCE = 120f;

    private float elapsed;
    private TextMeshProUGUI tmp;
    private RectTransform rt;
    private Vector2 startPos;

    private void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / DURATION;

        // Float upward
        rt.anchoredPosition = startPos + Vector2.up * FLOAT_DISTANCE * t;

        // Fade out
        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = 1f - t;
            tmp.color = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}
