using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    public MenuManager menuManager;
    public PlayerController player;
    public List<AttackBehaviour> attackBehaviours = new List<AttackBehaviour>();
    public int currentRound = 1;
    public float waveTimer = 30f; // Значение, задаваемое в инспекторе
    public int playerHP = 100;
    public int souls = 0; // Души даём только в игре

    public Character selectedCharacter;
    public List<Ability> abilities = new List<Ability>();
    public List<Item> items = new List<Item>();
    public List<Ability> allAbilities = new List<Ability>();
    public List<Item> allItems = new List<Item>();
    private UIManager uiManager;
    public ShopOptionType[] currentShopOptions = new ShopOptionType[3];
    private string currentAbilityName;
    private Item currentItem;
    public Tier shopTier = Tier.D; // Текущий тир магазина

    // Новые настройки в инспекторе
    [Header("Experience Settings")]
    public int experiencePerKill = 2; // Опыт за убийство
    public int maxLevel = 5; // Максимальный уровень персонажа
    [Tooltip("Experience required for each level (index 0 for level 2, index 1 for level 3, etc.)")]
    public int[] expToNextLevel = new int[] { 10, 20, 30, 40, 80, 100, 150, 200 }; // Опыт для следующего уровня, увеличен для уровня 7

    [Header("Soul Settings")]
    public int soulsPerKill = 5; // Души за убийство

    [Header("Shop Tier Unlock Levels")]
    public int levelForTierC = 1; // Уровень для открытия Tier C
    public int levelForTierB = 2; // Уровень для открытия Tier B
    public int levelForTierA = 3; // Уровень для открытия Tier A
    public int levelForTierS = 4; // Уровень для открытия Tier S

    [Header("Shop Availability")]
    public int shopUnlockRound = 1; // Раунд, с которого магазин доступен

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Сохраняем GameManager между сценами
        uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null) Debug.LogError("UIManager not found in scene!");
        EnsureAttackBehaviours();
        InitializeAllAbilitiesAndItems();
        if (selectedCharacter == null)
        {
            selectedCharacter = new Character { name = "Warrior", moveSpeed = 5f };
            selectedCharacter.expToNextLevel = new int[expToNextLevel.Length];
            System.Array.Copy(expToNextLevel, selectedCharacter.expToNextLevel, expToNextLevel.Length); // Используем System.Array.Copy
        }
        if (allAbilities == null) allAbilities = new List<Ability>();
        if (abilities == null) abilities = new List<Ability>();
    }

    void Start()
    {
        if (uiManager != null) uiManager.Initialize(this);
        souls = 50; // Начальные души в игре
        StartCoroutine(WaveLoop());
    }

    private void EnsureAttackBehaviours()
    {
        while (attackBehaviours.Count < 6)
        {
            GameObject attackObj = new GameObject($"Attack_{attackBehaviours.Count}");
            attackObj.transform.SetParent(player != null ? player.transform : null);
            AttackBehaviour ab = attackObj.AddComponent<AttackBehaviour>();
            if (player != null) ab.attackEffectPool = player.GetComponentInChildren<ObjectPool>();
            attackBehaviours.Add(ab);
        }
    }

    private void InitializeAllAbilitiesAndItems()
    {
        if (allAbilities == null) allAbilities = new List<Ability>();
        if (allItems == null) allItems = new List<Item>();
        for (int i = 0; i < 100; i++)
        {
            Tier tier = (Tier)Random.Range(0, 5);
            allAbilities.Add(new Ability
            {
                name = $"Ability{i + 1}",
                damage = 10 + (int)tier * 5,
                cooldown = 2 - (int)tier * 0.2f,
                radius = 4 + (int)tier * 0.5f,
                aoeRadius = 2 + (int)tier * 0.2f,
                damageType = (DamageType)Random.Range(0, 4),
                effectPrefab = Resources.Load<GameObject>($"VFX/Effect{Random.Range(1, 4)}"),
                tier = tier
            });
        }
        for (int i = 0; i < 50; i++)
        {
            Tier tier = (Tier)Random.Range(0, 5);
            allItems.Add(new Item
            {
                name = $"Item{i + 1}",
                bonusValue = 5 + (int)tier * 2,
                bonusType = "Damage",
                tier = tier
            });
        }
    }

    public void ConfirmCharacterSelection()
    {
        if (player != null) player.moveSpeed = selectedCharacter.moveSpeed;
        if (menuManager != null) menuManager.OnStartGame();
    }

    public void StartGame()
    {
        if (abilities == null) abilities = new List<Ability>();
        if (abilities.Count == 0)
        {
            Debug.LogError("No ability selected!");
            if (UIManager.Instance != null) UIManager.Instance.ShowAbilitySelection();
            return;
        }
        ApplyAbilities();
    }

    public void AddAbility(Ability ability)
    {
        if (abilities == null) abilities = new List<Ability>();
        if (abilities.Count < 6 && ability != null && ability.tier <= (Tier)System.Enum.GetValues(typeof(Tier)).Length - 1)
        {
            abilities.Add(ability);
            ApplyAbilities();
            if (uiManager != null) uiManager.UpdateUI();
        }
        else
        {
            Debug.LogWarning("Maximum of 6 abilities reached, ability is null, or tier too high!");
        }
    }

    public void GenerateShopOptions(TMP_Text[] shopOptionTexts)
    {
        if (shopOptionTexts == null)
        {
            Debug.LogError("shopOptionTexts is null in GenerateShopOptions!");
            return;
        }
        // Проверяем, доступен ли магазин
        if (currentRound < shopUnlockRound || souls < 5)
        {
            for (int i = 0; i < shopOptionTexts.Length; i++)
            {
                if (shopOptionTexts[i] != null) shopOptionTexts[i].text = currentRound < shopUnlockRound ? "Shop Locked!" : "Not enough souls!";
            }
            return;
        }

        List<ShopOptionType> possibleOptions = new List<ShopOptionType>();
        if (abilities == null) abilities = new List<Ability>();
        if (allAbilities == null) allAbilities = new List<Ability>();
        if (abilities.Count < 6) possibleOptions.Add(ShopOptionType.AddAbility);
        var availableAbilities = allAbilities.Except(abilities).Where(a => a != null && a.tier <= shopTier).ToList();

        for (int i = 0; i < shopOptionTexts.Length; i++)
        {
            if (availableAbilities.Count == 0 || shopOptionTexts[i] == null)
            {
                if (shopOptionTexts[i] != null) shopOptionTexts[i].text = "No abilities available!";
                continue;
            }
            int index = Random.Range(0, availableAbilities.Count);
            var ability = availableAbilities[index];
            if (ability != null && shopOptionTexts[i] != null)
            {
                shopOptionTexts[i].text = $"Add {ability.name} (Tier {ability.tier}) - 15 Souls";
                availableAbilities.RemoveAt(index);
                currentShopOptions[i] = ShopOptionType.AddAbility;
                currentAbilityName = ability.name;
            }
        }
    }

    public void ApplyShopOption(int index)
    {
        if (index < 0 || index >= 3 || souls < 15 || currentRound < shopUnlockRound) return;

        int cost = 15;
        switch (currentShopOptions[index])
        {
            case ShopOptionType.AddAbility:
                if (allAbilities == null) allAbilities = new List<Ability>();
                if (abilities == null) abilities = new List<Ability>();
                var availableAbilities = allAbilities.Except(abilities).Where(a => a != null && a.tier <= shopTier).ToList();
                if (availableAbilities.Count > 0)
                {
                    AddAbility(availableAbilities[0]);
                    souls -= cost;
                    if (uiManager != null) uiManager.UpdateUI();
                }
                break;
        }
    }

    public void RefreshShop()
    {
        if (souls >= 5 && currentRound >= shopUnlockRound)
        {
            souls -= 5;
            if (uiManager != null && uiManager.shopOptionTexts != null) GenerateShopOptions(uiManager.shopOptionTexts);
            if (uiManager != null) uiManager.UpdateUI();
        }
        else
        {
            Debug.LogWarning("Not enough souls or shop is locked!");
        }
    }

    private void ApplyAbilities()
    {
        if (abilities == null) abilities = new List<Ability>();
        for (int i = 0; i < abilities.Count && i < attackBehaviours.Count; i++)
        {
            var ab = attackBehaviours[i];
            var ability = abilities[i];
            if (ab != null && ability != null)
            {
                ab.attackData = ScriptableObject.CreateInstance<AttackData>();
                ab.attackData.attackName = ability.name;
                ab.attackData.damage = ability.damage;
                ab.attackData.cooldown = ability.cooldown;
                ab.attackData.radius = ability.radius;
                ab.attackData.aoeRadius = ability.aoeRadius;
                ab.attackData.damageType = ability.damageType;
                ab.attackData.effectPrefab = ability.effectPrefab;
                ab.attackData.tier = ability.tier;
                ab.enabled = true;
            }
        }
        for (int i = abilities.Count; i < attackBehaviours.Count; i++)
        {
            if (attackBehaviours[i] != null) attackBehaviours[i].enabled = false;
        }
    }

    public void AddExperience(int amount)
    {
        if (selectedCharacter == null) return;
        selectedCharacter.experience += amount;
        // Убрано фиксированное добавление душ, теперь только через soulsPerKill в EnemyStats
        int targetIndex = selectedCharacter.level - 1; // Индекс для текущего уровня (уровень 1 -> индекс 0)
        if (targetIndex >= 0 && targetIndex < expToNextLevel.Length)
        {
            while (selectedCharacter.level < maxLevel && selectedCharacter.experience >= expToNextLevel[targetIndex])
            {
                selectedCharacter.experience -= expToNextLevel[targetIndex]; // Сбрасываем накопленный опыт после повышения
                selectedCharacter.level++;
                targetIndex = selectedCharacter.level - 1;
                if (targetIndex >= expToNextLevel.Length) break;
                UpdateShopTier();
                Debug.Log($"Level up to {selectedCharacter.level}! Required exp: {expToNextLevel[targetIndex]}");
            }
        }
        if (uiManager != null) uiManager.UpdateUI();
    }

    private void UpdateShopTier()
    {
        if (selectedCharacter.level >= levelForTierS) shopTier = Tier.S;
        else if (selectedCharacter.level >= levelForTierA) shopTier = Tier.A;
        else if (selectedCharacter.level >= levelForTierB) shopTier = Tier.B;
        else if (selectedCharacter.level >= levelForTierC) shopTier = Tier.C;
        else shopTier = Tier.D;
    }

    System.Collections.IEnumerator WaveLoop()
    {
        while (true)
        {
            float initialWaveTimer = waveTimer;
            while (waveTimer > 0)
            {
                yield return new WaitForSeconds(1f); // Уменьшаем раз в секунду
                waveTimer -= 1f;
                if (uiManager != null) uiManager.UpdateUI(); // Обновляем UI каждую секунду
            }
            currentRound++;
            waveTimer = initialWaveTimer; // Сбрасываем таймер для следующего раунда
            if (uiManager != null) uiManager.UpdateRoundText();
        }
    }
}