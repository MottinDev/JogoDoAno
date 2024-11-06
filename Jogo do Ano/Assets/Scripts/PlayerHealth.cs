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

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
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

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }

        Invoke("RestartScene", 1f);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
}
