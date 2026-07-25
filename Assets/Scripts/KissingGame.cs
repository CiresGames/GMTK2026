using UnityEngine;

public class KissingGame : MonoBehaviour
{
    private bool player1Kissing = false;
    private bool player2Kissing = false;

    public void OnPlayerCollision(int playerIndex)
    {
        if (playerIndex == 1) player1Kissing = true;
        if (playerIndex == 2) player2Kissing = true;

        if (player1Kissing && player2Kissing)
        {
            Debug.Log("Les deux joueurs se sont embrassés!");
            player1Kissing = false;
            player2Kissing = false;
        }
    }
}