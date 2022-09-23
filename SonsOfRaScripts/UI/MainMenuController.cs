using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;

public class MainMenuController : MonoBehaviour
{
	private Player p;
	[SerializeField] private EventSystem myEventSystem;
	[SerializeField] private GraphicRaycaster raycaster;
	private PointerEventData pointerData;
	public mainMenuAnim anim;
	[SerializeField] private GameObject DefaultMenuOption;
	public List<Button> MainMenuOptions;
	[SerializeField] private List<Button> SinglePlayerOptions, MultiplayerOptions;
    [SerializeField] private List<Button> DifficultyOptions;
	//[SerializeField] private List<Button> ConquestOptions;
	[SerializeField] private GameObject Main, SinglePlayer, Multiplayer, Difficulties/*, ConquestStart*/;
	private bool singlePlayerSelection = false;
    private bool multiplayerSelection = false;
	private bool difficultySelection = false;
	//private bool conquestSelection = false;

	[Header("UI References")]
	[SerializeField] private GameObject MainMenuCanvas;
	[SerializeField] private GameObject SettingsMenuCanvas;
	[SerializeField] private GameObject CustomizationMenuCanvas;
	//[SerializeField] private CanvasGroupFade conquestInformational;
    [SerializeField] private BlurInterp myBlurLayer;
	[SerializeField] private GameObject glow;
	[SerializeField] private GameObject discordButton;
	[SerializeField] private GameObject quitButton;
	[SerializeField] private GameObject customizationButton;
	[SerializeField] private GameObject backButton;
	[SerializeField] private TextMeshProUGUI versionNum;
	[SerializeField] private mainMenuStartup Startup;

    [Header("FX Values")]
	[SerializeField] private float vertSlideTime;
	[SerializeField] private float slidePause;
	[SerializeField] private List<int> buttonYValues;

	#region System Functions
	private void Awake() {
		SonsOfRa.Events.GeneralEvents.ControllerAssignmentChange += UpdateOnControllerReassignment;
	}

	void Start() {
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

        //make sure main menu is in singleplayer
        SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.quickplay);

		SinglePlayer.SetActive(false);
        Multiplayer.SetActive(false);
        Difficulties.SetActive(false);
		glow.SetActive(false);
        backButton.SetActive(false);

		versionNum.SetText("v" + Application.version);

		DefaultMenuOption = MainMenuOptions[0].gameObject;

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
		if (p.GetButtonDown(RewiredConsts.Action.UIBack)) {
            GoBack();
		}
		
		if (CustomizationMenuCanvas.GetComponent<CanvasGroup>().alpha != 1 && myEventSystem.currentSelectedGameObject != null && myEventSystem.currentSelectedGameObject != discordButton
			&& myEventSystem.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().color == Color.white
			&& myEventSystem.currentSelectedGameObject.GetComponent<TMPButtonHandler>()
			&& myEventSystem.currentSelectedGameObject.GetComponent<TMPButtonHandler>().colorSwap) {
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
		}

		// keep button text the right color
		if (SettingsMenuCanvas.activeSelf) {
			MainMenuOptions[2].GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
		}
		else if (CustomizationMenuCanvas.activeSelf) {
			customizationButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
		}

        // make sure that buttons are enabled for mouse
        if (MainMenuOptions[0].GetComponent<Button>().interactable == false && p.controllers.hasKeyboard) {
            EnableMainMenuButtons();
        }

