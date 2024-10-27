using UnityEngine;

public class FollowEagle : MonoBehaviour
{
    public Transform eagle;       // Referência para a águia
    public Vector3 offset;        // Offset para posicionar a barra de vida acima da águia

    void Update()
    {
        if (eagle != null)
        {
            // Atualiza a posição do painel com base na posição da águia e o offset
            transform.position = Camera.main.WorldToScreenPoint(eagle.position + offset);
        }
    }
}
