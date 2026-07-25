using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FusedPlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float drag = 4f;

    private Rigidbody rb;
    private Vector2 moveInput1;
    private Vector2 moveInput2;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = drag;
        rb.freezeRotation = true;
    }

    public void OnMove(InputValue value)
    {
        moveInput1 = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 combined = moveInput1 + moveInput2;
        combined = Vector2.ClampMagnitude(combined, 1f);

        Vector3 direction = new Vector3(combined.x, 0f, combined.y);
        rb.AddForce(direction * speed, ForceMode.Force);

        Vector3 flat = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flat.magnitude > speed)
        {
            Vector3 capped = flat.normalized * speed;
            rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
        }
    }
}