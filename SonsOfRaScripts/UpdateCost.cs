using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateCost : MonoBehaviour {

    //This is a script for updating costs, designed specifically for units. Blessings and Towers are done within the game manager when assigning the loadout selections.

    public GameObject objectPrefab;

	// Use this for initialization
	void Start () {
		UnitAI AI = objectPrefab.GetComponent<UnitAI>();
		gameObject.GetComponent<Text>().text = AI.cost.ToString() + " G";
	}
}
