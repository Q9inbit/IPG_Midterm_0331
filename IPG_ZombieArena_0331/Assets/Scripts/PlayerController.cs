using UnityEngine;
using UnityEngine.Audio;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 200f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCD = 3f; // Higher = Slower Rate
    [SerializeField] private float bulletTravelSpeed = 20f;
    private float fireTimer = 0f;
    GameManager gameManager;

    private Rigidbody rb;
    [SerializeField]
    private float health = 20f;

    [SerializeField] private AudioClip fireSound;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = GameManager.Instance;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        fireTimer = fireCD;
    }

    void Update()
    {
        if (!gameManager.isPaused)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (fireTimer >= fireCD)
                {
                    FireBullet();
                }
            }
            fireTimer += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(h, 0f, v).normalized;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

            rb.linearVelocity = inputDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void FireBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<BulletScript>().SetSpeed(bulletTravelSpeed);

        audioSource.PlayOneShot(fireSound);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(health);
        if ((health <= 0))
        {
            gameManager.GameOver();
        }
    }
}
