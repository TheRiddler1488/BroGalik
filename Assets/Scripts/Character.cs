using UnityEngine;

[System.Serializable]
public class Character
{
    public string name;
    public float moveSpeed = 5f;
    public int level = 1;
    public int experience = 0;
    public int[] expToNextLevel; // Массив опыта для следующего уровня
}