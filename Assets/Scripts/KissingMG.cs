using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KissingMG : MicroGamePlayer
{
    [SerializeField] MicroGameSO microGame;
    [SerializeField] TextMeshProUGUI outcomeLabel;

    [SerializeField] InputActionReference debugAction;

    private void OnEnable()
    {
        if (debugAction != null)
            debugAction.action.performed += OnDebugPerformed;
    }

    private void OnDisable()
    {
        if (debugAction != null)
            debugAction.action.performed -= OnDebugPerformed;
    }

    private void Start()
    {
        StartCoroutine(RunGame(microGame));
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
        outcomeLabel.gameObject.SetActive(true);
        outcomeLabel.text = "Success!";
        yield return null;
    }

    public override IEnumerator Failure()
    {
        outcomeLabel.gameObject.SetActive(true);
        outcomeLabel.text = "Failure!";
        yield return null; 
    }

}
