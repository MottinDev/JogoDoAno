using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // References to components
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Fire Attack Configuration
    [SerializeField] private GameObject fireballPrefab; // Prefab of the fireball
    [SerializeField] private Transform fireballSpawnPointRight; // Spawn point when facing right
    [SerializeField] private Transform fireballSpawnPointLeft;  // Spawn point when facing left
    [SerializeField] private float fireballSpeed = 10f;         // Speed of the fireball

    private void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Control the fireball attack
        if (Input.GetKeyDown(KeyCode.K))
        {
            FireballAttack();
        }
    }

    private void FireballAttack()
    {
        // Trigger the fireball attack animation
        anim.SetTrigger("FireballAttack");

        // Determine the facing direction using spriteRenderer.flipX
        bool isFacingRight = !spriteRenderer.flipX;

        // Select the correct spawn point based on the facing direction
        Transform selectedSpawnPoint = isFacingRight ? fireballSpawnPointRight : fireballSpawnPointLeft;

        // Instantiate the fireball at the correct spawn point
        GameObject fireball = Instantiate(fireballPrefab, selectedSpawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();

        // Set the fireball's velocity based on the facing direction
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * fireballSpeed, 0f);

        // Adjust the scale of the fireball to face the correct direction
        fireball.transform.localScale = new Vector3(direction, 1f, 1f);
    }
}
