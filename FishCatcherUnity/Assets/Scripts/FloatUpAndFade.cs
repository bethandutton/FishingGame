using UnityEngine;
using TMPro;

public class FloatUpAndFade : MonoBehaviour
{
    private float elapsed;
    private const float DURATION = 0.5f;
    private TextMeshPro tmp;
    private Vector3 startPos;

    private void Start()
    {
        tmp = GetComponent<TextMeshPro>();
        startPos = transform.position;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / DURATION;

        transform.position = startPos + Vector3.up * t * 1f;

        if (tmp != null)
        {
            Color c = tmp.color;
            c.a = 1f - t;
            tmp.color = c;
        }
    }
}
