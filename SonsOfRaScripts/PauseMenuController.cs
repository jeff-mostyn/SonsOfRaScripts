using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
	[SerializeField] private EventSystem myEventSystem;
	[SerializeField] private GameObject defaultOption;
    [SerializeField] private GameObject restartButton;
	private Player p;

	// Start is called before the first frame update
	void Start() {
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		if (p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(null);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else {
			myEventSystem.SetSelectedGameObject(defaultOption);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

        if (SettingsManager.Instance.GetIsOnline() || SettingsManager.Instance.GetIsArcade()) {
            restartButton.SetActive(false);
        }
	}

	private void Update() {
		if (p.GetButtonDown(RewiredConsts.Action.UIBack)) {
			GetComponent<MenuScripts>().ResumeGame();
			LeaveMenu();
		}

		if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(defaultOption);
		}
	}

	public void OpenMenu() {
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		if (!p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(defaultOption);
			myEventSystem.currentSelectedGameObject.GetComponent<Button>().OnSelect(null);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else {
			myEventSystem.SetSelectedGameObject(null);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		if (SettingsManager.Instance.GetIsOnline() || SettingsManager.Instance.GetIsArcade()) {
			restartButton.SetActive(false);
		}
	}

	public void LeaveMenu() {
		if (p.controllers.hasKeyboard != false) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
