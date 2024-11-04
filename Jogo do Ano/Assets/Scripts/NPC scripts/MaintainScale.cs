using UnityEngine;

public class MaintainScale : MonoBehaviour
{
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Garante que a escala do GameObject permane�a constante
        transform.localScale = initialScale;
    }
}
