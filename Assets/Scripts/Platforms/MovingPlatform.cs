using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis
    {
        Horizontal,
        Vertical
    }

    public enum StartDirection
    {
        Positive,
        Negative
    }

    [Header("Movement")]
    public MoveAxis moveAxis = MoveAxis.Horizontal;

    [Tooltip("Positive: Horizontal = Right, Vertical = Up. Negative: Horizontal = Left, Vertical = Down.")]
    public StartDirection startDirection = StartDirection.Positive;

    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public bool startFromCurrentPosition = true;

    [Header("Difficulty")]
    public DynamicDifficultyManager difficultyManager;

    private Rigidbody2D rb;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool movingToTarget = true;

    private float baseMoveSpeed;

    private Rigidbody2D playerRb;
    private bool playerOnPlatform = false;

    private Vector2 previousPlatformPosition;
    private Vector2 platformDelta;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }

        startPosition = rb.position;
        previousPlatformPosition = rb.position;

        baseMoveSpeed = moveSpeed;

        UpdateTargetPosition();
    }

    private void FixedUpdate()
    {
        ApplyDifficultySpeed();
        MovePlatform();
        MovePlayerWithPlatform();
    }

    private void MovePlatform()
    {
        Vector2 destination = movingToTarget ? targetPosition : startPosition;

        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            destination,
            moveSpeed * Time.fixedDeltaTime
        );

        platformDelta = newPosition - rb.position;

        rb.MovePosition(newPosition);

        if (Vector2.Distance(newPosition, destination) < 0.01f)
        {
            movingToTarget = !movingToTarget;
        }

        previousPlatformPosition = newPosition;
    }

    private void MovePlayerWithPlatform()
    {
        if (!playerOnPlatform)
            return;

        if (playerRb == null)
            return;

        playerRb.position += platformDelta;
    }

    private void ApplyDifficultySpeed()
    {
        if (difficultyManager == null)
        {
            moveSpeed = baseMoveSpeed;
            return;
        }

        moveSpeed = baseMoveSpeed * difficultyManager.PlatformSpeedMultiplier;
    }

    private void UpdateTargetPosition()
    {
        float distance = Mathf.Abs(moveDistance);
        float direction = startDirection == StartDirection.Positive ? 1f : -1f;

        if (moveAxis == MoveAxis.Horizontal)
        {
            targetPosition = startPosition + new Vector2(distance * direction, 0f);
        }
        else
        {
            targetPosition = startPosition + new Vector2(0f, distance * direction);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TrySetPlayerOnPlatform(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TrySetPlayerOnPlatform(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rigidbody2D exitingRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (exitingRb == playerRb)
        {
            playerOnPlatform = false;
            playerRb = null;
        }
    }

    private void TrySetPlayerOnPlatform(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (!IsPlayerStandingOnTop(collision))
            return;

        playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        playerOnPlatform = playerRb != null;
    }

    private bool IsPlayerStandingOnTop(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (contact.normal.y < -0.5f)
            {
                return true;
            }
        }

        return collision.transform.position.y > transform.position.y;
    }

    private void OnDisable()
    {
        playerOnPlatform = false;
        playerRb = null;
    }

    private void OnDestroy()
    {
        playerOnPlatform = false;
        playerRb = null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 previewStart = transform.position;
        Vector3 previewEnd;

        float distance = Mathf.Abs(moveDistance);
        float direction = startDirection == StartDirection.Positive ? 1f : -1f;

        if (moveAxis == MoveAxis.Horizontal)
        {
            previewEnd = previewStart + new Vector3(distance * direction, 0f, 0f);
        }
        else
        {
            previewEnd = previewStart + new Vector3(0f, distance * direction, 0f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(previewStart, previewEnd);
        Gizmos.DrawWireSphere(previewStart, 0.15f);
        Gizmos.DrawWireSphere(previewEnd, 0.15f);
    }
}