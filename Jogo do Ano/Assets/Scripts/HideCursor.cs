using UnityEngine;

public class HideCursor : MonoBehaviour
{
    // Tempo em segundos para esconder o cursor ap�s inatividade
    public float timeToHide = 5.0f;

    // Vari�vel para armazenar o tempo desde a �ltima atividade do mouse
    private float lastMouseMovementTime;

    // Vari�veis para armazenar a posi��o do mouse e verificar inatividade
    private Vector3 lastMousePosition;

    // Vari�vel para controlar se o cursor est� oculto
    private bool isCursorHidden = false;

    // Vari�vel para garantir que haja apenas uma inst�ncia
    private static HideCursor instance;

    void Awake()
    {
        // Se j� existe uma inst�ncia deste objeto, destr�i a nova
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Define a inst�ncia e a preserva ao carregar novas cenas
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // Inicializa a posi��o do mouse
        lastMousePosition = Input.mousePosition;
        lastMouseMovementTime = Time.time;

        // Garante que o cursor esteja vis�vel no in�cio
        Cursor.visible = true;
    }

    void Update()
    {
        // Verifica se houve movimento do mouse
        if (Input.mousePosition != lastMousePosition)
        {
            // Atualiza o tempo da �ltima atividade
            lastMouseMovementTime = Time.time;
            lastMousePosition = Input.mousePosition;
        }

        // Verifica se o tempo de inatividade passou e o cursor ainda n�o est� oculto
        if (Time.time - lastMouseMovementTime >= timeToHide && !isCursorHidden)
        {
            // Esconde o cursor ap�s 5 segundos de inatividade
            Cursor.visible = false;
            isCursorHidden = true;
        }

        // Verifica se a tecla "Left Alt" foi pressionada
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            // Reativa o cursor quando "Left Alt" for pressionado
            Cursor.visible = true;
            isCursorHidden = false;

            // Reseta o tempo de inatividade
            lastMouseMovementTime = Time.time;
        }
    }
}
