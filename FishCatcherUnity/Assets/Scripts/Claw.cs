using UnityEngine;

public class Claw : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float dropSpeed = 10f;
    [SerializeField] private float raiseSpeed = 8f;
    [SerializeField] private float minX = -3.2f;
    [SerializeField] private float maxX = 3.2f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = -13f;

    [Header("References")]
    [SerializeField] private Transform clawHead;
    [SerializeField] private LineRenderer rope;
    [SerializeField] private Transform clawLeft;
    [SerializeField] private Transform clawRight;
    [SerializeField] private GameManager gameManager;

    private enum ClawState { IDLE, HOLDING }
    private ClawState currentState = ClawState.IDLE;

    private bool isTouching;
    private bool isEnabled = true;
    private Fish grabbedFish;
    private Vector3 initialPosition;
    private float clawHeadLocalY;

    private void Start()
    {
        initialPosition = transform.position;
        clawHeadLocalY = 0f;
        UpdateRope();
        OpenClaw();
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    private void Update()
    {
        if (!isEnabled) return;

        HandleInput();
        HandleClawMovement();
        UpdateRope();
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
                    CloseClaw();
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
                    if (grabbedFish == null) TryGrabFish();
                    break;
            }
            return;
        }

        // Mouse fallback
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
            CloseClaw();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isTouching = false;
            if (grabbedFish == null) TryGrabFish();
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

    private void HandleClawMovement()
    {
        if (isTouching)
        {
            // Move claw head down
            clawHeadLocalY -= dropSpeed * Time.deltaTime;
            clawHeadLocalY = Mathf.Max(clawHeadLocalY, maxY);
        }
        else
        {
            // Move claw head up
            clawHeadLocalY += raiseSpeed * Time.deltaTime;

            // Check for bucket while rising with a fish
            if (grabbedFish != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(clawHead.position, 1.5f);
                foreach (var hit in hits)
                {
                    DropZone dz = hit.GetComponent<DropZone>();
                    if (dz != null)
                    {
                        Destroy(grabbedFish.gameObject);
                        grabbedFish = null;
                        if (gameManager != null) gameManager.OnFishDropped();
                        currentState = ClawState.IDLE;
                        OpenClaw();
                        break;
                    }
                }
            }

            if (clawHeadLocalY >= 0f)
            {
                clawHeadLocalY = 0f;
                if (grabbedFish != null)
                {
                    // Reached top without hitting bucket - release fish
                    grabbedFish.Release();
                    grabbedFish = null;
                }
                currentState = ClawState.IDLE;
                OpenClaw();
            }
        }

        clawHead.localPosition = new Vector3(0, clawHeadLocalY, 0);
    }

    private void UpdateRope()
    {
        if (rope == null) return;
        rope.SetPosition(0, new Vector3(0, 1.5f, 0)); // top anchor (local)
        rope.SetPosition(1, clawHead.localPosition);
    }

    private void UpdateGrabbedFish()
    {
        if (grabbedFish != null)
            grabbedFish.transform.position = clawHead.position + Vector3.down * 0.3f;
    }

    private void TryGrabFish()
    {
        // Search below the claw head where the hook tip is
        Vector3 grabPoint = clawHead.position + Vector3.down * 0.4f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(grabPoint, 0.8f);
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
        // Bring fish in front of everything so it's visible
        SpriteRenderer sr = fish.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 12;
        CloseClaw();
    }

    public bool HasFish()
    {
        return grabbedFish != null;
    }

    public void DropFishInBucket()
    {
        if (grabbedFish != null)
            Destroy(grabbedFish.gameObject);
        grabbedFish = null;
        OpenClaw();
        currentState = ClawState.IDLE;
    }

    private void OpenClaw()
    {
        // Hook doesn't open/close - no-op
    }

    private void CloseClaw()
    {
        // Hook doesn't open/close - no-op
    }

    public void ResetClaw()
    {
        transform.position = initialPosition;
        clawHeadLocalY = 0f;
        if (clawHead != null) clawHead.localPosition = Vector3.zero;
        currentState = ClawState.IDLE;
        isTouching = false;
        if (grabbedFish != null) Destroy(grabbedFish.gameObject);
        grabbedFish = null;
        OpenClaw();
        UpdateRope();
    }
}
