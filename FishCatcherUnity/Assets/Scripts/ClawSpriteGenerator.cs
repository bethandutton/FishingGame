using UnityEngine;

/// <summary>
/// Generates procedural fishing hook and bucket sprites.
/// </summary>
public static class ClawSpriteGenerator
{
    private static Sprite _hookSprite;
    private static Sprite _bucketSprite;

    public static void ClearCache()
    {
        _hookSprite = null;
        _bucketSprite = null;
    }

    public static Sprite GetClawBaseSprite() => GetHookSprite();
    public static Sprite GetClawArmSprite() => GetHookSprite();

    public static Sprite GetHookSprite()
    {
        if (_hookSprite != null) return _hookSprite;

        int w = 64, h = 128;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color silver = new Color(0.8f, 0.82f, 0.85f);
        Color highlight = new Color(0.9f, 0.92f, 0.95f);
        Color clear = new Color(0, 0, 0, 0);

        // Clear
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        int shaftX = w / 2;
        int thickness = 4;

        // Straight shaft from top down to curve start (top 60% of sprite)
        int curveStartY = (int)(h * 0.35f);
        for (int y = h - 1; y >= curveStartY; y--)
        {
            for (int dx = -thickness; dx <= thickness; dx++)
            {
                int px = shaftX + dx;
                if (px >= 0 && px < w)
                {
                    // Add highlight on one side for 3D effect
                    Color c = dx <= 0 ? highlight : silver;
                    tex.SetPixel(px, y, c);
                }
            }
        }

        // Curved J-hook at the bottom
        int curveRadius = 14;
        int curveCX = shaftX + curveRadius;
        int curveCY = curveStartY;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - curveCX;
                float dy = y - curveCY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist >= curveRadius - thickness && dist <= curveRadius + thickness)
                {
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    // Draw the bottom curve (J shape)
                    if (angle >= 80f || angle <= -90f)
                    {
                        Color c = dx <= 0 ? highlight : silver;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
        }

        // Hook point / barb (sharp tip pointing up-right)
        int tipX = curveCX;
        int tipY = curveCY - curveRadius;
        for (int i = 0; i < 12; i++)
        {
            for (int dx = -thickness; dx <= thickness + 1; dx++)
            {
                int px = tipX + dx + i / 3;
                int py = tipY + i;
                if (px >= 0 && px < w && py >= 0 && py < h)
                    tex.SetPixel(px, py, silver);
            }
        }

        // Small barb (angled line from tip)
        for (int i = 0; i < 8; i++)
        {
            int px = tipX - i / 2;
            int py = tipY + i;
            for (int dx = -2; dx <= 2; dx++)
            {
                if (px + dx >= 0 && px + dx < w && py >= 0 && py < h)
                    tex.SetPixel(px + dx, py, silver);
            }
        }

        // Eye hole at top of shaft (small circle)
        int eyeX = shaftX;
        int eyeY = h - 8;
        int eyeR = 4;
        for (int y2 = eyeY - eyeR; y2 <= eyeY + eyeR; y2++)
        {
            for (int x2 = eyeX - eyeR; x2 <= eyeX + eyeR; x2++)
            {
                if (x2 >= 0 && x2 < w && y2 >= 0 && y2 < h)
                {
                    float d = Mathf.Sqrt((x2 - eyeX) * (x2 - eyeX) + (y2 - eyeY) * (y2 - eyeY));
                    if (d <= eyeR && d >= eyeR - 2)
                        tex.SetPixel(x2, y2, highlight);
                }
            }
        }

        tex.Apply();
        _hookSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.95f), 32f);
        _hookSprite.name = "FishingHook";
        return _hookSprite;
    }

    public static Sprite GetBucketSprite()
    {
        if (_bucketSprite != null) return _bucketSprite;

        int w = 64, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color bucketColor = new Color(0.6f, 0.4f, 0.25f);
        Color waterColor = new Color(0.2f, 0.5f, 0.7f, 0.8f);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / h;
                float halfW = Mathf.Lerp(24f, 30f, t);
                float cx = w * 0.5f;

                if (Mathf.Abs(x - cx) <= halfW)
                {
                    if (Mathf.Abs(x - cx) > halfW - 4f)
                        tex.SetPixel(x, y, bucketColor);
                    else if (y < 8)
                        tex.SetPixel(x, y, bucketColor);
                    else if (y > h * 0.35f)
                        tex.SetPixel(x, y, waterColor);
                    else
                        tex.SetPixel(x, y, clear);
                }
                else
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }

        tex.Apply();
        _bucketSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        _bucketSprite.name = "Bucket";
        return _bucketSprite;
    }
}
