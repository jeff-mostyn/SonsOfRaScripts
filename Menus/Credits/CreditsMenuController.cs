using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreditsMenuController : MonoBehaviour
{
    // ---------------- public variables ---------------------
	// references
	public Player p;
	public Player p2;

	[Header("Canvas References")]
	[SerializeField] private GameObject MainMenuCanvas;
	[SerializeField] private GameObject CreditsCanvas;
	[SerializeField] private BlurInterp blurScript;

	// --------------- nonpublic variables -------------------
	// references
	private SceneChangeManager sc;
	
	private EventSystem myEventSystem;

	#region System Functions
	// Use this for initialization
	void Start() {
		sc = SceneChangeManager.Instance;

		p = ReInput.players.GetPlayer(PlayerIDs.player1);
		p2 = ReInput.players.GetPlayer(PlayerIDs.player2);
	}

	// Update is called once per frame
	private void Update() {
		if (p.GetButtonDown(RewiredConsts.Action.UIBack) || p2.GetButtonDown(RewiredConsts.Action.UIBack) || Input.GetKey("m")) {
			ReturnToMainMenu();
		}
	}

	public void ReturnToMainMenu() {
		MainMenuCanvas.SetActive(true);
		//MainMenuCanvas.GetComponentInChildren<MainMenuController>().UnhighlightButton("credits");
		if (!p.controllers.hasMouse) {
			MainMenuCanvas.GetComponentInChildren<MainMenuController>().SelectOption("credits");
		}
		blurScript.InterpToOgBlur();

        MainMenuCanvas.GetComponentInChildren<MainMenuController>().EnableMainMenuButtons();

        CreditsCanvas.SetActive(false);
	}
	#endregion
}
