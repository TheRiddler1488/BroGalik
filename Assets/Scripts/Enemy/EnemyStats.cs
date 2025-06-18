using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private ObjectPool pool;
    public int damage = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    private float lastAttackTime;
    private PlayerController player;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        player = FindFirstObjectByType<PlayerController>();
        if (gameManager == null || player == null)
        {
            Debug.LogError("GameManager или PlayerController не найдены в сцене!");
            enabled = false;
            return;
        }
    }

    public void Init(ObjectPool fromPool)
    {
        pool = fromPool;
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (player != null && Time.time - lastAttackTime >= attackCooldown)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(int dmg, DamageType type)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void AttackPlayer()
    {
        if (player != null && gameManager != null)
        {
            gameManager.PlayerHP -= damage; // Use public property
        }
    }

    void Die()
    {
        if (gameManager != null)
        {
            gameManager.AddSouls(gameManager.settings.soulsPerKill); // Use public method
            gameManager.AddExperience(gameManager.settings.experiencePerKill);
        }
        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Debug.LogWarning("Pool не задан, враг будет уничтожен!");
            Destroy(gameObject);
        }
    }
}