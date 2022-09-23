using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;

public class DiscordMenuController : MonoBehaviour
{
	private Player p;
	private EventSystem myEventSystem;
	[SerializeField] private GameObject DefaultMenuOption;
	[SerializeField] private List<Button> MainMenuOptions;
	[SerializeField] private List<Button> PlayModesOptions;
	private bool playModeSelection = false;

	void Start() {
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		myEventSystem = EventSystem.current;

		// Deactivate and remove graphics settings buttons if on xbox
#if !UNITY_XBOXONE
		if (!p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
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

	private void Update() {
		if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null
			&& (p.GetButtonDown(RewiredConsts.Action.Select) || Mathf.Abs(p.GetAxis(RewiredConsts.Action.MoveVertical)) > 0)) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
		}
		else if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
		}
	}

	public void UnselectDiscordButton() {
		if (p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(null);
		}
	}
}
