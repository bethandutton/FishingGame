using UnityEngine;

/// <summary>
/// Generates procedural claw sprites matching the Godot Polygon2D shapes.
/// </summary>
public static class ClawSpriteGenerator
{
    private static Sprite _clawBaseSprite;
    private static Sprite _clawArmSprite;
    private static Sprite _bucketSprite;

    public static Sprite GetClawBaseSprite()
    {
        if (_clawBaseSprite != null) return _clawBaseSprite;

        int w = 50, h = 30;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color baseColor = new Color(0.7f, 0.1f, 0.1f);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Trapezoidal shape: wider at top, narrower at bottom
                float topLeft = 0f;
                float topRight = (float)w;
                float botLeft = 5f;
                float botRight = w - 5f;

                float t = (float)y / h;
                float left = Mathf.Lerp(topLeft, botLeft, t);
                float right = Mathf.Lerp(topRight, botRight, t);

                tex.SetPixel(x, y, (x >= left && x <= right) ? baseColor : clear);
            }
        }

        tex.Apply();
        _clawBaseSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1f), 32f);
        _clawBaseSprite.name = "ClawBase";
        return _clawBaseSprite;
    }

    public static Sprite GetClawArmSprite()
    {
        if (_clawArmSprite != null) return _clawArmSprite;

        int w = 12, h = 36;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color armColor = new Color(0.8f, 0.2f, 0.2f);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / h;
                float halfW = Mathf.Lerp(w * 0.5f, w * 0.3f, t);
                float cx = w * 0.5f;
                tex.SetPixel(x, y, Mathf.Abs(x - cx) <= halfW ? armColor : clear);
            }
        }

        tex.Apply();
        _clawArmSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1f), 32f);
        _clawArmSprite.name = "ClawArm";
        return _clawArmSprite;
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
                // Bucket widens toward bottom
                float halfW = Mathf.Lerp(24f, 30f, t);
                float cx = w * 0.5f;

                if (Mathf.Abs(x - cx) <= halfW)
                {
                    // Bucket walls (outer 4 pixels)
                    if (Mathf.Abs(x - cx) > halfW - 4f)
                        tex.SetPixel(x, y, bucketColor);
                    else if (y < 8) // rim
                        tex.SetPixel(x, y, bucketColor);
                    else if (y > h * 0.35f) // water
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
