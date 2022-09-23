using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerDirectionalInput : MonoBehaviour
{
	private Vector2 mouseDefaultPosition;

	public void SetMouseDefaultPosition(Vector2 pos) {
		mouseDefaultPosition = pos;
	}

    public float GetHorizRadialInput(Player rewiredPlayer, Human_PlayerController.radialStates state) {
		if (rewiredPlayer.controllers.hasMouse) {
			if ((rewiredPlayer.GetButton(RewiredConsts.Action.OpenRadial) || state == Human_PlayerController.radialStates.upgrades) && true) {
				Vector2 currentMousePosition = Input.mousePosition;
				Vector2 relativeMousePosition = currentMousePosition - mouseDefaultPosition;
				return relativeMousePosition.x;
			}
			else {
				return 0;
			}
		}
		else {
			return rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);
		}
	}

	public float GetVertRadialInput(Player rewiredPlayer, Human_PlayerController.radialStates state) {
		if (rewiredPlayer.controllers.hasMouse) {
			if ((rewiredPlayer.GetButton(RewiredConsts.Action.OpenRadial) || state == Human_PlayerController.radialStates.upgrades) && true) {
				Vector2 currentMousePosition = Input.mousePosition;
				Vector2 relativeMousePosition = currentMousePosition - mouseDefaultPosition;
				return -1 * relativeMousePosition.y;    // multiply by negative one to correct for vertical weirdness
			}
			else {
				return 0;
			}
		}
		else {
			return rewiredPlayer.GetAxis(RewiredConsts.Action.MoveVertical);
		}
	}

	public float GetHorizNonRadialInput(Player rewiredPlayer) {
		if (rewiredPlayer.controllers.hasKeyboard) {
			if (rewiredPlayer.GetButton(RewiredConsts.Action.DPadLeft)) {
				return -1;
			}
			else if (rewiredPlayer.GetButton(RewiredConsts.Action.DPadRight)) {
				return 1;
			}
			else {
				return 0;
			}
		}
		else {
			return rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);
		}
	}

	public float GetVertNonRadialInput(Player rewiredPlayer) {
		if (rewiredPlayer.controllers.hasKeyboard) {
			if (rewiredPlayer.GetButton(RewiredConsts.Action.DPadDown)) {
				return 1;
			}
			else if (rewiredPlayer.GetButton(RewiredConsts.Action.DPadUp)) {
				return -1;
			}
			else {
				return 0;
			}
		}
		else {
			return rewiredPlayer.GetAxis(RewiredConsts.Action.MoveVertical);
		}
	}
}
