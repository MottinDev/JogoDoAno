using UnityEngine;

public class HideCursor : MonoBehaviour
{
    // Tempo em segundos para esconder o cursor após inatividade
    public float timeToHide = 5.0f;

    // Variável para armazenar o tempo desde a última atividade do mouse
    private float lastMouseMovementTime;

    // Variáveis para armazenar a posição do mouse e verificar inatividade
    private Vector3 lastMousePosition;

    // Variável para controlar se o cursor está oculto
    private bool isCursorHidden = false;

    // Variável para garantir que haja apenas uma instância
    private static HideCursor instance;

    void Awake()
    {
        // Se já existe uma instância deste objeto, destrói a nova
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // Define a instância e a preserva ao carregar novas cenas
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // Inicializa a posição do mouse
        lastMousePosition = Input.mousePosition;
        lastMouseMovementTime = Time.time;

        // Garante que o cursor esteja visível no início
        Cursor.visible = true;
    }

    void Update()
    {
        // Verifica se houve movimento do mouse
        if (Input.mousePosition != lastMousePosition)
        {
            // Atualiza o tempo da última atividade
            lastMouseMovementTime = Time.time;
            lastMousePosition = Input.mousePosition;
        }

        // Verifica se o tempo de inatividade passou e o cursor ainda não está oculto
        if (Time.time - lastMouseMovementTime >= timeToHide && !isCursorHidden)
        {
            // Esconde o cursor após 5 segundos de inatividade
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
