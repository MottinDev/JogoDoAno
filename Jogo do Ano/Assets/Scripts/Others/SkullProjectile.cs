using UnityEngine;

public class SkullProjectile : MonoBehaviour
{
    public float speed = 5f;          // Velocidade da caveira (aumentado de 3f para 5f)
    public float amplitude = 1f;      // Altura da onda (aumentado de 0.5f para 1f)
    public float frequency = 2f;      // Velocidade da onda
    public float lifeTime = 5f;       // Tempo antes da caveira ser destru�da

    private Vector2 direction;
    private Vector2 startPosition;
    private float spawnTime;

    void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;
    }

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        // Flip da sprite da caveira baseado na dire��o
        if (direction.x < 0)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        // Movimento horizontal
        float xMovement = direction.x * speed * Time.deltaTime;

        // Movimento vertical (onda senoidal)
        float yMovement = Mathf.Sin((Time.time - spawnTime) * frequency) * amplitude * Time.deltaTime;

        // Aplica o movimento
        transform.Translate(new Vector2(xMovement, yMovement));

        // Destr�i a caveira ap�s o tempo de vida expirar
        if (Time.time - spawnTime >= lifeTime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com o jogador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Dano no jogador
            collision.GetComponent<PlayerHealth>().TakeDamage(1);
            // Destr�i a caveira
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Destr�i a caveira se atingir o ch�o
            Destroy(gameObject);
        }
    }
}
