using UnityEngine;

public class Trap : MonoBehaviour
{
    public int damage = 1; // Dano causado pela armadilha
    public float pushForce = 5f; // Força do empurrão aplicado ao jogador

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto colidido tem a tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // Acessa o componente de vida do jogador para aplicar o dano (assumindo que o jogador tenha um script de vida)
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Aplica a força de empurrão na direção contrária da armadilha
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
            }
        }
    }
}
