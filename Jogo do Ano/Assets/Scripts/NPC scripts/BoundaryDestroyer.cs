using UnityEngine;

public class BoundaryDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou na área é uma bala de canhão
        if (other.gameObject.CompareTag("Enemy"))
        {
            Destroy(other.gameObject); // Destroi a bala
        }
    }
}
