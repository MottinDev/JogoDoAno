using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // Referências aos componentes
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    // Configuração do Ataque de Fogo
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPointRight;
    [SerializeField] private Transform fireballSpawnPointLeft;
    [SerializeField] private float fireballSpeed = 10f;

    // Configuração de Cooldown
    [SerializeField] private float cooldownTime = 10f;
    private float nextFireTime = 0f;

    // Configuração de Delay
    [SerializeField] private float fireballDelay = 0.5f;

    // Dano base da bola de fogo
    [SerializeField] private int fireballDamage = 50;

    // Áudio do ataque
    [SerializeField] private AudioClip attackSound;

    // --- Variáveis para o Super Ataque (Genki Dama) ---
    [Header("Super Ataque")]
    [SerializeField] private GameObject genkiDamaPrefab; // Prefab da Genki Dama
    [SerializeField] private float genkiDamaSpeed = 2f;  // Velocidade da Genki Dama
    [SerializeField] private float genkiDamaMaxSize = 3f; // Tamanho máximo da Genki Dama
    [SerializeField] private float genkiDamaChargeTime = 5f; // Tempo para carregar completamente
    [SerializeField] private float superAttackCooldown = 120f; // Cooldown do super ataque
    private float nextSuperAttackTime = 0f; // Próximo tempo para o super ataque
    private bool isChargingSuperAttack = false; // Se está carregando o super ataque
    private float chargeStartTime; // Tempo em que o carregamento começou
    [SerializeField] private int genkiDamaDamage = 50; // Dano da Genki Dama
    [SerializeField] private Transform genkiDamaSpawnPointRight;
    [SerializeField] private Transform genkiDamaSpawnPointLeft;

    // Referência para a Genki Dama atual
    private GameObject currentGenkiDama;

    // Animações
    [SerializeField] private string fireballAttackTrigger = "FireballAttack";
    [SerializeField] private string chargeAttackTrigger = "ChargeAttack";
    [SerializeField] private string releaseAttackTrigger = "ReleaseAttack";

    private void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleFireballAttack();
        HandleSuperAttack();
    }

    private void HandleFireballAttack()
    {
        if (Input.GetKeyDown(KeyCode.K) && Time.time >= nextFireTime)
        {
            anim.SetTrigger(fireballAttackTrigger);
            nextFireTime = Time.time + cooldownTime;
            Invoke(nameof(FireballAttack), fireballDelay);

            // Toca o som do ataque
            audioSource.PlayOneShot(attackSound);
        }
    }

    private void FireballAttack()
    {
        bool isFacingRight = !spriteRenderer.flipX;
        Transform selectedSpawnPoint = isFacingRight ? fireballSpawnPointRight : fireballSpawnPointLeft;

        GameObject fireball = Instantiate(fireballPrefab, selectedSpawnPoint.position, Quaternion.identity);
        fireball.GetComponent<Fireball>().SetDamage(fireballDamage);

        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * fireballSpeed, 0f);
        fireball.transform.localScale = new Vector3(direction, 1f, 1f);
    }

    // --- Métodos para o Super Ataque (Genki Dama) ---

    private void HandleSuperAttack()
    {
        // Inicia o carregamento da Genki Dama ao pressionar "L"
        if (Input.GetKeyDown(KeyCode.L) && Time.time >= nextSuperAttackTime)
        {
            StartChargingGenkiDama();
        }

        // Se o botão "L" estiver pressionado, continua carregando a Genki Dama
        if (isChargingSuperAttack && Input.GetKey(KeyCode.L))
        {
            ChargeGenkiDama();
        }

        // Solta a Genki Dama quando o botão "L" é liberado
        if (Input.GetKeyUp(KeyCode.L))
        {
            ReleaseSuperAttack();
        }
    }

    private void StartChargingGenkiDama()
    {
        isChargingSuperAttack = true;
        chargeStartTime = Time.time;
        anim.SetTrigger(chargeAttackTrigger);

        bool isFacingRight = !spriteRenderer.flipX;
        Transform spawnPoint = isFacingRight ? genkiDamaSpawnPointRight : genkiDamaSpawnPointLeft;

        // Instancia a Genki Dama e a inicializa
        currentGenkiDama = Instantiate(genkiDamaPrefab, spawnPoint.position, Quaternion.identity, transform);
        currentGenkiDama.GetComponent<GenkiDama>().Initialize(0, 0, isFacingRight);
        currentGenkiDama.transform.localScale = Vector3.zero; // Começa com escala zero para crescer
    }

    private void ChargeGenkiDama()
    {
        // Calcula o tempo de carregamento e a proporção de carga
        float chargeTime = Time.time - chargeStartTime;
        float chargeRatio = Mathf.Clamp01(chargeTime / genkiDamaChargeTime);

        // Atualiza o tamanho da Genki Dama conforme a carga
        float currentSize = Mathf.Lerp(0, genkiDamaMaxSize, chargeRatio);
        currentGenkiDama.transform.localScale = new Vector3(currentSize * (spriteRenderer.flipX ? -1 : 1), currentSize, 1f);

        // Opcionalmente, atualize o animador com a proporção de carga
        // anim.SetFloat("ChargeRatio", chargeRatio);
    }

    private void ReleaseSuperAttack()
    {
        if (currentGenkiDama != null)
        {
            isChargingSuperAttack = false;
            nextSuperAttackTime = Time.time + superAttackCooldown;
            anim.SetTrigger(releaseAttackTrigger);

            // Desanexar a Genki Dama do jogador
            currentGenkiDama.transform.parent = null;

            // Calcula a proporção final da carga
            float chargeTime = Time.time - chargeStartTime;
            float chargeRatio = Mathf.Clamp01(chargeTime / genkiDamaChargeTime);

            // Define o dano, velocidade e tamanho final da Genki Dama com base na carga
            int finalDamage = Mathf.RoundToInt(Mathf.Lerp(0, genkiDamaDamage, chargeRatio));
            float finalSpeed = Mathf.Lerp(0, genkiDamaSpeed, chargeRatio);
            float finalSize = Mathf.Lerp(0, genkiDamaMaxSize, chargeRatio);

            // Inicializa a Genki Dama com os valores finais
            bool isFacingRight = !spriteRenderer.flipX;
            currentGenkiDama.GetComponent<GenkiDama>().Initialize(finalDamage, finalSpeed, isFacingRight);

            // Define o tamanho final
            currentGenkiDama.transform.localScale = new Vector3(finalSize * (isFacingRight ? 1 : -1), finalSize, 1f);

            // Aplica a velocidade final à Genki Dama
            Rigidbody2D rb = currentGenkiDama.GetComponent<Rigidbody2D>();
            float direction = isFacingRight ? 1f : -1f;
            rb.velocity = new Vector2(direction * finalSpeed, 0f);

            // Limpa a referência
            currentGenkiDama = null;
        }
    }
}
