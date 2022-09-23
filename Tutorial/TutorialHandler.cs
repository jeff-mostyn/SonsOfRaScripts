using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;

public class TutorialHandler : MonoBehaviour {

    public GameObject P1, P2;
    public string TutorialState; //controls where in the tutorial the players are
    public string rewiredPlayerKey;

    //UI objects to handle
    public GameObject[] canvas;
    public GameObject[] highlightsP1; //clockwise starting in right
    public GameObject[] highlightsP2;
    public GameObject[] ready;
    public GameObject arrowsToKeep;

    public bool P1ready = false, P2ready = false;

    private Player rewiredPlayer1;
    private Player rewiredPlayer2;

    private bool inDialogue;

    private PlayerController player1, player2;

    void Start () {
        rewiredPlayer1 = ReInput.players.GetPlayer("Player0");
        rewiredPlayer2 = ReInput.players.GetPlayer("Player1");
        player1 = P1.GetComponent<PlayerController>();
        player2 = P2.GetComponent<PlayerController>();
        Intro();
	}
	
	// Update is called once per frame
	void Update () {
		switch (TutorialState)
        {
            case "tower":
                if(!P1.activeSelf && !P2.activeSelf)
                {
                    blessingsTut();
                }
                break;
            case "unit":
                if (!P1.activeSelf && !P2.activeSelf)
                {
                    towersTut();
                }
                break;
            case "blessing":
                if (!P1.activeSelf && !P2.activeSelf)
                {
                    endTut();
                }
                break;
            case "end":
                if (!P1.activeSelf && !P2.activeSelf)
                {
                    canvas[4].SetActive(false);
                }
                break;

        }

        if (inDialogue && bothReady())
        {
            if(TutorialState == "intro") //from intro to units
            {
                //inDialogue = false;
                if (canvas[0].activeSelf)
                {
                    canvas[0].SetActive(false);
                    canvas[1].SetActive(true);
                    setNotReady();
                }
                else if (canvas[1].activeSelf)
                {
                    canvas[1].SetActive(false);
                    canvas[2].SetActive(true);
                    arrowsToKeep.SetActive(true);
                    setNotReady();
                }
                else if (canvas[2].activeSelf)
                {
                    arrowsToKeep.SetActive(false);
                    canvas[2].SetActive(false);
                    canvas[3].SetActive(true);
                    setNotReady();
                    unitsTut();
                    //highlightsP1[1].SetActive(true);
                    //highlightsP2[1].SetActive(true);
                    inDialogue = false;
                }
            }
            else if(TutorialState == "end")
            {
                Destroy(GameObject.Find("PlayMusic"));
                Destroy(GameObject.Find("LoadoutManager"));
                SceneManager.LoadScene("Menu_Main");
            }
        }
        checkReady();
        tutorialStateControl();
    }

    //the setting and game will be introduced here. Leads into building towers.
    void Intro()
    {
        TutorialState = "intro";
        inDialogue = true;

        P1.SetActive(false);
        P2.SetActive(false);

        //activate canvas with intro
        canvas[0].SetActive(true);
    }

    
    void towersTut() 
    {
        canvas[3].SetActive(false);
        canvas[4].SetActive(true);
        TutorialState = "tower";
        P1.SetActive(true);
        P2.SetActive(true);
    }

    void unitsTut()
    {
        canvas[7].SetActive(false);
        TutorialState = "unit";
        P1.SetActive(true);
        P2.SetActive(true);
    }

    void blessingsTut()
    {
        canvas[4].SetActive(false);
        canvas[5].SetActive(true);
        TutorialState = "blessing";
        P1.SetActive(true);
        P2.SetActive(true);

    }

    void endTut()
    {
        P1.SetActive(false);
        P2.SetActive(false);
        setNotReady();
        canvas[7].SetActive(true);
        canvas[5].SetActive(false);
        canvas[6].SetActive(true);
        TutorialState = "end";

        inDialogue = true;
    }

    void tutorialStateControl()
    {
        if (TutorialState == "tower")
        {
            if (LivingTowerDictionary.dict[PlayerIDs.player1].Count >= 3)
            {
                P1.SetActive(false);
            }

            if (LivingTowerDictionary.dict[PlayerIDs.player2].Count >= 3)
            {
                P2.SetActive(false);
            }
        }
        else if(TutorialState == "unit")
        {
            if (LivingUnitDictionary.dict[PlayerIDs.player1].Count >= 5)
            {
                P1.SetActive(false);
            }

            if (LivingUnitDictionary.dict[PlayerIDs.player2].Count >= 5)
            {
                P2.SetActive(false);
            }
        }
        else if(TutorialState == "blessing")
        {
            if(player1.getFavor() < 100)
            {
                P1.SetActive(false);
            }
            if(player2.getFavor() < 100)
            {
                P2.SetActive(false);
            }
        }

    }

    public void checkReady()
    {
        if (rewiredPlayer1.GetButtonDown(RewiredConsts.Action.Select))
        {
            ready[0].SetActive(true);
            P1ready = true;
        }

        if (rewiredPlayer2.GetButtonDown(RewiredConsts.Action.Select))
        {
            ready[1].SetActive(true);
            P2ready = true;
        }
    }

    public void setNotReady()
    {
        ready[0].SetActive(false);
        ready[1].SetActive(false);
        P1ready = false;
        P2ready = false;
    }

    public bool bothReady()
    {
        if (P1ready && P2ready)
            return true;

        return false;
    }

    public string getState()
    {
        return TutorialState;
    }
}
