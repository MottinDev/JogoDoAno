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

    // Animações
    [SerializeField] private string fireballAttackTrigger = "FireballAttack";

    private void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleFireballAttack();
    }

    private void HandleFireballAttack()
    {
        if (Input.GetKeyDown(KeyCode.K) && Time.time >= nextFireTime)
        {
            anim.SetTrigger(fireballAttackTrigger);
            nextFireTime = Time.time + cooldownTime;
            Invoke(nameof(FireballAttack), fireballDelay);

            // Toca o som do ataque
            if (attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
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
}
