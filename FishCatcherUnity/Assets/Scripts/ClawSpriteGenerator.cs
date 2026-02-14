using UnityEngine;

/// <summary>
/// Generates procedural hook and bucket sprites.
/// </summary>
public static class ClawSpriteGenerator
{
    private static Sprite _hookSprite;
    private static Sprite _bucketSprite;

    public static Sprite GetClawBaseSprite()
    {
        return GetHookSprite();
    }

    public static Sprite GetClawArmSprite()
    {
        // No longer used - hook replaces claw arms
        return GetHookSprite();
    }

    public static Sprite GetHookSprite()
    {
        if (_hookSprite != null) return _hookSprite;

        int w = 32, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color hookColor = new Color(0.75f, 0.75f, 0.78f);
        Color clear = new Color(0, 0, 0, 0);

        // Clear all pixels
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);

        int cx = w / 2;
        int thickness = 3;

        // Straight shaft from top down to the curve start
        for (int y = h - 1; y >= h / 3; y--)
        {
            for (int x = cx - thickness; x <= cx + thickness; x++)
            {
                if (x >= 0 && x < w)
                    tex.SetPixel(x, y, hookColor);
            }
        }

        // Curved hook at the bottom (J shape)
        int curveRadius = 8;
        int curveCenterX = cx + curveRadius;
        int curveCenterY = h / 3;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - curveCenterX;
                float dy = y - curveCenterY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Draw the curve (bottom half of circle, left side)
                if (dist >= curveRadius - thickness && dist <= curveRadius + thickness)
                {
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    // Bottom-left quadrant of the circle (90 to 270 degrees)
                    if (angle >= 90f || angle <= -90f)
                        tex.SetPixel(x, y, hookColor);
                }
            }
        }

        // Hook point (small barb)
        int pointX = curveCenterX;
        int pointY = curveCenterY - curveRadius;
        for (int dy = 0; dy < 6; dy++)
        {
            for (int dx = -1; dx <= 3; dx++)
            {
                int px = pointX + dx;
                int py = pointY + dy;
                if (px >= 0 && px < w && py >= 0 && py < h)
                    tex.SetPixel(px, py, hookColor);
            }
        }

        tex.Apply();
        _hookSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1f), 32f);
        _hookSprite.name = "Hook";
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
