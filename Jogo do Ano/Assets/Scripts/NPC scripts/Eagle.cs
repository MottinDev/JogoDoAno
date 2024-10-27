using System.Collections;
using UnityEngine;

public class Eagle : MonoBehaviour
{
    public Transform pointA;                   // Ponto A para patrulha
    public Transform pointB;                   // Ponto B para patrulha
    public float moveSpeed = 5f;               // Velocidade de movimento
    public float detectionRange = 6f;          // Distância de detecção para iniciar o ataque

    public float rushSpeedMultiplier = 12f;    // Multiplicador de velocidade para o ataque Rush (ajustado)
    public float attackChargeTime = 0.5f;      // Tempo de carregamento antes do Rush
    public float maxRushDistance = 20f;        // Distância máxima que a águia pode percorrer no Rush (ajustada)
    public float cooldownTime = 1f;            // Tempo de cooldown após o ataque Rush

    private Transform player;
    private bool movingToB = true;
    private bool isAttacking = false;
    private bool isCooldown = false;
    private SpriteRenderer spriteRenderer;
    private Vector2 rushDirection;

    private void Start()
    {
        // Encontra o jogador usando a tag "Player"
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isAttacking || isCooldown) return;

        // Verifica a distância até o jogador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Se o jogador estiver dentro do alcance, inicia o carregamento do ataque
        if (distanceToPlayer <= detectionRange)
        {
            StartCoroutine(ChargeAndRushAttack());
        }
        else
        {
            // Patrulha entre os pontos A e B
            Patrol();
        }
    }

    private void Patrol()
    {
        // Define o ponto de destino com base no movimento atual
        Transform target = movingToB ? pointB : pointA;

        // Movimenta-se para o ponto de destino
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Inverte o sprite para olhar na direção em que está se movendo
        FlipSprite(target.position.x - transform.position.x);

        // Verifica se chegou ao ponto de destino e inverte a direção
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            movingToB = !movingToB;
        }
    }

    private IEnumerator ChargeAndRushAttack()
    {
        isAttacking = true;

        // Define a direção do ataque baseada na posição atual do jogador
        rushDirection = (player.position - transform.position).normalized;

        // Inverte o sprite para olhar em direção ao jogador
        FlipSprite(player.position.x - transform.position.x);

        // Pisca em branco para indicar que está carregando o ataque
        yield return FlashWhite();

        // Aguarda o tempo de carregamento
        yield return new WaitForSeconds(attackChargeTime);

        // Executa o ataque Rush
        Vector3 startPosition = transform.position;
        float rushSpeed = moveSpeed * rushSpeedMultiplier;

        while (Vector2.Distance(startPosition, transform.position) < maxRushDistance)
        {
            transform.position += (Vector3)rushDirection * rushSpeed * Time.deltaTime;
            yield return null;
        }

        // Cooldown após o ataque
        StartCoroutine(CooldownAfterAttack());
    }

    private void FlipSprite(float direction)
    {
        // Corrigindo a lógica de inversão de sprite
        spriteRenderer.flipX = direction > 0;
    }

    private IEnumerator FlashWhite()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;   // Muda a cor do sprite para branco
        yield return new WaitForSeconds(0.1f); // Pisca rapidamente
        spriteRenderer.color = originalColor; // Volta para a cor original
    }

    private IEnumerator CooldownAfterAttack()
    {
        isAttacking = false;
        isCooldown = true;

        // Aguarda o tempo de cooldown
        yield return new WaitForSeconds(cooldownTime);

        isCooldown = false;
    }
}
