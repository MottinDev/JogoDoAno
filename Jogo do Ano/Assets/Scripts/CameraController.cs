using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Refer�ncia ao Transform do jogador
    public Vector3 offset;   // Deslocamento entre o jogador e a c�mera
    public float smoothSpeed = 0.125f; // Velocidade de suaviza��o

    void LateUpdate()
    {
        // Verifica se o jogador est� definido para evitar erros
        if (player != null)
        {
            // Posi��o desejada da c�mera, com base na posi��o do jogador e no deslocamento
            Vector3 desiredPosition = player.position + offset;

            // Suaviza a transi��o da posi��o atual para a posi��o desejada
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Define a posi��o da c�mera para a posi��o suavizada
            transform.position = smoothedPosition;
        }
    }
}
