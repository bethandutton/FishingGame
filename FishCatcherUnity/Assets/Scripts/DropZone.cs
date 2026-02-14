using UnityEngine;
using TMPro;

public class DropZone : MonoBehaviour
{
    [SerializeField] private Claw claw;
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the claw is carrying a fish
        if (claw != null && claw.HasFish())
        {
            claw.DropFishInBucket();
            gameManager.OnFishDropped();
            ShowScorePopup();
        }
    }

    private void ShowScorePopup()
    {
        // Create floating "+1" text
        GameObject popup = new GameObject("ScorePopup");
        popup.transform.position = transform.position + Vector3.up * 0.5f;

        TextMeshPro tmp = popup.AddComponent<TextMeshPro>();
        tmp.text = "+1";
        tmp.fontSize = 6;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.2f, 1f, 0.4f);
        tmp.sortingOrder = 10;

        Destroy(popup, 0.5f);

        // Animate upward
        popup.AddComponent<FloatUpAndFade>();
    }
}
