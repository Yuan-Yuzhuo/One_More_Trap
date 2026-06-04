using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour
{
    public float speed = 2f;
    public float chaseSpeed = 3f;
    public float chaseRange = 4f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.8f;

    public int maxHealth = 2;

    public float hurtFlashTime = 0.1f;
    public float hurtDuration = 0.2f;

    private int direction = 1;
    private int currentHealth;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float attackTimer = 0f;
    private bool isHurt = false;
    private float hurtTimer = 0f;
    private bool hasGroundAhead = true;

    public Transform groundCheck;
    public float checkDistance = 0.5f;
    public LayerMask groundLayer;

    public LayerMask playerLayer;
    public Transform attackPoint;
    public GameObject attackFxPrefab;
    public float attackFxDuration = 0.2f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Handles patrol, chase, attack cooldown, hurt stun, and edge detection each frame.
    void Update()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        if (isHurt)
        {
            hurtTimer -= Time.deltaTime;

            if (hurtTimer <= 0f)
            {
                isHurt = false;
            }

            return;
        }

        float moveSpeed = speed;

        Collider2D playerHit = Physics2D.OverlapCircle(transform.position, chaseRange, playerLayer);
        if (playerHit != null)
        {
            direction = playerHit.transform.position.x >= transform.position.x ? 1 : -1;
            moveSpeed = chaseSpeed;

            float dist = Vector2.Distance(transform.position, playerHit.transform.position);
            if (dist <= attackRange && attackTimer <= 0f)
            {
                TriggerAttackFx();
                attackTimer = attackCooldown;
            }
        }

        if (rb != null)
        {
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);
        }

        if (groundCheck != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                checkDistance,
                groundLayer
            );

            if (!hit && hasGroundAhead)
            {
                hasGroundAhead = false;
                Flip();
            }
            else if (hit)
            {
                hasGroundAhead = true;
            }
        }
    }

    // Reverses facing and movement direction unless the enemy is in hurt stun.
    void Flip()
    {
        if (isHurt) return;

        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    // Flips patrol direction when hitting another enemy or a wall-like ground object.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Ground"))
        {
            Flip();
        }
    }

    // Applies damage, knockback, hit feedback, and destruction when health is depleted.
    public void TakeDamage(int amount, Vector2 knockback)
    {
        currentHealth -= amount;

        isHurt = true;
        hurtTimer = hurtDuration;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(HurtFlash());
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Plays the enemy attack animation and optional attack visual effect.
    void TriggerAttackFx()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackFxPrefab != null)
        {
            Vector3 pos = attackPoint != null ? attackPoint.position : transform.position;
            GameObject fx = Instantiate(attackFxPrefab, pos, Quaternion.identity);

            Vector3 scale = fx.transform.localScale;
            scale.x = transform.localScale.x >= 0f ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            fx.transform.localScale = scale;

            Destroy(fx, attackFxDuration);
        }
    }

    // Temporarily flashes the sprite red after taking damage.
    private IEnumerator HurtFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(hurtFlashTime);
        spriteRenderer.color = originalColor;
    }

    // Draws the edge-check ray in the editor for patrol tuning.
    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * checkDistance
            );
        }
    }

    // Draws chase and attack ranges in the editor.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
