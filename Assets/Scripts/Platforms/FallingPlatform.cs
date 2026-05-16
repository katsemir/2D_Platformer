using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Timing")]
    public float delayBeforeFall = 0.5f;
    public float destroyAfterFall = 0.5f;

    [Header("Physics")]
    public float fallGravityScale = 3f;

    private Rigidbody2D rb;
    private bool hasTriggered = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("FallingPlatform: Rigidbody2D is missing on " + gameObject.name);
            return;
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        hasTriggered = true;
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFall);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(destroyAfterFall);

        Destroy(gameObject);
    }
}