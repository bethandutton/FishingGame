using UnityEngine;

/// <summary>
/// Generates procedural pixel art sprites for the underwater background.
/// All sprites use FilterMode.Point for 8-bit retro aesthetic.
/// </summary>
public static class BackgroundGenerator
{
    private static Sprite _waterGradientSprite;
    private static Sprite _sandSprite;
    private static Sprite _seaweedSprite;
    private static Sprite _coralSmallSprite;
    private static Sprite _coralLargeSprite;
    private static Sprite _bubbleSprite;
    private static Sprite _lightRaySprite;

    public static void ClearCache()
    {
        _waterGradientSprite = null;
        _sandSprite = null;
        _seaweedSprite = null;
        _coralSmallSprite = null;
        _coralLargeSprite = null;
        _bubbleSprite = null;
        _lightRaySprite = null;
    }

    public static Sprite GetWaterGradientSprite()
    {
        if (_waterGradientSprite != null) return _waterGradientSprite;

        int w = 1, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color deep    = new Color(0.02f, 0.05f, 0.15f);
        Color darkMid = new Color(0.05f, 0.12f, 0.28f);
        Color midBlue = new Color(0.10f, 0.30f, 0.50f);
        Color surface = new Color(0.18f, 0.50f, 0.72f);

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            Color c;
            if (t < 0.25f)
                c = Color.Lerp(deep, darkMid, t / 0.25f);
            else if (t < 0.65f)
                c = Color.Lerp(darkMid, midBlue, (t - 0.25f) / 0.4f);
            else
                c = Color.Lerp(midBlue, surface, (t - 0.65f) / 0.35f);
            tex.SetPixel(0, y, c);
        }

