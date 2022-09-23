using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateLoadoutDesc : MonoBehaviour {

    public bool ShowFCost;
    public bool ShowDur;
    public bool ShowGCost;
    public bool ShowDamage;
    public bool ShowSunDam;
    public bool ShowHealth;

    public GameObject objectPrefab;
    string toAddString;

	// Use this for initialization
	void Start () {
        if (ShowFCost) toAddString += "Cost: " + objectPrefab.GetComponent<Blessing>().cost.ToString() + " favor$";
        if (ShowDur) toAddString += "Duration: " + objectPrefab.GetComponent<Blessing>().cooldown.ToString() + " sec";    //this will break if cooldown =/= duration; right now they are
        if (ShowGCost) toAddString += "Cost: " + objectPrefab.GetComponent<TowerState>().cost.ToString() + " G$";
        if (ShowDamage) toAddString += "Damage: " + objectPrefab.GetComponentInChildren<TowerAttacker>().damage.ToString() + " | ";
        if (ShowSunDam) toAddString += "Damage: " + objectPrefab.GetComponentInChildren<SunTowerAttacker>().damage.ToString() + " | ";
        if (ShowGCost) toAddString += "Health: " + objectPrefab.GetComponent<TowerHealth>().health.ToString();

        toAddString = toAddString.Replace('$', '\n'); //line breaks don't work without this for some reason
        gameObject.GetComponent<Text>().text += toAddString;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
