using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class ButtonPrompts : MonoBehaviour
{
    private ControllerManager CM;
    private SettingsManager SM;
    public int player = 1;
    public Constants.ButtonActions action;

    // Start is called before the first frame update
    void Start()
    {
        CM = ControllerManager.Instance;
        SM = SettingsManager.Instance;

        if (ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard)
        {
            foreach (var p in CM.prompts)
            {
                //Debug.Log(p.action);
                if (p.action == action)
                {
                    this.GetComponent<Image>().sprite = p.kb;
                    if (p.action == Constants.ButtonActions.menuConfirm)
                    {
                        this.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 97.9f);
                        this.GetComponent<RectTransform>().localPosition -= new Vector3(25, 0, 0);
                    }
                }
            }
        }
        if(player == 1)
        {
            GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeInScene.Add(this);
        }
        else
        {
            GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeP2.Add(this);
        }
    }

    public void SwitchPrompts() //not nintendo switch
    {

        if (ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard)
        {
            foreach (var p in CM.prompts)
            {
                //Debug.Log(p.action);
                if (p.action == action)
                {
                    this.GetComponent<Image>().sprite = p.kb;
                    if (p.action == Constants.ButtonActions.menuConfirm)
                    {
                        this.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 97.9f);
                        this.GetComponent<RectTransform>().localPosition -= new Vector3(25, 0, 0);
                    }
                }
            }
        }
        else
        {
            Debug.Log("controller");
            foreach (var p in CM.prompts)
            {
                //Debug.Log(p.action);
                if (p.action == action)
                {
                    this.GetComponent<Image>().sprite = p.xbox;
                    if (p.action == Constants.ButtonActions.menuConfirm)
                    {
                        this.GetComponent<RectTransform>().sizeDelta = new Vector2(98, 97.9f);
                        this.GetComponent<RectTransform>().localPosition += new Vector3(25, 0, 0);
                    }
                }
            }
        }

        if (ReInput.players.GetPlayer(PlayerIDs.player2).controllers.hasKeyboard)
        {
            foreach (var p in CM.prompts)
            {
                //Debug.Log(p.action);
                if (p.action == action)
                {
                    this.GetComponent<Image>().sprite = p.kb;
                }
            }
        }
        else
        {
            Debug.Log("controller");
            foreach (var p in CM.prompts)
            {
                //Debug.Log(p.action);
                if (p.action == action)
                {
                    this.GetComponent<Image>().sprite = p.xbox;
                }
            }
        }
    }
}
