using UnityEngine;
using UnityEngine.UI;

public class WolfHealth : MonoBehaviour, IDamageable
{
    public int maxLives = 3;                 // Número máximo de vidas
    private int currentLives;                // Vidas atuais

    public Image[] hearts;                   // Array de imagens de corações na UI
    public Sprite fullHeart;                 // Imagem do coração cheio
    public Sprite emptyHeart;                // Imagem do coração vazio
    private bool isDead = false;             // Flag para verificar se o Wolf já morreu

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Evita múltiplas chamadas após a morte

        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateHearts();

        if (currentLives <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("O Wolf morreu!");
        // Aqui você pode adicionar animações de morte, sons, etc.
        Destroy(gameObject);  // Destrói o objeto do Wolf
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