		// big ugly to try making sure text is right color
		if (p.controllers.hasMouse 
			&& SettingsMenuCanvas.activeSelf == false 
			&& CustomizationMenuCanvas.GetComponent<CanvasGroup>().alpha != 1) {
			//Set up the new Pointer Event
			pointerData = new PointerEventData(myEventSystem);
			//Set the Pointer Event Position to that of the mouse position
			pointerData.position = Input.mousePosition;

			//Create a list of Raycast Results
			List<RaycastResult> results = new List<RaycastResult>();

			//Raycast using the Graphics Raycaster and mouse click position
			raycaster.Raycast(pointerData, results);
			List<GameObject> raycastObjects = results.Select(x => x.gameObject).ToList();

			foreach (Button b in MainMenuOptions) {
				if (!raycastObjects.Contains(b.gameObject)) {
					b.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
				}
				else {
					b.GetComponentInChildren<TMPButtonHandler>().buttonSelectQuiet();
				}
			}
			if (!raycastObjects.Contains(customizationButton)) {
				customizationButton.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
			}
			else {
				customizationButton.GetComponentInChildren<TMPButtonHandler>().buttonSelectQuiet();
			}

			if (!raycastObjects.Contains(backButton)) {	// make sure the back button isn't completely black
				backButton.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
			}
			else {
				backButton.GetComponentInChildren<TMPButtonHandler>().buttonSelectQuiet();
			}

			if (!raycastObjects.Find(x => x.GetComponentInChildren<TMPButtonHandler>())) {
				MoveGlow(-1);
			}
		}
		else if (!p.controllers.hasMouse) {
			if (!singlePlayerSelection && !multiplayerSelection && !difficultySelection) {
				if ((myEventSystem.currentSelectedGameObject == customizationButton && CustomizationMenuCanvas.GetComponent<CanvasGroup>().alpha != 1)
					|| CustomizationMenuCanvas.GetComponent<CanvasGroup>().alpha == 1) {
					customizationButton.GetComponentInChildren<TMPButtonHandler>().buttonSelectQuiet();
				}
				else {
					customizationButton.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
				}
			}
		}
    }

	private void OnEnable() {
		if (p == null) {
			p = ReInput.players.GetPlayer(PlayerIDs.player1);
		}
		if (!p.controllers.hasKeyboard) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
		}
	}

	private void OnDestroy() {
		SonsOfRa.Events.GeneralEvents.ControllerAssignmentChange -= UpdateOnControllerReassignment;
	}
	#endregion

	#region Submenus
	public void GoBack() {
        if (singlePlayerSelection && !CustomizationMenuCanvas.activeSelf) {
			SoundManager.Instance.sound_back_menu();
            LeaveSinglePlayerSelection();
        }
        else if (multiplayerSelection && !CustomizationMenuCanvas.activeSelf) {
			SoundManager.Instance.sound_back_menu();
			LeaveMultiplayerSelection();
        }
        else if (difficultySelection) {
			SoundManager.Instance.sound_back_menu();
			LeaveDifficultySelection();
        }
    }

	public void SelectSinglePlayer() {
		if (!Startup.IntroInProgress) {
			// unhighlight buttons
			UnhighlightButton("settings");

			singlePlayerSelection = true;

			SoundManager.Instance.sound_click();

			StartCoroutine(SlideOptionsVertically(Main, MainMenuOptions, SinglePlayer, SinglePlayerOptions));

			DefaultMenuOption = SinglePlayerOptions[0].gameObject;

			if (myBlurLayer.GetBlur() > 0) {
				myBlurLayer.InterpToOgBlur();
			}

			SettingsMenuCanvas.SetActive(false);
			backButton.SetActive(true);
			discordButton.SetActive(false);
			quitButton.SetActive(false);
		}
    }

    public void SelectMultiplayer() {
		if (!Startup.IntroInProgress) {
			// unhighlight buttons
			UnhighlightButton("settings");

			multiplayerSelection = true;

			SoundManager.Instance.sound_click();

			StartCoroutine(SlideOptionsVertically(Main, MainMenuOptions, Multiplayer, MultiplayerOptions));

			DefaultMenuOption = MultiplayerOptions[0].gameObject;

			if (myBlurLayer.GetBlur() > 0) {
				myBlurLayer.InterpToOgBlur();
			}

			SettingsMenuCanvas.SetActive(false);
			backButton.SetActive(true);
			discordButton.SetActive(false);
			quitButton.SetActive(false);
		}
	}

    public void SelectQuickDiff() {
		if (!Startup.IntroInProgress) {
			singlePlayerSelection = false;
			difficultySelection = true;

			DefaultMenuOption = DifficultyOptions[0].gameObject;

			SoundManager.Instance.sound_click();

			StartCoroutine(SlideOptionsVertically(SinglePlayer, SinglePlayerOptions, Difficulties, DifficultyOptions));
		}
	}

	private void LeaveSinglePlayerSelection() {
		UnhighlightButton("customization");

		singlePlayerSelection = false;
		difficultySelection = false;

		DefaultMenuOption = MainMenuOptions[0].gameObject;

		StartCoroutine(SlideOptionsVertically(SinglePlayer, SinglePlayerOptions, Main, MainMenuOptions));

        backButton.SetActive(false);
        discordButton.SetActive(true);
		quitButton.SetActive(true);
	}

    private void LeaveMultiplayerSelection() {
		UnhighlightButton("customization");

		multiplayerSelection = false;

		DefaultMenuOption = MainMenuOptions[0].gameObject;

		StartCoroutine(SlideOptionsVertically(Multiplayer, MultiplayerOptions, Main, MainMenuOptions));

        backButton.SetActive(false);
        discordButton.SetActive(true);
		quitButton.SetActive(true);
	}

    private void LeaveDifficultySelection() {
		singlePlayerSelection = true;
		difficultySelection = false;

		DefaultMenuOption = SinglePlayerOptions[0].gameObject;

		StartCoroutine(SlideOptionsVertically(Difficulties, DifficultyOptions, SinglePlayer, SinglePlayerOptions));
	}
	#endregion

	public void UnselectDiscordButton() {
		if (p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(null);
		}
	}

	#region UI Effects
	public void MoveGlow(int i) {
		switch (i) {
			case -1:
				glow.SetActive(false);
				break;
            case 5:
                glow.SetActive(true);
                glow.GetComponent<RectTransform>().anchoredPosition = discordButton.GetComponent<RectTransform>().anchoredPosition;
                break;
            case 6:
                glow.SetActive(true);
                glow.GetComponent<RectTransform>().anchoredPosition = backButton.GetComponent<RectTransform>().anchoredPosition;
                break;
            default:
				glow.SetActive(true);
				glow.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, buttonYValues[i], 10f);
				break;
		}
	}


	IEnumerator SlideOptionsVertically(GameObject closingMenu, List<Button> closingOptions, GameObject openingMenu, List<Button> openingOptions) {
		float elapsedTime = 0f;
		anim.FreezeAnim(); //don't do anim checks when UI is sliding

		// set up target positions
		List<float> closingStart = new List<float>();
		List<float> openingEnd = new List<float>();
		glow.GetComponent<CanvasGroup>().alpha = 0;
		backButton.GetComponent<Button>().interactable = false;
		for (int i = 0; i < closingOptions.Count; i++) {
			closingOptions[i].GetComponentInChildren<TMPButtonHandler>().Mute(true);
			closingStart.Add(closingOptions[i].gameObject.GetComponent<RectTransform>().anchoredPosition.y);
			closingOptions[i].GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
            closingOptions[i].interactable = false;
		}
		for (int i = 0; i < openingOptions.Count; i++) {
			openingOptions[i].GetComponentInChildren<TMPButtonHandler>().Mute(true);
			openingEnd.Add(-95 * i);
            openingOptions[i].GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            openingOptions[i].interactable = true;
        }
		backButton.GetComponent<Button>().interactable = true;

		// refs
		CanvasGroup closingGroup = closingMenu.GetComponent<CanvasGroup>();
		CanvasGroup openingGroup = openingMenu.GetComponent<CanvasGroup>();
		openingGroup.alpha = 0f;

		// slide up closing menu
		while (elapsedTime < vertSlideTime) {
			closingGroup.alpha = Mathf.Lerp(1, 0, Mathf.SmoothStep(0, 1, elapsedTime / vertSlideTime));
			for (int i = 0; i < closingOptions.Count; i++) {
				closingOptions[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,
					Mathf.Lerp(closingStart[i], 0, Mathf.SmoothStep(0, 1, elapsedTime / vertSlideTime)),
					0f);
			}

			yield return null;
			elapsedTime += Time.deltaTime;
		}

		// finalize positions, toggle menu activity, and reset time
		for (int i = 0; i < closingOptions.Count; i++) {
			closingOptions[i].gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		}
		closingGroup.alpha = 0f;
		closingMenu.SetActive(false);
		openingMenu.SetActive(true);
		elapsedTime = 0f;

		yield return new WaitForSeconds(slidePause);

		// slide down new menu
		while (elapsedTime < vertSlideTime) {
			openingGroup.alpha = Mathf.Lerp(0, 1, Mathf.SmoothStep(0, 1, elapsedTime / vertSlideTime));
			glow.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0, 1, Mathf.SmoothStep(0, 1, elapsedTime / vertSlideTime));
			for (int i = 0; i < openingOptions.Count; i++) {
				openingOptions[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,
					Mathf.Lerp(0, openingEnd[i], Mathf.SmoothStep(0, 1, elapsedTime / vertSlideTime)),
					0f);
			}

			yield return null;
			elapsedTime += Time.deltaTime;
		}

		for (int i = 0; i < openingOptions.Count; i++) {
			openingOptions[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f,
					openingEnd[i],
					0f);
            openingOptions[i].GetComponentInChildren<TMPButtonHandler>().Mute(false);
        }
		openingGroup.alpha = 1f;
		glow.GetComponent<CanvasGroup>().alpha = 1f;
		anim.UnfreezeAnim(); //re-allow animation checks

