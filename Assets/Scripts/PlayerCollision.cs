using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    public int playerIndex; // 1 ou 2, à set dans l'inspecteur
    private KissingGame kissingGame;

    void Awake()
    {
        kissingGame = GetComponentInParent<KissingGame>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            kissingGame.OnPlayerCollision(playerIndex);
        }
    }
}