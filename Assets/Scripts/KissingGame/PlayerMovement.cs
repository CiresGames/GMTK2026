using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 6f;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(moveInput.x, moveInput.y, 0f);
        rb.AddForce(direction * moveForce, ForceMode.Force);

        Vector3 planarVel = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
        if (planarVel.magnitude > maxSpeed)
        {
            Vector3 clamped = planarVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(clamped.x, clamped.y, rb.linearVelocity.z);
        }
    }
}