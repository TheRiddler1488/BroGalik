using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public ObjectPool enemyPool;
    public Transform player;
    public float spawnRadius = 15f;

    [Header("Wave Settings")]
    public int baseEnemiesPerWave = 5;
    public float baseWaveInterval = 10f;
    public float baseSpawnDelay = 0.2f;

    [Header("Max Limits")]
    public int maxEnemiesPerWave = 20;
    public float minWaveInterval = 5f;
    public float minSpawnDelay = 0.05f;

    private float waveTimer = 0f;
    private int currentWave = 0;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager не найден в сцене!");
            enabled = false;
            return;
        }
        currentWave = gameManager.currentRound - 1;
        UpdateWaveSettings();
    }

    void Update()
    {
        if (gameManager == null) return;

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            currentWave = gameManager.currentRound;
            UpdateWaveSettings();
            StartCoroutine(SpawnWave((int)Mathf.Min(maxEnemiesPerWave, baseEnemiesPerWave + currentWave * 2)));
            waveTimer = Mathf.Max(minWaveInterval, baseWaveInterval - currentWave * 0.5f);
        }
    }

    private void UpdateWaveSettings()
    {
        enemiesPerWave = (int)Mathf.Min(maxEnemiesPerWave, baseEnemiesPerWave + currentWave * 2);
        waveInterval = Mathf.Max(minWaveInterval, baseWaveInterval - currentWave * 0.5f);
        spawnDelay = Mathf.Max(minSpawnDelay, baseSpawnDelay - currentWave * 0.01f);
    }

    private System.Collections.IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = player.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            GameObject enemy = enemyPool.Get(spawnPos);

            if (enemy != null && enemy.TryGetComponent(out EnemyStats stats))
            {
                stats.Init(enemyPool);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private int enemiesPerWave;
    private float waveInterval;
    private float spawnDelay;
}