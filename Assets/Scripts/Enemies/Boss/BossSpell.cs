using UnityEngine;

public class BossSpell : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;
    public float lifeTime = 2f;

    [Header("Visual Fix")]
    public int sortingOrder = 50;
    public bool forceVisible = true;

    private bool hasDealtDamage = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D spellCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spellCollider = GetComponent<Collider2D>();

        if (forceVisible)
        {
            MakeVisible();
        }
    }

    private void Start()
    {
        if (lifeTime <= 0f)
        {
            lifeTime = 2f;
        }

        Destroy(gameObject, lifeTime);
    }

    public void Initialize(int newDamage, float newLifeTime)
    {
        damage = Mathf.Max(0, newDamage);
        lifeTime = Mathf.Max(0.2f, newLifeTime);

        if (forceVisible)
        {
            MakeVisible();
        }

        CancelInvoke();
        Destroy(gameObject, lifeTime);
    }

    private void MakeVisible()
    {
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) < 0.01f ? 1f : transform.localScale.x,
            Mathf.Abs(transform.localScale.y) < 0.01f ? 1f : transform.localScale.y,
            Mathf.Abs(transform.localScale.z) < 0.01f ? 1f : transform.localScale.z
        );

        Vector3 position = transform.position;
        position.z = 0f;
        transform.position = position;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;

            spriteRenderer.sortingOrder = sortingOrder;
        }

        if (spellCollider != null)
        {
            spellCollider.enabled = true;
            spellCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDamagePlayer(collision);
    }

    private void TryDamagePlayer(Collider2D collision)
    {
        if (hasDealtDamage)
            return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth = collision.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        hasDealtDamage = true;
        playerHealth.TakeDamage(damage);
    }

    public void DestroySpell()
    {
        Destroy(gameObject);
    }
}