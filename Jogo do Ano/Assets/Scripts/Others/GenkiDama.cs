using UnityEngine;

public class GenkiDama : MonoBehaviour
{
    private int damage;
    private float speed;
    private bool isFacingRight;

    private Rigidbody2D rb;
    private CircleCollider2D coll;

    // Vari�veis para crescimento da Genki Dama
    private bool isGrowing = true;
    private float maxSize = 10f; // Tamanho m�ximo de crescimento
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

        // Define a dire��o inicial (come�a sem movimento enquanto carrega)
        rb.velocity = Vector2.zero;

        // Ajusta a escala inicial (come�a no tamanho zero)
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // Se estiver crescendo e ainda n�o atingiu o tamanho m�ximo
        if (isGrowing && transform.localScale.x < maxSize)
        {
            Grow();
        }
    }

    private void Grow()
    {
        // Cresce a Genki Dama em ambos os eixos X e Y
        float newSize = transform.localScale.x + growthRate * Time.deltaTime;
        newSize = Mathf.Min(newSize, maxSize); // Limita ao tamanho m�ximo

        transform.localScale = new Vector3(newSize * (isFacingRight ? 1 : -1), newSize, 1f);
    }

    public void Release()
    {
        isGrowing = false; // Para de crescer ao ser lan�ada

        // Define a dire��o final de movimento com inclina��o para baixo
        float directionX = isFacingRight ? 1f : -1f;
        float directionY = -0.5f; // Componente Y negativa para inclina��o para baixo
        rb.velocity = new Vector2(directionX * speed, directionY * speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica colis�es com terreno, NPCs, ou RafaBoss
        if (collision.CompareTag("Terrain") || collision.CompareTag("Enemy") || collision.CompareTag("RafaBoss"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Efeito visual da explos�o pode ser adicionado aqui

        // Detecta inimigos pr�ximos e aplica dano
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, coll.radius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy") || enemy.CompareTag("RafaBoss"))
            {
                // Aplica dano ao inimigo
                enemy.GetComponent<EagleHealth>().TakeDamage(damage);
            }
        }

        // Destroi a Genki Dama ap�s a explos�o
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha o raio de dano no editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, coll.radius);
    }
}
