using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerManager : MonoBehaviour
{
	public static ControllerManager Instance;
	private Player player1;
	private Player player2;

    [System.Serializable]
    public struct ButtonImages
    {
        public Constants.ButtonActions action;
        public Sprite xbox;
        public Sprite kb;
    }

    public List<ButtonImages> prompts = new List<ButtonImages>();

    private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;

			// Subscribe to events
			ReInput.ControllerConnectedEvent += OnControllerConnected;
			ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
			SceneManager.sceneLoaded += OnSceneLoaded;

			player1 = ReInput.players.GetPlayer(PlayerIDs.player1);
			player2 = ReInput.players.GetPlayer(PlayerIDs.player2);
		}
		else {
			Destroy(gameObject);
		}
	}

	private void Start() {
		AssignControllers();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (!SettingsManager.Instance.GetIsSinglePlayer() && !SettingsManager.Instance.GetIsOnline()) {
			if (ReInput.controllers.Joysticks.Count == 1) {
				player1.controllers.hasKeyboard = true;
				player1.controllers.hasMouse = true;
				player2.controllers.AddController(ReInput.controllers.Joysticks[0], true);
			}
			else if (ReInput.controllers.Joysticks.Count == 2) {
				player2.controllers.AddController(ReInput.controllers.Joysticks[1], true);
			}
		}
	}

	private void Update() {
		if (SettingsManager.Instance.GetIsSinglePlayer() && player2.controllers.joystickCount > 0) {
			player2.controllers.ClearAllControllers();
		}

		if (((SettingsManager.Instance.GetIsSinglePlayer() || SettingsManager.Instance.GetIsOnline()) && ReInput.controllers.Joysticks.Count > 0)
			|| (!SettingsManager.Instance.GetIsSinglePlayer() && ReInput.controllers.Joysticks.Count > 1)) {
			if (player1.controllers.hasMouse && 
				(ReInput.controllers.Joysticks[0].GetAnyButton() 
				|| Mathf.Abs(ReInput.controllers.Joysticks[0].GetAxis(RewiredConsts.Action.MoveVertical)) > 0.8
				|| Mathf.Abs(ReInput.controllers.Joysticks[0].GetAxis(RewiredConsts.Action.MoveHorizontal)) > 0.8)) {   
				AssignControllerToPlayer(player1, ReInput.controllers.Joysticks[0]);
			}
			else if (!player1.controllers.hasMouse && (ReInput.controllers.Mouse.GetAnyButton() || ReInput.controllers.Keyboard.GetAnyButton()) && !SettingsManager.Instance.RecordingMode) {
				AssignKeyboardToPlayer(player1);
			}
		}
	}

	#region Assignment
	public void AssignControllerToPlayer(Player player, Joystick joystick) {
		bool lockMouse = player.controllers.hasMouse;
		player.controllers.ClearAllControllers();
		player.controllers.AddController(joystick, true);

		SonsOfRa.Events.GeneralEvents.InvokeControllerAssignmentChange();

		if (lockMouse) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

        if (GameObject.Find("Button List")) {
            foreach (ButtonPrompts bp in GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeInScene) {
                bp.SwitchPrompts();
            }
			foreach (ButtonPrompts bp in GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeP2)
			{
				bp.SwitchPrompts();
			}
		}
    }

	public void AssignKeyboardToPlayer(Player player) {
		player.controllers.ClearAllControllers();
		player.controllers.hasKeyboard = true;
		player.controllers.hasMouse = true;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		SonsOfRa.Events.GeneralEvents.InvokeControllerAssignmentChange();

		if (GameObject.Find("Button List")) {
            foreach (ButtonPrompts bp in GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeInScene) {
                bp.SwitchPrompts();
            }
			foreach (ButtonPrompts bp in GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeP2)
			{
				bp.SwitchPrompts();
			}
		}
    }

	public void AssignControllers() {
		int gamepads = ReInput.controllers.Joysticks.Count;

		if (gamepads == 0) {
			AssignKeyboardToPlayer(player1);
			player2.controllers.hasKeyboard = false;
			player2.controllers.hasMouse = false;
		}
		else if (gamepads == 1) {
			if (SettingsManager.Instance.GetIsSinglePlayer()) {
				player2.controllers.ClearAllControllers();
				AssignControllerToPlayer(player1, ReInput.controllers.Joysticks[0]);
			}
			else {
				AssignKeyboardToPlayer(player1);
				AssignControllerToPlayer(player2, ReInput.controllers.Joysticks[0]);
			}
		}
		else {
			AssignControllerToPlayer(player1, ReInput.controllers.Joysticks[0]);
			if (!SettingsManager.Instance.GetIsSinglePlayer()) {
				AssignControllerToPlayer(player2, ReInput.controllers.Joysticks[1]);
			}
		}

        foreach(ButtonPrompts bp in GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeInScene)
        {
            bp.SwitchPrompts();
        }
		foreach (ButtonPrompts bp in GameObject.Find("Button List").GetComponent<ButtonList>().buttonsToChangeP2)
		{
			bp.SwitchPrompts();
		}
	}
	#endregion

	public bool KeyboardInUseMenus() {
		if (player1.controllers.hasKeyboard) {
			return true;
		}
		return false;
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args) {
		Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
		AssignControllers();
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args) {
		Debug.Log("A controller was disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
		AssignControllers();
	}
}
