using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody2D rb;

    // Estados de movimento do boss
    private enum BossState { Idle, Walking, Jumping, Falling }
    private BossState currentState;

    // Par�metros de persegui��o
    public Transform player; // Refer�ncia ao jogador
    public float speed = 2f; // Velocidade de persegui��o
    public float followRange = 10f; // Dist�ncia para come�ar a seguir o jogador
    public float stopDistance = 1.5f; // Dist�ncia m�nima para parar de seguir

    // Par�metros de pulo
    public float jumpForce = 5f; // For�a do pulo
    public float jumpCooldown = 2f; // Tempo entre pulos
    private float lastJumpTime;

    // Detec��o de ch�o
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;

    // Par�metros de vida e fase
    public int maxHealth = 100;
    private int currentHealth;
    private int phase = 1; // Fase inicial do boss

    // Flag de transi��o de fase
    private bool isTransitioningPhase = false;

    // Par�metros de ataque
    public float attackRange = 1f; // Alcance para ataque corpo a corpo
    public float attackCooldown = 2f; // Tempo entre ataques
    private float lastAttackTime;

    // Par�metros de ataque de proj�til
    public GameObject projectile1Prefab; // Prefab para Projectile1
    public GameObject projectile2Prefab; // Prefab para Projectile2
    public Transform firePoint; // Ponto de onde o Projectile1 � disparado
    public float projectileSpeed = 5f; // Velocidade dos proj�teis

    // Spawner para Projectile2
    public Transform[] projectile2SpawnPoints; // Array de pontos de spawn ao redor do boss

    // Par�metros para ataque de caveiras
    public GameObject skullPrefab;            // Prefab para as caveiras
    public Transform leftSkullSpawnPoint;     // Ponto de spawn do lado esquerdo
    public Transform rightSkullSpawnPoint;    // Ponto de spawn do lado direito

    // Pontos de ataque para ataques corpo a corpo em cada lado
    public Transform attackPointLeft; // Ponto de ataque quando o jogador est� � esquerda
    public Transform attackPointRight; // Ponto de ataque quando o jogador est� � direita
    public LayerMask playerLayer;

    // Controle de ataques para altern�ncia
    private int attackSequenceIndex = 0; // �ndice para controlar a sequ�ncia de ataques
    private int[] attackSequence = { 1, 2, 3 }; // Sequ�ncia de ataques: 1 - Projectile1, 2 - Projectile2, 3 - SkullAttack

    // Componentes de �udio
    private AudioSource audioSource;

    // �udios de fase
    public AudioClip phase1AudioClip;        // �udio para tocar na fase 1
    public AudioClip[] phase2AudioClips;     // �udios para tocar aleatoriamente na fase 2
    private List<AudioClip> phase2AudioQueue; //fila para guardar os audios

    // �udios de dano
    public AudioClip damageAudioClipPhase1;  // �udio quando o boss toma dano na fase 1
    public AudioClip damageAudioClipPhase2;  // �udio quando o boss toma dano na fase 2

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        animator.SetInteger("Phase", phase); // Inicializa a fase no animator

        // Inicializa o componente de �udio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Adiciona o componente AudioSource se n�o existir
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Inicia a corrotina de reprodu��o de �udios
        StartCoroutine(PlayPhaseAudio());
    }

    void Update()
    {
        if (!isTransitioningPhase) // Executa apenas se n�o estiver em transi��o de fase
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            UpdateAnimationState();
            FollowPlayer();
            HandleAttacks(); // Lida com os ataques
        }
    }

    private void UpdateAnimationState()
    {
        if (!isGrounded)
        {
            if (rb.velocity.y > 0.1f)
            {
                ChangeState(BossState.Jumping);
            }
            else if (rb.velocity.y < -0.1f)
            {
                ChangeState(BossState.Falling);
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            ChangeState(BossState.Walking);
        }
        else
        {
            ChangeState(BossState.Idle);
        }
    }

    private void ChangeState(BossState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        // Reseta todos os triggers antes de setar o novo
        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walking");
        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Fall");

        // Ativa o trigger apropriado com base na fase atual e estado
        switch (currentState)
        {
            case BossState.Idle:
                animator.SetTrigger(phase == 1 ? "Idle" : "IdlePhase2");
                break;
            case BossState.Walking:
                animator.SetTrigger(phase == 1 ? "Walking" : "WalkPhase2");
                break;
            case BossState.Jumping:
                animator.SetTrigger(phase == 1 ? "Jump" : "JumpPhase2");
                break;
            case BossState.Falling:
                animator.SetTrigger(phase == 1 ? "Fall" : "FallPhase2");
                break;
        }
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < followRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Faz o boss olhar para o jogador
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            // Move o boss no eixo X
            if (distanceToPlayer > stopDistance)
            {
                rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
            }
            else
            {
                // Para o movimento horizontal quando estiver perto o suficiente
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

            // Calcula a diferen�a vertical entre o boss e o jogador
            float verticalDistanceToPlayer = player.position.y - transform.position.y;

            // Executa o pulo se o jogador estiver acima por uma certa dist�ncia
            if (verticalDistanceToPlayer > 1f && isGrounded && Time.time > lastJumpTime + jumpCooldown)
            {
                // Pula em dire��o ao jogador
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                lastJumpTime = Time.time;
            }
        }
        else
        {
            // Para o movimento horizontal se o jogador estiver fora de alcance
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    // M�todo para receber dano
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Toca o �udio de dano correspondente � fase
        if (phase == 1)
        {
            animator.SetTrigger("HurtPhase1");
            audioSource.PlayOneShot(damageAudioClipPhase1);
        }
        else if (phase == 2)
        {
            animator.SetTrigger("HurtPhase2");
            audioSource.PlayOneShot(damageAudioClipPhase2);
        }

        // Feedback visual de piscar em vermelho
        StartCoroutine(FlashRed());

        // Verifica se a vida caiu abaixo de 50% para mudar de fase
        if (currentHealth <= maxHealth / 2 && phase == 1)
        {
            // Transi��o para a Fase 2
            phase = 2;
            isTransitioningPhase = true; // Ativa a flag de transi��o
            rb.velocity = Vector2.zero; // Para o movimento imediatamente
            animator.SetTrigger("PhaseTwo"); // Ativa a anima��o de transi��o para a Fase 2
            animator.SetInteger("Phase", phase); // Atualiza o par�metro Phase no Animator
        }

        // Verifica se a vida do boss chegou a zero
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;

        // Define a cor do sprite para vermelho
        spriteRenderer.color = Color.red;

        // Aguarda brevemente para criar um efeito de "piscar"
        yield return new WaitForSeconds(0.1f);

        // Retorna � cor original
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    // M�todo chamado no final da anima��o de transi��o para a Fase 2
    public void OnPhaseTransitionEnd()
    {
        isTransitioningPhase = false; // Desativa a flag de transi��o
        attackSequenceIndex = 0; // Reseta a sequ�ncia de ataques
    }

    private void HandleAttacks()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (phase == 1)
            {
                // Na fase 1, apenas ataque corpo a corpo
                if (distanceToPlayer <= attackRange)
                {
                    MeleeAttackPhase1();
                    lastAttackTime = Time.time;
                }
            }
            else if (phase == 2)
            {
                // Na fase 2, alterna entre os ataques de proj�til e skull
                int attackChoice = attackSequence[attackSequenceIndex];

                switch (attackChoice)
                {
                    case 1:
                        SpawnProjectile1();
                        break;
                    case 2:
                        SpawnProjectile2();
                        break;
                    case 3:
                        SpawnSkullAttack();
                        break;
                }

                // Atualiza o �ndice para o pr�ximo ataque
                attackSequenceIndex = (attackSequenceIndex + 1) % attackSequence.Length;

                lastAttackTime = Time.time;
            }
        }
    }

    private void MeleeAttackPhase1()
    {
        // Toca a anima��o de ataque corpo a corpo para a fase 1
        animator.SetTrigger("MeleeAttackPhase1");

        // Verifica de que lado o jogador est� e usa o ponto de ataque correspondente
        if (player.position.x < transform.position.x)
        {
            // Jogador est� � esquerda
            Attack(attackPointLeft);
        }
        else
        {
            // Jogador est� � direita
            Attack(attackPointRight);
        }
    }

    private void MeleeAttackPhase2()
    {
        // Toca a anima��o de ataque corpo a corpo para a fase 2
        animator.SetTrigger("MeleeAttackPhase2");

        // Verifica de que lado o jogador est� e usa o ponto de ataque correspondente
        if (player.position.x < transform.position.x)
        {
            // Jogador est� � esquerda
            Attack(attackPointLeft);
        }
        else
        {
            // Jogador est� � direita
            Attack(attackPointRight);
        }
    }

    private void Attack(Transform attackPoint)
    {
        // Detecta o jogador no alcance de ataque
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D player in hitPlayers)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(1);
        }
    }

    private void SpawnProjectile1()
    {
        // Instancia o proj�til
        GameObject projectile = Instantiate(projectile1Prefab, firePoint.position, Quaternion.identity);

        // Inicializa o proj�til para seguir o jogador
        Projectile1 proj1 = projectile.GetComponent<Projectile1>();
        if (proj1 != null)
        {
            proj1.Initialize(player);
        }
    }

    private void SpawnProjectile2()
    {
        // Instancia todos os proj�teis de uma vez
        foreach (Transform spawnPoint in projectile2SpawnPoints)
        {
            GameObject projectile = Instantiate(projectile2Prefab, spawnPoint.position, Quaternion.identity);

            // Calcula a dire��o do boss para o ponto de spawn
            Vector2 direction = (spawnPoint.position - transform.position).normalized;

            // Inicializa o proj�til com dire��o e velocidade
            Projectile2 proj2 = projectile.GetComponent<Projectile2>();
            if (proj2 != null)
            {
                proj2.Initialize(direction, projectileSpeed);
            }
        }
    }

    private void SpawnSkullAttack()
    {
        // Instancia caveiras em ambos os lados
        GameObject leftSkull = Instantiate(skullPrefab, leftSkullSpawnPoint.position, Quaternion.identity);
        GameObject rightSkull = Instantiate(skullPrefab, rightSkullSpawnPoint.position, Quaternion.identity);

        // Inicializa as caveiras com dire��es
        SkullProjectile leftSkullScript = leftSkull.GetComponent<SkullProjectile>();
        if (leftSkullScript != null)
        {
            // Move para a esquerda
            leftSkullScript.Initialize(Vector2.left);
        }

        SkullProjectile rightSkullScript = rightSkull.GetComponent<SkullProjectile>();
        if (rightSkullScript != null)
        {
            // Move para a direita
            rightSkullScript.Initialize(Vector2.right);
        }
    }

    private IEnumerator PlayPhaseAudio()
    {
        while (currentHealth > 0)
        {
            if (phase == 1)
            {
                // Espera um intervalo aleat�rio entre 2 e 6 segundos
                float waitTime = Random.Range(2f, 6f);
                yield return new WaitForSeconds(waitTime);

                // Toca o �udio da fase 1
                if (phase == 1 && phase1AudioClip != null)
                {
                    audioSource.PlayOneShot(phase1AudioClip);

                    // Aguarda o tempo de dura��o do �udio
                    yield return new WaitForSeconds(phase1AudioClip.length);
                }
            }
            else if (phase == 2)
            {
                // Inicializa a lista e a embaralha se estiver vazia
                if (phase2AudioQueue == null || phase2AudioQueue.Count == 0)
                {
                    phase2AudioQueue = new List<AudioClip>(phase2AudioClips);
                    Shuffle(phase2AudioQueue);
                }

                // Toca o pr�ximo �udio da fila
                AudioClip clipToPlay = phase2AudioQueue[0];
                phase2AudioQueue.RemoveAt(0);

                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);

                    // Aguarda o tempo de dura��o do �udio
                    yield return new WaitForSeconds(clipToPlay.length);

                    // Espera um intervalo aleat�rio entre 1 e 4 segundos
                    float waitTime = Random.Range(1f, 4f);
                    yield return new WaitForSeconds(waitTime);
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    // M�todo para embaralhar a lista de �udios
    private void Shuffle(List<AudioClip> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            AudioClip temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    void OnDrawGizmosSelected()
    {
        if (attackPointLeft != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPointLeft.position, attackRange);
        }

        if (attackPointRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPointRight.position, attackRange);
        }

        // Desenha gizmos para os pontos de spawn do projectile2
        if (projectile2SpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in projectile2SpawnPoints)
            {
                Gizmos.DrawSphere(spawnPoint.position, 0.1f);
            }
        }
    }
}
