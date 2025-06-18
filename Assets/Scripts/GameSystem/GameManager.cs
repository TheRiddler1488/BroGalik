using UnityEngine;
using System.Collections;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameSettings settings;
    public CharacterData selectedCharacter;
    public List<AbilityData> abilities = new List<AbilityData>();
    public List<ItemData> items = new List<ItemData>();
    [SerializeField] private PlayerController player;
    [SerializeField] private AttackSystem attackSystem;
    [SerializeField] private ShopSystem shopSystem;
    [SerializeField] private AbilityData[] abilityDataAssets;
    private List<AbilityData> allAbilities = new List<AbilityData>();
    private List<ItemData> allItems = new List<ItemData>();

    private int currentRound = 1;
    public int CurrentRound => currentRound;
    private float waveTimer;
    private int playerHP = 100;
    public int PlayerHP
    {
        get => playerHP;
        set
        {
            playerHP = Mathf.Max(0, value);
            GameEvents.RaisePlayerHPChanged(playerHP);
        }
    }
    private int souls;
    public int Souls
    {
        get => souls;
        private set
        {
            souls = Mathf.Max(0, value);
            GameEvents.RaiseSoulsChanged(souls);
        }
    }
    private Tier shopTier = Tier.D;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent != null)
            {
                Debug.LogWarning("GameManager должен быть корневым объектом! Исправляем...");
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAllAbilities();
        InitializeAllItems();
    }

    private void InitializeAllAbilities()
    {
        allAbilities.Clear();
        if (abilityDataAssets == null || abilityDataAssets.Length == 0)
        {
            Debug.LogError("AbilityDataAssets не назначены в GameManager!");
        }
        else
        {
            allAbilities.AddRange(abilityDataAssets);
            Debug.Log($"Загружено {allAbilities.Count} способностей: {string.Join(", ", allAbilities.Select(a => $"{a.attackName} (Tier {a.tier})"))}");
        }
    }

    private void InitializeAllItems()
    {
        allItems = Resources.LoadAll<ItemData>("Items").ToList();
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogWarning("Не удалось загрузить предметы из Assets/Resources/Items!");
        }
    }

    public void StartGame(CharacterData character, AbilityData initialAbility)
    {
        if (settings == null)
        {
            Debug.LogError("GameSettings не назначен в GameManager!");
            return;
        }

        selectedCharacter = character;
        abilities.Clear();
        abilities.Add(initialAbility);
        PlayerHP = 100;
        Souls = settings.startingSouls;
        waveTimer = settings.waveTimer;
        currentRound = 1;
        ApplyAbilities();
        GameEvents.RaiseGameStarted();
        StartCoroutine(WaveLoop()); // Запускаем WaveLoop только после начала игры
    }

    public void AddExperience(int amount)
    {
        if (selectedCharacter == null) return;
        selectedCharacter.AddExperience(amount, settings.expToNextLevel, settings.maxLevel);
        UpdateShopTier();
        int targetIndex = selectedCharacter.Level - 1;
        int requiredExp = targetIndex < settings.expToNextLevel.Length ? settings.expToNextLevel[targetIndex] : 0;
        GameEvents.RaiseExperienceChanged(selectedCharacter.Experience, requiredExp, selectedCharacter.Level);
        GameEvents.RaiseSoulsChanged(Souls);
    }

    public void AddSouls(int amount)
    {
        Souls += amount;
    }

    public void TakeDamage(int damage)
    {
        PlayerHP -= damage;
    }

    public int GetSouls() => Souls;
    public void SpendSouls(int amount)
    {
        Souls -= amount;
    }

    public Tier GetShopTier() => shopTier;

    public List<AbilityData> GetAvailableAbilities(Tier maxTier)
    {
        return allAbilities.Where(a => a.tier <= maxTier && !abilities.Contains(a)).ToList();
    }

    public List<ItemData> GetAvailableItems(Tier maxTier)
    {
        return allItems.Where(i => i.tier <= maxTier && !items.Contains(i)).ToList();
    }

    public List<AbilityData> FindAbilitiesByTier(Tier tier)
    {
        var filteredAbilities = allAbilities.Where(a => a.tier == tier).ToList();
        Debug.Log($"Найдено {filteredAbilities.Count} способностей Tier {tier}: {string.Join(", ", filteredAbilities.Select(a => a.attackName))}");
        return filteredAbilities;
    }

    public void UpgradeAbility(UpgradeData upgrade)
    {
        var ability = abilities.FirstOrDefault(a => a.attackName == upgrade.abilityName);
        if (ability != null)
        {
            if (upgrade.upgradeType == "Damage")
            {
                ability.damage += upgrade.value;
            }
            else if (upgrade.upgradeType == "Cooldown")
            {
                ability.cooldown = Mathf.Max(0.1f, ability.cooldown - upgrade.value);
            }
            ApplyAbilities();
        }
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
    }

    private void UpdateShopTier()
    {
        if (settings == null) return;
        Tier newTier = Tier.D;
        if (selectedCharacter.Level >= settings.levelForTierS) newTier = Tier.S;
        else if (selectedCharacter.Level >= settings.levelForTierA) newTier = Tier.A;
        else if (selectedCharacter.Level >= settings.levelForTierB) newTier = Tier.B;
        else if (selectedCharacter.Level >= settings.levelForTierC) newTier = Tier.C;

        if (newTier != shopTier)
        {
            shopTier = newTier;
            GameEvents.RaiseShopTierChanged(shopTier);
        }
    }

    public void ApplyAbilities()
    {
        if (player == null)
        {
            Debug.LogWarning("Player не назначен в GameManager!");
            return;
        }
        var attackBehaviour = player.GetComponent<AttackBehaviour>();
        if (attackBehaviour != null)
        {
            attackBehaviour.ClearAbilities();
            foreach (var ability in abilities)
            {
                attackBehaviour.AddAbility(ability);
            }
        }
        else
        {
            Debug.LogWarning("AttackBehaviour не найден на Player!");
        }
    }

    private IEnumerator WaveLoop()
    {
        if (settings == null)
        {
            Debug.LogError("GameSettings не назначен в GameManager, WaveLoop не запущен!");
            yield break;
        }

        while (true)
        {
            while (waveTimer > 0)
            {
                yield return new WaitForSeconds(1f);
                waveTimer -= 1f;
                GameEvents.RaiseWaveTimerChanged(waveTimer);
            }
            currentRound++;
            waveTimer = settings.waveTimer;
            GameEvents.RaiseRoundChanged(currentRound);
            if (currentRound % settings.shopInterval == 0 && currentRound >= settings.shopUnlockRound)
            {
                if (shopSystem != null)
                {
                    shopSystem.OpenShop();
                    yield return new WaitUntil(() => !shopSystem.IsShopOpen);
                }
                else
                {
                    Debug.LogWarning("ShopSystem не назначен, магазин не открывается!");
                }
            }
        }
    }
}