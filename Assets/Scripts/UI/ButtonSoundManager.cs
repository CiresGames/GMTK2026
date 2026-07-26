using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundManager : MonoBehaviour
{
    public AudioClip buttonClickSound;

    private AudioSource audioSource;
    private HashSet<Button> registeredButtons = new();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (!registeredButtons.Contains(button))
            {
                registeredButtons.Add(button);
                button.onClick.AddListener(PlayButtonSound);
            }
        }
    }

    void PlayButtonSound()
    {
        if (buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }
}