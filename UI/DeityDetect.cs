using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeityDetect : MonoBehaviour
{
    //This is Jo's code. I'm sorry if it's fucked. I know it's inefficient.

    public int playerID; //either 0 (player 1) or 1 (player 2) 
    public Sprite SetSpr, AnubisSpr, IsisSpr, MontuSpr, SekhmetSpr;

    private GameManager g;
    private PlayerController playerController;
    private Image mine;

    // Start is called before the first frame update
    void Start()
    {
        g = GameManager.Instance;

        if(playerID == 0)
        {
            playerController = g.player1Controller;
        } else {
            playerController = g.player2Controller;
        }

        mine = GetComponent<Image>();

        if ((int)playerController.patron == 4)
            mine.sprite = SetSpr;
        if ((int)playerController.patron == 3)
            mine.sprite = SekhmetSpr;
        if ((int)playerController.patron == 2)
            mine.sprite = AnubisSpr;
        if ((int)playerController.patron == 1)
           mine.sprite = IsisSpr;
        if ((int)playerController.patron == 0)
            mine.sprite = MontuSpr;

    }
}
