using UnityEngine;

/// <summary>
/// Attach to the FishPrefab root to auto-assign the procedural fish sprite on Awake.
/// This ensures fish get their sprite before Initialize() is called.
/// </summary>
[RequireComponent(typeof(Fish))]
public class ProceduralSetup : MonoBehaviour
{
    private void Awake()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
            sr.sprite = FishSpriteGenerator.GetFishSprite();
    }
}
