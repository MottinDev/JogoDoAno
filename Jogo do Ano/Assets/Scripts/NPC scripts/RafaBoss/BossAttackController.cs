using System.Collections;
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody2D rb;

    // Boss movement states
    private enum BossState { Idle, Walking, Jumping, Falling }
    private BossState currentState;

    // Pursuit parameters
    public Transform player; // Reference to the player
    public float speed = 2f; // Pursuit speed
    public float followRange = 10f; // Distance at which the boss starts following the player
    public float stopDistance = 1.5f; // Minimum distance to stop following

    // Jump parameters
    public float jumpForce = 5f; // Jump force
    public float jumpCooldown = 2f; // Time between jumps
    private float lastJumpTime;

    // Ground detection
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;

    // Health and phase parameters
    public int maxHealth = 100;
    private int currentHealth;
    private int phase = 1; // Initial phase of the boss

    // Phase transition flag
    private bool isTransitioningPhase = false;

    // Attack parameters
    public float attackRange = 1f; // Range for melee attack
    public float attackCooldown = 2f; // Time between attacks
    private float lastAttackTime;

    // Projectile attack parameters
    public GameObject projectile1Prefab; // Prefab for Projectile1
    public GameObject projectile2Prefab; // Prefab for Projectile2
    public Transform firePoint; // Point from which Projectile1 is fired
    public float projectileSpeed = 5f; // Speed of the projectiles

    // For Projectile2 spawner
    public Transform[] projectile2SpawnPoints; // Array of spawn points around the boss

    // Attack points for melee attacks on each side
    public Transform attackPointLeft; // Point from which melee attack is performed when player is on the left
    public Transform attackPointRight; // Point from which melee attack is performed when player is on the right
    public LayerMask playerLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        animator.SetInteger("Phase", phase); // Initialize the phase in the animator
    }

    void Update()
    {
        if (!isTransitioningPhase) // Execute only if not transitioning phase
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            UpdateAnimationState();
            FollowPlayer();
            HandleAttacks(); // Handle attacks
        }
    }

    private void UpdateAnimationState()
    {
        if (!isGrounded)
        {
            if (rb.velocity.y > 0.1f)
            {
                ChangeState(BossState.Jumping);
            }
            else if (rb.velocity.y < -0.1f)
            {
                ChangeState(BossState.Falling);
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            ChangeState(BossState.Walking);
        }
        else
        {
            ChangeState(BossState.Idle);
        }
    }

    private void ChangeState(BossState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // Reset all triggers before setting the new one
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walking");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Fall");

        // Activate the appropriate trigger based on the current phase and state
        switch (currentState)
        {
            case BossState.Idle:
                animator.SetTrigger(phase == 1 ? "Idle" : "IdlePhase2");
                break;
            case BossState.Walking:
                animator.SetTrigger(phase == 1 ? "Walking" : "WalkPhase2");
                break;
            case BossState.Jumping:
                animator.SetTrigger(phase == 1 ? "Jump" : "JumpPhase2");
                break;
            case BossState.Falling:
                animator.SetTrigger(phase == 1 ? "Fall" : "FallPhase2");
                break;
        }
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < followRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Move the boss on the X-axis
            if (distanceToPlayer > stopDistance)
            {
                rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            }
            else
            {
                // Stop horizontal movement when close enough
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

            // Make the boss face the player
            if (direction.x > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (direction.x < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            // Calculate the vertical difference between the boss and the player
            float verticalDistanceToPlayer = player.position.y - transform.position.y;

            // Execute jump if the player is above by a certain distance
            if (verticalDistanceToPlayer > 1f && isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                // Jump towards the player
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                lastJumpTime = Time.time;
            }
        }
        else
        {
            // Stop horizontal movement if the player is out of range
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Activate the "Hurt" animation based on the phase
        if (phase == 1)
        {
            animator.SetTrigger("HurtPhase1");
        }
        else if (phase == 2)
        {
            animator.SetTrigger("HurtPhase2");
        }

        // Display visual feedback of flashing red
        StartCoroutine(FlashRed());

        // Check if health fell below 50% to change phase
        if (currentHealth <= maxHealth / 2 && phase == 1)
        {
            // Transition to Phase 2
            phase = 2;
            isTransitioningPhase = true; // Activate the transition flag
            rb.velocity = Vector2.zero; // Stop movement immediately
            animator.SetTrigger("PhaseTwo"); // Activate the transition animation to Phase 2
            animator.SetInteger("Phase", phase); // Update the Phase parameter in the Animator
        }

        // Check if the boss's health reached zero
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;

        // Set the sprite to red
        spriteRenderer.color = Color.red;

        // Wait briefly to create a "flash" effect
        yield return new WaitForSeconds(0.1f);

        // Return to the original color
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    // Method to be called at the end of the transition animation to Phase 2
    public void OnPhaseTransitionEnd()
    {
        isTransitioningPhase = false; // Deactivate the transition flag
    }

    private void HandleAttacks()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (phase == 1)
            {
                // In phase 1, only melee attack for phase 1
                if (distanceToPlayer <= attackRange)
                {
                    MeleeAttackPhase1();
                    lastAttackTime = Time.time;
                }
            }
            else if (phase == 2)
            {
                // In phase 2, randomly select an attack
                int attackChoice = Random.Range(0, 3); // 0: MeleePhase2, 1: Projectile1, 2: Projectile2

                if (attackChoice == 0 && distanceToPlayer <= attackRange)
                {
                    MeleeAttackPhase2();
                    lastAttackTime = Time.time;
                }
                else if (attackChoice == 1)
                {
                    SpawnProjectile1(); // Updated method
                    lastAttackTime = Time.time;
                }
                else if (attackChoice == 2)
                {
                    SpawnProjectile2(); // Updated method
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    private void MeleeAttackPhase1()
    {
        // Play melee attack animation for phase 1
        animator.SetTrigger("MeleeAttackPhase1");

        // Check which side the player is on and use the corresponding attack point
        if (player.position.x < transform.position.x)
        {
            // Player is on the left side
            Attack(attackPointLeft);
        }
        else
        {
            // Player is on the right side
            Attack(attackPointRight);
        }
    }

    private void MeleeAttackPhase2()
    {
        // Play melee attack animation for phase 2
        animator.SetTrigger("MeleeAttackPhase2");

        // Check which side the player is on and use the corresponding attack point
        if (player.position.x < transform.position.x)
        {
            // Player is on the left side
            Attack(attackPointLeft);
        }
        else
        {
            // Player is on the right side
            Attack(attackPointRight);
        }
    }

    private void Attack(Transform attackPoint)
    {
        // Detect player in attack range
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D player in hitPlayers)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(1);
        }
    }

    private void SpawnProjectile1()
    {
        // Play projectile attack animation
        animator.SetTrigger("ProjectileAttack1");

        // Instantiate projectile
        GameObject projectile = Instantiate(projectile1Prefab, firePoint.position, Quaternion.identity);

        // Initialize projectile to follow the player
        Projectile1 proj1 = projectile.GetComponent<Projectile1>();
        if (proj1 != null)
        {
            proj1.Initialize(player);
        }
    }

    private void SpawnProjectile2()
    {
        // Play projectile attack animation
        animator.SetTrigger("ProjectileAttack2");

        // Instantiate projectiles at multiple spawn points
        foreach (Transform spawnPoint in projectile2SpawnPoints)
        {
            GameObject projectile = Instantiate(projectile2Prefab, spawnPoint.position, Quaternion.identity);

            // Calculate direction from boss to spawn point
            Vector2 direction = (spawnPoint.position - transform.position).normalized;

            // Initialize projectile with direction and speed
            Projectile2 proj2 = projectile.GetComponent<Projectile2>();
            if (proj2 != null)
            {
                proj2.Initialize(direction, projectileSpeed);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPointLeft != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPointLeft.position, attackRange);
        }

        if (attackPointRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
        }

        // Draw gizmos for projectile2 spawn points
        if (projectile2SpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in projectile2SpawnPoints)
            {
                Gizmos.DrawSphere(spawnPoint.position, 0.1f);
            }
        }
    }
}
