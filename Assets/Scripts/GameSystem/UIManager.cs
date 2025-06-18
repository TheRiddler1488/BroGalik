using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text waveTimerText;
    [SerializeField] private TMP_Text playerHPText;
    [SerializeField] private TMP_Text soulText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text attackCounterText;
    [SerializeField] private GameObject shopPanel;
    private Dictionary<string, int> attackHits = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        GameEvents.OnRoundChanged += UpdateRoundText;
        GameEvents.OnWaveTimerChanged += UpdateTimerText; // Правильная регистрация
        GameEvents.OnPlayerHPChanged += UpdateHPText;
        GameEvents.OnSoulsChanged += UpdateSoulsText;
        GameEvents.OnExperienceChanged += UpdateLevelText;
        GameEvents.OnShopOpened += ShowShop;
        GameEvents.OnShopClosed += HideShop;
    }

    private void UpdateRoundText(int round)
    {
        if (roundText) roundText.text = $"Round: {round}";
    }

    private void UpdateTimerText(float timer)
    {
        if (waveTimerText) waveTimerText.text = $"Time Left: {Mathf.Ceil(timer)}s";
    }

    private void UpdateHPText(int hp)
    {
        if (playerHPText) playerHPText.text = $"HP: {hp}";
    }

    private void UpdateSoulsText(int souls)
    {
        if (soulText) soulText.text = $"Souls: {souls}";
    }

    private void UpdateLevelText(int exp, int reqExp, int level)
    {
        if (levelText) levelText.text = $"Level: {level} (Exp: {exp}/{reqExp}) Shop Tier: {GameManager.Instance.GetShopTier()}";
    }

    public void IncrementAttackCounter(string attackName)
    {
        if (attackHits.ContainsKey(attackName)) attackHits[attackName]++;
        else attackHits[attackName] = 1;
        if (attackCounterText != null)
        {
            string counterText = "Attacks:\n";
            foreach (var pair in attackHits)
            {
                counterText += $"{pair.Key}: {pair.Value}\n";
            }
            attackCounterText.text = counterText;
        }
    }

    private void ShowShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);
    }

    public void HideShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}