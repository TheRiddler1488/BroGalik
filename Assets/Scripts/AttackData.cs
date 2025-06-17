using UnityEngine;

[CreateAssetMenu(fileName = "New AttackData", menuName = "ScriptableObjects/AttackData", order = 1)]
public class AttackData : ScriptableObject
{
    public string attackName;
    public float damage;
    public float cooldown;
    public float radius;
    public float aoeRadius;
    public DamageType damageType;
    public GameObject effectPrefab;
    public Tier tier;
}