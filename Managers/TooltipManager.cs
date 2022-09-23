using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour {

	public static TooltipManager Instance;

	public GameObject RedTooltipBackdrop, BlueTooltipBackdrop;
	public GameObject P1TooltipText, P2TooltipText;

	public bool P1Active, P2Active;

	void Awake() {

		if (Instance == null) {
			//DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
		}
		else {
			Instance = this;
			GameObject.Destroy(this);
		}

		if (GameObject.FindObjectsOfType<LoadoutManager>().Length > 1) {
			Destroy(this.gameObject);
		}
	}

	void Start() {
		P1Active = false;
		P2Active = false;
	}

	public void DisplayUnitTooltip(string playerID, UnitAI u) {
		if (!P1Active && playerID == PlayerIDs.player1) {
			RedTooltipBackdrop.SetActive(true);
			P1TooltipText.GetComponent<Text>().text = Lang.unitTooltips[u.type][SettingsManager.Instance.language];
			P1Active = true;
		}
		else if (!P2Active && playerID == PlayerIDs.player2) {
			BlueTooltipBackdrop.SetActive(true);
			P2TooltipText.GetComponent<Text>().text = Lang.unitTooltips[u.type][SettingsManager.Instance.language];
			P2Active = true;
		}
	}

	public void DisplayTowerTooltip(string playerID, TowerState t) {
		if (!P1Active && playerID == PlayerIDs.player1) {
			RedTooltipBackdrop.SetActive(true);
			P1TooltipText.GetComponent<Text>().text = Lang.towerTooltips[t.type][SettingsManager.Instance.language];
			P1Active = true;
		}
		else if (!P2Active && playerID == PlayerIDs.player2) {
			BlueTooltipBackdrop.SetActive(true);
			P2TooltipText.GetComponent<Text>().text = Lang.towerTooltips[t.type][SettingsManager.Instance.language];
			P2Active = true;
		}
	}

	public void DisplayBlessingTooltip(string playerID, Blessing b) {
		if (!P1Active && playerID == PlayerIDs.player1) {
			RedTooltipBackdrop.SetActive(true);
			P1TooltipText.GetComponent<Text>().text = Lang.blessingTooltips[b.bID][SettingsManager.Instance.language];
			P1Active = true;
		}
		else if (!P2Active && playerID == PlayerIDs.player2) {
			BlueTooltipBackdrop.SetActive(true);
			P2TooltipText.GetComponent<Text>().text = Lang.blessingTooltips[b.bID][SettingsManager.Instance.language];
			P2Active = true;
		}
	}

	public void DisplayExpansionTooltip(string playerID, BaseExpansion b) {
		if (!P1Active && playerID == PlayerIDs.player1) {
			RedTooltipBackdrop.SetActive(true);
			P1TooltipText.GetComponent<Text>().text = Lang.ExpansionTooltips[b.type][SettingsManager.Instance.language];
			P1Active = true;
		}
		else if (!P2Active && playerID == PlayerIDs.player2) {
			BlueTooltipBackdrop.SetActive(true);
			P2TooltipText.GetComponent<Text>().text = Lang.ExpansionTooltips[b.type][SettingsManager.Instance.language];
			P2Active = true;
		}
	}

	public void UndisplayTooltip(string playerID) {
		if (playerID == PlayerIDs.player1) {
			RedTooltipBackdrop.SetActive(false);
			P1TooltipText.GetComponent<Text>().text = "";
			P1Active = false;
		}
		else {
			BlueTooltipBackdrop.SetActive(false);
			P2TooltipText.GetComponent<Text>().text = "";
			P2Active = false;
		}
	}
}
