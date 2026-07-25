using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // ---------- Singleton ----------
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float maxTime = 30f;
    public float currentTime; 
    [SerializeField] TextMeshProUGUI timerLabel;

    [Header("Input")]
    [SerializeField] private InputActionReference reloadAction; // drag the "Reload" action asset here

    public bool isGameOver = false;
    public bool hasStarted = false;

    public MicroGameManager microGameManager; 

    private void Awake()
    {
        // Enforce singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentTime = maxTime;
    }

    private void OnEnable()
    {
        if (reloadAction != null)
        {
            reloadAction.action.performed += OnReloadPerformed;
            reloadAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (reloadAction != null)
        {
            reloadAction.action.performed -= OnReloadPerformed;
            reloadAction.action.Disable();
        }
    }

    private void OnReloadPerformed(InputAction.CallbackContext ctx)
    {
        ResetTimer();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("mainScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("mainMenu");
    }

    public void ResetTimer()
    {
        currentTime = maxTime;
    }

    public float TimeLeft()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            TimeOut(true); 
        }
        return currentTime;
    }

    public void UpdateTimerLabel()
    {
        currentTime = TimeLeft();
        timerLabel.text = $"{currentTime:F2} seconds";
    }

    private void Update()
    {
        if (!isGameOver && hasStarted)
        {
            UpdateTimerLabel();
        }
    }

    public void TimeOut(bool flag)
    {
        
        isGameOver = flag;
       
    }

    public void StartTimer(bool flag)
    {
        hasStarted = flag; 
    }
}