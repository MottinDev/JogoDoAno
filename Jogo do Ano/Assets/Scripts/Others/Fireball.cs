using UnityEngine;

public class Fireball : MonoBehaviour
{
    private int fireballDamageEnemy = 1;  // Dano para inimigos normais
    private int fireballDamageBoss = 10;  // Dano para o RafaBoss
    private int damage;

    // Configura o dano base da bola de fogo (pode ser ajustado pelo PlayerAttack)
    public void SetDamage(int baseDamage)
    {
        damage = baseDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com um inimigo normal
        if (collision.CompareTag("Enemy"))
        {
            // Assumindo que o inimigo normal tem um script EnemyHealth
            collision.GetComponent<EagleHealth>().TakeDamage(fireballDamageEnemy);
        }
        // Verifica se colidiu com o RafaBoss
        else if (collision.CompareTag("RafaBoss"))
        {
            // Assumindo que o RafaBoss tem o script BossAttackController com o método TakeDamage
            BossAttackController boss = collision.GetComponent<BossAttackController>();
            if (boss != null)
            {
                boss.TakeDamage(fireballDamageBoss); // Aplica dano maior ao boss
            }
        }

    }

    private void Start()
    {
        // Destroi a bola de fogo após 1.25 segundos para evitar consumo de memória
        Destroy(gameObject, 1.25f);
    }
}
