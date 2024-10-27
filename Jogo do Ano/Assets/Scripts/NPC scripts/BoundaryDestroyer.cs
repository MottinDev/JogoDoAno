using UnityEngine;

public class BoundaryDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou na �rea � uma bala de canh�o
        if (other.gameObject.CompareTag("Enemy"))
        {
            Destroy(other.gameObject); // Destroi a bala
        }
    }
}
