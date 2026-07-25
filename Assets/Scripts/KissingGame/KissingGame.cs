using UnityEngine;
using UnityEngine.InputSystem;

public class KissingGame : MonoBehaviour
{
    private bool player1Kissing = false;
    private bool player2Kissing = false;

    public GameObject player1;
    public GameObject player2;
    public GameObject fusedPlayerPrefab;

    public void OnLipsContact(int playerIndex, bool hitLips)
    {
        if (!hitLips)
        {
            Debug.Log("Raté! Pas sur les lèvres.");
            return;
        }

        if (playerIndex == 1) player1Kissing = true;
        if (playerIndex == 2) player2Kissing = true;

        if (player1Kissing && player2Kissing)
        {
            Fuse();
        }
    }

    private void Fuse()
    {
        Vector3 middlePoint = (player1.transform.position + player2.transform.position) / 2f;

        GameObject fused = Instantiate(fusedPlayerPrefab, middlePoint, Quaternion.identity);

        PlayerInput fusedInput = fused.GetComponent<PlayerInput>();
        var p1Input = player1.GetComponent<PlayerInput>();
        var p2Input = player2.GetComponent<PlayerInput>();

        fusedInput.SwitchCurrentControlScheme("Gamepad",
            p1Input.devices[0],
            p2Input.devices[0]);

        player1.SetActive(false);
        player2.SetActive(false);

        Debug.Log("Les deux joueurs s'embrassent!");
    }
}