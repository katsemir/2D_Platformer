using UnityEngine;

public class MushroomProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1;
    public float lifeTime = 4f;
    public float defaultSpeed = 8f;

    private float direction = 1f;
    private float speed;
    private SpriteRenderer spriteRenderer;
    private bool initialized = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        speed = defaultSpeed;
    }

    void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!initialized)
            return;

        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    public void Initialize(float newDirection, float newSpeed)
    {
        Initialize(newDirection, newSpeed, damage);
    }

    public void Initialize(float newDirection, float newSpeed, int newDamage)
    {
        direction = newDirection;
        speed = newSpeed > 0f ? newSpeed : defaultSpeed;
        damage = Mathf.Max(0, newDamage);
        initialized = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized)
            return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = collision.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }
    }
}