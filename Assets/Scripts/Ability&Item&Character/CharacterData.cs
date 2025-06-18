using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public float moveSpeed = 5f;
    public bool isUnlocked = true;
    public CharacterBonus bonus;

    [System.Serializable]
    public struct CharacterBonus
    {
        public string bonusType; // Например, "FireDamage", "Evasion"
        public float bonusValue; // Например, 0.15f для +15% урона
    }

    private int level = 1;
    private int experience = 0;

    public int Level => level;
    public int Experience => experience;

    public void AddExperience(int amount, int[] expToNextLevel, int maxLevel)
    {
        experience += amount;
        int targetIndex = level - 1;
        while (level < maxLevel && targetIndex < expToNextLevel.Length && experience >= expToNextLevel[targetIndex])
        {
            experience -= expToNextLevel[targetIndex];
            level++;
            targetIndex = level - 1;
        }
    }
}