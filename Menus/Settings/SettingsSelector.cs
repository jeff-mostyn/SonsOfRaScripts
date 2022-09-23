using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsSelector : MonoBehaviour
{
	#region Declarations
	public enum settingOptions { useTimer, roundTime, keepHealth, matchPlay, bestOf };

	// ---------------- public variables ---------------------
	// references
	public Player p;
	public Player p2;

	// --------------- nonpublic variables -------------------
	// references
	private SceneChangeManager sc;
	private GameManager g;
	
	private EventSystem myEventSystem;

	// Setting UI displays
	[Header("UI Elements")]
	[SerializeField] private List<GameObject> menuOptions;
	[SerializeField] private TextMeshProUGUI useTimerDisplay;
	[SerializeField] private TextMeshProUGUI roundTimeDisplay;
	[SerializeField] private TextMeshProUGUI keepHealthDisplay;
	[SerializeField] private TextMeshProUGUI matchPlayDisplay;
	[SerializeField] private TextMeshProUGUI bestOfDisplay;
	private List<Image> Arrows;
	[SerializeField] private GameObject upArrow, downArrow;
	[SerializeField] private GameObject ready, detail, options, back, restoreDefaults, backButton;
	[SerializeField] private GameObject DefaultMenuOption;

	// gameplay values
	[Header("'Gameplay' Values")]
	[SerializeField] private float moveSensitivity = 0.8f;
	[SerializeField] private float optionSwitchCooldown;
	[SerializeField] private float arrowMaxScale;

	// misc references
	[Header("References")]
	[SerializeField] private PlayerPatronSelect patronSelect1;
	[SerializeField] private PlayerPatronSelect patronSelect2;

	// selectors and such
	[SerializeField] private menuOptionSelector matchLengthSelector, keepHealthSelector, bestOfSelector;
	private menuOptionSelector activeSelector;

	private settingOptions selectedSettingOption;
	private Dictionary<settingOptions, bool> isToggledOff;
	private float vertMove = 0f;
	private float horzMove = 0f;
	private int optionCount;
	private float optionSwitchCooldownTimer;
	private bool isSettingSelected = false;

	// Setting values
	private bool useTimer;
	private bool matchPlay;

	#endregion

	#region System Functions
	// Use this for initialization
	void Start() {
		sc = SceneChangeManager.Instance;
		g = GameManager.Instance;
		p = ReInput.players.GetPlayer(PlayerIDs.player1);
		p2 = ReInput.players.GetPlayer(PlayerIDs.player2);

		optionCount = menuOptions.Count;
		Arrows = new List<Image>();

		myEventSystem = EventSystem.current;

		InitializeStats();
		UpdateDisplay();

#if !UNITY_XBOXONE
		if (p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(null);
		}
#endif

        transform.parent.gameObject.SetActive(false);
    }

	// Update is called once per frame
	void Update() {
		// handle swapping between controller and keyboard
		if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null
			&& (p.GetButtonDown(RewiredConsts.Action.Select) || Mathf.Abs(p.GetAxis(RewiredConsts.Action.MoveVertical)) > 0)) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
		}
		else if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(menuOptions[0].gameObject);
		}

		switchOnCooldown();

		vertMove = p.GetAxis(RewiredConsts.Action.MoveVertical);
		horzMove = p.GetAxis(RewiredConsts.Action.MoveHorizontal);

		if (p.GetButtonDown(RewiredConsts.Action.UIBack)) {
            BackButton();
		}
		else if (p.GetButtonDown(RewiredConsts.Action.DefaultSettings)) {
            SetBackToDefault();
        }
		else if ((horzMove > moveSensitivity || horzMove < moveSensitivity * (-1)) && !switchOnCooldown()) {
			optionSwitchCooldownTimer = optionSwitchCooldown;

			if (activeSelector) {
				AdjustSelectorWithStick();
			}
		}

		UpdateDisplay();
	}
	#endregion

	private void AdjustSelectorWithStick() {
		if (horzMove > 0) {
			activeSelector.IncrementOption();
		}
		else if (horzMove < 0) {
			activeSelector.DecrementOption();
		}
	}

	private bool switchOnCooldown() {
		if (optionSwitchCooldownTimer > 0) {
			optionSwitchCooldownTimer -= Time.deltaTime;
			return true;
		}
		else
			return false;
	}

	#region Display Functions
	private void UpdateDisplay() {
		// UseTimer
		useTimerDisplay.SetText(useTimer ? Lang.SettingsText[Lang.settingsText.on][SettingsManager.Instance.language]
			: Lang.SettingsText[Lang.settingsText.off][SettingsManager.Instance.language]);

		// Match Play
		matchPlayDisplay.SetText(matchPlay ? Lang.SettingsText[Lang.settingsText.on][SettingsManager.Instance.language]
			: Lang.SettingsText[Lang.settingsText.off][SettingsManager.Instance.language]);

		// Restore defaults should only be active if not adjusting a value
		if (back.activeSelf && !isSettingSelected) {
			restoreDefaults.SetActive(true);
		}
		else {
			restoreDefaults.SetActive(false);
		}
	}

	public void DisableNonSelectedButtons(int selectedButton) {
		for (int i = 0; i<menuOptions.Count; i++) {
			if (i != selectedButton) {
				menuOptions[i].GetComponent<Button>().interactable = false;
			}
		}

		isSettingSelected = true;
	}

	public void ReinableNonSelectedButtons(int selectedButton) {
		for (int i = 0; i < menuOptions.Count; i++) {
			if (!(i == selectedButton && !SettingsManager.Instance.GetUseTimer()) && !isToggledOff[(settingOptions)i]) {
				menuOptions[i].GetComponent<Button>().interactable = true;
			}
		}
	}

	public void EnableButtonsBasedOnToggle(int selectedButton) {
		if (selectedButton == (int)settingOptions.useTimer) {
			menuOptions[(int)settingOptions.roundTime].GetComponent<Button>().interactable = SettingsManager.Instance.GetUseTimer();
			isToggledOff[settingOptions.roundTime] = !menuOptions[(int)settingOptions.roundTime].GetComponent<Button>().interactable;
		}
		else if (selectedButton == (int)settingOptions.matchPlay) {
			menuOptions[(int)settingOptions.bestOf].GetComponent<Button>().interactable = SettingsManager.Instance.GetMatchPlay();
			isToggledOff[settingOptions.bestOf] = !menuOptions[(int)settingOptions.bestOf].GetComponent<Button>().interactable;
		}
	}

	public void ResetDisplay() {
		menuOptions[(int)settingOptions.roundTime].GetComponent<Button>().interactable = SettingsManager.Instance.GetUseTimer();
		isToggledOff[settingOptions.roundTime] = !menuOptions[(int)settingOptions.roundTime].GetComponent<Button>().interactable;

		menuOptions[(int)settingOptions.bestOf].GetComponent<Button>().interactable = SettingsManager.Instance.GetMatchPlay();
		isToggledOff[settingOptions.bestOf] = !menuOptions[(int)settingOptions.bestOf].GetComponent<Button>().interactable;
	}

	#region Swap With Patron Select
	public void ClearScreen() {
		patronSelect1.mainCanvas.SetActive(false);
		patronSelect1.detMenu.SetActive(false);
		patronSelect1.RemoveGodImages();
		patronSelect1.blackOverlay.SetActive(false);
		patronSelect1.SetIsReady(false);

		patronSelect2.mainCanvas.SetActive(false);
		patronSelect2.detMenu.SetActive(false);
		patronSelect2.RemoveGodImages();
		patronSelect2.blackOverlay.SetActive(false);
		patronSelect2.SetIsReady(false);

		options.SetActive(false);
		detail.SetActive(false);
		ready.SetActive(false);
		back.SetActive(true);
		backButton.SetActive(false);
		restoreDefaults.SetActive(true);

		if (!p.controllers.hasMouse) {
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().Mute(true);
			myEventSystem.SetSelectedGameObject(menuOptions[0]);
            myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().Mute(false);
		}
	}

	public void UnclearScreen() {
		patronSelect1.mainCanvas.SetActive(true);
		if (patronSelect1.GetIsDetailShowing()) {
			patronSelect1.detMenu.SetActive(true);
			patronSelect1.blackOverlay.SetActive(true);
		}
		else {
			for (int i=0; i<patronSelect1.detMenu.transform.childCount; i++) {
				if (patronSelect1.detMenu.transform.GetChild(i).GetComponent<Text>()) {
					patronSelect1.detMenu.transform.GetChild(i).GetComponent<Text>().color = new Color(1, 1, 1, 0);
				}
			}
		}
		patronSelect1.setImagesByPatron(0);

		patronSelect2.mainCanvas.SetActive(true);
		if (patronSelect2.GetIsDetailShowing()) {
			patronSelect2.detMenu.SetActive(true);
			patronSelect2.blackOverlay.SetActive(true);
		}
		else {
			for (int i = 0; i < patronSelect2.detMenu.transform.childCount; i++) {
				if (patronSelect2.detMenu.transform.GetChild(i).GetComponent<Text>()) {
					patronSelect2.detMenu.transform.GetChild(i).GetComponent<Text>().color = new Color(1, 1, 1, 0);
				}
			}
		}
		patronSelect2.setImagesByPatron(0);

		options.SetActive(true);
		detail.SetActive(true);
		ready.SetActive(true);
		back.SetActive(false);
		backButton.SetActive(true);
		restoreDefaults.SetActive(false);

		myEventSystem.SetSelectedGameObject(null);
	}
	#endregion

	#region Arrows
	private void EnableArrows(int selectedButtonIndex) {
		for (int i = 0; i < menuOptions[selectedButtonIndex].transform.parent.GetChild(1).childCount; i++) {    // this is so ugly I love and hate it
			menuOptions[selectedButtonIndex].transform.parent.GetChild(1).GetChild(i).gameObject.SetActive(true);
			Arrows.Add(menuOptions[selectedButtonIndex].transform.parent.GetChild(1).GetChild(i).gameObject.GetComponent<Image>());
		}
	}

	private void DisableArrows() {
		if (Arrows.Count > 0) {
			foreach (Image im in Arrows) {
				if (im != null) {
					im.gameObject.SetActive(false);
				}
			}

			Arrows.Clear();
		}
	}

	private IEnumerator arrowScaler(GameObject arrow) {
		arrow.transform.localScale = new Vector3(arrowMaxScale, arrowMaxScale, arrowMaxScale);

		float timer = 0;
		float scaleDelta = arrowMaxScale - 1;
		float newScale;

		while (timer < optionSwitchCooldown / 2) {
			newScale = arrowMaxScale - (scaleDelta * (timer / (optionSwitchCooldown / 2)));
			arrow.transform.localScale = new Vector3(newScale, newScale, newScale);

			yield return new WaitForEndOfFrame();
			timer += Time.deltaTime;
		}
		arrow.transform.localScale = new Vector3(1f, 1f, 1f);
	}
	#endregion

	#endregion

	#region Setting OnClick Functions
	public void ToggleUseTimer() {
		if (!switchOnCooldown()) {
			optionSwitchCooldownTimer = optionSwitchCooldown;
			if (!p.GetButton(RewiredConsts.Action.UIBack)) {
				useTimer = !useTimer;
				SaveSettings();

				EnableButtonsBasedOnToggle((int)settingOptions.useTimer);
				if (p.controllers.hasMouse) {
					myEventSystem.SetSelectedGameObject(null);
				}
			}
		}
	}

	public void SelectChangeRoundTime() {
		if (!p.GetButton(RewiredConsts.Action.UIBack)) {
			selectedSettingOption = settingOptions.roundTime;
			activeSelector = matchLengthSelector;

			DisableNonSelectedButtons((int)selectedSettingOption);
			EnableArrows((int)selectedSettingOption);
		}
	}

	public void SelectChangeKeepHealth() {
		if (!p.GetButton(RewiredConsts.Action.UIBack)) {
			selectedSettingOption = settingOptions.keepHealth;
			activeSelector = keepHealthSelector;

			DisableNonSelectedButtons((int)selectedSettingOption);
			EnableArrows((int)selectedSettingOption);
		}
	}

	public void ToggleMatchPlay() {
		if (!switchOnCooldown()) {
			optionSwitchCooldownTimer = optionSwitchCooldown;
			if (!p.GetButton(RewiredConsts.Action.UIBack)) {
				matchPlay = !matchPlay;
				SaveSettings();

				EnableButtonsBasedOnToggle((int)settingOptions.matchPlay);
				if (p.controllers.hasMouse) {
					myEventSystem.SetSelectedGameObject(null);
				}
			}
		}
	}

	public void SelectChangeBestOf() {
		if (!p.GetButton(RewiredConsts.Action.UIBack)) {
			selectedSettingOption = settingOptions.bestOf;
			activeSelector = bestOfSelector;

			DisableNonSelectedButtons((int)selectedSettingOption);
			EnableArrows((int)selectedSettingOption);
		}
	}
	#endregion

	private void InitializeStats() {
		useTimer = SettingsManager.Instance.GetUseTimer();
		matchLengthSelector.Initialize();
		keepHealthSelector.Initialize();
		matchPlay = SettingsManager.Instance.GetMatchPlay();
		bestOfSelector.Initialize();

		isToggledOff = new Dictionary<settingOptions, bool>() {
			{settingOptions.useTimer, false },
			{settingOptions.roundTime, !useTimer },
			{settingOptions.keepHealth, false },
			{settingOptions.matchPlay, false },
			{settingOptions.bestOf, !matchPlay }
		};

		for (int i=0; i<menuOptions.Count; i++) {
			if (isToggledOff[(settingOptions)i]) {
				menuOptions[i].GetComponent<Button>().interactable = false;
			}
		}
	}

    public void SetBackToDefault()
    {
        if (!isSettingSelected)
        {
            useTimer = SettingsManager.Instance.GetDefaultUseTimer();
            matchLengthSelector.SetValue(SettingsManager.Instance.GetDefaultRoundTimeMinutes());
            keepHealthSelector.SetValue(SettingsManager.Instance.GetDefaultKeepHealth());
            matchPlay = SettingsManager.Instance.GetDefaultMatchPlay();
            bestOfSelector.SetValue(SettingsManager.Instance.GetDefaultBestOf());

            SaveSettings();

            ResetDisplay();
        }
    }

    public void BackButton()
    {
        if (isSettingSelected)
        {
            ReinableNonSelectedButtons(menuOptions.IndexOf(myEventSystem.currentSelectedGameObject));
            activeSelector = null;
            DisableArrows();
            SaveSettings();
            isSettingSelected = false;
        }
        else
        {
            patronSelect1.DeactivateSettingsMenu();
        }
    }

	private void SaveSettings() {
		SettingsManager.Instance.SetUseTimer(useTimer);
		SettingsManager.Instance.SetRoundTimeMinutes(matchLengthSelector.GetValue<int>());
		SettingsManager.Instance.SetKeepHealth(keepHealthSelector.GetValue<int>());
		SettingsManager.Instance.SetMatchPlay(matchPlay);
		SettingsManager.Instance.SetBestOf(bestOfSelector.GetValue<int>());
	}
}
