using UnityEngine;

public class Killzone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto colidido tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Obt�m o script de vida do jogador
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Ativa o m�todo de morte instant�nea
                playerHealth.Die();
            }
        }
    }
}
