using UnityEngine;

/// <summary>
/// Attach this to the needle (the child GameObject of the clock/chronometer).
/// Rotates the needle 360° over the total timer duration (maxTime), so that
/// it returns to its starting position exactly when currentTime reaches 0.
/// </summary>
public class Chronometer : MonoBehaviour
{
    [Header("Rotation settings")]
    [Tooltip("Check if the needle should rotate clockwise.")]
    [SerializeField] private bool clockwise = true;

    [Tooltip("Axis the needle rotates around. Z for a 2D/UI object, Y for a flat 3D object.")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward; // Z by default (2D / UI)

    private Quaternion initialRotation;

    private void Start()
    {
        // Store the needle's starting rotation (the "12 o'clock" position)
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        float maxTime = GameManager.Instance.maxTime;
        if (maxTime <= 0f)
            return; // avoid division by zero

        float currentTime = GameManager.Instance.currentTime;

        // Elapsed time progression: 0 = start (maxTime), 1 = end (0)
        float progress = 1f - Mathf.Clamp01(currentTime / maxTime);

        float angle = progress * 360f;
        if (clockwise)
            angle = -angle; // in Unity, a positive rotation on Z goes counter-clockwise

        transform.localRotation = initialRotation * Quaternion.AngleAxis(angle, rotationAxis);
    }
}
