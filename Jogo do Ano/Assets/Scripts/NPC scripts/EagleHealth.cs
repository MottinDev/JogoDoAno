using UnityEngine;
using UnityEngine.UI;

public class EagleHealth : MonoBehaviour
{
    public int maxLives = 3;                 // N�mero m�ximo de vidas
    private int currentLives;                // Vidas atuais

    public Image[] hearts;                   // Array de imagens de cora��es na UI
    public Sprite fullHeart;                 // Imagem do cora��o cheio
    public Sprite emptyHeart;                // Imagem do cora��o vazio
    private bool isDead = false;             // Flag para verificar se a �guia j� morreu

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();
    }

    public void TakeDamage(int damage)
    {
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
        Debug.Log("A �guia morreu!");
        Destroy(gameObject);  // Destr�i o objeto da �guia
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
