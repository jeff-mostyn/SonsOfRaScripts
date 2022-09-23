using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyController : MonoBehaviour
{
    public Difficulty[] difficulties;
    public Strategy[] strategies;
    [SerializeField] private int easyLess_gp5; //a mine 175 gp5
    [SerializeField] private int hardExtra_gp5;

    private SettingsManager settings;
    private AI_PlayerController myPlayerController;

    // Start is called before the first frame update
    void Start()
    {
        settings = SettingsManager.Instance;
        if (settings.GetIsSinglePlayer())
        {
            myPlayerController = gameObject.GetComponent<AI_PlayerController>();

            int myDiff = settings.GetDifficulty();

            //if hard mode, give AI extra gold gen
            if(myDiff == 0)
            {
                myPlayerController.SetExtraGold(easyLess_gp5);
            }

            //if hard mode, give AI extra gold gen
            if(myDiff == 2)
            {
                myPlayerController.SetExtraGold(hardExtra_gp5);
            }

            //pass difficulty to ai_playercontroller
            myPlayerController.SetAiDifficulty(difficulties[ myDiff ], strategies[ myDiff ]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
