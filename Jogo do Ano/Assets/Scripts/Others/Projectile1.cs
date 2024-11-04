using UnityEngine;

public class Projectile1 : MonoBehaviour
{
    public float speed = 5f; // Speed of the projectile
    public float lifetime = 5f; // Lifetime before destruction
    private Transform target; // Target to follow

    public void Initialize(Transform playerTarget)
    {
        target = playerTarget;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            // Move towards the player
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }
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
