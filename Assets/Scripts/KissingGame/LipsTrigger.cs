using UnityEngine;

public class LipsTrigger : MonoBehaviour
{
    public int playerIndex;
    private KissingMG kissingGame;

    void Awake()
    {
        kissingGame = GetComponentInParent<KissingMG>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lips"))
            kissingGame.OnLipsContact(playerIndex, true);
        else if (other.CompareTag("Head"))
            kissingGame.OnLipsContact(playerIndex, false);
    }
}