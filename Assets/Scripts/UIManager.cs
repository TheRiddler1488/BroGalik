using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TMP_Text roundText;
    public TMP_Text waveTimerText;
    public TMP_Text playerHPText;
    public TMP_Text soulText;
    public TMP_Text attackCounterText;
    public TMP_Text levelText;
    public TMP_Text[] shopOptionTexts = new TMP_Text[3]; // Для магазина, инициализируем с размером 3
    public GameObject abilitySelectionUI; // Панель выбора способности в игре

    private GameManager gameManager;
    private Dictionary<string, int> attackHits = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene!");
        }
        else
        {
            Initialize(gameManager);
        }
    }

    public void Initialize(GameManager gm)
    {
        if (gm == null)
        {
            Debug.LogError("GameManager is null in UIManager.Initialize!");
            return;
        }
        gameManager = gm;
        if (shopOptionTexts == null || shopOptionTexts.Length == 0)
        {
            Debug.LogWarning("shopOptionTexts is not properly set in UIManager! Please assign TMP_Text objects in the Inspector.");
        }
        UpdateUI();
    }

    void Update()
    {
        if (gameManager != null) UpdateUI();
    }

    public void UpdateUI()
    {
        if (gameManager == null)
        {
            Debug.LogError("gameManager is null in UpdateUI!");
            return;
        }
        if (roundText != null) roundText.text = $"Round: {gameManager.currentRound}";
        if (waveTimerText != null) waveTimerText.text = $"Time Left: {Mathf.Ceil(gameManager.waveTimer):F0}s";
        if (playerHPText != null) playerHPText.text = $"HP: {gameManager.playerHP}";
        if (soulText != null) soulText.text = $"Souls: {gameManager.souls}";
        if (levelText != null)
        {
            int currentLevel = gameManager.selectedCharacter.level;
            int requiredExp = (currentLevel - 1 < gameManager.expToNextLevel.Length) ? gameManager.expToNextLevel[Mathf.Min(currentLevel - 1, gameManager.expToNextLevel.Length - 1)] : 0;
            levelText.text = $"Level: {currentLevel} (Exp: {gameManager.selectedCharacter.experience}/{requiredExp}) Shop Tier: {gameManager.shopTier}";
        }
        if (attackCounterText != null)
        {
            string counterText = "Attacks:\n";
            foreach (var pair in attackHits)
            {
                counterText += $"{pair.Key}: {pair.Value}\n";
            }
            attackCounterText.text = counterText;
        }
        if (shopOptionTexts != null && shopOptionTexts.Length > 0 && gameManager != null)
        {
            gameManager.GenerateShopOptions(shopOptionTexts);
        }
    }

    public void UpdateRoundText()
    {
        if (roundText != null && gameManager != null) roundText.text = $"Round: {gameManager.currentRound}";
    }

    public void IncrementAttackCounter(string attackName)
    {
        if (attackHits.ContainsKey(attackName)) attackHits[attackName]++;
        else attackHits[attackName] = 1;
        UpdateUI();
    }

    public void OnShopOptionSelected(int index)
    {
        if (gameManager != null) gameManager.ApplyShopOption(index);
    }

    public void OnRefreshShop()
    {
        if (gameManager != null) gameManager.RefreshShop();
    }

    public void ShowAbilitySelection()
    {
        if (abilitySelectionUI != null && gameManager != null)
        {
            abilitySelectionUI.SetActive(true);
            gameManager.GenerateShopOptions(shopOptionTexts); // Показываем доступные способности
        }
    }
}