using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3; // N�mero m�ximo de vidas
    private int currentLives; // Vidas atuais

    public Image[] hearts; // Array de imagens de cora��es na UI
    public Sprite fullHeart; // Imagem do cora��o cheio
    public Sprite emptyHeart; // Imagem do cora��o vazio
    public Animator animator; // Refer�ncia ao Animator do personagem
    private bool isDead = false; // Flag para verificar se o personagem j� morreu
    private PlayerMovement playerMovement; // Refer�ncia ao script de movimenta��o do player

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();

        // Obtendo refer�ncia ao script de movimenta��o do personagem
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
            animator.SetTrigger("Hurt"); // Ativa a anima��o de tomar dano
        }
        else if (currentLives <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death"); // Ativa a anima��o de morte
        Debug.Log("Game Over!");

        // Desativa a movimenta��o do player
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

        // Reinicia o cen�rio ap�s 1 segundo
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
