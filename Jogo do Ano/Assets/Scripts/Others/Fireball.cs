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
        Debug.Log("Fireball colidiu com: " + collision.name + " | Tag: " + collision.tag);

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            if (collision.CompareTag("RafaBoss"))
            {
                damageable.TakeDamage(fireballDamageBoss);
                Debug.Log("Dano aplicado ao RafaBoss: " + fireballDamageBoss);
            }
            else
            {
                damageable.TakeDamage(fireballDamageEnemy);
                Debug.Log("Dano aplicado a um inimigo normal: " + fireballDamageEnemy);
            }
        }
        else
        {
            Debug.Log("Objeto colidido não implementa IDamageable.");
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        // Destroi a bola de fogo após 1.25 segundos para evitar consumo de memória
        Destroy(gameObject, 1.25f);
    }
}
