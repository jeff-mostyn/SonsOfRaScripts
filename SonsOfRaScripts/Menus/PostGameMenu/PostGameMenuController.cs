using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;

public class PostGameMenuController : MonoBehaviour {

	// -------------------- public variables -----------------------
	// references
	public EventSystem myEventSystem;
	public GameObject DefaultMenuOption;

	[Header("Buttons")]
	public GameObject godSelectButton;
	public GameObject rematchButton, onlineRematchButton, mainMenuButton, returnToLobbyButton;

	// -------------------- private variables ----------------------
	// references
	private Player p;
	private SceneChangeManager sc;

	void Start () {
		sc = SceneChangeManager.Instance;
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		// levels and online menu stuff
		if (SettingsManager.Instance.GetIsOnline()) {
            godSelectButton.SetActive(false);
            rematchButton.SetActive(false);
            onlineRematchButton.SetActive(true);
			returnToLobbyButton.SetActive(true);

			ContentManager.Instance.RecordGamesPlayed();
        }

		// calculate experience earned
		int experienceGained = 0;
		experienceGained += ContentManager.Instance.GetPlayGameXP();
		if ((SettingsManager.Instance.GetIsOnline()
			&& (GameObject.Find("StatDisplay").GetComponent<statDisplay>().GetWinner() == PlayerIDs.player1 && OnlineManager.Instance.GetIsHost()
				|| GameObject.Find("StatDisplay").GetComponent<statDisplay>().GetWinner() == PlayerIDs.player2 && !OnlineManager.Instance.GetIsHost()))
			|| (!SettingsManager.Instance.GetIsOnline() && GameObject.Find("StatDisplay").GetComponent<statDisplay>().GetWinner() == PlayerIDs.player1)) {
			experienceGained += ContentManager.Instance.GetWinGameXP();
		}

		ContentManager.Instance.GainExperience(experienceGained);

#if !UNITY_XBOXONE
		if (!p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
            myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else {
			myEventSystem.SetSelectedGameObject(null);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
#endif
	}

	void Update () {
		if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
		}
	}

    public void RequestOnlineRematch() {
        LobbyManager.Instance.VoteRematch();
		onlineRematchButton.GetComponent<Button>().interactable = false;
    }

	public void ReturnToLobby() {
		LobbyManager.Instance.InformReturnToLobby();
		LobbyManager.Instance.StopRematchCountdown();

		SceneChangeManager.Instance.setNextSceneName(Constants.sceneNames[Constants.gameScenes.lobby]);
		SceneChangeManager.Instance.LoadNextScene();
	}

	public void DisableButtonsOnOpponentLeave() {
		if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == onlineRematchButton) {
			myEventSystem.SetSelectedGameObject(mainMenuButton);
		}
		onlineRematchButton.GetComponent<Button>().interactable = false;
	}

	public void DisableButtonsOnRematchCountdown() {
		onlineRematchButton.GetComponent<Button>().interactable = false;
		mainMenuButton.GetComponent<Button>().interactable = false;
		returnToLobbyButton.GetComponent<Button>().interactable = false;
	}

	public void EnableButtonsOnRematchCancel() {
		mainMenuButton.GetComponent<Button>().interactable = true;
		returnToLobbyButton.GetComponent<Button>().interactable = true;
	}
}
