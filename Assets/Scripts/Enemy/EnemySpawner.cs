using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameSettings settings; // Ссылка на GameSettings
    public ObjectPool enemyPool;
    public Transform player;
    public float spawnRadius = 15f;

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
        if (settings == null)
        {
            Debug.LogError("GameSettings не назначен в EnemySpawner!");
            enabled = false;
            return;
        }
        currentWave = gameManager.CurrentRound - 1;
        UpdateWaveSettings();
    }

    void Update()
    {
        if (gameManager == null || settings == null) return;

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            currentWave = gameManager.CurrentRound - 1;
            UpdateWaveSettings();
            StartCoroutine(SpawnWave());
        }
    }

    private void UpdateWaveSettings()
    {
        // Используем настройки из GameSettings
        int enemiesToSpawn = Mathf.Min(settings.baseEnemiesPerWave + currentWave, settings.maxEnemiesPerWave);
        float waveInterval = Mathf.Max(settings.baseWaveInterval - (currentWave * 0.5f), settings.minWaveInterval);
        waveTimer = waveInterval;
    }

    private IEnumerator SpawnWave()
    {
        int enemiesToSpawn = Mathf.Min(settings.baseEnemiesPerWave + currentWave, settings.maxEnemiesPerWave);
        float spawnDelay = Mathf.Max(settings.baseSpawnDelay - (currentWave * 0.01f), settings.minSpawnDelay);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPos = player.position + (Random.insideUnitSphere * spawnRadius);
            spawnPos.y = 0f; // Предполагаем плоскую поверхность
            GameObject enemy = enemyPool.Get(spawnPos);
            if (enemy != null)
            {
                enemy.GetComponent<EnemyStats>()?.Init(enemyPool);
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}