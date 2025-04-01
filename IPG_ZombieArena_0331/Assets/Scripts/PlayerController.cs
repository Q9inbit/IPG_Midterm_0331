using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 50f;
    [SerializeField]
    private float turnSpeed = 100f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(h, 0, v).normalized;

        if (inputDirection != Vector3.zero)
        {
            // Move player
            Vector3 move = transform.position + inputDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(move);

            // Rotate player toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}
