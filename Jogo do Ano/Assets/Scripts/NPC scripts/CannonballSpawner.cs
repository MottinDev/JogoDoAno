using UnityEngine;

public class CannonballSpawner : MonoBehaviour
{
    public GameObject cannonballPrefab; // Prefab da bala de canhão
    public float minSpawnInterval = 1f; // Intervalo mínimo de spawn
    public float maxSpawnInterval = 3f; // Intervalo máximo de spawn
    public Vector2 spawnDirection = new Vector2(1, 0); // Direção do disparo
    public float minSpawnForce = 10f; // Força mínima aplicada à bala
    public float maxSpawnForce = 25f; // Força máxima aplicada à bala

    private void Start()
    {
        // Inicia o primeiro spawn com uma taxa aleatória
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        // Gera um intervalo de spawn aleatório
        float randomSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        Invoke("SpawnCannonball", randomSpawnInterval);
    }

    private void SpawnCannonball()
    {
        // Instancia a bala de canhão na posição do spawner
        GameObject cannonball = Instantiate(cannonballPrefab, transform.position, Quaternion.identity);

        // Obtém o Rigidbody2D da bala de canhão
        Rigidbody2D rb = cannonball.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Gera uma força aleatória entre min e max
            float randomSpawnForce = Random.Range(minSpawnForce, maxSpawnForce);

            // Aplica uma força na direção desejada
            rb.AddForce(spawnDirection.normalized * randomSpawnForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("Rigidbody2D não encontrado no prefab da bala de canhão.");
        }

        // Agendar o próximo spawn com aleatoriedade
        ScheduleNextSpawn();
    }
}
