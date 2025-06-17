using UnityEngine;

[System.Serializable]
public class Ability
{
    public string name;
    public float damage = 20f;
    public float cooldown = 1f;
    public float radius = 4f;
    public float aoeRadius = 2f;
    public DamageType damageType = DamageType.Physical;
    public int level = 1;
    public int maxLevel = 5;
    public GameObject effectPrefab;
    public Tier tier;
}

public enum DamageType
{
    Physical, Fire, Ice, Electric
}

public enum Tier
{
    D, C, B, A, S
}