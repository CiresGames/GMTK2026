using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadAssigner : MonoBehaviour
{
    // Glisse Player1 en premier, Player2 en deuxième dans l'inspecteur
    [SerializeField] private PlayerInput[] playerInputs;

    private int nextIndex = 0;

    private void Awake()
    {
        // On désactive les PlayerInput au départ pour qu'aucun joueur
        // ne soit contrôlable tant qu'une manette ne lui est pas assignée
        foreach (var pi in playerInputs)
        {
            pi.enabled = false;
        }
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;

        // Gère les manettes déjà branchées au lancement du jeu
        foreach (var gamepad in Gamepad.all)
        {
            AssignGamepad(gamepad);
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added && device is Gamepad gamepad)
        {
            AssignGamepad(gamepad);
        }
    }

    private void AssignGamepad(Gamepad gamepad)
    {
        if (nextIndex >= playerInputs.Length) return;

        PlayerInput pi = playerInputs[nextIndex];
        pi.enabled = true;
        pi.SwitchCurrentControlScheme("Gamepad", gamepad);

        Debug.Log($"{pi.gameObject.name} assigné à {gamepad.displayName}");

        nextIndex++;
    }
}