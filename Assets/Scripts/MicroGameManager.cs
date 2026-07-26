using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class MicroGameManager : MonoBehaviour
{
    [SerializeField] Animator listAnimator;
    public List<MicroGamePlayer> gameList;
    [SerializeField] List<TextMeshProUGUI> gameNames;
    [SerializeField] List<RectTransform> currentGameIcon; 
    [SerializeField] List<RectTransform> completedGame;
    public int currentGameIndex = 0;
    private const string TRIGGER_ANIM = "triggerAnim";
    [SerializeField] AudioSource source;
    [SerializeField] bool hasWon;
    [SerializeField] Image finalImage;
    [SerializeField] Sprite win, lose; 


    public InputActionReference debugAction;

    private void Start()
    {
        Initialize(); 
    }

    public void PlaySound()
    {
        source.Play(); 
    }


    public void ShowResolution(bool flag)
    {
        listAnimator.SetBool(TRIGGER_ANIM, flag); 
    }


    public void CompleteGameCoroutine(int index)
    {
        StartCoroutine(CompleteGame(index)); 
    }


    public IEnumerator CompleteGame(int index)
    {
        Debug.Log("CompleteGame start, index=" + index);
        gameList[index].gameObject.SetActive(false);
        ShowResolution(true);
        yield return null;
        Debug.Log("After ShowResolution");
        float clipLength = listAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(0.5f);
        Debug.Log("About to set completedGame active, count=" + completedGame.Count);
        currentGameIcon[index].gameObject.SetActive(false);
        completedGame[index].gameObject.SetActive(true);
        Debug.Log("completedGame set active");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Checking next game, index+1=" + (index + 1) + " count=" + currentGameIcon.Count);
        if (!(index + 1 >= currentGameIcon.Count))
        {
            currentGameIcon[index + 1].gameObject.SetActive(true);
            currentGameIndex++;
            StartGame(currentGameIndex);
        }

        else
        {
            hasWon = true;
            ShowResolution(true);
            yield return null;
            Debug.Log("After ShowResolution");
            yield return new WaitForSeconds(0.5f);
            Debug.Log("About to set completedGame active, count=" + completedGame.Count);
            completedGame[index].gameObject.SetActive(true);

            FinalScreen(); 
            
        }
    }


    public void FinalScreen()
    {
        if (hasWon)
        {
            finalImage.sprite = win;

        }

        else { finalImage.sprite = lose; }
        
        finalImage.enabled = true; 
    }

    public void StartGame(int index)
    {
        gameList[index].gameObject.SetActive(true);
        gameList[index].RunGameCoroutine(); 
    }

    public void Initialize()
    {
        Debug.Log("Initialize called on " + gameObject.name + " instance " + GetInstanceID());

        for (int i = 0; i < gameNames.Count; i++)
        {
            gameNames[i].text = gameList[i].microGame.uid; 
        }

        StartCoroutine(FirstGame()); 
        
    }
    
    public void OnAnimationComplete()
    {
        ShowResolution(false);
    }
   

    public IEnumerator FirstGame()
    {
        yield return new WaitForSeconds(1f); 
        currentGameIndex = 0;
        ShowResolution(true);
        currentGameIcon[0].gameObject.SetActive(true);
        GameManager.Instance.StartTimer(true); 
        yield return new WaitForSeconds(2.5f);
        StartGame(0);

        Debug.Log(gameList[currentGameIndex] + "has started"); 
    }


}
