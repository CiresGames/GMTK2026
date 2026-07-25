using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class MicroGamePlayer : MonoBehaviour
{
    public MicroGameSO microGame;
    [SerializeField] TextMeshProUGUI instructionLabel;
    public bool hasResolved = false;
    public bool canInteract = false;
    [SerializeField] Image p1Image, p2Image;

    public void UpdateInstructions(string instruction, Sprite p1Control, Sprite p2Control)
    {
        instructionLabel.text = instruction;
        p1Image.sprite = p1Control;
        p2Image.sprite = p2Control; 
    }

    public void DisplayInstructions(bool flag)
    {
        instructionLabel.gameObject.SetActive(flag);
    }


    public IEnumerator RunGame(MicroGameSO microGameSO)
    {
        DisplayInstructions(true);
        UpdateInstructions(microGameSO.instruction, microGameSO.p1Control, microGameSO.p2Control);
        canInteract = true;

        float timer = 0f;
        while (timer < microGameSO.instructionDuration && !hasResolved)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        DisplayInstructions(false);

        while (!GameManager.Instance.isGameOver && !hasResolved)
        {
            yield return null;
        }
        canInteract = false;
        if (hasResolved)
        {
            StartCoroutine(Success());
        }
        else StartCoroutine(Failure());
    }


    public virtual bool ResolveGame()
    {
        return true; 
    }

    public virtual void Update()
    {
       
    }

    public virtual IEnumerator Success()
    {
        yield return null;
    }

    public virtual IEnumerator Failure()
    {
        yield return null;
    }




}
