using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class BulletScript : MonoBehaviour
{
    private float bulletSpeed;
    [SerializeField] private int damage;
    [SerializeField] private float trackingStrength;

    private int rayCount = 5;
    [SerializeField] private int spreadAngle;
    [SerializeField] private float rayMaxDistance;
    [SerializeField] private float raycastBackOffset;
    [SerializeField] private LayerMask Enemy;
    private float angleOffset;

    private float bulletLifetime = 0f;
    [SerializeField] private float trackingDuration = 1f;

    private Transform target;

    private void Start()
    {
        float snapRadius = 3f; // Distance to snap to nearest enemy if very close
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        // STEP 1: Direct proximity check (snap to enemy if nearby)
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, snapRadius, Enemy);
        foreach (Collider enemy in nearbyEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = enemy.transform;
                }
            }
        }

        // STEP 2: If no nearby enemy, use ray-based target search
        if (bestTarget == null)
        {
            rayCount = spreadAngle;
            for (int i = 0; i < rayCount; i++)
            {
                float halfSpread = spreadAngle / 2f;
                float increment = spreadAngle / (rayCount - 1);
                angleOffset = -halfSpread + (increment * i);

                Vector3 rayDirection = Quaternion.AngleAxis(angleOffset, Vector3.up) * transform.forward;
                Vector3 rayOrigin = transform.position - transform.forward * raycastBackOffset;

                Debug.DrawRay(rayOrigin, rayDirection * rayMaxDistance, Color.red, 2f);

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayMaxDistance, Enemy))
                {
                    if (hit.collider.CompareTag("Enemy") && hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        bestTarget = hit.collider.transform;
                    }
                }
            }
        }

        // STEP 3: Lock target
        if (bestTarget != null)
        {
            target = bestTarget;
        }
    }

    private void Update()
    {
        bulletLifetime += Time.deltaTime;

        if (target != null && bulletLifetime <= trackingDuration)
        {
            Vector3 targetDirection = (target.position - transform.position);
            targetDirection.y = 0f; // Prevent tilting up/down
            Vector3 newForward = Vector3.Lerp(transform.forward, targetDirection.normalized, trackingStrength * Time.deltaTime).normalized;
            transform.forward = newForward;
        }

        // Move only on XZ plane, preserve Y
        Vector3 move = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized * bulletSpeed * Time.deltaTime;
        transform.position += move;
    }



    private bool hasHit = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!hasHit)
        {
            if (other.CompareTag("Enemy"))
            {
                ZombieAIController zombie = other.GetComponent<ZombieAIController>();
                zombie.TakeDamage(damage);
            }
            hasHit = true;
        }
        Destroy(gameObject);
    }

    public void SetSpeed(float speed)
    {
        bulletSpeed = speed;
    }
}
