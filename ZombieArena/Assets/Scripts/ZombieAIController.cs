using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using static GameManager;
using System.Collections.Generic;

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

    private bool bloodMoonApplied = false;
    private bool materialsInitialized = false;
    private float blinkTimer = 0f;

    private List<Renderer> zombieRenderers = new List<Renderer>();
    private List<Material> originalMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();

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
                transform.localScale = new Vector3(0.7f, 1f, 0.7f);
                break;

            case ZombieType.Tank:
                transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                transform.position += new Vector3(0f, 0.25f, 0f); // vertical offset for scale
                break;

            default:
                transform.localScale = Vector3.one;
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

        if (GameManager.Instance.IsBloodMoon)
        {
            if (!bloodMoonApplied)
            {
                bloodMoonApplied = true;

                // Apply stat boost
                health *= GameManager.Instance.bloodMoonStatMultiplier;
                maxHealth = health;
                speed *= GameManager.Instance.bloodMoonStatMultiplier;
                agent.speed = speed;
                damage *= GameManager.Instance.bloodMoonStatMultiplier;

                // Apply scale boost
                transform.localScale *= GameManager.Instance.bloodMoonScaleMultiplier;

                // Setup blink material
                if (!materialsInitialized)
                {
                    zombieRenderers.Clear();
                    originalMaterials.Clear();
                    originalColors.Clear();

                    foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                    {
                        if (rend.GetComponentInParent<Canvas>() == null)
                        {
                            originalMaterials.Add(rend.sharedMaterial);
                            originalColors.Add(rend.sharedMaterial.color);
                            rend.material = new Material(rend.sharedMaterial); // use instance
                            zombieRenderers.Add(rend);
                        }
                    }

                    materialsInitialized = true;
                }
            }

            // Blinking effect
            blinkTimer += Time.deltaTime * GameManager.Instance.bloodMoonBlinkSpeed;
            float lerpValue = Mathf.PingPong(blinkTimer, 1f);
            Material blinkMat = GameManager.Instance.bloodMoonBlinkMaterial;

            for (int i = 0; i < zombieRenderers.Count; i++)
            {
                if (zombieRenderers[i] != null)
                {
                    zombieRenderers[i].material.color = Color.Lerp(originalColors[i], blinkMat.color, lerpValue);
                }
            }
        }
        else if (bloodMoonApplied)
        {
            // Restore materials
            for (int i = 0; i < zombieRenderers.Count; i++)
            {
                if (zombieRenderers[i] != null)
                {
                    zombieRenderers[i].material = originalMaterials[i];
                }
            }

            // Restore stats
            ZombieStats stats = GameManager.Instance.GetZombieStats(zombieType);
            float healthRatio = health / maxHealth;
            maxHealth = stats.health;
            health = healthRatio * maxHealth;

            if (healthBar != null)
            {
                healthBar.SetHealthBar(health / maxHealth);
            }

            speed = stats.speed;
            agent.speed = speed;
            damage = stats.damage;

            // Restore scale
            switch (zombieType)
            {
                case ZombieType.Fast:
                    transform.localScale = new Vector3(0.7f, 1f, 0.7f);
                    break;
                case ZombieType.Tank:
                    transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    break;
                default:
                    transform.localScale = Vector3.one;
                    break;
            }

            bloodMoonApplied = false;
            materialsInitialized = false;
            blinkTimer = 0f;
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
