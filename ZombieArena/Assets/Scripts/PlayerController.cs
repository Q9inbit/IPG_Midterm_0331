using UnityEngine;
using System.Collections;
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

    [SerializeField] private Transform gunTransform;
    [SerializeField] private float gunRecoil = 2f;
    private Vector3 gunDefaultLocalPos;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        gameManager = GameManager.Instance;
        fireTimer = fireCD;
        gunDefaultLocalPos = gunTransform.localPosition;
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
        StartCoroutine(RecoilGun());
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

    private IEnumerator RecoilGun()
    {
        float duration = fireCD / (3f*5f);
        float halfDuration = duration / 2f;
        float timer = 0f;
        Vector3 recoilOffset = Vector3.back * gunRecoil; // Adjust recoil distance as needed

        // Recoil back
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            gunTransform.localPosition = Vector3.Lerp(gunDefaultLocalPos, gunDefaultLocalPos + recoilOffset, t);
            timer += Time.deltaTime;
            yield return null;
        }

        // Return forward
        timer = 0f;
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            gunTransform.localPosition = Vector3.Lerp(gunDefaultLocalPos + recoilOffset, gunDefaultLocalPos, t);
            timer += Time.deltaTime;
            yield return null;
        }

        gunTransform.localPosition = gunDefaultLocalPos;
    }

}
