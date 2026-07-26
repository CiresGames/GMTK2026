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
    [SerializeField] AudioSource kissSource, ambient, music; 



    private bool player1Kissing = false;
    private bool player2Kissing = false;

    private void Awake()
    {
        moveAction = p1.actions["Kiss/Move"]; 
    }



    private void OnEnable()
    {
        moveAction.Enable();
        ambient.Play();
        music.Play(); 
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



    
    public override bool ResolveGame()
    {
        return true; 
    }

    public override IEnumerator Success()
    {

        GameManager.Instance.StartTimer(false);
        Fuse();
        yield return new WaitForSeconds(3f);
        GameManager.Instance.microGameManager.CompleteGameCoroutine(GameManager.Instance.microGameManager.currentGameIndex);
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
            hasResolved = true;
        }
    }

    private void Fuse()
    {
        Vector3 middlePoint = (player1.transform.position + player2.transform.position) / 2f;
        kissSource.Play(); 

        var p1Input = player1.GetComponent<PlayerInput>();
        var p2Input = player2.GetComponent<PlayerInput>();

        fusedPlayerPrefab.SetActive(true); 
        fusedPlayerPrefab.transform.position = middlePoint;

        player1.SetActive(false);
        player2.SetActive(false);

        Debug.Log("Les deux joueurs s'embrassent!");
    }

}
