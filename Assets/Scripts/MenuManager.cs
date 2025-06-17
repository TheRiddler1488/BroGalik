using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject mapSelectionUI;
    public GameObject characterSelectionUI;

    private GameManager gameManager;
    private string selectedMap;
    private Character selectedCharacterData;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null) gameManager.menuManager = this;
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState state)
    {
        mainMenuUI.SetActive(state == GameState.MainMenu);
        mapSelectionUI.SetActive(state == GameState.MapSelection);
        characterSelectionUI.SetActive(state == GameState.CharacterSelection);
        // Убрано abilityShopUI, так как выбор способности интегрирован в characterSelectionUI
    }

    public void OnPlayClicked() => SetState(GameState.MapSelection);
    public void OnMapSelected(string mapName)
    {
        selectedMap = mapName;
        SetState(GameState.CharacterSelection);
    }
    public void OnCharacterSelected(string characterName)
    {
        selectedCharacterData = CreateCharacterWithBonus(characterName);
        if (gameManager != null) gameManager.selectedCharacter = selectedCharacterData;
        // Вместо перехода к abilityShopUI, сразу выбираем способность и начинаем игру
        AcceptShopOption(0); // Пример: выбираем первую доступную способность (нужно настроить UI)
        OnStartGame();
    }
    public void OnStartGame()
    {
        if (gameManager != null)
        {
            gameManager.StartGame();
            SceneManager.LoadScene(selectedMap);
        }
    }

    private Character CreateCharacterWithBonus(string characterName)
    {
        Character character = new Character { name = characterName, moveSpeed = 5f };
        switch (characterName)
        {
            case "Warrior":
                character.moveSpeed += 1f; // +1 скорость
                break;
            case "Mage":
                // Бонус +15% урона от огня (реализуем в AttackBehaviour)
                break;
            case "Rogue":
                // Бонус +30% уклонения (реализуем позже)
                break;
        }
        return character;
    }

    public void AcceptShopOption(int index)
    {
        if (gameManager != null) gameManager.ApplyShopOption(index); // Выбор способности перед началом игры
    }

    public void DeclineShopOption() => SetState(GameState.InGame);
}

public enum GameState
{
    MainMenu,
    MapSelection,
    CharacterSelection,
    InGame
}