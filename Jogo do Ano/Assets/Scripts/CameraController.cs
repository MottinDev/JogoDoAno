using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    void Update()
    {
        // Aplique o ajuste fixo de 2.8 no eixo Y, mantendo o valor original do eixo Z
        transform.position = new Vector3(player.position.x, player.position.y + 2.8f, transform.position.z);
    }
}
