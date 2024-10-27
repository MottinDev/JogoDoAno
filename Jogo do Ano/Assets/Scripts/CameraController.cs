using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Referência ao Transform do jogador
    public Vector3 offset;   // Deslocamento entre o jogador e a câmera
    public float smoothSpeed = 0.125f; // Velocidade de suavização

    void LateUpdate()
    {
        // Verifica se o jogador está definido para evitar erros
        if (player != null)
        {
            // Posição desejada da câmera, com base na posição do jogador e no deslocamento
            Vector3 desiredPosition = player.position + offset;

            // Suaviza a transição da posição atual para a posição desejada
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Define a posição da câmera para a posição suavizada
            transform.position = smoothedPosition;
        }
    }
}
