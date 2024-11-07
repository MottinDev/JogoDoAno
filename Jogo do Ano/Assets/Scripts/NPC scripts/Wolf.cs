using System.Collections;
using UnityEngine;

public class Wolf : MonoBehaviour
{
    [Header("Patrulha")]
    public Transform pointA;               // Ponto A para patrulha
    public Transform pointB;               // Ponto B para patrulha
    public float moveSpeed = 3f;           // Velocidade de movimento

    [Header("Detec��o")]
    public float detectionRange = 8f;      // Dist�ncia de detec��o para iniciar a persegui��o
    public float attackRange = 1.5f;       // Dist�ncia para iniciar o ataque

    [Header("Ground Check")]
    public LayerMask groundLayer;          // Layer do terreno para verifica��o de ch�o
    public Transform groundCheck;          // Ponto para checar o ch�o
    public float groundCheckRadius = 0.2f; // Raio para checar o ch�o

    [Header("Audio")]
    public AudioClip roarClip;             // �udio do rugido
    public AudioClip attackClip;           // �udio do ataque
    public AudioSource roarAudioSource;    // AudioSource para rugidos
    public AudioSource attackAudioSource;  // AudioSource para ataques

    [Header("Cooldowns")]
    public float attackCooldown = 2f;      // Tempo de espera entre ataques

    private Transform player;
    private bool movingToB = true;
    private bool isAttacking = false;
    private bool isFollowingPlayer = false;
    private bool isGrounded = true;        // Indica se o Wolf est� no ch�o
    private bool isDead = false;           // Indica se o Wolf est� morto
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;

    private float lastAttackTime = -Mathf.Infinity;

    private void Start()
    {
        // Encontra o jogador usando a tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player n�o encontrado! Certifique-se de que o jogador possui a tag 'Player'.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Refer�ncia ao Animator
        rb = GetComponent<Rigidbody2D>();

        // Configura��es adicionais do Rigidbody2D para melhor detec��o de colis�es
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Verifica se os AudioSources est�o atribu�dos, se n�o, adiciona automaticamente
        if (roarAudioSource == null)
        {
            roarAudioSource = gameObject.AddComponent<AudioSource>();
            roarAudioSource.playOnAwake = false;
            roarAudioSource.loop = false;
            roarAudioSource.spatialBlend = 0f; // Som 2D
        }

        if (attackAudioSource == null)
        {
            attackAudioSource = gameObject.AddComponent<AudioSource>();
            attackAudioSource.playOnAwake = false;
            attackAudioSource.loop = false;
            attackAudioSource.spatialBlend = 0f; // Som 2D
        }

        // Inicia a coroutine para rugidos peri�dicos
        StartCoroutine(PlayRoar());
    }

    private void Update()
    {
        if (isDead) return;

        if (isAttacking) return;

        // Verifica se o Wolf est� no ch�o antes de qualquer movimento
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!isGrounded)
        {
            // Wolf est� no ar; n�o faz nada
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Inicia a persegui��o ao jogador se estiver dentro do detectionRange e ainda n�o estiver seguindo
        if (!isFollowingPlayer && distanceToPlayer <= detectionRange)
        {
            isFollowingPlayer = true;
            Debug.Log("Wolf iniciou a persegui��o ao jogador.");
        }

        // No Update(), apenas gerencia transi��es de estado
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isAttacking) return;

        if (isFollowingPlayer)
        {
            FollowPlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("Patrol points n�o atribu�dos.");
            return;
        }

        Transform target = movingToB ? pointB : pointA;
        Vector2 targetPosition = target.position;
        Vector2 currentPosition = transform.position;

        // Calcula a dire��o para o movimento
        Vector2 direction = (targetPosition - currentPosition).normalized;

        // Checar se h� obst�culos antes de mover
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveSpeed * Time.fixedDeltaTime, groundLayer);
        if (hit.collider != null)
        {
            // Se houver obst�culo, inverte a dire��o de patrulha
            movingToB = !movingToB;
            Debug.Log("Wolf encontrou um obst�culo e mudou de dire��o.");
            return;
        }

        // Move o Wolf usando Rigidbody2D
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        FlipSprite(direction.x);

        // Atualiza a anima��o para "walk"
        animator.SetBool("isWalking", true);

        // Verifica se chegou ao ponto de patrulha
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            movingToB = !movingToB;
            Debug.Log("Wolf chegou a um ponto de patrulha e mudou de dire��o.");
        }
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Inicia o ataque se dentro do range de ataque e se n�o estiver em cooldown
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Faz o Wolf olhar para o jogador
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            // Checar se h� obst�culos antes de mover
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, moveSpeed * Time.fixedDeltaTime, groundLayer);
            if (hit.collider != null)
            {
                // N�o move se h� um obst�culo
                rb.velocity = new Vector2(0, rb.velocity.y);
                animator.SetBool("isWalking", false);
                Debug.Log("Wolf encontrou um obst�culo ao seguir o jogador e parou.");
                return;
            }

            // Move o Wolf no eixo X
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
            animator.SetBool("isWalking", true);
        }
    }

    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        animator.SetBool("isWalking", false); // Para a anima��o de caminhada
        animator.SetTrigger("attack"); // Executa a anima��o de ataque
        Debug.Log("Wolf est� atacando!");

        // Toca o som de ataque
        if (attackAudioSource != null && attackClip != null)
        {
            attackAudioSource.PlayOneShot(attackClip);
        }
        else
        {
            Debug.LogWarning("AttackAudioSource ou AttackClip n�o est� atribu�do.");
        }

        // Pisca em branco para indicar que est� atacando
        yield return FlashWhite();

        // Aqui voc� poderia adicionar o dano ao jogador
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                Debug.Log("Player recebeu dano!");
            }
            else
            {
                Debug.LogError("Componente PlayerHealth n�o encontrado no jogador!");
            }
        }

        // Aguarda o cooldown de ataque
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    private IEnumerator PlayRoar()
    {
        while (true)
        {
            // Espera um tempo aleat�rio entre 4 e 8 segundos
            float waitTime = Random.Range(4f, 8f);
            yield return new WaitForSeconds(waitTime);

            // Toca o rugido
            if (roarAudioSource != null && roarClip != null)
            {
                roarAudioSource.PlayOneShot(roarClip);
                Debug.Log("Wolf rugiu!");
            }
            else
            {
                Debug.LogWarning("RoarAudioSource ou RoarClip n�o est� atribu�do.");
            }
        }
    }

    private void FlipSprite(float direction)
    {
        spriteRenderer.flipX = direction < 0;
    }

    private IEnumerator FlashWhite()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha o raio de verifica��o de ch�o no editor para visualiza��o
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Desenha o alcance de detec��o
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Desenha o alcance de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // M�todo para simular a morte do Wolf (para fins de teste)
    public void Die()
    {
        isDead = true;
        Debug.Log("Wolf morreu!");

        // Opcional: Destruir o GameObject ap�s a anima��o de morte
        Destroy(gameObject); // Ajuste o tempo conforme necess�rio
    }
}
