using UnityEngine;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{
    
    [SerializeField] Button startButton, quitButton, settingsButton;

    private void Start()
    {
        startButton.onClick.AddListener(GameManager.Instance.StartGame);
        quitButton.onClick.AddListener(GameManager.Instance.QuitGame);
    }






}
