using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryMenu : MonoBehaviour {

	void Start () {
		
	}

	void Update () {
		//if either player presses the select button on their controller
		if (Input.GetButtonDown ("Select1") || Input.GetButtonDown ("Select2")) {
            //reset the game
            foreach (KeyValuePair<string, List<GameObject>> pair in InfluenceTileDictionary.dict)
            {
                InfluenceTileDictionary.dict[pair.Key].Clear();
            }
            SceneManager.LoadScene ("Main");
		}
		//if either player presses the B button on their controller
		if (Input.GetButtonDown ("B1") || Input.GetButtonDown ("B2")) {
			//Go back to the main menu
			SceneManager.LoadScene ("Menu");
		}
	}
}