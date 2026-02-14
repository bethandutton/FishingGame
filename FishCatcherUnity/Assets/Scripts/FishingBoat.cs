using UnityEngine;
using TMPro;

public class FishingBoat : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float castSpeed = 10f;
    [SerializeField] private float reelSpeed = 8f;
    [SerializeField] private float minX = -3.5f;
    [SerializeField] private float maxX = 3.5f;
    [SerializeField] private float maxRodLength = -15f;

    [Header("Grab Settings")]
    [SerializeField] private float grabRadius = 0.6f;

    [Header("References")]
    [SerializeField] private Transform rodTip;
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private SpriteRenderer boatRenderer;
    [SerializeField] private GameManager gameManager;

    [Header("Rod Anchor")]
    [SerializeField] private Vector3 rodAnchorOffset = new Vector3(-1f, -0.5f, 0f);

    private enum BoatState { IDLE, CASTING, RETRACTING }
    private BoatState currentState = BoatState.IDLE;

    private bool isTouching;
    private bool isEnabled = true;
    private Fish grabbedFish;
    private Vector3 initialPosition;
    private float rodTipLocalY;

    private void Start()
    {
        initialPosition = transform.position;
        rodTipLocalY = 0f;
        UpdateLine();
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    private void Update()
    {
        if (!isEnabled) return;

        HandleInput();
        HandleRodMovement();
        UpdateLine();
        UpdateGrabbedFish();
    }

    private void HandleInput()
    {
        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isTouching = true;
                    if (currentState == BoatState.IDLE)
                        currentState = BoatState.CASTING;
                    break;
                case TouchPhase.Moved:
                    if (isTouching)
                    {
                        Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(touch.deltaPosition.x, 0, 0))
                            - Camera.main.ScreenToWorldPoint(Vector3.zero);
                        transform.position += new Vector3(worldDelta.x, 0, 0);
                        ClampPosition();
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    if (currentState == BoatState.CASTING)
                        currentState = BoatState.RETRACTING;
                    break;
            }
            return;
        }

        // Mouse fallback
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
            if (currentState == BoatState.IDLE)
                currentState = BoatState.CASTING;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isTouching = false;
            if (currentState == BoatState.CASTING)
                currentState = BoatState.RETRACTING;
        }

        if (Input.GetMouseButton(0) && isTouching)
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.position += new Vector3(mouseX * 0.3f, 0, 0);
            ClampPosition();
        }
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    private void HandleRodMovement()
    {
        switch (currentState)
        {
            case BoatState.CASTING:
                // Extend rod downward
                rodTipLocalY -= castSpeed * Time.deltaTime;
                rodTipLocalY = Mathf.Max(rodTipLocalY, maxRodLength);

                // Auto-grab: check for fish every frame while casting
                if (grabbedFish == null)
                    TryGrabFish();

                // If we grabbed a fish, start retracting
                if (grabbedFish != null)
                    currentState = BoatState.RETRACTING;
                break;

            case BoatState.RETRACTING:
                // Reel rod back up
                rodTipLocalY += reelSpeed * Time.deltaTime;

                if (rodTipLocalY >= 0f)
                {
                    rodTipLocalY = 0f;

                    if (grabbedFish != null)
                    {
                        // Fish reached the boat — score!
                        Destroy(grabbedFish.gameObject);
                        grabbedFish = null;
                        if (gameManager != null) gameManager.OnFishDropped();
                        ShowScorePopup();
                    }

                    currentState = BoatState.IDLE;
                }
                break;

            case BoatState.IDLE:
                // Nothing to do
                break;
        }

        if (rodTip != null)
            rodTip.localPosition = new Vector3(rodAnchorOffset.x, rodAnchorOffset.y + rodTipLocalY, 0);
    }

    private void UpdateLine()
    {
        if (fishingLine == null || rodTip == null) return;
        // Line goes from rod anchor on boat to rod tip
        fishingLine.SetPosition(0, rodAnchorOffset);
        fishingLine.SetPosition(1, rodTip.localPosition);
    }

    private void UpdateGrabbedFish()
    {
        if (grabbedFish != null && rodTip != null)
            grabbedFish.transform.position = rodTip.position + Vector3.down * 0.3f;
    }

    private void TryGrabFish()
    {
        if (rodTip == null) return;

        Vector3 grabPoint = rodTip.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(grabPoint, grabRadius);
        float closestDist = float.MaxValue;
        Fish closestFish = null;

        foreach (var hit in hits)
        {
            Fish fish = hit.GetComponentInParent<Fish>();
            if (fish != null && !fish.IsGrabbed)
            {
                float dist = Vector2.Distance(grabPoint, fish.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestFish = fish;
                }
            }
        }

        if (closestFish != null)
            GrabFish(closestFish);
    }

    private void GrabFish(Fish fish)
    {
        grabbedFish = fish;
        fish.Grab();
        SpriteRenderer sr = fish.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 12;
    }

    private void ShowScorePopup()
    {
        GameObject popup = new GameObject("ScorePopup");
        popup.transform.position = transform.position + Vector3.down * 0.5f;

        TextMeshPro tmp = popup.AddComponent<TextMeshPro>();
        tmp.text = "+1";
        tmp.fontSize = 6;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.2f, 1f, 0.4f);
        tmp.sortingOrder = 10;

        popup.AddComponent<FloatUpAndFade>();
    }

    public void ResetBoat()
    {
        transform.position = initialPosition;
        rodTipLocalY = 0f;
        if (rodTip != null) rodTip.localPosition = new Vector3(rodAnchorOffset.x, rodAnchorOffset.y, 0);
        currentState = BoatState.IDLE;
        isTouching = false;
        if (grabbedFish != null) Destroy(grabbedFish.gameObject);
        grabbedFish = null;
        UpdateLine();
    }
}
