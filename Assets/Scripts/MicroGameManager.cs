using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;


public class MicroGameManager : MonoBehaviour
{
    [SerializeField] Animator listAnimator;
    [SerializeField] List<MicroGamePlayer> gameList;
    [SerializeField] List<TextMeshProUGUI> gameNames;
    [SerializeField] List<RectTransform> currentGameIcon; 
    [SerializeField] List<RectTransform> completedGame;
    public int currentGameIndex = 0;
    private const string TRIGGER_ANIM = "triggerAnim";



    public InputActionReference debugAction;

    private void Start()
    {
        Initialize(); 
    }



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

    public void OnDebugPerformed(InputAction.CallbackContext ctx)
    {
        StartCoroutine(CompleteGame(currentGameIndex)); 
    }


    public void ShowResolution(bool flag)
    {
        listAnimator.SetBool(TRIGGER_ANIM, flag); 
    }


    private IEnumerator CompleteGame(int index)
    {
        ShowResolution(true);
        yield return null;
        float clipLength = listAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(0.5f);

        currentGameIcon[index].gameObject.SetActive(false);
        completedGame[index].gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);


        if (!(index + 1 >= currentGameIcon.Count))
        {
            currentGameIcon[index + 1].gameObject.SetActive(true);
            currentGameIndex++;
        }


    }

    public void StartGame(int index)
    {
        gameList[index].gameObject.SetActive(true); 
        StartCoroutine(gameList[index].RunGame(gameList[index].microGame));
    }

    public void Initialize()
    {
        for (int i = 0; i < gameNames.Count; i++)
        {
            gameNames[i].text = gameList[i].microGame.uid; 
        }
    }
    
    public void OnAnimationComplete()
    {
        ShowResolution(false);
    }
   

}
