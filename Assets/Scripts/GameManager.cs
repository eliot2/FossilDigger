﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LoLSDK;

public class GameManager : MonoBehaviour {

    public QuizMan _QuizMan;
    public CharCon Player;
    public BoardMan _BoardMan;

    public int StartingEnergy;
    public int StartingHealth;
    public int StartingDepth;
    public int StartingAbsoluteAge;

    private float depth;
    private float deepestDepth;
    private float absoluteAge;

    private int energy;
    private int health;
    private int maxEnergy;
    private int maxHealth;

    public int ZeroHealthPunishment;
    public int ZeroEnergyPunishment;

    public GameObject GameOverScreen;
    public Text CongratsText;
    public Text CongratsTextBG;


    // Use this for initialization
    void Start () {
        LOLSDK.Init("com.kiteliongames.fossildigger");
        CBUG.Do("PreQHandle");
        LOLSDK.Instance.QuestionsReceived += new QuestionListReceivedHandler(this.QuestionsReceived);
        CBUG.Do("PostQHandle");
        LOLSDK.Instance.SubmitProgress(0, 0, 100);
        HUDMan.SetAbsoluteAge(StartingAbsoluteAge);
        HUDMan.SetDepth(StartingDepth);
        HUDMan.SetEnergy(StartingEnergy, StartingEnergy);
        HUDMan.SetHealth(StartingHealth, StartingHealth);
        energy = StartingEnergy;
        health = StartingHealth;
        maxEnergy = StartingEnergy;
        maxHealth = StartingHealth;
        //AudioManager.PlayM(0);
    }
	
	// Update is called once per frame
	void Update () {

        //Arbitrarily Catch Debug - Breakpoint!
        if (Input.GetKeyDown("k"))
            CBUG.Do("Debug!");

	}



    public void QuestionsReceived(MultipleChoiceQuestionList questionList)
    {
        Debug.Log("Questions Received!");
        _QuizMan.GetQuestions(questionList);
    }

    /// <summary>
    /// Called every time the player moves.
    /// 
    /// Alternative programmatic designs include:
    /// Using a Dictionary of Item.Type and Functions as a lookup table.
    /// Some OO method of handing over an Item class
    /// Or the C# way of using Delegate functions
    /// 
    /// Not using any of these because a Switch in this case won't be too large
    /// and is the easiest to maintain when we want to iterate quickly.
    /// </summary>
    /// <param name="Direction"></param>
    /// <param name="TargetItem">Item the player landed on.</param>
    public void Update(BoardMan.Direction Direction, Item.Type TargetItem, bool onEdge, bool teleporting)
    {
        //Teleporting doesn't consume energy.
        if (!teleporting)
        {
            energy--;   
        }
        else
        {
            TargetItem = Item.Type.None;
        }
        //where should item handle code go? Does GameManager actually do the item action?
        if(Direction != BoardMan.Direction.None)
        {
            if(Direction == BoardMan.Direction.North)
            {
                depth -= 10;
                absoluteAge -= 3;
            }
            else if(Direction == BoardMan.Direction.South)
            {
                depth += 10;
                absoluteAge += 3;
                if (depth > deepestDepth)
                    deepestDepth = depth;
            }
        }

        if(!onEdge)
            CharCon.Move(Direction);
        else
            CharCon.Rotate(Direction);

        
        switch (TargetItem)
        {
            case Item.Type.None:
                //Player Data Calls
                //UI Calls
                break;
            case Item.Type.Fossil:
                //Player Data Calls
                //UI Calls
                //QNA Popup
                _BoardMan.GamePaused = true;
                _QuizMan.Init(true);
                break;
            case Item.Type.Energy:
                //give energy
                if(energy < maxEnergy)
                     energy++;
                //blah
                break;
            case Item.Type.Damage:
                //Player Data Calls
                health--;
                //UI Calls
                break;
            case Item.Type.EBoost:
                maxEnergy++;
                energy = maxEnergy;
                break;
            case Item.Type.HBoost:
                maxHealth++;
                health = maxHealth;
                break;
            default:
                CBUG.Error("Bad Item type given! " + TargetItem.ToString());
                break;
        }
        if (energy == 0 || health == 0)
        {
            int totalPunishment =
                (energy == 0 ? ZeroEnergyPunishment : 0) +
                (health == 0 ? ZeroHealthPunishment : 0);
            energy = maxEnergy;
            health = maxHealth;
            _BoardMan.TeleportDistance(totalPunishment);
        }
        HUDMan.SetEnergy(energy, maxEnergy);
        HUDMan.SetHealth(health, maxHealth);
        HUDMan.SetDepth(depth);
        HUDMan.SetAbsoluteAge(absoluteAge + (3) * 10f); //arbitrary age calculation
    }

    public void Update(BoardMan.Direction Direction, Item.Type Item)
    {
        //where should item handle code go? Does GameManager actually do the item action?
        Update(Direction, Item, false, false);
    }

    private IEnumerator endGame()
    {
        yield return new WaitForSeconds(5f);
        LOLSDK.Instance.CompleteGame();
    }

    public void GameOver()
    {
        CongratsText.text = "Game is over!\nCongrats! The furthest you dug was: " + deepestDepth;
        CongratsTextBG.text = "Game is over!\nCongrats! The furthest you dug was: " + deepestDepth;
        GameOverScreen.SetActive(true);
        LOLSDK.Instance.SubmitProgress((int)deepestDepth, 100, 100);
        StartCoroutine(endGame());
    }

    public float Depth {
        get {
            return depth;
        }

        //set {
        //    depth = value;
        //}
    }
}
