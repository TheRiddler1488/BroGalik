using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 0)]
public class GameSettings : ScriptableObject
{
    [Header("Experience Settings")]
    public int experiencePerKill = 2;
    public int maxLevel = 5;
    public int[] expToNextLevel = { 10, 20, 30, 40, 80 };

    [Header("Soul Settings")]
    public int soulsPerKill = 5;
    public int startingSouls = 50;

    [Header("Shop Settings")]
    public int shopUnlockRound = 1;
    public int shopRefreshCost = 5;
    public int shopOptionCost = 15;
    public int shopInterval = 3; // Магазин каждые 3 раунда
    public int levelForTierC = 1;
    public int levelForTierB = 2;
    public int levelForTierA = 3;
    public int levelForTierS = 4;

    [Header("Wave Settings")]
    public float waveTimer = 30f;
    public int baseEnemiesPerWave = 5;
    public float baseWaveInterval = 10f;
    public float baseSpawnDelay = 0.2f;
    public int maxEnemiesPerWave = 20;
    public float minWaveInterval = 5f;
    public float minSpawnDelay = 0.05f;
}