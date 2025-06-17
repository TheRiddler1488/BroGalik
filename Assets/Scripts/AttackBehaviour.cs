using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttackBehaviour : MonoBehaviour
{
    public AttackData attackData;
    private float cooldownTimer = 0f;
    private PlayerController owner;
    public ObjectPool attackEffectPool;
    private AttackSystem attackSystem;

    void Awake()
    {
        owner = GetComponentInParent<PlayerController>();
        if (owner == null)
        {
            Debug.LogError("AttackBehaviour требует компонент PlayerController на родительском объекте.");
            enabled = false;
            return;
        }

        attackSystem = FindFirstObjectByType<AttackSystem>();
        if (attackSystem == null)
        {
            Debug.LogError("AttackSystem не найден в сцене.");
            enabled = false;
            return;
        }

        attackSystem.Register(this);
        cooldownTimer = 0f;
    }

    void OnDestroy()
    {
        if (attackSystem != null)
        {
            attackSystem.Unregister(this);
        }
    }

    public void Tick(float deltaTime)
    {
        if (attackData == null || !enabled) return;

        cooldownTimer -= deltaTime;
        if (cooldownTimer <= 0f)
        {
            TryAttack();
            cooldownTimer = attackData.cooldown;
        }
    }

    private void TryAttack()
    {
        if (attackData == null || owner == null)
        {
            Debug.LogError("Не удалось выполнить атаку: attackData или owner равны null.");
            return;
        }

        GameObject closestEnemy = FindClosestEnemy(owner.transform.position, attackData.radius);
        if (closestEnemy == null)
        {
            Debug.Log("Нет врагов в радиусе атаки.");
            return;
        }

        Vector3 attackCenter = closestEnemy.transform.position;
        Collider[] targets = Physics.OverlapSphere(attackCenter, attackData.aoeRadius);
        foreach (var target in targets)
        {
            if (target.TryGetComponent<EnemyStats>(out var enemy))
            {
                enemy.TakeDamage((int)attackData.damage, attackData.damageType);
            }
        }

        Vector3 effectPosition = attackCenter + Vector3.up;
        Vector3 direction = (attackCenter - owner.transform.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        rotation *= Quaternion.Euler(0, 0, 0);

        if (attackEffectPool != null)
        {
            GameObject effect = attackEffectPool.Get(effectPosition);
            effect.transform.rotation = rotation;
            var pooledEffect = effect.GetComponent<PooledEffect>();
            if (pooledEffect != null) pooledEffect.Init(attackEffectPool, 1f);
            UIManager.Instance?.IncrementAttackCounter(attackData.effectPrefab?.name ?? attackData.attackName);
        }
        else if (attackData.effectPrefab)
        {
            GameObject effect = Instantiate(attackData.effectPrefab, effectPosition, rotation);
            UIManager.Instance?.IncrementAttackCounter(attackData.effectPrefab.name);
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
        if (attackData == null) return;

        GameObject closestEnemy = FindClosestEnemy(transform.position, attackData.radius);
        Vector3 attackCenter = closestEnemy != null ? closestEnemy.transform.position : transform.position;
        attackCenter.y = 0.01f;

        Handles.color = new Color(0f, 0f, 1f, 0.3f);
        Handles.DrawWireDisc(transform.position, Vector3.up, attackData.radius);

        Handles.color = new Color(1f, 0f, 0f, 0.3f);
        Handles.DrawWireDisc(attackCenter, Vector3.up, attackData.aoeRadius);
    }
    #endif
}