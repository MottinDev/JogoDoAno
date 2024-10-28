using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Referências aos componentes
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Configuração do Ataque de Fogo
    [SerializeField] private GameObject fireballPrefab; // Prefab da bola de fogo
    [SerializeField] private Transform fireballSpawnPointRight; // Ponto de spawn quando virado para a direita
    [SerializeField] private Transform fireballSpawnPointLeft;  // Ponto de spawn quando virado para a esquerda
    [SerializeField] private float fireballSpeed = 10f;         // Velocidade da bola de fogo

    // Configuração de Cooldown
    [SerializeField] private float cooldownTime = 10f; // Duração do cooldown em segundos
    private float nextFireTime = 0f; // Próximo tempo em que o jogador pode atacar

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
            FireballAttack();
            nextFireTime = Time.time + cooldownTime;
        }
    }

    private void FireballAttack()
    {
        // Dispara a animação de ataque de bola de fogo
        anim.SetTrigger("FireballAttack");

        // Determina a direção usando spriteRenderer.flipX
        bool isFacingRight = !spriteRenderer.flipX;

        // Seleciona o ponto de spawn correto com base na direção
        Transform selectedSpawnPoint = isFacingRight ? fireballSpawnPointRight : fireballSpawnPointLeft;

        // Instancia a bola de fogo no ponto de spawn correto
        GameObject fireball = Instantiate(fireballPrefab, selectedSpawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();

        // Define a velocidade da bola de fogo com base na direção
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * fireballSpeed, 0f);

        // Ajusta a escala da bola de fogo para a direção correta
        fireball.transform.localScale = new Vector3(direction, 1f, 1f);
    }
}
