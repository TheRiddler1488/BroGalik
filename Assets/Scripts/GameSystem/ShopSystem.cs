using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    [SerializeField] private GameSettings settings;
    [SerializeField] private TMP_Text[] abilityOptions = new TMP_Text[5];
    [SerializeField] private TMP_Text[] upgradeOptions = new TMP_Text[3];
    [SerializeField] private TMP_Text[] itemOptions = new TMP_Text[3];
    [SerializeField] private Button readyButton;
    [SerializeField] private Button refreshButton;
    private List<AbilityData> currentAbilityOptions = new List<AbilityData>();
    private List<UpgradeData> currentUpgradeOptions = new List<UpgradeData>();
    private List<ItemData> currentItemOptions = new List<ItemData>();
    public bool IsShopOpen { get; private set; }

    void Awake()
    {
        readyButton.onClick.AddListener(CloseShop);
        refreshButton.onClick.AddListener(RefreshShop);
    }

    public void OpenShop()
    {
        IsShopOpen = true;
        GenerateShopOptions();
        GameEvents.RaiseShopOpened();
    }

    public void CloseShop()
    {
        IsShopOpen = false;
        GameEvents.RaiseShopClosed();
    }

    private void GenerateShopOptions()
    {
        currentAbilityOptions.Clear();
        currentUpgradeOptions.Clear();
        currentItemOptions.Clear();

        var availableAbilities = GameManager.Instance.GetAvailableAbilities(GameManager.Instance.GetShopTier());
        for (int i = 0; i < abilityOptions.Length; i++)
        {
            if (availableAbilities.Count > 0)
            {
                int index = Random.Range(0, availableAbilities.Count);
                currentAbilityOptions.Add(availableAbilities[index]);
                abilityOptions[i].text = $"Add {currentAbilityOptions[i].attackName} (Tier {currentAbilityOptions[i].tier}) - {settings.shopOptionCost} Souls";
                availableAbilities.RemoveAt(index);
            }
            else
                abilityOptions[i].text = "No abilities available!";
        }

        var activeAbilities = GameManager.Instance.abilities;
        for (int i = 0; i < upgradeOptions.Length; i++)
        {
            if (activeAbilities.Count > 0)
            {
                var ability = activeAbilities[Random.Range(0, activeAbilities.Count)];
                var upgrade = new UpgradeData { abilityName = ability.attackName, upgradeType = "Damage", value = 5f };
                currentUpgradeOptions.Add(upgrade);
                upgradeOptions[i].text = $"Upgrade {ability.attackName} (+5 Damage) - {settings.shopOptionCost} Souls";
            }
            else
                upgradeOptions[i].text = "No upgrades available!";
        }

        var availableItems = GameManager.Instance.GetAvailableItems(GameManager.Instance.GetShopTier());
        for (int i = 0; i < itemOptions.Length; i++)
        {
            if (availableItems.Count > 0)
            {
                int index = Random.Range(0, availableItems.Count);
                currentItemOptions.Add(availableItems[index]);
                itemOptions[i].text = $"Add {currentItemOptions[i].itemName} (Tier {currentItemOptions[i].tier}) - {settings.shopOptionCost} Souls";
                availableItems.RemoveAt(index);
            }
            else
                itemOptions[i].text = "No items available!";
        }
    }

    public void PurchaseAbility(int index)
    {
        if (!IsShopOpen || index < 0 || index >= abilityOptions.Length || GameManager.Instance.GetSouls() < settings.shopOptionCost) return;

        var ability = currentAbilityOptions[index];
        if (ability != null && GameManager.Instance.abilities.Count < 6)
        {
            GameManager.Instance.abilities.Add(ability);
            GameManager.Instance.SpendSouls(settings.shopOptionCost);
            GameManager.Instance.ApplyAbilities();
            GenerateShopOptions();
        }
    }

    public void PurchaseUpgrade(int index)
    {
        if (!IsShopOpen || index < 0 || index >= upgradeOptions.Length || GameManager.Instance.GetSouls() < settings.shopOptionCost) return;

        var upgrade = currentUpgradeOptions[index];
        if (upgrade != null)
        {
            GameManager.Instance.UpgradeAbility(upgrade);
            GameManager.Instance.SpendSouls(settings.shopOptionCost);
            GenerateShopOptions();
        }
    }

    public void PurchaseItem(int index)
    {
        if (!IsShopOpen || index < 0 || index >= itemOptions.Length || GameManager.Instance.GetSouls() < settings.shopOptionCost) return;

        var item = currentItemOptions[index];
        if (item != null)
        {
            GameManager.Instance.AddItem(item);
            GameManager.Instance.SpendSouls(settings.shopOptionCost);
            GenerateShopOptions();
        }
    }

    public void RefreshShop()
    {
        if (GameManager.Instance.GetSouls() >= settings.shopRefreshCost)
        {
            GameManager.Instance.SpendSouls(settings.shopRefreshCost);
            GenerateShopOptions();
        }
    }
}

[System.Serializable]
public class UpgradeData
{
    public string abilityName;
    public string upgradeType;
    public float value;
}