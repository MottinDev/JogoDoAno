using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public Animator animator;
    private bool isDead = false;
    private PlayerMovement playerMovement;

    // Áudio para dano e morte
    private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    // Referência ao SpriteRenderer para mudar a cor ao tomar dano
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color flashColor = Color.red; // A cor para o flash de dano
    public float flashDuration = 0.1f; // Duração do flash

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();

        // Inicializa o SpriteRenderer e armazena a cor original
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Enemy") || other.CompareTag("RafaBoss")) && !isDead)
        {
            TakeDamage(1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("RafaBoss")) && !isDead)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateHearts();

        if (currentLives > 0)
        {
            animator.SetTrigger("Hurt");
            audioSource.PlayOneShot(damageSound); // Toca o som de dano
            FlashRed(); // Faz o jogador piscar em vermelho
        }
        else if (currentLives <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        currentLives = 0;
        UpdateHearts();

        animator.SetTrigger("Death");
        Debug.Log("Game Over!");
        audioSource.PlayOneShot(deathSound); // Toca o som de morte

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Carrega a tela de Game Over
        SceneManager.LoadScene("TelaGameOver");
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentLives)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }

    void FlashRed()
    {
        spriteRenderer.color = flashColor; // Muda para a cor de dano
        Invoke("ResetColor", flashDuration); // Restaura a cor após a duração do flash
    }

    void ResetColor()
    {
        spriteRenderer.color = originalColor; // Restaura a cor original
    }
}
