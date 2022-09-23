using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.UI.ControlMapper;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameSettingsSelector : MonoBehaviour {
	#region Declarations
	public enum settingOptions { musicVolume, fxVolume, resolution, fullscreen, targetframerate };

	// ---------------- public variables ---------------------
	// references
	public Player p;

	// --------------- nonpublic variables -------------------
	[SerializeField] private EventSystem myEventSystem;
	private PlayerDirectionalInput pInput;

	// Setting UI displays
	[Header("UI Elements")]
	[SerializeField] private List<Button> menuOptions;
	[SerializeField] private List<JeffSlider> settingSliders;
	[SerializeField] private menuOptionSelector resolutionOptionSelector;
	[SerializeField] private menuOptionSelector qualityOptionSelector;
	[SerializeField] private menuOptionSelector fullscreenOptionSelector;
	[SerializeField] private menuOptionSelector targetFrameRateOptionSelector;
	[SerializeField] private menuOptionSelector languageOptionSelector;
	private JeffSlider selectedSlider = null;
	private menuOptionSelector selectedSelector = null;
    public bool changedScreenParameters;

    // Setting Confirmation
	[SerializeField] private GameObject settingConfirmation;
    [SerializeField] private GameObject saveSettingsButton;

	// gameplay values
	[Header("'Gameplay' Values")]
	[SerializeField] private float moveSensitivity = 0.8f;
	[SerializeField] private float optionSwitchCooldown;
	[SerializeField] private float arrowMaxScale;
	[SerializeField] private float slowSliderIntervalTime, fastSliderIntervalTime;
	[SerializeField] private int movesBeforeFastSlider;
	private int slowSliderMoveCount = 0;
	private float sliderMoveCounter = float.MaxValue;
	private bool displayConfirmPrompt = false;

	private float vertMove = 0f;
	private float horzMove = 0f;
	private int currentIndex = 0;
	private int optionCount;
	private float optionSwitchCooldownTimer;
	private bool isSettingSelected = false;

	[Header("System Settings UI References")]
	public GameObject MainMenuCanvas;
	public GameObject SettingsMenuCanvas;
	[SerializeField] private GameObject DefaultSystemSettingsMenuOption;
	[SerializeField] private CanvasGroup SystemSettingsCG;
	[SerializeField] private Button SystemSettingsButton;
	[SerializeField] private BlurInterp blurScript;

	[Header("Controls Settings UI References")]
	[SerializeField] private CanvasGroup ControlsSettingsCG;
	[SerializeField] private Button ControlsSettingsButton;
	[SerializeField] private ControlMapper controlMapper;

	[Header("Credits UI References")]
	[SerializeField] private Button CreditsButton;
	[SerializeField] private CanvasGroup CreditsCG;


	// hold official values to detect changes
	float officialMusicVolume = 0f;
    float officialFxVolume = 0f;
    Vector2 officialResolution = new Vector2(0f, 0f);
    int officialQuality = 0;
	int officialLanguage = (int)Lang.language.English;
    FullScreenMode officialScreenMode = FullScreenMode.ExclusiveFullScreen;
    SettingsManager.targetFrameRates officialFrameRate = SettingsManager.targetFrameRates.Uncapped;
	#endregion

	#region System Functions
	// Use this for initialization
	void Start() {
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		optionCount = menuOptions.Count;

		InitializeStats();

		// Deactivate and remove graphics settings buttons if on xbox
#if UNITY_XBOXONE
		for (int i=2; i<menuOptions.Count; i++) {
			menuOptions[i].gameObject.transform.parent.gameObject.SetActive(false);
		}
		menuOptions.RemoveRange(2, 3);
#else
		if (!p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(menuOptions[currentIndex].gameObject);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else {
			myEventSystem.SetSelectedGameObject(null);
			currentIndex = -1;
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
#endif
		pInput = GetComponent<PlayerDirectionalInput>();

        settingConfirmation.SetActive(false);
		SettingsMenuCanvas.SetActive(false);

		SystemSettingsButton.gameObject.GetComponent<TMPro.TextMeshProUGUI>().SetText(Lang.SettingsText[Lang.settingsText.system][SettingsManager.Instance.language]);
		ControlsSettingsButton.gameObject.GetComponent<TMPro.TextMeshProUGUI>().SetText(Lang.SettingsText[Lang.settingsText.controls][SettingsManager.Instance.language]);
		CreditsButton.gameObject.GetComponent<TMPro.TextMeshProUGUI>().SetText(Lang.MenuText[Lang.menuText.credits][SettingsManager.Instance.language]);
	}

	// Update is called once per frame
	void Update() {
		if (p.controllers.hasMouse) {
			foreach (Button b in menuOptions) {
				if (!b.gameObject.name.Contains("Apply")) {
					b.gameObject.GetComponent<TMPButtonHandler>().buttonDeselect();
				}
			}
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			if (currentIndex != -1) {
				myEventSystem.SetSelectedGameObject(menuOptions[currentIndex].gameObject);
			}
			else {
				myEventSystem.SetSelectedGameObject(menuOptions[0].gameObject);
			}
		}

		if (ControlsSettingsCG.alpha == 0) {
			ControlsSettingsButton.gameObject.SetActive(p.controllers.hasKeyboard);
		}
		else if (!p.controllers.hasKeyboard) {
			TAB_OpenSystemSettings(false);
		}

        if (optionSwitchCooldownTimer > 0) {
            optionSwitchCooldownTimer -= Time.deltaTime;
        }

        // Controller-specific functionality
        if (!p.controllers.hasMouse) {
            horzMove = pInput.GetHorizNonRadialInput(p);

            if (selectedSelector == null && selectedSlider == null) {
                // enable option buttons, disable arrow buttons
                foreach (Button b in GetComponentsInChildren<Button>()) {
                    if (b.gameObject.name == "Left" || b.gameObject.name == "Right") {
                        b.interactable = false;
                    }
                    else {
                        b.interactable = true;
                    }
                }
            }

            if (p.GetButtonDown(RewiredConsts.Action.UIBack)) {
                if (isSettingSelected) {
                    ReinableNonSelectedButtons(currentIndex);
                    EnableButtonNavigation(menuOptions[currentIndex]);

                    isSettingSelected = false;

                    selectedSlider = null;
                    selectedSelector = null;
                }
                else {
                    if (NewChanges()) {
                        PromptToSave();
                    }
                    else {
                        ReturnToMainMenu();
                    }
                }
            }
            else if ((horzMove > moveSensitivity || horzMove < moveSensitivity * (-1)) && isSettingSelected) {
                if (selectedSlider) {
                    AdjustSliderValueWithStick();
                }
                else if (canSwitch() && selectedSelector) {
                    AdjustSelectorWithStick();
                }
            }

            if (horzMove < moveSensitivity && horzMove > moveSensitivity * (-1)) {
                sliderMoveCounter = float.MaxValue;
                slowSliderMoveCount = 0;
            }

			// swap tabs
			if (p.GetButtonDown(RewiredConsts.Action.RBumperUI)) {
				if (SystemSettingsCG.alpha == 1) {
					if (p.controllers.hasKeyboard) {
						TAB_OpenControlsSettings();
					}
					else {
						TAB_OpenCredits();
					}
				}
				else if (ControlsSettingsCG.alpha == 1) {
					TAB_OpenCredits();
				}
			}
			else if (p.GetButtonDown(RewiredConsts.Action.LBumperUI)) {
				if (ControlsSettingsCG.alpha == 1) {
					TAB_OpenSystemSettings();
				}
				else if (CreditsCG.alpha == 1) {
					if (p.controllers.hasKeyboard) {
						TAB_OpenControlsSettings();
					}
					else {
						TAB_OpenSystemSettings();
					}
				}
			}
		}
        else {  // mouse specific stuff
            // enable arrow buttons
            foreach (Button b in GetComponentsInChildren<Button>()) {
                if (b.gameObject.name == "Left" || b.gameObject.name == "Right") {
                    b.interactable = true;
                }
            }

            if (p.GetButtonDown(RewiredConsts.Action.UIBack)) {
                if (NewChanges()) {
                    PromptToSave();
                }
                else {
					ReturnToMainMenu();
                }
            }
        }

        menuOptions[menuOptions.Count - 1].interactable = NewChanges();
        if (NewChanges()) {
            menuOptions[menuOptions.Count - 1].GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
        }
        else {
            menuOptions[menuOptions.Count - 1].GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Normal;
        }

        if (menuOptions[menuOptions.Count - 1].interactable == false) {
            Navigation nav = menuOptions[menuOptions.Count - 2].navigation;
            nav.selectOnDown = menuOptions[0];
            menuOptions[menuOptions.Count - 2].navigation = nav;

            nav = menuOptions[0].navigation;
            nav.selectOnUp = menuOptions[menuOptions.Count - 2];
            menuOptions[0].navigation = nav;
        }
        else {
            Navigation nav = menuOptions[menuOptions.Count - 2].navigation;
            nav.selectOnDown = menuOptions[menuOptions.Count - 1];
            menuOptions[menuOptions.Count - 2].navigation = nav;

            nav = menuOptions[0].navigation;
            nav.selectOnUp = menuOptions[menuOptions.Count - 1];
            menuOptions[0].navigation = nav;
        }
    }

	private void OnEnable() {
		GetComponentInParent<CanvasGroup>().alpha = 1;
		if (p == null) {
			p = ReInput.players.GetPlayer(PlayerIDs.player1);
		}
		if (!p.controllers.hasKeyboard && myEventSystem) {
			DefaultSystemSettingsMenuOption.GetComponentInChildren<TMPButtonHandler>().Mute(true);
			myEventSystem.SetSelectedGameObject(DefaultSystemSettingsMenuOption);
			DefaultSystemSettingsMenuOption.GetComponentInChildren<TMPButtonHandler>().Mute(false);
			currentIndex = 0;
		}

		TAB_OpenSystemSettings(false);
	}
	#endregion

	#region System Settings
	#region Value Adjustment
	private void AdjustSliderValueWithStick() {
		sliderMoveCounter += Time.deltaTime;
		if ((slowSliderMoveCount < movesBeforeFastSlider && sliderMoveCounter > slowSliderIntervalTime) ||
			slowSliderMoveCount >= movesBeforeFastSlider && sliderMoveCounter > fastSliderIntervalTime) {
			if (horzMove > 0) {
				selectedSlider.IncrementValue();
			}
			else if (horzMove < 0) {
				selectedSlider.DecrementValue();
			}

			if (slowSliderMoveCount < movesBeforeFastSlider) {
				slowSliderMoveCount++;
			}

			sliderMoveCounter = 0;

			UpdateVolumes();
		}
	}

	private void AdjustSelectorWithStick() {
		if (horzMove > 0) {
			selectedSelector.IncrementOption();
		}
		else if (horzMove < 0) {
			selectedSelector.DecrementOption();
		}
	}

	private bool canSwitch() {
        if (optionSwitchCooldownTimer <= 0) {
            optionSwitchCooldownTimer = optionSwitchCooldown;
            return true;
        }
        else {
            return false;
        }
	}
    #endregion

    public void PromptToSave() {
        settingConfirmation.SetActive(true);

        if (!p.controllers.hasKeyboard) {
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().Mute(true);
			myEventSystem.SetSelectedGameObject(saveSettingsButton);
            myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().Mute(false);
		}
	}

    public void SaveAndReturnToMainMenu() {
		//SaveSettings();
		ReturnToMainMenu();
    }

    public void DiscardAndReturnToMainMenu() {
        InitializeStats();
		ReturnToMainMenu();
    }

    public void ReturnToMainMenu() {
        GetComponentInParent<CanvasGroup>().alpha = 0;
        MainMenuCanvas.GetComponentInChildren<MainMenuController>().UnhighlightButton("settings");
        if (!p.controllers.hasMouse) {
            MainMenuCanvas.GetComponentInChildren<MainMenuController>().SelectOption("settings");
        }
        blurScript.InterpToOgBlur();

        settingConfirmation.SetActive(false);
        SettingsMenuCanvas.SetActive(false);

        changedScreenParameters = false;

        SaveSettings();

		controlMapper.Close(true);
	}

    #region Display Functions
	public void DisableNonSelectedButtons(int selectedButton) {
		for (int i = 0; i < menuOptions.Count; i++) {
			if (i != selectedButton) {
				menuOptions[i].GetComponent<Button>().interactable = false;
			}
		}

		isSettingSelected = true;
	}

	public void ReinableNonSelectedButtons(int selectedButton) {
		for (int i = 0; i < menuOptions.Count; i++) {
			if (i != selectedButton) {
				menuOptions[i].GetComponent<Button>().interactable = true;
			}
		}
	}

#region Arrows
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
	public void SelectMusicVolumeSlider(int index) {
        if (!p.controllers.hasMouse) {
            DisableButtonNavigation(menuOptions[index]);

            selectedSlider = settingSliders[(int)settingOptions.musicVolume];
            currentIndex = 0;
            DisableNonSelectedButtons(index);

            displayConfirmPrompt = false;
        }
	}

	public void SelectFXVolumeSlider(int index) {
        if (!p.controllers.hasMouse) {
            DisableButtonNavigation(menuOptions[index]);

            selectedSlider = settingSliders[(int)settingOptions.fxVolume];
            currentIndex = 1;
            DisableNonSelectedButtons(index);

            displayConfirmPrompt = false;
        }
	}

	public void SelectResolutionSelector(int index) {
        if (!p.controllers.hasMouse) {
            selectedSelector = resolutionOptionSelector;
            SelectorOnClick(index);
        }
	}

	public void SelectQualitySelector(int index) {
        if (!p.controllers.hasMouse) {
            selectedSelector = qualityOptionSelector;
            SelectorOnClick(index);
        }
	}

	public void SelectFullscreenSelector(int index) {
        if (!p.controllers.hasMouse) {
            selectedSelector = fullscreenOptionSelector;
            SelectorOnClick(index);
        }
	}

	public void SelectFrameRateSelector(int index) {
        if (!p.controllers.hasMouse) {
            selectedSelector = targetFrameRateOptionSelector;
            SelectorOnClick(index);
        }
	}

	public void SelectLanguageSelector(int index) {
		if (!p.controllers.hasMouse) {
			selectedSelector = languageOptionSelector;
			SelectorOnClick(index);
		}
	}

	private void SelectorOnClick(int index) {
        DisableButtonNavigation(menuOptions[index]);
        currentIndex = index;
		DisableNonSelectedButtons(index);
		displayConfirmPrompt = selectedSelector.getPromptConfirmation();
	}
	#endregion

	#region Settings Manager Interaction
	private void InitializeStats() {
        changedScreenParameters = false;

		if (!SettingsManager.Instance.GetFileLoaded()) {
			SettingsManager.Instance.LoadPlayerPreferences();
		}

		settingSliders[(int)settingOptions.musicVolume].Initialize((int)SettingsManager.Instance.GetMusicVolume(),
			(int)SettingsManager.Instance.GetMinVolume(),
			(int)SettingsManager.Instance.GetMaxVolume());

		settingSliders[(int)settingOptions.fxVolume].Initialize((int)SettingsManager.Instance.GetFXVolume(),
			(int)SettingsManager.Instance.GetMinVolume(),
			(int)SettingsManager.Instance.GetMaxVolume());

		languageOptionSelector.Initialize();

#if !UNITY_XBOXONE
		resolutionOptionSelector.Initialize();
		qualityOptionSelector.Initialize();
		fullscreenOptionSelector.Initialize();
		targetFrameRateOptionSelector.Initialize();
#endif

        SetOfficialValues();
    }

	public void UpdateVolumes() {
		SettingsManager.Instance.SetMusicVolume(settingSliders[(int)settingOptions.musicVolume].GetValue());
		SettingsManager.Instance.SetFXVolume(settingSliders[(int)settingOptions.fxVolume].GetValue());

		FMOD.Studio.Bus musicBus;
		musicBus = FMODUnity.RuntimeManager.GetBus("Bus:/Music");
		musicBus.setVolume(SettingsManager.Instance.GetMusicVolumeFloat());

		FMOD.Studio.Bus sfxBus;
		sfxBus = FMODUnity.RuntimeManager.GetBus("Bus:/SFX");
		sfxBus.setVolume(SettingsManager.Instance.GetFXVolumeFloat());
	}

	private void SaveSettings() {
		SettingsManager.Instance.SetMusicVolume(settingSliders[(int)settingOptions.musicVolume].GetValue());
		SettingsManager.Instance.SetFXVolume(settingSliders[(int)settingOptions.fxVolume].GetValue());
		SettingsManager.Instance.language = languageOptionSelector.GetValue<Lang.language>();

#if !UNITY_XBOXONE
        // Save resolution
		int resInd = resolutionOptionSelector.GetValue<int>();
		int[] res = new int[2];
		res[0] = Screen.resolutions[resInd].width;
		res[1] = Screen.resolutions[resInd].height;

        // check if we're changing screen parameters
        if ((res[0] != SettingsManager.Instance.GetResolution(0) || res[1] != SettingsManager.Instance.GetResolution(1))
            || fullscreenOptionSelector.GetComponent<ScreenModeSelector>().GetMode() != SettingsManager.Instance.GetFullscreen()) {
			Debug.Log("changed parameters");
            changedScreenParameters = true;
        }

        // save quality
        SettingsManager.Instance.SetQualityLevel(qualityOptionSelector.GetValue<int>());
        QualitySettings.SetQualityLevel(qualityOptionSelector.GetValue<int>());
		SettingsManager.Instance.SetFullScreen(fullscreenOptionSelector.GetComponent<ScreenModeSelector>().GetMode());
		SettingsManager.Instance.SetResolution(res[0], res[1]);

		if (changedScreenParameters) {
			if (fullscreenOptionSelector.GetComponent<ScreenModeSelector>().GetMode() == FullScreenMode.Windowed) {
				Screen.SetResolution(res[0], res[1], false);
			}
			else {
				Screen.SetResolution(res[0], res[1], true);
			}
			Screen.fullScreenMode = fullscreenOptionSelector.GetComponent<ScreenModeSelector>().GetMode();
		}
		SettingsManager.Instance.SetFrameRate(targetFrameRateOptionSelector.GetValue<int>());
		int currentTargetFrameRate = (int)SettingsManager.Instance.GetFrameRate();
		Application.targetFrameRate = currentTargetFrameRate == 0 ? Screen.currentResolution.refreshRate : currentTargetFrameRate; 
#endif

		SettingsManager.Instance.SavePlayerPreferences();

		FMOD.Studio.Bus musicBus;
		musicBus = FMODUnity.RuntimeManager.GetBus("Bus:/Music");
		musicBus.setVolume(SettingsManager.Instance.GetMusicVolumeFloat());

		FMOD.Studio.Bus sfxBus;
		sfxBus = FMODUnity.RuntimeManager.GetBus("Bus:/SFX");
		sfxBus.setVolume(SettingsManager.Instance.GetFXVolumeFloat());

		// reset if screen parameters have changed
		changedScreenParameters = false;

        SetOfficialValues();
    }
    #endregion

    #region Button Navigation Adjustment
    private void DisableButtonNavigation(Button b) {
        Navigation nav = b.navigation;
        nav.mode = Navigation.Mode.None;
        b.navigation = nav;
    }

    private void EnableButtonNavigation(Button b) {
        int i = menuOptions.IndexOf(b);
        Navigation nav = b.navigation;
        nav.mode = Navigation.Mode.Explicit;

        if (i == 0) {
            nav.selectOnUp = menuOptions[menuOptions.Count - 1];
            nav.selectOnDown = menuOptions[i + 1];
        }
        else if (i == menuOptions.Count - 1) {
            nav.selectOnUp = menuOptions[i - 1];
            nav.selectOnDown = menuOptions[0];
        }
        else {
            nav.selectOnUp = menuOptions[i - 1];
            nav.selectOnDown = menuOptions[i + 1];
        }

        b.navigation = nav;
    }
    #endregion

    private void SetOfficialValues() {
        officialMusicVolume = SettingsManager.Instance.GetMusicVolume();
        officialFxVolume = SettingsManager.Instance.GetFXVolume();
        officialResolution = new Vector2(SettingsManager.Instance.GetResolution(0), SettingsManager.Instance.GetResolution(1));
        officialQuality = SettingsManager.Instance.GetQualityLevel();
        officialScreenMode = SettingsManager.Instance.GetFullscreen();
        officialFrameRate = SettingsManager.Instance.GetFrameRate();
		officialLanguage = (int)SettingsManager.Instance.language;
    }

    private bool NewChanges() {
        int resInd = resolutionOptionSelector.GetValue<int>();
        int[] res = new int[2];
        res[0] = Screen.resolutions[resInd].width;
        res[1] = Screen.resolutions[resInd].height;

        List<bool> settingChangeTests = new List<bool>();
        settingChangeTests.Add(settingSliders[(int)settingOptions.musicVolume].GetValue() != (int)officialMusicVolume);
        settingChangeTests.Add(settingSliders[(int)settingOptions.fxVolume].GetValue() != (int)officialFxVolume);
        settingChangeTests.Add(new Vector2(res[0], res[1]) != officialResolution);
        settingChangeTests.Add(qualityOptionSelector.GetValue<int>() != officialQuality);
        settingChangeTests.Add(fullscreenOptionSelector.GetComponent<ScreenModeSelector>().GetMode() != officialScreenMode);
        settingChangeTests.Add(targetFrameRateOptionSelector.GetValue<int>() != (int)officialFrameRate);
		settingChangeTests.Add(languageOptionSelector.GetValue<int>() != officialLanguage);

        for (int i = 0; i < settingChangeTests.Count; i++) {
            if (settingChangeTests[i]) {
                menuOptions[i].GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold;
            }
            else {
                menuOptions[i].GetComponent<TMPro.TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Normal;
            }
        }

        if (settingChangeTests.Contains(true)) {
            return true;
        }
        else {
            return false;
        }
    }

    public void ClickOut() {
        if (NewChanges()) {
			if (SystemSettingsCG.alpha != 1) {
				TAB_OpenSystemSettings(false);
			}
            PromptToSave();
        }
        else {
			ReturnToMainMenu();
        }
    }

    public void ApplyChanges() {
        if (!p.controllers.hasKeyboard) {
            myEventSystem.SetSelectedGameObject(menuOptions[0].gameObject);
        }

        SaveSettings();
    }
	#endregion

	#region Control Settings

	#endregion

	#region Tab Switching
	public void TAB_OpenSystemSettings(bool playSound = true) {
		if (!p.controllers.hasKeyboard && myEventSystem) {
			myEventSystem.SetSelectedGameObject(DefaultSystemSettingsMenuOption);
		}

		ToggleSystemSettings(true);
		ToggleControlsSetings(false);
		ToggleCredits(false);

		if (playSound) {
			SoundManager.Instance.sound_click();
		}
	}

	public void TAB_OpenControlsSettings(bool playSound = true) {
		ToggleSystemSettings(false);
		ToggleControlsSetings(true);
		ToggleCredits(false);

		if (playSound) {
			SoundManager.Instance.sound_click();
		}
	}

	public void TAB_OpenCredits(bool playSound = true) {
		ToggleSystemSettings(false);
		ToggleControlsSetings(false);
		ToggleCredits(true);

		if (playSound) {
			SoundManager.Instance.sound_click();
		}
	}
	#endregion

	#region Enable and Disable Tabs
	private void ToggleSystemSettings(bool on) {
		SystemSettingsCG.alpha = on ? 1f : 0f;
		SystemSettingsCG.blocksRaycasts = on;
		SystemSettingsCG.interactable = on;

		SystemSettingsButton.interactable = !on;
	}

	private void ToggleControlsSetings(bool on) {
		if (on) {
			controlMapper.Open();
		}
		else {
			controlMapper.Close(true);
		}

		ControlsSettingsCG.alpha = on ? 1f : 0f;
		ControlsSettingsCG.blocksRaycasts = on;
		ControlsSettingsCG.interactable = on;

		ControlsSettingsButton.interactable = !on;
	}

	private void ToggleCredits(bool on) {
		CreditsCG.alpha = on ? 1f : 0f;
		CreditsCG.blocksRaycasts = on;
		CreditsCG.interactable = on;

		CreditsButton.interactable = !on;
	}
	#endregion
}
