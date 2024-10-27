using UnityEngine;

public class CannonballSpawner : MonoBehaviour
{
    public GameObject cannonballPrefab; // Prefab da bala de canh�o
    public float minSpawnInterval = 1f; // Intervalo m�nimo de spawn
    public float maxSpawnInterval = 3f; // Intervalo m�ximo de spawn
    public Vector2 spawnDirection = new Vector2(1, 0); // Dire��o do disparo
    public float minSpawnForce = 10f; // For�a m�nima aplicada � bala
    public float maxSpawnForce = 25f; // For�a m�xima aplicada � bala

    private void Start()
    {
        // Inicia o primeiro spawn com uma taxa aleat�ria
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        // Gera um intervalo de spawn aleat�rio
        float randomSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        Invoke("SpawnCannonball", randomSpawnInterval);
    }

    private void SpawnCannonball()
    {
        // Instancia a bala de canh�o na posi��o do spawner
        GameObject cannonball = Instantiate(cannonballPrefab, transform.position, Quaternion.identity);

        // Obt�m o Rigidbody2D da bala de canh�o
        Rigidbody2D rb = cannonball.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Gera uma for�a aleat�ria entre min e max
            float randomSpawnForce = Random.Range(minSpawnForce, maxSpawnForce);

            // Aplica uma for�a na dire��o desejada
            rb.AddForce(spawnDirection.normalized * randomSpawnForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("Rigidbody2D n�o encontrado no prefab da bala de canh�o.");
        }

        // Agendar o pr�ximo spawn com aleatoriedade
        ScheduleNextSpawn();
    }
}
