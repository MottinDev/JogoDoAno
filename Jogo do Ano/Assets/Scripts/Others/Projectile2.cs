using UnityEngine;

public class Projectile2 : MonoBehaviour
{
    private Vector2 moveDirection;
    private float speed;
    public float lifetime = 5f; // Lifetime before destruction

    public void Initialize(Vector2 direction, float projectileSpeed)
    {
        moveDirection = direction.normalized;
        speed = projectileSpeed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move in the set direction
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>().TakeDamage(1);
            Destroy(gameObject); // Destroy the projectile upon hitting the player
        }
    }
}
