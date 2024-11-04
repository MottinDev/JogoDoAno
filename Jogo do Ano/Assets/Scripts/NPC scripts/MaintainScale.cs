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
        // Garante que a escala do GameObject permaneça constante
        transform.localScale = initialScale;
    }
}
