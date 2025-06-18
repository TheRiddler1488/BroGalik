using UnityEngine;
using System.Collections.Generic;

public class AttackBehaviour : MonoBehaviour
{
    [SerializeField] private ObjectPool attackEffectPool;
    private PlayerController owner;
    private AttackSystem attackSystem;
    private List<AbilityData> abilities = new List<AbilityData>();
    private List<float> cooldownTimers = new List<float>();

    void Awake()
    {
        owner = GetComponentInParent<PlayerController>();
        if (owner == null)
        {
            Debug.LogError("AttackBehaviour requires PlayerController on parent.");
            enabled = false;
            return;
        }

        attackSystem = FindFirstObjectByType<AttackSystem>();
        if (attackSystem == null)
        {
            Debug.LogError("AttackSystem not found in scene.");
            enabled = false;
            return;
        }

        attackSystem.Register(this);
    }

    void OnDestroy()
    {
        if (attackSystem != null)
        {
            attackSystem.Unregister(this);
        }
    }

    public void ClearAbilities()
    {
        abilities.Clear();
        cooldownTimers.Clear();
    }

    public void AddAbility(AbilityData ability)
    {
        if (abilities.Count >= 6) return;
        abilities.Add(ability);
        cooldownTimers.Add(0f);
    }

    public void Tick(float deltaTime)
    {
        if (!enabled) return;

        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] == null) continue;

            cooldownTimers[i] -= deltaTime;
            if (cooldownTimers[i] <= 0f)
            {
                TryAttack(i);
                cooldownTimers[i] = abilities[i].cooldown;
            }
        }
    }

    private void TryAttack(int abilityIndex)
    {
        var abilityData = abilities[abilityIndex];
        if (abilityData == null || owner == null) return;

        GameObject closestEnemy = FindClosestEnemy(owner.transform.position, abilityData.radius);
        if (closestEnemy == null) return;

        Vector3 attackCenter = closestEnemy.transform.position;
        Collider[] targets = Physics.OverlapSphere(attackCenter, abilityData.aoeRadius);
        foreach (var target in targets)
        {
            if (target.TryGetComponent<EnemyStats>(out var enemy))
            {
                enemy.TakeDamage((int)abilityData.damage, abilityData.damageType);
            }
        }

        Vector3 effectPosition = attackCenter + Vector3.up;
        Vector3 direction = (attackCenter - owner.transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        if (attackEffectPool != null)
        {
            GameObject effect = attackEffectPool.Get(effectPosition);
            effect.transform.rotation = rotation;
            var pooledEffect = effect.GetComponent<PooledEffect>();
            if (pooledEffect != null) pooledEffect.Init(attackEffectPool, 1f);
            UIManager.Instance?.IncrementAttackCounter(abilityData.attackName); // Используем attackName
        }
        else if (abilityData.effectPrefab)
        {
            GameObject effect = Instantiate(abilityData.effectPrefab, effectPosition, rotation);
            UIManager.Instance?.IncrementAttackCounter(abilityData.attackName); // Используем attackName
        }
    }

    private GameObject FindClosestEnemy(Vector3 center, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        GameObject closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyStats>(out var enemy))
            {
                float distance = Vector3.Distance(center, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = hit.gameObject;
                }
            }
        }
        return closestEnemy;
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        foreach (var ability in abilities)
        {
            if (ability == null) continue;

            GameObject closestEnemy = FindClosestEnemy(transform.position, ability.radius);
            Vector3 attackCenter = closestEnemy != null ? closestEnemy.transform.position : transform.position;
            attackCenter.y = 0.01f;

            UnityEditor.Handles.color = new Color(0f, 0f, 1f, 0.3f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, ability.radius);

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.3f);
            UnityEditor.Handles.DrawWireDisc(attackCenter, Vector3.up, ability.aoeRadius);
        }
    }
    #endif
}