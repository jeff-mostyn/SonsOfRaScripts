using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
	#region Declarations
	// --------------- constants -----------------
	private const float STICK_SENSITIVITY = 0.6f;

	// --------------- variables ------------------
	[Header("Interactivity Values")]
	[SerializeField] private string rewiredPlayerKey;
	public float switchCooldown;
	private float switchCooldownTimer;
	private int availablePalettes = 0;

	[Header("UI Variables")]
	[SerializeField] private Vector3 selectionScaleUp;
	[SerializeField] private float baseScaleTime;

	[Header("UI References")]
	[SerializeField] private GameObject blackOverlay;
	[SerializeField] private PlayerColorPalette paletteDisplay;
	[SerializeField] private GameObject activationButtonPrompt;

	[Header("Display Nodes")]
	[SerializeField] private List<GameObject> nodes;
	[SerializeField] private int selectedIndex;

	[Header("Color Palettes")]
	[SerializeField] private PlayerColorPalette nullPalate;
	[SerializeField] private List<PlayerColorPalette> colorPalettes;

	private Player rewiredPlayer;
	private PlayerPatronSelect pSelect;
	private bool disableOverlayOnExit;
	#endregion

	private void Start() {
		rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerKey);

		int j = 0;
		for (int i=0; i<nodes.Count; i++) {
			if (i < colorPalettes.Count) {
				nodes[i].GetComponentInChildren<PlayerColorPalette>().AssignColorPalette(colorPalettes[i]);
				availablePalettes++;
			}
			else {
				nodes[i].SetActive(false);
			}
		}

		selectedIndex = 0;
		if (!rewiredPlayer.controllers.hasKeyboard) {
			nodes[selectedIndex].GetComponent<iconScaler>().scaleUp(selectionScaleUp);
		}
		SelectColorPalette(selectedIndex);

		gameObject.SetActive(false);
	}

	private void Update() {
		switchOnCooldown();

		float horzInput = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);
		float vertInput = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveVertical);

		if (horzInput >= STICK_SENSITIVITY && !switchOnCooldown()) {
			changeSelection(1);
			switchCooldownTimer = switchCooldown;
		}
		else if (horzInput <= (STICK_SENSITIVITY * -1) && !switchOnCooldown()) {
			changeSelection(-1);
			switchCooldownTimer = switchCooldown;
		}
		else if (vertInput >= STICK_SENSITIVITY && !switchOnCooldown()) {
			changeSelection(5);
			switchCooldownTimer = switchCooldown;
		}
		else if (vertInput <= (STICK_SENSITIVITY * -1) && !switchOnCooldown()) {
			changeSelection(-5);
			switchCooldownTimer = switchCooldown;
		}

		if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.Select) && !rewiredPlayer.GetButtonDown(RewiredConsts.Action.LClick)) {
			SelectColorPalette(selectedIndex);
			ActivatePatronSelect();
		}
		else if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.UIBack)) {
			ActivatePatronSelect();
		}
	}

	public void SelectColorPalette(int index) {
		LoadoutManager l = LoadoutManager.Instance;

		for (int i = 0; i < colorPalettes.Count; i++) {
			if (i == index) {
				nodes[i].GetComponent<Image>().enabled = true;
			}
			else {
				nodes[i].GetComponent<Image>().enabled = false;
			}
		}

		//if (rewiredPlayerKey == PlayerIDs.player1) {
		//	l.p1Colors = nodes[index].GetComponentInChildren<PlayerColorPalette>().GetColorPalette();
		//}
		//else {
		//	l.p2Colors = nodes[index].GetComponentInChildren<PlayerColorPalette>().GetColorPalette();
		//}

		paletteDisplay.AssignColorPalette(nodes[index].GetComponentInChildren<PlayerColorPalette>());
	}

	private void changeSelection(int direction) {
		if (selectedIndex + direction >= 0 && selectedIndex + direction < colorPalettes.Count) {
			nodes[selectedIndex].GetComponent<iconScaler>().scaleDown();
			selectedIndex += direction;
			nodes[selectedIndex].GetComponent<iconScaler>().scaleUp(selectionScaleUp);
		}
	}

	#region Helper Functions
	private bool switchOnCooldown() {
		if (switchCooldownTimer > 0) {
			switchCooldownTimer -= Time.deltaTime;
			return true;
		}
		else
			return false;
	}

	public void PrimeColorSelect() {
		if (!rewiredPlayer.controllers.hasKeyboard) {
			nodes[selectedIndex].GetComponent<iconScaler>().scaleUp(selectionScaleUp);
		}
	}

	private void ActivatePatronSelect() {
		pSelect.mainCanvas.SetActive(true);
		if (pSelect.GetIsDetailShowing()) {
			pSelect.detMenu.SetActive(true);
		}
		else {
			for (int i = 0; i < pSelect.detMenu.transform.childCount; i++) {
				if (pSelect.detMenu.transform.GetChild(i).GetComponent<Text>() && !SettingsManager.Instance.GetIsArcade()) {
					pSelect.detMenu.transform.GetChild(i).GetComponent<Text>().color = new Color(1, 1, 1, 0);
				}
			}
		}
		pSelect.setImagesByPatron(0);

		if (disableOverlayOnExit) {
			blackOverlay.SetActive(false);
		}

		this.gameObject.SetActive(false);
	}

	private void OnEnable() {
		disableOverlayOnExit = !blackOverlay.activeSelf;
		blackOverlay.SetActive(true);
	}
	#endregion

	#region Setup Functions
	public void SetPlayerPatronSelect(PlayerPatronSelect p) {
		pSelect = p;
	}

	public void DisableInteraction() {
		paletteDisplay.gameObject.GetComponent<Button>().interactable = false;
		activationButtonPrompt.SetActive(false);
	}
	#endregion
}
