using UnityEngine;

public class GenkiDama : MonoBehaviour
{
    private int damage;
    private float speed;
    private bool isFacingRight;

    private Rigidbody2D rb;
    private CircleCollider2D coll;

    // Variáveis para crescimento da Genki Dama
    private bool isGrowing = true;
    private float maxSize = 3f; // Tamanho máximo de crescimento
    private float growthRate = 1f; // Taxa de crescimento por segundo

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CircleCollider2D>();
    }

    public void Initialize(int damage, float speed, bool isFacingRight)
    {
        this.damage = damage;
        this.speed = speed;
        this.isFacingRight = isFacingRight;

        // Define a direção inicial
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = Vector2.zero; // Começa sem movimento enquanto carrega

        // Ajusta a escala inicial
        transform.localScale = Vector3.zero; // Começa no tamanho zero
    }

    private void Update()
    {
        // Se estiver crescendo e ainda não atingiu o tamanho máximo
        if (isGrowing && transform.localScale.x < maxSize)
        {
            Grow();
        }
    }

    private void Grow()
    {
        // Cresce a Genki Dama em ambos os eixos X e Y
        float newSize = transform.localScale.x + growthRate * Time.deltaTime;
        newSize = Mathf.Min(newSize, maxSize); // Limita ao tamanho máximo

        transform.localScale = new Vector3(newSize * (isFacingRight ? 1 : -1), newSize, 1f);
    }

    public void Release()
    {
        isGrowing = false; // Para de crescer ao ser lançada

        // Define a direção final de movimento
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * speed, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica colisões com terreno, NPCs, ou RafaBoss
        if (collision.CompareTag("Terrain") || collision.CompareTag("Enemy") || collision.CompareTag("RafaBoss"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Efeito visual da explosão pode ser adicionado aqui

        // Detecta inimigos próximos e aplica dano
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, coll.radius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy") || enemy.CompareTag("RafaBoss"))
            {
                // Aplica dano ao inimigo
                enemy.GetComponent<EagleHealth>().TakeDamage(damage);
            }
        }

        // Destroi a Genki Dama após a explosão
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha o raio de dano no editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>().radius);
    }
}
