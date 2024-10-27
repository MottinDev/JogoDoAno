using UnityEngine;

public class FollowEagle : MonoBehaviour
{
    public Transform eagle;       // Refer�ncia para a �guia
    public Vector3 offset;        // Offset para posicionar a barra de vida acima da �guia

    void Update()
    {
        if (eagle != null)
        {
            // Atualiza a posi��o do painel com base na posi��o da �guia e o offset
            transform.position = Camera.main.WorldToScreenPoint(eagle.position + offset);
        }
    }
}
