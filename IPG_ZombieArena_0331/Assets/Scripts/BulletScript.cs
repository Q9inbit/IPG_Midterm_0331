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

    private Transform target;

    private void Start()
    {
        rayCount = spreadAngle;
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;


        for (int i = 0; i < rayCount; i++)
        {
            float halfSpread = spreadAngle / 2f;
            float increment = spreadAngle / (rayCount - 1);
            angleOffset = -halfSpread + (increment * i);

            Vector3 rayDirection = Quaternion.AngleAxis(angleOffset, Vector3.up) * transform.forward;

            Vector3 rayOrigin = transform.position;
            rayOrigin -= transform.forward * raycastBackOffset;

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
        if (bestTarget != null)
        {
            target = bestTarget;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 targetDirection = (target.position - transform.position).normalized;
            Vector3 newForward = Vector3.Lerp(transform.forward, targetDirection, trackingStrength * Time.deltaTime).normalized;
            transform.forward = newForward;
        }
        transform.position += transform.forward * bulletSpeed * Time.deltaTime;
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
