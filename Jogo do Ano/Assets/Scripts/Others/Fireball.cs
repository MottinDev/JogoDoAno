using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private int fireballDamage = 1; // Dano que a bola de fogo causará

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com o inimigo
        if (collision.CompareTag("Enemy"))
        {
            // Assumindo que o inimigo tem um script EnemyHealth
            collision.GetComponent<EagleHealth>().TakeDamage(fireballDamage);
            
        }
    }

    private void Start()
    {
        // Destroi a bola de fogo após 2 segundos para evitar consumo de memória
        Destroy(gameObject, 1.25f);
    }
}
