using UnityEngine;
using UnityEngine.AI;
using static GameManager;

public class ZombieAIController : MonoBehaviour
{
    private float health;
    private float maxHealth;
    private float speed;
    private float damage;
    [HideInInspector]
    public ZombieType zombieType;
    private NavMeshAgent agent;

    [SerializeField] private float damageCD = 3f;
    private float damageTimer = 0f;

    [SerializeField] private float approachOffset = 2f;

    private ZombieHealthBar healthBar;

    private void Start()
    {
        healthBar = GetComponentInChildren<ZombieHealthBar>();
        healthBar.SetHealthBar(1f);

        damageTimer = damageCD;


        ZombieStats stats = GameManager.Instance.GetZombieStats(zombieType);
        health = stats.health;
        maxHealth = health;
        speed = stats.speed;
        damage = stats.damage;

        switch (zombieType)
        {
            case ZombieType.Fast:
                transform.localScale = new Vector3(
                    transform.localScale.x * 0.7f,
                    transform.localScale.y,
                    transform.localScale.z * 0.7f
                );
                break;

            case ZombieType.Tank:
                float scaleFactor = 1.5f;
                transform.localScale = new Vector3(
                    transform.localScale.x * scaleFactor,
                    transform.localScale.y * scaleFactor,
                    transform.localScale.z * scaleFactor
                );
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + ((scaleFactor - 1f) * 0.5f),
                    transform.position.z
                );
                break;

            default:
                break;
        }


        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = approachOffset;
        agent.speed = speed;
        agent.acceleration = 300f;
        agent.angularSpeed = 300f;
        agent.autoBraking = false;
    }

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        agent.SetDestination(player.transform.position);

        if (damageTimer <= damageCD)
        {
            damageTimer += Time.deltaTime;
        }
    }

    public void TakeDamage(int damageTaken)
    {
        health -= damageTaken;
        float value = Mathf.Clamp01(health / maxHealth);
        healthBar.SetHealthBar(value);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerHealth = other.GetComponent<PlayerController>();
            if (damageTimer >= damageCD)
            {
                playerHealth.TakeDamage(damage);
                damageTimer = 0f;
            }
        }
    }
}
