using UnityEngine;
using UnityEngine.UI;

public class EagleHealth : MonoBehaviour, IDamageable
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
        if (isDead) return; // Evita m�ltiplas chamadas ap�s a morte

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
        // Aqui voc� pode adicionar anima��es de morte, sons, etc.
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
