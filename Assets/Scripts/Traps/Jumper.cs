using UnityEngine;

public class Jumper : MonoBehaviour
{
    [Header("Bounce")]
    public float bounceForce = 14f;

    [Header("Detection")]
    public string playerTag = "Player";

    [Header("Animation")]
    public Animator animator;
    public string triggerName = "Jump";

    private bool isActivated = false;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isActivated)
            return;

        if (!collision.gameObject.CompareTag(playerTag))
            return;

        Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        ContactPoint2D contact = collision.GetContact(0);

        bool playerIsAbove = contact.normal.y < -0.5f;

        if (!playerIsAbove)
            return;

        ActivateJumper(playerRb);
    }

    private void ActivateJumper(Rigidbody2D playerRb)
    {
        isActivated = true;

        Vector2 velocity = playerRb.linearVelocity;
        velocity.y = bounceForce;
        playerRb.linearVelocity = velocity;

        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    public void ResetJumper()
    {
        isActivated = false;
    }
}