        tex.Apply();
        _waterGradientSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 8f);
        _waterGradientSprite.name = "WaterGradient";
        return _waterGradientSprite;
    }

    public static Sprite GetSandSprite()
    {
        if (_sandSprite != null) return _sandSprite;

        int w = 128, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear     = new Color(0, 0, 0, 0);
        Color sandBase  = new Color(0.76f, 0.65f, 0.40f);
        Color sandDark  = new Color(0.60f, 0.48f, 0.28f);
        Color sandLight = new Color(0.88f, 0.78f, 0.52f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float edge = 10f + Mathf.Sin(x * 0.5f) * 1.5f + Mathf.Sin(x * 0.3f) * 1f;
                if (y > (int)edge)
                {
                    tex.SetPixel(x, y, clear);
                    continue;
                }

                int hash = (x * 7 + y * 13) % 100;
                Color c = sandBase;
                if (hash < 15) c = sandDark;
                else if (hash < 25) c = sandLight;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        _sandSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1f), 16f);
        _sandSprite.name = "Sand";
        return _sandSprite;
    }

    public static Sprite GetSeaweedSprite()
    {
        if (_seaweedSprite != null) return _seaweedSprite;

        int w = 16, h = 48;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear = new Color(0, 0, 0, 0);
        Color stem  = new Color(0.15f, 0.50f, 0.20f);
        Color leaf  = new Color(0.25f, 0.70f, 0.30f);
        Color tip   = new Color(0.35f, 0.80f, 0.40f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        tex.SetPixels(pixels);

        int cx = w / 2;
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            int sway = (int)(Mathf.Sin(y * 0.4f) * 2f);
            int stemWidth = y > h - 4 ? 1 : 2;
            Color col = t > 0.85f ? tip : stem;

            for (int dx = -stemWidth; dx <= stemWidth; dx++)
            {
                int px = cx + sway + dx;
                if (px >= 0 && px < w) tex.SetPixel(px, y, col);
            }

            // Alternating leaves every ~10 pixels
            if (y % 10 < 4 && y > 4 && y < h - 6)
            {
                int side = ((y / 10) % 2 == 0) ? 1 : -1;
                for (int lx = 1; lx <= 4 - (y % 10); lx++)
                {
                    int px = cx + sway + side * (stemWidth + lx);
                    if (px >= 0 && px < w) tex.SetPixel(px, y, leaf);
                }
            }
        }

        tex.Apply();
        _seaweedSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
        _seaweedSprite.name = "Seaweed";
        return _seaweedSprite;
    }

    public static Sprite GetCoralSprite(bool large)
    {
        if (large && _coralLargeSprite != null) return _coralLargeSprite;
        if (!large && _coralSmallSprite != null) return _coralSmallSprite;

        int w = large ? 16 : 12;
        int h = large ? 12 : 8;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear     = new Color(0, 0, 0, 0);
        Color coralPink = new Color(0.90f, 0.40f, 0.45f);
        Color coralRed  = new Color(0.75f, 0.25f, 0.30f);
        Color coralHi   = new Color(1.0f, 0.70f, 0.60f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        tex.SetPixels(pixels);

        if (large)
        {
            // Fan coral with branches
            int cx = w / 2;
            // Central trunk
            for (int y = 0; y < h; y++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int px = cx + dx;
                    if (px >= 0 && px < w) tex.SetPixel(px, y, coralPink);
                }
            }
            // Left branch
            for (int i = 0; i < 5; i++)
            {
                int px = cx - 2 - i;
                int py = 4 + i;
                if (px >= 0 && py < h) tex.SetPixel(px, py, coralPink);
                if (px + 1 >= 0 && px + 1 < w && py < h) tex.SetPixel(px + 1, py, coralHi);
            }
            // Right branch
            for (int i = 0; i < 5; i++)
            {
                int px = cx + 2 + i;
                int py = 5 + i;
                if (px < w && py < h) tex.SetPixel(px, py, coralPink);
                if (px - 1 >= 0 && py < h) tex.SetPixel(px - 1, py, coralHi);
            }
            // Small branch left
            for (int i = 0; i < 3; i++)
            {
                int px = cx - 1 - i;
                int py = 8 + i;
                if (px >= 0 && py < h) tex.SetPixel(px, py, coralRed);
            }
        }
        else
        {
            // Small rounded mound coral
            int cx = w / 2;
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                int halfW = (int)(cx * (0.5f + t * 0.5f));
                if (y > h * 0.7f) halfW = (int)(cx * (1f - (t - 0.7f) * 2f));

                for (int x = cx - halfW; x <= cx + halfW; x++)
                {
                    if (x >= 0 && x < w)
                    {
                        int hash = (x * 11 + y * 7) % 100;
                        Color c = coralPink;
                        if (hash < 20) c = coralRed;
                        else if (hash < 35) c = coralHi;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
        }

        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
        sprite.name = large ? "CoralLarge" : "CoralSmall";

        if (large) _coralLargeSprite = sprite;
        else _coralSmallSprite = sprite;

        return sprite;
    }

    public static Sprite GetBubbleSprite()
    {
        if (_bubbleSprite != null) return _bubbleSprite;

        int size = 8;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear     = new Color(0, 0, 0, 0);
        Color outline   = new Color(0.6f, 0.8f, 1f, 0.5f);
        Color highlight = new Color(0.9f, 0.95f, 1f, 0.7f);
        float cx = 3.5f, cy = 3.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist >= 2.2f && dist <= 3.5f)
                    tex.SetPixel(x, y, outline);
                else
                    tex.SetPixel(x, y, clear);
            }
        }

        tex.SetPixel(2, 5, highlight);
        tex.SetPixel(2, 6, highlight);

        tex.Apply();
        _bubbleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        _bubbleSprite.name = "Bubble";
        return _bubbleSprite;
    }

    public static Sprite GetLightRaySprite()
    {
        if (_lightRaySprite != null) return _lightRaySprite;

        int w = 16, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        tex.SetPixels(pixels);

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / h;
            float alpha = t * 0.12f;
            float centerX = 8f + (1f - t) * 4f;
            int bandHalf = 2 + (int)(t * 1.5f);

            for (int x = (int)centerX - bandHalf; x <= (int)centerX + bandHalf; x++)
            {
                if (x >= 0 && x < w)
                    tex.SetPixel(x, y, new Color(0.7f, 0.85f, 1f, alpha));
            }
        }

        tex.Apply();
        _lightRaySprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1f), 8f);
        _lightRaySprite.name = "LightRay";
        return _lightRaySprite;
    }
}
