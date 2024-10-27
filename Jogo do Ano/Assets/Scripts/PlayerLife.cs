using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3; // Número máximo de vidas
    private int currentLives; // Vidas atuais

    public Image[] hearts; // Array de imagens de corações na UI
    public Sprite fullHeart; // Imagem do coração cheio
    public Sprite emptyHeart; // Imagem do coração vazio
    public Animator animator; // Referência ao Animator do personagem
    private bool isDead = false; // Flag para verificar se o personagem já morreu
    private PlayerMovement playerMovement; // Referência ao script de movimentação do player

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();

        // Obtendo referência ao script de movimentação do personagem
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isDead)
        {
            TakeDamage(1);
        }
    }

    void TakeDamage(int damage)
    {
        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateHearts();

        if (currentLives > 0)
        {
            animator.SetTrigger("Hurt"); // Ativa a animação de tomar dano
        }
        else if (currentLives <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death"); // Ativa a animação de morte
        Debug.Log("Game Over!");

        // Desativa a movimentação do player
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Congela o eixo X e Y
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }

        // Reinicia o cenário após 1 segundo
        Invoke("RestartScene", 1f);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reinicia a cena atual
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
