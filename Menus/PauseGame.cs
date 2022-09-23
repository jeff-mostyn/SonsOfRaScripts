using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Rewired;

public class PauseGame : MonoBehaviour {

	//canvas refers to the pause screen overlay
	public Transform canvas;
	//gameUI refers to the in-game UI options
	public Transform gameUI;
    public GameObject player1;
    public GameObject player2;
    public bool DisablePause = false; //used to stop players from pausing after game ended (or any other time we want to pause it)

    private Player rewiredPlayer1;
    private Player rewiredPlayer2;
    private PlayerController pc1, pc2;

	[SerializeField] private PauseMenuController pauseControl;

    void Start () {
        rewiredPlayer1 = ReInput.players.GetPlayer("Player0");
        rewiredPlayer2 = ReInput.players.GetPlayer("Player1");

        pc1 = player1.GetComponent<PlayerController>();
        pc2 = player2.GetComponent<PlayerController>();
    }

	void Update () {
		((Human_PlayerController)pc1).isPauseMenuOpen = ((Human_PlayerController)pc2).isPauseMenuOpen = pauseControl.gameObject.activeInHierarchy;
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("paused", pauseControl.gameObject.activeInHierarchy ? 1 : 0);

		//check if either player presses the start button on their controllers
		if ((rewiredPlayer1.GetButtonDown(RewiredConsts.Action.Start) || rewiredPlayer2.GetButtonDown(RewiredConsts.Action.Start)) && !DisablePause) {
			//if pause screen is inactive
			if (canvas.gameObject.activeInHierarchy == false) {
                //pause the game

                // this should make actual "pause" only happen if we're offline. otherwise just pulls the menu
                if (!SettingsManager.Instance.GetIsOnline()) {
                    Time.timeScale = 0;

                    pc1.isStunned = true;
                    pc2.isStunned = true;
                }

				//activate it
				canvas.gameObject.SetActive (true);

				pauseControl.OpenMenu();

				//deactivate the game UI
				gameUI.gameObject.SetActive (false);
			}
			else {
				//if pause screen is currently active, deactivate the pause screen
				canvas.gameObject.SetActive (false);
				//reactivate the game UI
				gameUI.gameObject.SetActive (true);

                // only worry about this if not online
                if (!SettingsManager.Instance.GetIsOnline()) {
                    pc1.isStunned = false;
                    pc2.isStunned = false;

                    Time.timeScale = 1;
                }
			}
		}
	}
}