#if !UNITY_XBOXONE
		if (!p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(null);
			myEventSystem.SetSelectedGameObject(openingOptions[0].gameObject);
			openingOptions[0].GetComponentInChildren<TMPButtonHandler>().buttonSelect();
		}
#else
		myEventSystem.SetSelectedGameObject(null);
		myEventSystem.SetSelectedGameObject(openingOptions[0].gameObject);
		openingOptions[0].GetComponentInChildren<TMPButtonHandler>().buttonSelect();
#endif
	}
#endregion

    #region Overlay Menus
	public void GoToSettingsMenu() {
        // handle button highlights
		MainMenuOptions[2].GetComponent<Button>().image.sprite = MainMenuOptions[2].GetComponent<Button>().spriteState.highlightedSprite;

		SoundManager.Instance.sound_click();

		if (myEventSystem.currentSelectedGameObject) {
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
		}
		SettingsMenuCanvas.SetActive(true);

		anim.SettingsStill();
        if (!myBlurLayer.isBlurred()) {
            myBlurLayer.InterpToTarget();
        }
    }

	public void GoToCustomizationMenu() {
		if (CustomizationMenuCanvas.GetComponentInChildren<CustomizationMenuController>().cameraMoveCoroutine == null) {
			// handle button highlights
			customizationButton.GetComponent<Button>().image.sprite = customizationButton.GetComponent<Button>().spriteState.highlightedSprite;

			SoundManager.Instance.sound_click();

			if (myEventSystem.currentSelectedGameObject) {
				myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
			}
			CustomizationMenuCanvas.SetActive(true);

			CustomizationMenuCanvas.GetComponentInChildren<CustomizationMenuController>().StartupVisualAdjsutments();

			anim.SettingsStill();

			ConditionallyEnableButtons(false);
		}
	}

	public void GoToCreditsMenu() {
		// handle button highlights
		MainMenuOptions[3].GetComponent<Button>().image.sprite = MainMenuOptions[3].GetComponent<Button>().spriteState.highlightedSprite;

		SoundManager.Instance.sound_click();

		if (myEventSystem.currentSelectedGameObject) {
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
		}
		SettingsMenuCanvas.SetActive(false);

		anim.SettingsStill();
        if (!myBlurLayer.isBlurred()) {
            myBlurLayer.InterpToTarget();
        }

        if (!p.controllers.hasKeyboard) {
            for (int i = 0; i < MainMenuOptions.Count; i++) {
                if (i != 3) {
                    MainMenuOptions[i].GetComponentInChildren<Button>().interactable = false;
                }
            }
            discordButton.GetComponentInChildren<Button>().interactable = false;
        }
    }

	public void ConditionallyEnableButtons(bool enable) {
		foreach (Button b in MainMenuOptions) {
			b.enabled = enable;
		}
		foreach (Button b in SinglePlayerOptions) {
			b.enabled = enable;
		}
		foreach (Button b in DifficultyOptions) {
			b.enabled = enable;
		}
        foreach (Button b in MultiplayerOptions) {
			b.enabled = enable;
		}
        
		discordButton.GetComponent<Button>().enabled = enable;
		quitButton.GetComponent<Button>().enabled = enable;
		backButton.GetComponent<Button>().enabled = enable;
	}

	public void SelectOption(string button) {
		int index = 0;
		if (button == "settings") {
			index = 2;
		}
		else if (button == "customization") {
			index = 3;
		}

		MainMenuOptions[index].gameObject.GetComponentInChildren<TMPButtonHandler>().Mute(true);
		myEventSystem.SetSelectedGameObject(MainMenuOptions[index].gameObject);
		MainMenuOptions[index].gameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
		MainMenuOptions[index].gameObject.GetComponentInChildren<TMPButtonHandler>().Mute(false);
	}

	public void UnhighlightButton(string menuBeingLeft) {
		int index = 0;
		if (menuBeingLeft == "settings") {
			index = 2;
		}
		else if (menuBeingLeft == "customization") {
			index = 3;
		}

		MainMenuOptions[index].GetComponent<Button>().image.sprite = MainMenuOptions[index].GetComponent<Button>().spriteState.pressedSprite;
		MainMenuOptions[index].GetComponentInChildren<TMPButtonHandler>().buttonDeselect();

		//exit still setting anim, return to normal
		anim.SettingsEndStill();
	}

    public void EnableMainMenuButtons() {
        for (int i = 0; i < MainMenuOptions.Count; i++) {
            MainMenuOptions[i].GetComponentInChildren<Button>().interactable = true;
        }
        discordButton.GetComponentInChildren<Button>().interactable = true;
        quitButton.GetComponentInChildren<Button>().interactable = true;
    }
	#endregion

	public void UpdateOnControllerReassignment() {
		if (!SettingsMenuCanvas.activeSelf) {
			if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
				if (singlePlayerSelection) {
					myEventSystem.SetSelectedGameObject(SinglePlayerOptions[0].gameObject);
				}
				else {
					myEventSystem.SetSelectedGameObject(DefaultMenuOption);
				}
			}
			else if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
				myEventSystem.SetSelectedGameObject(null);
			}
		}
	}
}
