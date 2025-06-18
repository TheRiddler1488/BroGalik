using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject characterSelectionPanel;
    [SerializeField] private GameObject abilitySelectionPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button[] mapButtons; // Кнопки выбора карты
    [SerializeField] private List<CharacterData> characters;
    [SerializeField] private Button[] characterButtons; // Кнопки персонажей
    [SerializeField] private Button[] abilityButtons; // Кнопки способностей
    [SerializeField] private Button exitButton; // Кнопка выхода
    [SerializeField] private Button[] backButtons; // Кнопки возврата (для MapSelection, CharacterSelection, AbilitySelection)

    private string selectedMap;
    private CharacterData selectedCharacter;
    private List<AbilityData> initialAbilities; // Для хранения способностей Tier D

    void Start()
    {
        if (playButton == null)
        {
            Debug.LogError("PlayButton не назначен в MenuManager!");
            return;
        }
        playButton.onClick.AddListener(OnPlayClicked);

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }

        SetupMapButtons();
        SetupCharacterButtons();
        SetupBackButtons();
        SetState(GameState.MainMenu);
    }

    private void SetupMapButtons()
    {
        if (mapButtons == null || mapButtons.Length == 0)
        {
            Debug.LogError("MapButtons не назначены в MenuManager!");
            return;
        }

        // Пример: MapButton1 -> "GameMap1", MapButton2 -> "GameMap2"
        for (int i = 0; i < mapButtons.Length; i++)
        {
            if (mapButtons[i] == null)
            {
                Debug.LogError($"MapButtons[{i}] не назначен!");
                continue;
            }
            int index = i;
            string mapName = $"GameMap{i + 1}"; // Имена сцен: GameMap1, GameMap2
            mapButtons[i].onClick.RemoveAllListeners();
            mapButtons[i].onClick.AddListener(() => OnMapSelected(mapName));
            var textComponent = mapButtons[i].GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = $"Map {i + 1}";
            }
        }
    }

    private void SetupCharacterButtons()
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogError("Список characters пуст или не инициализирован в MenuManager!");
            return;
        }
        if (characterButtons == null || characterButtons.Length == 0)
        {
            Debug.LogError("CharacterButtons не назначены в MenuManager!");
            return;
        }
        if (characterButtons.Length < characters.Count)
        {
            Debug.LogWarning($"Количество кнопок ({characterButtons.Length}) меньше количества персонажей ({characters.Count})!");
        }

        for (int i = 0; i < Mathf.Min(characterButtons.Length, characters.Count); i++)
        {
            if (characterButtons[i] == null)
            {
                Debug.LogError($"CharacterButtons[{i}] не назначен!");
                continue;
            }
            if (characters[i] == null)
            {
                Debug.LogError($"CharacterData[{i}] равен null!");
                continue;
            }

            var textComponent = characterButtons[i].GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError($"Кнопка персонажа {i} не содержит TMP_Text!");
                continue;
            }

            textComponent.text = characters[i].characterName;
            characterButtons[i].interactable = characters[i].isUnlocked;
            int index = i;
            characterButtons[i].onClick.RemoveAllListeners();
            characterButtons[i].onClick.AddListener(() => OnCharacterSelected(characters[index]));
        }
    }

    private void SetupBackButtons()
    {
        if (backButtons == null || backButtons.Length == 0)
        {
            Debug.LogError("BackButtons не назначены в MenuManager!");
            return;
        }

        for (int i = 0; i < backButtons.Length; i++)
        {
            if (backButtons[i] == null)
            {
                Debug.LogError($"BackButtons[{i}] не назначен!");
                continue;
            }
            int index = i;
            backButtons[i].onClick.RemoveAllListeners();
            backButtons[i].onClick.AddListener(() => OnBackClicked(index));
        }
    }

    private void SetState(GameState state)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(state == GameState.MainMenu);
        if (mapSelectionPanel != null) mapSelectionPanel.SetActive(state == GameState.MapSelection);
        if (characterSelectionPanel != null) characterSelectionPanel.SetActive(state == GameState.CharacterSelection);
        if (abilitySelectionPanel != null) abilitySelectionPanel.SetActive(state == GameState.AbilitySelection);
    }

    public void OnPlayClicked()
    {
        SetState(GameState.MapSelection);
    }

    public void OnMapSelected(string mapName)
    {
        selectedMap = mapName;
        SetState(GameState.CharacterSelection);
    }

    public void OnCharacterSelected(CharacterData character)
    {
        selectedCharacter = character;
        SetupAbilitySelection();
        SetState(GameState.AbilitySelection);
    }

    private void SetupAbilitySelection()
    {
        if (abilityButtons == null || abilityButtons.Length == 0)
        {
            Debug.LogError("AbilityButtons не назначены в MenuManager!");
            return;
        }

        initialAbilities = GameManager.Instance.FindAbilitiesByTier(Tier.D);
        if (initialAbilities == null || initialAbilities.Count == 0)
        {
            Debug.LogError("Нет доступных способностей Tier D!");
            return;
        }

        for (int i = 0; i < Mathf.Min(abilityButtons.Length, initialAbilities.Count); i++)
        {
            if (abilityButtons[i] == null)
            {
                Debug.LogError($"AbilityButtons[{i}] не назначен!");
                continue;
            }
            var textComponent = abilityButtons[i].GetComponentInChildren<TMP_Text>();
            if (textComponent == null)
            {
                Debug.LogError($"Кнопка способности {i} не содержит TMP_Text!");
                continue;
            }
            textComponent.text = $"Add {initialAbilities[i].attackName} (Tier D)";
            int index = i;
            abilityButtons[i].onClick.RemoveAllListeners();
            abilityButtons[i].onClick.AddListener(() => OnAbilitySelected(initialAbilities[index]));
        }
    }

    public void OnAbilitySelected(AbilityData ability)
    {
        if (GameManager.Instance != null && !string.IsNullOrEmpty(selectedMap))
        {
            GameManager.Instance.StartGame(selectedCharacter, ability);
            SceneManager.LoadScene(selectedMap);
        }
        else
        {
            Debug.LogError("GameManager.Instance или selectedMap не установлены!");
        }
    }

    public void OnBackClicked(int buttonId)
    {
        // backButtonId 0: MapSelection -> MainMenu
        // backButtonId 1: CharacterSelection -> MapSelection
        // backButtonId 2: AbilitySelection -> CharacterSelection
        switch (buttonId)
        {
            case 0:
                SetState(GameState.MainMenu);
                break;
            case 1:
                SetState(GameState.MapSelection);
                break;
            case 2:
                SetState(GameState.CharacterSelection);
                break;
            default:
                Debug.LogWarning($"Неизвестный ID кнопки возврата: {buttonId}");
                break;
        }
    }

    public void OnExitClicked()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

public enum GameState
{
    MainMenu,
    MapSelection,
    CharacterSelection,
    AbilitySelection,
    InGame
}