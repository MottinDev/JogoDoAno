using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private int fireballDamage = 1; // Dano que a bola de fogo causar�

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
        // Destroi a bola de fogo ap�s 2 segundos para evitar consumo de mem�ria
        Destroy(gameObject, 1.25f);
    }
}
