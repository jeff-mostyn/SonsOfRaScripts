using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class ConquestIntroInfo : MonoBehaviour
{
    protected Player player;

    public ConquestPlayer cp;
    public GameObject godInfo;
    public Image patronImg;
    public Text godName, godStory;
    public Animator fade;

    void Start()
    {
        player = ReInput.players.GetPlayer(PlayerIDs.player1);
        godName.text = Lang.patronNames[cp.patron.patronID][SettingsManager.Instance.language];
        godStory.text = Lang.ConquestIntroStories[cp.patron.patronID][SettingsManager.Instance.language];
        patronImg.sprite = cp.patron.PatronColorIllustration;
        cp.moving = true;

    }

    void Update()
    {
        if(player.GetButtonDown(RewiredConsts.Action.Select) || player.GetButtonDown(RewiredConsts.Action.LClick))
        {            
            godInfo.SetActive(false);
            cp.firstLoad = false;
            StartCoroutine("SetMovingFalse");
            this.enabled = false;
            fade.Play("FadeOUT", -1, 0f);

            Debug.Log("asdgjdgsajhdgsajhdgsjhdgsjdhsg");
			//GameObject.Find("Conquest Player").GetComponent<ConquestPlayer_Human>().DisplayTurnGraphic(ConquestManager.Instance.turn);
			//ConquestManager.Instance.players[ConquestManager.Instance.turn].GetComponent<ConquestPlayer>().StartTurn();
		}
        if (cp.firstLoad == false)
        {
            //Debug.Log("first load is false");
            this.enabled = false;
            StartCoroutine("SetMovingFalse");
            godInfo.SetActive(false);
            //GameObject.Find("Conquest Player").GetComponent<ConquestPlayer_Human>().DisplayTurnGraphic(ConquestManager.Instance.turn);
            //ConquestManager.Instance.players[ConquestManager.Instance.turn].GetComponent<ConquestPlayer>().StartTurn();
        }
    }

    IEnumerator SetMovingFalse() // so it doesnt zoom in on info confirm
    {
        yield return new WaitForSeconds(0.5f);
        cp.moving = false;
    }
}
