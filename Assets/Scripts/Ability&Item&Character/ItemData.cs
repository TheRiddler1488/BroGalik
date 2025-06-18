using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 3)]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string effectType; // Например, "Invulnerability"
    public float effectValue; // Например, 10f для 10 секунд неуязвимости
    public Tier tier = Tier.D;
}