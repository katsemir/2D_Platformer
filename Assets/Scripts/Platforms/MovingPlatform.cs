using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis
    {
        Horizontal,
        Vertical
    }

    [Header("Movement")]
    public MoveAxis moveAxis = MoveAxis.Horizontal;
    public float moveDistance = 3f;
    public float moveSpeed = 2f;
    public bool startFromCurrentPosition = true;

    [Header("Difficulty")]
    public DynamicDifficultyManager difficultyManager;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool movingToTarget = true;
    private bool isShuttingDown = false;
    private float baseMoveSpeed;

    void Start()
    {
        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<DynamicDifficultyManager>();
        }

        startPosition = transform.position;
        baseMoveSpeed = moveSpeed;
        UpdateTargetPosition();
    }

    void Update()
    {
        ApplyDifficultySpeed();

        Vector3 destination = movingToTarget ? targetPosition : startPosition;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            movingToTarget = !movingToTarget;
        }
    }

    void ApplyDifficultySpeed()
    {
        if (difficultyManager == null)
        {
            moveSpeed = baseMoveSpeed;
            return;
        }

        moveSpeed = baseMoveSpeed * difficultyManager.PlatformSpeedMultiplier;
    }

    void UpdateTargetPosition()
    {
        if (moveAxis == MoveAxis.Horizontal)
        {
            targetPosition = startPosition + new Vector3(moveDistance, 0f, 0f);
        }
        else
        {
            targetPosition = startPosition + new Vector3(0f, moveDistance, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isShuttingDown)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            SafeSetParent(collision.transform, transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isShuttingDown)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            SafeSetParent(collision.transform, null);
        }
    }

    private void SafeSetParent(Transform child, Transform parent)
    {
        if (child == null)
            return;

        if (!gameObject.activeInHierarchy)
            return;

        child.SetParent(parent);
    }

    private void OnDisable()
    {
        isShuttingDown = true;
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 previewStart = transform.position;
        Vector3 previewEnd;

        if (moveAxis == MoveAxis.Horizontal)
        {
            previewEnd = previewStart + new Vector3(moveDistance, 0f, 0f);
        }
        else
        {
            previewEnd = previewStart + new Vector3(0f, moveDistance, 0f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(previewStart, previewEnd);
        Gizmos.DrawWireSphere(previewStart, 0.15f);
        Gizmos.DrawWireSphere(previewEnd, 0.15f);
    }
}