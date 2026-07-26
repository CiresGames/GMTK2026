using TMPro;
using UnityEngine;
using System.Collections;

public class CountdownUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Animation")]
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float animationDuration = 0.25f;

    private int lastSecond = -1;
    private bool hasFinished = false;

    private void Start()
    {
        countdownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        float time = GameManager.Instance.currentTime;

        if (time <= 5 && time > 0)
        {
            int seconds = Mathf.CeilToInt(time);

            if (seconds != lastSecond)
            {
                lastSecond = seconds;
                StartCoroutine(ShowCountdown(seconds.ToString()));
            }
        }

        if (time <= 0 && !hasFinished)
        {
            hasFinished = true;
            StartCoroutine(ShowCountdown("HAPPY NEW YEAR!"));
        }
    }

    private IEnumerator ShowCountdown(string message)
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = message;

        countdownText.transform.localScale = Vector3.one * startScale;

        float elapsed = 0;

        // Scale up
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / animationDuration;

            countdownText.transform.localScale =
                Vector3.Lerp(
                    Vector3.one * startScale,
                    Vector3.one * popScale,
                    t
                );

            yield return null;
        }

        // Scale back down slightly
        elapsed = 0;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / animationDuration;

            countdownText.transform.localScale =
                Vector3.Lerp(
                    Vector3.one * popScale,
                    Vector3.one,
                    t
                );

            yield return null;
        }

        countdownText.transform.localScale = Vector3.one;
    }
}