using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Refer�ncias aos componentes
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Configura��o do Ataque de Fogo
    [SerializeField] private GameObject fireballPrefab; // Prefab da bola de fogo
    [SerializeField] private Transform fireballSpawnPointRight; // Ponto de spawn quando virado para a direita
    [SerializeField] private Transform fireballSpawnPointLeft;  // Ponto de spawn quando virado para a esquerda
    [SerializeField] private float fireballSpeed = 10f;         // Velocidade da bola de fogo

    // Configura��o de Cooldown
    [SerializeField] private float cooldownTime = 10f; // Dura��o do cooldown em segundos
    private float nextFireTime = 0f; // Pr�ximo tempo em que o jogador pode atacar

    // Configura��o de Delay
    [SerializeField] private float fireballDelay = 0.5f; // Tempo de espera at� spawnar a bola de fogo

    // Dano base da bola de fogo (para refer�ncia)
    [SerializeField] private int fireballDamage = 10; // Quantidade de dano que a bola de fogo causa

    private void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Controle do ataque de bola de fogo com cooldown
        if (Input.GetKeyDown(KeyCode.K) && Time.time >= nextFireTime)
        {
            // Dispara a anima��o de ataque de bola de fogo
            anim.SetTrigger("FireballAttack");

            // Define o pr�ximo tempo em que o jogador poder� atacar
            nextFireTime = Time.time + cooldownTime;

            // Chama o m�todo FireballAttack ap�s o tempo de delay
            Invoke(nameof(FireballAttack), fireballDelay);
        }
    }

    private void FireballAttack()
    {
        // Determina a dire��o usando spriteRenderer.flipX
        bool isFacingRight = !spriteRenderer.flipX;

        // Seleciona o ponto de spawn correto com base na dire��o
        Transform selectedSpawnPoint = isFacingRight ? fireballSpawnPointRight : fireballSpawnPointLeft;

        // Instancia a bola de fogo no ponto de spawn correto
        GameObject fireball = Instantiate(fireballPrefab, selectedSpawnPoint.position, Quaternion.identity);

        // Configura o dano base da bola de fogo para refer�ncia
        fireball.GetComponent<Fireball>().SetDamage(fireballDamage);

        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();

        // Define a velocidade da bola de fogo com base na dire��o
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * fireballSpeed, 0f);

        // Ajusta a escala da bola de fogo para a dire��o correta
        fireball.transform.localScale = new Vector3(direction, 1f, 1f);
    }
}
