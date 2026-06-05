using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float airMoveSpeed = 10f;
    public float accel = 40f;
    public float decel = 50f;
    public float jumpForce = 14f;
    public float jumpCutMultiplier = 0.5f;
    public float fallMultiplier = 2.0f;
    public float lowJumpMultiplier = 1.4f;

    public float dashSpeed = 14f;
    public float dashTime = 0.15f;
    public float dashCooldown = 0.5f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private int dashDir = 1;
    private bool canAirDash = true;

    private int jumpCount = 0;
    public int maxJumpCount = 2;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private float defaultGravityScale = 3f;

    public float coyoteTime = 0.1f;
    private float coyoteTimer;

    public Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask enemyLayer;
    public int attackDamage = 1;
    public float attackKnockback = 6f;
    public GameObject attackFxPrefab;
    public float attackFxDuration = 0.2f;

    public float fallThreshold = -20f;

    public int maxHealth = 3;
    public float invincibleTime = 0.6f;
    public float hitKnockback = 8f;

    private int currentHealth;
    private bool isInvincible = false;
    private float invincibleTimer = 0f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource runAudioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private float jumpVolume = 1.6f;
    [SerializeField] private float runVolume = 0.7f;
    [SerializeField] private float deathVolume = 1f;
    [SerializeField] private float dashVolume = 1f;

    private int facingDir = 1;
    private Vector3 attackPointStartLocalPos;

    private HashSet<int> groundColliderIds = new HashSet<int>();
    private HashSet<Collider2D> ignoredGroundColliders = new HashSet<Collider2D>();

    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public LayerMask groundLayer;
    [SerializeField] private float groundTrapReleaseDepth = 0.08f;
    [SerializeField] private float groundTrapReleaseProbePadding = 0.08f;
    [SerializeField] private float groundTrapFallSpeed = 8f;

    private bool isDead = false;

    private SpacecraftPlatform currentSpacecraft;
    private Vector2 lastSpacecraftPosition;
    private Collider2D[] playerColliders;
    private Coroutine restoreGroundCollisionsRoutine;

    // Resolves component references and loads player audio clips from Resources.
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerColliders = GetComponentsInChildren<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (runAudioSource == null)
            runAudioSource = gameObject.AddComponent<AudioSource>();

        runAudioSource.playOnAwake = false;
        runAudioSource.loop = true;
        runAudioSource.volume = runVolume;

        if (jumpClip == null)
            jumpClip = Resources.Load<AudioClip>("jump");

        if (runClip == null)
            runClip = Resources.Load<AudioClip>("run");

        if (deathClip == null)
            deathClip = Resources.Load<AudioClip>("death");

        if (dashClip == null)
            dashClip = Resources.Load<AudioClip>("whoosh");

        if (runAudioSource.clip == null)
            runAudioSource.clip = runClip;

        if (attackPoint != null)
            attackPointStartLocalPos = attackPoint.localPosition;
    }

    void Start()
    {
        currentHealth = maxHealth;
        defaultGravityScale = rb.gravityScale;
    }

    // Handles player input, movement, jump, dash, attack, animation state, and fall death.
    void Update()
    {
        if (isDead)
            return;

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        UpdateInvincible();

        float move = PlayerInputConfig.GetHorizontalMove();

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(move));
            animator.SetBool("Grounded", isGrounded);
        }

        if (Mathf.Abs(move) > 0.01f)
            SetFacing(move > 0 ? 1 : -1);

        UpdateRunSound(move);

        if (!isDashing)
        {
            if (isGrounded)
            {
                float targetSpeed = move * speed;
                float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel;

                float newSpeed = Mathf.MoveTowards(
                    rb.velocity.x,
                    targetSpeed,
                    accelRate * Time.deltaTime
                );

                rb.velocity = new Vector2(newSpeed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(move * airMoveSpeed, rb.velocity.y);
            }
        }

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (!isDashing &&
            Input.GetKeyDown(PlayerInputConfig.JumpKey) &&
            (jumpCount < maxJumpCount || coyoteTimer > 0f))
        {
            float currentJumpForce = jumpForce;

            if (jumpCount == 1)
            {
                currentJumpForce = jumpForce * 0.6f;
                GameStatsTracker.RegisterDoubleJumpUse();
            }

            rb.velocity = new Vector2(rb.velocity.x, 0f);

            rb.AddForce(
                Vector2.up * currentJumpForce,
                ForceMode2D.Impulse
            );

            if (animator != null)
                animator.SetTrigger("Jump");

            PlayJumpSound();

            jumpCount++;
            coyoteTimer = 0f;
        }

        if (Input.GetKeyUp(PlayerInputConfig.JumpKey) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                rb.velocity.y * jumpCutMultiplier
            );
        }

        if (!isDashing &&
            dashCooldownTimer <= 0f &&
            Input.GetKeyDown(PlayerInputConfig.DashKey))
        {
            if (isGrounded || canAirDash)
            {
                StartDash(move);
            }

            if (animator != null)
                animator.SetTrigger("Rush");
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            rb.velocity = new Vector2(
                dashDir * dashSpeed,
                0f
            );

            if (dashTimer <= 0f)
                EndDash();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        if (transform.position.y < fallThreshold)
        {
            Die();
        }
    }

    // Tracks ground contacts, resets jump/dash resources, and applies enemy collision damage.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            int colId =
                collision.collider != null
                ? collision.collider.GetInstanceID()
                : collision.gameObject.GetInstanceID();

            bool hasUpContact = false;

            if (collision.contacts != null &&
                collision.contacts.Length > 0)
            {
                for (int i = 0; i < collision.contacts.Length; i++)
                {
                    if (collision.contacts[i].normal.y > 0.5f)
                    {
                        hasUpContact = true;
                        break;
                    }
                }
            }

            if (hasUpContact)
            {
                if (!groundColliderIds.Contains(colId))
                {
                    groundColliderIds.Add(colId);
                }

                isGrounded = true;
                jumpCount = 0;
                canAirDash = true;

                SpacecraftPlatform spacecraft =
                    collision.collider.GetComponentInParent<SpacecraftPlatform>();

                if (spacecraft != null)
                {
                    currentSpacecraft = spacecraft;
                    lastSpacecraftPosition = spacecraft.transform.position;
                }
            }
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector2 hitDir =
                (transform.position - collision.transform.position).normalized;

            TakeDamage(1, hitDir * hitKnockback);
        }
    }

    // Keeps grounded state stable while the player remains on one or more ground colliders.
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            int colId =
                collision.collider != null
                ? collision.collider.GetInstanceID()
                : collision.gameObject.GetInstanceID();

            if (!groundColliderIds.Contains(colId))
            {
                bool hasUpContact = IsStandingOnTop(collision);

                if (hasUpContact)
                {
                    groundColliderIds.Add(colId);
                }
            }

            isGrounded = groundColliderIds.Count > 0;

            SpacecraftPlatform spacecraft =
                collision.collider.GetComponentInParent<SpacecraftPlatform>();

            if (spacecraft != null && IsStandingOnTop(collision))
            {
                currentSpacecraft = spacecraft;
                lastSpacecraftPosition = spacecraft.transform.position;
            }
        }
    }

    // Returns true when collision normals show that the player is standing on top.
    bool IsStandingOnTop(Collision2D collision)
    {
        if (collision.contacts == null || collision.contacts.Length == 0)
            return false;

        for (int i = 0; i < collision.contacts.Length; i++)
        {
            if (collision.contacts[i].normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    // Drops the player through solid ground when a moving platform wedges them into level geometry.
    public void ReleaseIfEmbeddedInGround(Collider2D activeGroundCollider)
    {
        if (isDead || activeGroundCollider == null)
            return;

        Physics2D.SyncTransforms();

        Bounds playerBounds;

        if (!TryGetPlayerBodyBounds(out playerBounds))
            return;

        if (!IsSolidGroundCollider(activeGroundCollider))
            return;

        bool activeGroundEmbedded = IsPlayerEmbeddedInGround(activeGroundCollider);

        if (!activeGroundEmbedded && !IsGroundConstrainingPlayer(activeGroundCollider))
            return;

        bool blockedByUpperGround = false;
        bool embeddedBlockerFound = false;
        List<Collider2D> groundsToIgnore = new List<Collider2D>();
        groundsToIgnore.Add(activeGroundCollider);

        Collider2D[] nearbyColliders = Physics2D.OverlapBoxAll(
            playerBounds.center,
            new Vector2(
                playerBounds.size.x + groundTrapReleaseProbePadding * 2f,
                playerBounds.size.y + groundTrapReleaseProbePadding * 2f
            ),
            0f
        );

        for (int i = 0; i < nearbyColliders.Length; i++)
        {
            Collider2D groundCollider = nearbyColliders[i];

            if (groundCollider == activeGroundCollider)
                continue;

            if (IsSolidGroundCollider(groundCollider) &&
                IsGroundBlockingPlayerBody(playerBounds, groundCollider.bounds))
            {
                blockedByUpperGround = true;

                if (IsPlayerEmbeddedInGround(groundCollider) &&
                    !groundsToIgnore.Contains(groundCollider))
                {
                    embeddedBlockerFound = true;
                    groundsToIgnore.Add(groundCollider);
                }
            }
        }

        if (!blockedByUpperGround || (!activeGroundEmbedded && !embeddedBlockerFound))
            return;

        IgnoreGroundCollisionsTemporarily(groundsToIgnore);
    }

    // Builds a combined bounds around the player's non-trigger body colliders.
    private bool TryGetPlayerBodyBounds(out Bounds playerBounds)
    {
        playerBounds = new Bounds(transform.position, Vector3.zero);
        bool hasBodyCollider = false;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider = playerColliders[i];

            if (!IsBodyCollider(playerCollider))
                continue;

            if (!hasBodyCollider)
            {
                playerBounds = playerCollider.bounds;
                hasBodyCollider = true;
            }
            else
            {
                playerBounds.Encapsulate(playerCollider.bounds);
            }
        }

        return hasBodyCollider;
    }

    // Returns true for enabled non-trigger player colliders that should block ground.
    private bool IsBodyCollider(Collider2D playerCollider)
    {
        return playerCollider != null &&
            playerCollider.enabled &&
            !playerCollider.isTrigger;
    }

    // Returns true for enabled solid ground colliders.
    private bool IsSolidGroundCollider(Collider2D groundCollider)
    {
        return groundCollider != null &&
            groundCollider.enabled &&
            !groundCollider.isTrigger &&
            groundCollider.CompareTag("Ground");
    }

    // Checks whether a ground collider is currently touching or pressing the player.
    private bool IsGroundConstrainingPlayer(Collider2D groundCollider)
    {
        if (!IsSolidGroundCollider(groundCollider))
            return false;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider = playerColliders[i];

            if (!IsBodyCollider(playerCollider))
                continue;

            ColliderDistance2D distance = Physics2D.Distance(playerCollider, groundCollider);

            if (distance.isOverlapped ||
                distance.distance <= groundTrapReleaseProbePadding)
            {
                return true;
            }
        }

        return false;
    }

    // Detects when the player's body is sunk into a ground collider instead of only touching it.
    private bool IsPlayerEmbeddedInGround(Collider2D groundCollider)
    {
        if (!IsSolidGroundCollider(groundCollider))
            return false;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider = playerColliders[i];

            if (!IsBodyCollider(playerCollider))
                continue;

            ColliderDistance2D distance = Physics2D.Distance(playerCollider, groundCollider);

            if (distance.isOverlapped &&
                distance.distance < -groundTrapReleaseDepth)
            {
                return true;
            }
        }

        return false;
    }

    // Detects a second ground surface pressing into the player's body from above or the side.
    private bool IsGroundBlockingPlayerBody(Bounds playerBounds, Bounds groundBounds)
    {
        float horizontalOverlap =
            Mathf.Min(playerBounds.max.x, groundBounds.max.x) -
            Mathf.Max(playerBounds.min.x, groundBounds.min.x);

        bool reachesPlayerBody =
            groundBounds.min.y < playerBounds.max.y + groundTrapReleaseProbePadding &&
            groundBounds.max.y > playerBounds.center.y;

        return horizontalOverlap > groundTrapReleaseDepth && reachesPlayerBody;
    }

    // Temporarily disables player-ground collision pairs and gives gravity control again.
    private void IgnoreGroundCollisionsTemporarily(List<Collider2D> groundColliders)
    {
        bool ignoredNewGround = false;

        for (int i = 0; i < groundColliders.Count; i++)
        {
            Collider2D groundCollider = groundColliders[i];

            if (!IsSolidGroundCollider(groundCollider))
                continue;

            SetGroundCollisionIgnored(groundCollider, true);

            if (!ignoredGroundColliders.Contains(groundCollider))
            {
                ignoredGroundColliders.Add(groundCollider);
                ignoredNewGround = true;
            }
        }

        if (!ignoredNewGround)
            return;

        groundColliderIds.Clear();
        isGrounded = false;
        currentSpacecraft = null;

        if (isDashing)
        {
            EndDash();
        }

        rb.velocity = new Vector2(
            rb.velocity.x,
            Mathf.Min(rb.velocity.y, -Mathf.Abs(groundTrapFallSpeed))
        );

        if (restoreGroundCollisionsRoutine == null)
        {
            restoreGroundCollisionsRoutine = StartCoroutine(RestoreIgnoredGroundCollisions());
        }
    }

    // Restores ignored ground collisions after the player has fully separated from each collider.
    private IEnumerator RestoreIgnoredGroundCollisions()
    {
        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        while (ignoredGroundColliders.Count > 0)
        {
            yield return waitForFixedUpdate;

            Collider2D[] colliders = new Collider2D[ignoredGroundColliders.Count];
            ignoredGroundColliders.CopyTo(colliders);

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D groundCollider = colliders[i];

                if (groundCollider == null ||
                    !groundCollider.enabled ||
                    !IsPlayerOverlappingGround(groundCollider))
                {
                    SetGroundCollisionIgnored(groundCollider, false);
                    ignoredGroundColliders.Remove(groundCollider);
                }
            }
        }

        restoreGroundCollisionsRoutine = null;
    }

    // Returns true while any player body collider is still inside the ignored ground.
    private bool IsPlayerOverlappingGround(Collider2D groundCollider)
    {
        if (groundCollider == null)
            return false;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider = playerColliders[i];

            if (!IsBodyCollider(playerCollider))
                continue;

            ColliderDistance2D distance = Physics2D.Distance(playerCollider, groundCollider);

            if (distance.isOverlapped)
            {
                return true;
            }
        }

        return false;
    }

    // Applies or clears Physics2D.IgnoreCollision for all player body colliders.
    private void SetGroundCollisionIgnored(Collider2D groundCollider, bool ignored)
    {
        if (groundCollider == null)
            return;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider = playerColliders[i];

            if (!IsBodyCollider(playerCollider))
                continue;

            Physics2D.IgnoreCollision(playerCollider, groundCollider, ignored);
        }
    }

    // Moves the player by the spacecraft platform delta so they ride moving platforms cleanly.
    void LateUpdate()
    {
        if (currentSpacecraft == null)
            return;

        if (!isGrounded)
            return;

        Vector2 currentPlatformPosition = currentSpacecraft.transform.position;
        Vector2 platformDelta = currentPlatformPosition - lastSpacecraftPosition;

        rb.position += platformDelta;

        lastSpacecraftPosition = currentPlatformPosition;
    }

    // Clears ground and spacecraft state when leaving collision contacts.
    private void OnCollisionExit2D(Collision2D collision)
    {
        SpacecraftPlatform spacecraft =
            collision.collider.GetComponentInParent<SpacecraftPlatform>();

        if (spacecraft != null && spacecraft == currentSpacecraft)
        {
            currentSpacecraft = null;

            float move = PlayerInputConfig.GetHorizontalMove();
            rb.velocity = new Vector2(move * airMoveSpeed, rb.velocity.y);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            int colId =
                collision.collider != null
                ? collision.collider.GetInstanceID()
                : collision.gameObject.GetInstanceID();

            if (groundColliderIds.Contains(colId))
            {
                groundColliderIds.Remove(colId);
            }

            isGrounded = groundColliderIds.Count > 0;
        }
    }

    // Updates the visual facing direction and mirrors the attack point.
    void SetFacing(int dir)
    {
        if (dir == facingDir)
            return;

        facingDir = dir;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingDir < 0;
        }

        if (attackPoint != null)
        {
            Vector3 p = attackPointStartLocalPos;
            p.x = Mathf.Abs(p.x) * facingDir;
            attackPoint.localPosition = p;
        }
    }

    // Updates invincibility timing and blink feedback after damage.
    void UpdateInvincible()
    {
        if (!isInvincible)
            return;

        invincibleTimer -= Time.deltaTime;

        float blink = Mathf.PingPong(Time.time * 12f, 1f);

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(0.35f, 1f, blink);
            spriteRenderer.color = c;
        }

        if (invincibleTimer <= 0f)
        {
            isInvincible = false;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }
    }

    // Plays the jump sound once for each successful jump.
    void PlayJumpSound()
    {
        if (audioSource == null || jumpClip == null)
            return;

        audioSource.PlayOneShot(jumpClip, jumpVolume);
    }

    // Loops the run sound only while the player is moving on the ground.
    void UpdateRunSound(float move)
    {
        if (runAudioSource == null || runClip == null)
            return;

        bool shouldPlay = Mathf.Abs(move) > 0.01f && isGrounded && !isDashing;

        if (shouldPlay)
        {
            runAudioSource.volume = runVolume;

            if (runAudioSource.clip != runClip)
                runAudioSource.clip = runClip;

            if (!runAudioSource.isPlaying)
                runAudioSource.Play();
        }
        else if (runAudioSource.isPlaying)
        {
            runAudioSource.Stop();
        }
    }

    // Performs a melee attack and applies damage to enemies inside the attack radius.
    void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackFxPrefab != null && attackPoint != null)
        {
            GameObject fx = Instantiate(
                attackFxPrefab,
                attackPoint.position,
                Quaternion.identity
            );

            Vector3 scale = fx.transform.localScale;

            scale.x =
                facingDir >= 0
                ? Mathf.Abs(scale.x)
                : -Mathf.Abs(scale.x);

            fx.transform.localScale = scale;

            Destroy(fx, attackFxDuration);
        }

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                enemyLayer
            );

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyPatrol patrol =
                enemy.GetComponent<EnemyPatrol>();

            if (patrol != null)
            {
                Vector2 dir =
                    (enemy.transform.position - transform.position).normalized;

                patrol.TakeDamage(
                    attackDamage,
                    dir * attackKnockback
                );
            }
        }
    }

    // Applies player damage, knockback, invincibility, and death when health reaches zero.
    void TakeDamage(int amount, Vector2 knockback)
    {
        rb.velocity = Vector2.zero;

        rb.AddForce(
            knockback,
            ForceMode2D.Impulse
        );

        if (isInvincible)
            return;

        currentHealth -= amount;

        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Starts a dash in the current input direction or facing direction.
    void StartDash(float inputDir)
    {
        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;

        rb.gravityScale = 0f;

        dashDir =
            inputDir != 0f
            ? (int)Mathf.Sign(inputDir)
            : facingDir;

        if (!isGrounded)
        {
            canAirDash = false;
        }

        PlayDashSound();
    }

    // Ends dash state and restores normal gravity.
    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = defaultGravityScale;
    }

    // Plays the dash sound effect.
    void PlayDashSound()
    {
        if (audioSource == null || dashClip == null)
            return;

        audioSource.PlayOneShot(dashClip, dashVolume);
    }

    // Registers death, stops player motion, plays audio feedback, and reloads the scene.
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        GameStatsTracker.RegisterDeath();

        if (runAudioSource != null && runAudioSource.isPlaying)
            runAudioSource.Stop();

        rb.velocity = Vector2.zero;
        rb.gravityScale = defaultGravityScale;

        PlayDeathSound();

        StartCoroutine(RestartAsync());
    }

    // Requests a scene reload through the global transition controller.
    IEnumerator RestartAsync()
    {
        yield return null;
        SceneTransitionController.LoadSceneWithoutSound(SceneManager.GetActiveScene().buildIndex);
    }

    // Plays death audio from a temporary object so it survives scene reload.
    void PlayDeathSound()
    {
        if (deathClip == null)
            return;

        GameObject soundObject = new GameObject("DeathSound");
        DontDestroyOnLoad(soundObject);

        AudioSource deathAudioSource = soundObject.AddComponent<AudioSource>();
        deathAudioSource.playOnAwake = false;
        deathAudioSource.PlayOneShot(deathClip, deathVolume);

        Destroy(soundObject, deathClip.length + 0.1f);
    }

    // Draws the melee attack radius in the editor.
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }
}
