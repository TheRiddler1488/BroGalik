using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityData", menuName = "ScriptableObjects/AbilityData", order = 2)]
public class AbilityData : ScriptableObject
{
    public string attackName; // Use attackName instead
    public float damage = 20f;
    public float cooldown = 1f;
    public float radius = 4f;
    public float aoeRadius = 2f;
    public DamageType damageType = DamageType.Physical;
    public GameObject effectPrefab;
    public Tier tier = Tier.D;
}