using UnityEngine;

/// <summary>
/// Generates procedural fish sprites at runtime since the original Godot game
/// used Polygon2D to draw fish. Attach to a GameObject with a SpriteRenderer.
/// </summary>
public class FishSpriteGenerator : MonoBehaviour
{
    private static Sprite _cachedFishSprite;

    public static void ClearCache()
    {
        _cachedFishSprite = null;
    }

    /// <summary>
    /// Creates a simple fish-shaped sprite procedurally.
    /// The SpriteRenderer color property handles coloring.
    /// </summary>
    public static Sprite GetFishSprite()
    {
        if (_cachedFishSprite != null)
            return _cachedFishSprite;

        int width = 64;
        int height = 56;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color transparent = new Color(0, 0, 0, 0);
        Color white = Color.white;
        Color black = Color.black;
        Color body = Color.white; // Will be tinted by SpriteRenderer.color

        // Clear
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = transparent;
        tex.SetPixels(pixels);

        // Draw elliptical body
        int cx = width / 2;
        int cy = height / 2;
        float rx = 26f;
        float ry = 22f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                    tex.SetPixel(x, y, body);
            }
        }

        // Draw tail (triangle on the left side)
        for (int y = cy - 10; y <= cy + 10; y++)
        {
            int tailWidth = (int)(8 * (1f - Mathf.Abs(y - cy) / 10f));
            for (int x = cx - 26; x >= cx - 26 - tailWidth; x--)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                    tex.SetPixel(x, y, body);
            }
        }

        // Draw eye (white circle with black pupil)
        int eyeX = cx + 8;
        int eyeY = cy + 6;
        for (int y = eyeY - 6; y <= eyeY + 6; y++)
        {
            for (int x = eyeX - 6; x <= eyeX + 6; x++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    float dist = Mathf.Sqrt((x - eyeX) * (x - eyeX) + (y - eyeY) * (y - eyeY));
                    if (dist <= 6f)
                        tex.SetPixel(x, y, white);
                    if (dist <= 3f)
                        tex.SetPixel(x, y, black);
                }
            }
        }

        // Draw mouth (small dark circle)
        int mouthX = cx + 2;
        int mouthY = cy - 8;
        for (int y = mouthY - 4; y <= mouthY + 4; y++)
        {
            for (int x = mouthX - 6; x <= mouthX + 6; x++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    float dist = Mathf.Sqrt((x - mouthX) * (x - mouthX) + (y - mouthY) * (y - mouthY));
                    if (dist <= 5f)
                        tex.SetPixel(x, y, new Color(0.15f, 0.1f, 0.1f));
                }
            }
        }

        tex.Apply();

        _cachedFishSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        _cachedFishSprite.name = "ProceduralFish";

        return _cachedFishSprite;
    }

    /// <summary>
    /// Creates a simple circular sprite for the claw grab area visualization (debug).
    /// </summary>
    public static Sprite GetCircleSprite(int radius = 16)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - radius) * (x - radius) + (y - radius) * (y - radius));
                tex.SetPixel(x, y, dist <= radius ? Color.white : new Color(0, 0, 0, 0));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
