using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KissingMG : MicroGamePlayer
{


    public GameObject player1;
    public GameObject player2;
    public GameObject fusedPlayerPrefab;
    [SerializeField] PlayerInput p1, p2;
    [SerializeField] InputAction moveAction; 


    private bool player1Kissing = false;
    private bool player2Kissing = false;

    private void Awake()
    {
        moveAction = p1.actions["Kiss/Move"]; 
    }



    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable(); 
    }

    private void Start()
    {
        var gp1 = GamepadAssigner.Instance.GetGamepadForPlayer(0);
        if (gp1 != null)
            p1.SwitchCurrentControlScheme("Gamepad", gp1);

        var gp2 = GamepadAssigner.Instance.GetGamepadForPlayer(1);
        if (gp2 != null)
            p2.SwitchCurrentControlScheme("Gamepad", gp2);

        Debug.Log($"gp1: {gp1?.displayName ?? "NULL"}, gp2: {gp2?.displayName ?? "NULL"}");


    }




    public override void Update()
    {
        if (canInteract)
        {
            player1.GetComponent<PlayerMovement>().enabled = true;
            player2.GetComponent<PlayerMovement>().enabled = true;
        }

        else
        {
            player1.GetComponent<PlayerMovement>().enabled = false;
            player2.GetComponent<PlayerMovement>().enabled = false;
        }

    }



    public void OnDebugPerformed(InputAction.CallbackContext ctx)
    {
        if (!canInteract) return;
        hasResolved = ResolveGame();
    }

    public override bool ResolveGame()
    {
        return true; 
    }

    public override IEnumerator Success()
    {
       
        yield return null;
    }

    public override IEnumerator Failure()
    {
       
        yield return null; 
    }

   

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

        var p1Input = player1.GetComponent<PlayerInput>();
        var p2Input = player2.GetComponent<PlayerInput>();

        PlayerInput fusedInput = PlayerInput.Instantiate(
            fusedPlayerPrefab,
            controlScheme: "Gamepad",
            pairWithDevices: new InputDevice[] { p1Input.devices[0], p2Input.devices[0] }
        );

        fusedInput.transform.position = middlePoint;

        player1.SetActive(false);
        player2.SetActive(false);

        Debug.Log("Les deux joueurs s'embrassent!");
    }

}
