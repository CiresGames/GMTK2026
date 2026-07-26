using UnityEngine;

public class IntroScreenAnimation : MonoBehaviour
{
    public GameObject introScreen;
    public Animation introScreenSlideInAnimation;

    public void PlayIntroScreenAnimation()
    {
        if (introScreen == null)
        {
            Debug.LogError("Intro Screen is not assigned!", this);
            return;
        }

        if (introScreenSlideInAnimation == null)
        {
            Debug.LogError("Intro Screen Animation is not assigned!", this);
            return;
        }

        introScreen.SetActive(true);
        introScreenSlideInAnimation.Play();
    }
}