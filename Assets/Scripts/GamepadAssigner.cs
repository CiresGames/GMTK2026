using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadAssigner : MonoBehaviour
{
    public static GamepadAssigner Instance { get; private set; }

    [SerializeField] private int maxPlayers = 2;
    private Gamepad[] assignedGamepads;
    private int nextIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        assignedGamepads = new Gamepad[maxPlayers];
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        Debug.Log($"[GamepadAssigner] Gamepad.all count at OnEnable: {Gamepad.all.Count}");
        foreach (var gamepad in Gamepad.all)
            AssignGamepad(gamepad);
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added && device is Gamepad gamepad)
            AssignGamepad(gamepad);
    }

    private void AssignGamepad(Gamepad gamepad)
    {
        foreach (var g in assignedGamepads)
            if (g == gamepad) return; // already assigned, ignore

        if (nextIndex >= maxPlayers) return;
        assignedGamepads[nextIndex] = gamepad;
        Debug.Log($"Player {nextIndex} assigné à {gamepad.displayName}");
        nextIndex++;
    }

    public Gamepad GetGamepadForPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= assignedGamepads.Length) return null;

        // Lazily catch any gamepads that existed before OnEnable finished enumerating
        if (assignedGamepads[playerIndex] == null)
        {
            foreach (var gamepad in Gamepad.all)
                AssignGamepad(gamepad);
        }

        return assignedGamepads[playerIndex];
    }
}