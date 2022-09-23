using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.EventSystems;

public class PlayerPatronSelect : MonoBehaviour {

	// --------------- constants -----------------
	private const float STICK_SENSITIVITY = 0.6f;

	// -------------- enumeration ----------------
	//public enum patronIndices { Montu, Isis };
	public enum playerNum { P1, P2 }

	// --------------- references ----------------
	[Header("References")]
	public List<Patron> patrons;
	public Image patronImage;
    public GameObject myPatronWall; //wall object to display god image
	public List<Image> blessingIcons;
	public List<GameObject> blessingIconDefaultLocations, blessingIconDetailLocations;
	public TextMeshProUGUI passiveTool, basic1Tool, basic2Tool, specialTool, ultimateTool, patronName,
        basic1Name, basic2Name, specialName, ultimateName;
    public GameObject mainCanvas, detMenu; //detail menu
    public GameObject blackOverlay;
	[SerializeField] private GameObject DemoText;
	[SerializeField] private GameObject SettingsMenu;
	[SerializeField] private GameObject LArrow, RArrow;

	// --------- other public variables ----------
	[Header("UI Variables")]
    public Constants.patrons defaultPatron;
	public playerNum pNum;

	[Header("UI Control Variables")]
	public float patronSwitchTime = 1f;
	public float patronSwitchCooldown;
	public float iconLerpTime = 0.5f;
	public float timeBetweenIconMovements = 0.1f;
	public float arrowMaxScale;

	// ------------ private variables ------------
	private List<iconSlider> blessingIconObjs;
	private int currentIndex;
	private string rewiredPlayerKey;
	private Player rewiredPlayer, player1;
	private float horzMove;
	private float patronSwitchCooldownTimer;
    private LoadoutManager lm;
    private bool detIsShowing;
    private bool isReady;
    private Material wallMaterial;
    private float myDissolveTimer;
    private Coroutine myDissCour = null;
    private SceneChangeManager sc;
	private bool canToggleDetails = true;
	private bool settingsMenuActive = false;
	private PlayerDirectionalInput pInput;

    // -------- Perlin Noise Gen Variables --------
    private int nPixWidth = 256;
    private int nPixHeight = 256;

    private float nScaleMin = 5f;
    private float nScaleMax = 10f;

    private float nOffsetMin = 0f;
    private float nOffsetMax = 100f;

	[Header("Plain Wall Textres")]
	public Texture OverlayTex2Plain;
	public Texture NormalTex2Plain;
	public Texture NormalAlpha2Plain;

	protected AudioSource src;


	// Use this for initialization
	void Start() {
		src = GetComponent<AudioSource>();
		sc = SceneChangeManager.Instance;
		lm = LoadoutManager.Instance;

		//get image wall material, set it to new instance of
		Material tempMat = myPatronWall.GetComponent<Renderer>().material;
		wallMaterial = Instantiate(tempMat);
		myPatronWall.GetComponent<Renderer>().material = wallMaterial;

		if (pNum == playerNum.P1) {
			rewiredPlayerKey = PlayerIDs.player1;
		}
		else {
			rewiredPlayerKey = PlayerIDs.player2;
		}

		rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerKey);
		player1 = ReInput.players.GetPlayer(PlayerIDs.player1);

		currentIndex = patrons.FindIndex(x => x.patronID == defaultPatron);
		setImagesByPatron(0);

		patronSwitchCooldownTimer = 0;

		// get icon image gameobjects so they can be moved
		blessingIconObjs = new List<iconSlider>();
		for (int i = 0; i < blessingIcons.Count; i++) {
			blessingIconObjs.Add(blessingIcons[i].transform.parent.GetComponent<iconSlider>());
			blessingIconObjs[i].SetUp(iconLerpTime);
		}

		blackOverlay.SetActive(false);
		pInput = GetComponent<PlayerDirectionalInput>();

#if !UNITY_XBOXONE
		if (rewiredPlayer.controllers.hasKeyboard || (player1.controllers.hasKeyboard && SettingsManager.Instance.GetIsSinglePlayer())) {
			LArrow.GetComponent<Button>().enabled = true;
			RArrow.GetComponent<Button>().enabled = true;

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else {
			LArrow.GetComponent<Button>().enabled = false;
			RArrow.GetComponent<Button>().enabled = false;
		}
#endif
	}
	
	// Update is called once per frame
	void Update () {
		//make sure timer is updated (WHY WASN'T THIS HERE BEFORE!?!)
		if (patronSwitchCooldownTimer > 0) {
			patronSwitchCooldownTimer -= Time.deltaTime;
		}

		if (!player1.controllers.hasMouse && LArrow.GetComponent<Button>().enabled == true) {
			LArrow.GetComponent<Button>().enabled = false;
			RArrow.GetComponent<Button>().enabled = false;
		}
		else if (player1.controllers.hasMouse && LArrow.GetComponent<Button>().enabled == false) {
			LArrow.GetComponent<Button>().enabled = true;
			RArrow.GetComponent<Button>().enabled = true;
		}

        if ((SettingsManager.Instance.GetIsArcade()) ||
			(!SettingsManager.Instance.GetIsArcade() && !SettingsMenu.activeSelf)) {
			// only be able to move stuff around if you are not readied up
			if (!lm.getReady(rewiredPlayerKey)) {
				if (!getButtonDownInput(RewiredConsts.Action.LClick) && getButtonDownInput(RewiredConsts.Action.Select) && CanSelect(currentIndex)) {    // ready up
					lm.setReady(rewiredPlayerKey, patrons[currentIndex]);
					SoundManager.Instance.sound_GodSelect();
					patronSelected(true);
					detIsShowing = false;
					StartCoroutine(DisplayDetails(detIsShowing));
				}
				else if ((getButtonDownInput(RewiredConsts.Action.GodDetails) || (getButtonDownInput(RewiredConsts.Action.UIBack) && detIsShowing))
					&& canToggleDetails && !SettingsManager.Instance.GetIsArcade()) {  // Display details
					StartCoroutine(DisplayDetails(!detIsShowing));

					detIsShowing = !detIsShowing;
				}
				else if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.UIBack)) { //going back to main menu
					SoundManager.Instance.sound_back_menu();
					ReturnToMainMenu();
				}

				horzMove = getHorzAxisInput();

				if (Mathf.Abs(horzMove) < STICK_SENSITIVITY) {
					patronSwitchCooldownTimer = 0;
				}
				else {
					if ((horzMove >= STICK_SENSITIVITY) || (getButtonDownInput(RewiredConsts.Action.DPadRight) && !SettingsManager.Instance.GetIsQuickplay())) {
						setImagesByPatron(1);
					}
					if ((horzMove <= STICK_SENSITIVITY * (-1)) || (getButtonDownInput(RewiredConsts.Action.DPadLeft) && !SettingsManager.Instance.GetIsQuickplay())) {
						setImagesByPatron(-1);
					}
				}
			}
			else {  // you can unready if you are readied up
				if (!SettingsManager.Instance.GetIsQuickplay()) {
					if (getButtonDownInput(RewiredConsts.Action.Start) || getButtonDownInput(RewiredConsts.Action.Select)
						|| getButtonDownInput(RewiredConsts.Action.UIBack)) {
						lm.unready(rewiredPlayerKey);
						patronSelected(false);
					}
				}
				else {	// can only unready with B in quickplay
					if (getButtonDownInput(RewiredConsts.Action.UIBack) && rewiredPlayerKey != PlayerIDs.player2) {
						if (player1.controllers.hasMouse) {
							lm.unready(PlayerIDs.player2);
							GameObject.Find("Player2PatronSelect").GetComponent<PlayerPatronSelect>().patronSelected(false);
							lm.unready(rewiredPlayerKey);
							patronSelected(false);
						}
						if (LoadoutManager.Instance.p2Ready) {
							lm.unready(PlayerIDs.player2);
							GameObject.Find("Player2PatronSelect").GetComponent<PlayerPatronSelect>().patronSelected(false);
						}
						else {
							lm.unready(rewiredPlayerKey);
							patronSelected(false);
						}
					}
				}
			}

			if (!SettingsManager.Instance.GetIsArcade() && getButtonDownInput(RewiredConsts.Action.CustomGameOptions) && rewiredPlayerKey == PlayerIDs.player1) {
                ActivateSettingsMenu();
			}
		}
	}

	// Take index of patron in list, set images in menu accordingly
	public void setImagesByPatron(int index) {
		if (!switchOnCooldown() && !lm.getReady(rewiredPlayerKey)) {
			do {
				if (currentIndex == 0 && index == -1) {
					currentIndex = patrons.Count - 1;
				}
				else if (currentIndex == patrons.Count - 1 && index == 1) {
					currentIndex = 0;
				}
				else {
					currentIndex += index;
				}
			} while (ContentManager.Instance.isLocked(patrons[currentIndex].name));

			patronName.SetText(Lang.patronNames[patrons[currentIndex].patronID][SettingsManager.Instance.language]);

			if (myDissCour != null) {
				StopCoroutine(myDissCour);  //make sure coroutine isn't already running
			}
			//setting all material variables for the wall transition
			myDissolveTimer = 0f;
			wallMaterial.SetFloat("_SwitVal", 0f);
			wallMaterial.SetFloat("_GlowThick", 0f);
			//previous target as base textures
			wallMaterial.SetTexture("_OverTex1", wallMaterial.GetTexture("_OverTex2"));
			wallMaterial.SetTexture("_NormMap1", wallMaterial.GetTexture("_NormMap2"));
			wallMaterial.SetTexture("_NormAlpha1", wallMaterial.GetTexture("_NormAlpha2"));
			//target textures
			wallMaterial.SetTexture("_OverTex2", patrons[currentIndex].PatronBwIllustration.texture);
			wallMaterial.SetTexture("_NormMap2", patrons[currentIndex].PatronNormalMap);
			wallMaterial.SetTexture("_NormAlpha2", patrons[currentIndex].PatronNormalAlpha.texture);
			wallMaterial.SetTexture("_NoiseTex", GeneratePerlinNoise(3f));
			//Coroutine to go through dissolve effect, can change durration
			myDissCour = StartCoroutine(PatronDissolve(patronSwitchTime));
			//patronImage.sprite = patrons[index].PatronIllustration;

			for (int i = 0; i < patrons[currentIndex].loadout.Count; i++) {
				blessingIcons[i+1].sprite = patrons[currentIndex].loadout[i].icon;
			}

			passiveTool.SetText(Lang.patronPassiveTooltips[patrons[currentIndex].patronID][SettingsManager.Instance.language]);
			basic1Tool.SetText(Lang.blessingTooltips[patrons[currentIndex].loadout[0].bID][SettingsManager.Instance.language]);
			basic2Tool.SetText(Lang.blessingTooltips[patrons[currentIndex].loadout[1].bID][SettingsManager.Instance.language]);
			specialTool.SetText(Lang.blessingTooltips[patrons[currentIndex].loadout[2].bID][SettingsManager.Instance.language]);
			ultimateTool.SetText(Lang.blessingTooltips[patrons[currentIndex].loadout[3].bID][SettingsManager.Instance.language]);

			basic1Name.SetText(Lang.blessingTitles[patrons[currentIndex].loadout[0].bID][SettingsManager.Instance.language]);
			basic2Name.SetText(Lang.blessingTitles[patrons[currentIndex].loadout[1].bID][SettingsManager.Instance.language]);
			specialName.SetText(Lang.blessingTitles[patrons[currentIndex].loadout[2].bID][SettingsManager.Instance.language]);
			ultimateName.SetText(Lang.blessingTitles[patrons[currentIndex].loadout[3].bID][SettingsManager.Instance.language]);

			if (CanSelect(currentIndex)) {
				DemoText.SetActive(false);
			}
			else {
				DemoText.SetActive(true);
			}

			patronSwitchCooldownTimer = patronSwitchCooldown;

			if (index == 1) {
				StartCoroutine(arrowScaler(RArrow));
				SoundManager.Instance.sound_godNext();
			}
			else if (index != 0) {
				StartCoroutine(arrowScaler(LArrow));
				SoundManager.Instance.sound_godPrev();
			}
		}

#if !UNITY_XBOXONE
		// make sure button is deslected so attempt to ready doesn't trigger patron switch again
		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
#endif
	}

	private bool CanSelect(int index) {
		if (SettingsManager.Instance.DemoMode && !Constants.PatronAvailableInDemo[patrons[index].patronID]) {
			return false;
		}
		else {
			return true;
		}
	}

    public void patronSelected(bool selectPatron) {
        if (myDissCour != null)
        {
            StopCoroutine(myDissCour);  //make sure coroutine isn't already running
        }
        //setting all material variables for the wall transition
        myDissolveTimer = 0f;
        wallMaterial.SetFloat("_SwitVal", 0f);
        wallMaterial.SetFloat("_GlowThick", 0.03f);
        //switch to color or not
        if (selectPatron) {
            wallMaterial.SetTexture("_OverTex1", wallMaterial.GetTexture("_OverTex2"));
            wallMaterial.SetTexture("_NormMap1", wallMaterial.GetTexture("_NormMap2"));
            wallMaterial.SetTexture("_NormAlpha1", wallMaterial.GetTexture("_NormAlpha2"));

            wallMaterial.SetTexture("_OverTex2", patrons[currentIndex].PatronColorIllustration.texture);
            wallMaterial.SetTexture("_NoiseTex", GeneratePerlinNoise(1f));
        } else {
			wallMaterial.SetTexture("_OverTex1", wallMaterial.GetTexture("_OverTex2"));
            wallMaterial.SetTexture("_NormMap1", wallMaterial.GetTexture("_NormMap2"));
            wallMaterial.SetTexture("_NormAlpha1", wallMaterial.GetTexture("_NormAlpha2"));

            wallMaterial.SetTexture("_OverTex2", patrons[currentIndex].PatronBwIllustration.texture);
            wallMaterial.SetTexture("_NoiseTex", GeneratePerlinNoise(1f));
        }
        //Coroutine to go through dissolve effect, can change durration
        myDissCour = StartCoroutine(PatronDissolve(patronSwitchTime));
    }

    // only let the player switch patrons in menu if not on "cooldown"
    private bool switchOnCooldown() {
		if (patronSwitchCooldownTimer > 0) {
			return true;
		}
		else {
			patronSwitchCooldownTimer = patronSwitchCooldown;
			return false;
		}
	}

	private void ActivateSettingsMenu() {
		SettingsMenu.SetActive(true);

		SettingsMenu.GetComponentInChildren<SettingsSelector>().ClearScreen();
	}

	public void DeactivateSettingsMenu() {
		SettingsMenu.GetComponentInChildren<SettingsSelector>().UnclearScreen();

		SettingsMenu.SetActive(false);
	}

	public bool GetIsDetailShowing() {
		return detIsShowing;
	}

	public void ReturnToMainMenu() {
		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.mainMenu]);
		sc.LoadNextScene();
	}

	#region Visual Specific Functions
	public void RemoveGodImages() {
		wallMaterial.SetTexture("_OverTex2", OverlayTex2Plain);
		wallMaterial.SetTexture("_NormMap2", NormalTex2Plain);
		wallMaterial.SetTexture("_NormAlpha2", NormalAlpha2Plain);
	}

	private IEnumerator DisplayDetails(bool detail) {
		canToggleDetails = false;
		detMenu.SetActive(true);
		if (detail) {
			blackOverlay.SetActive(detail);

			for (int i = blessingIconObjs.Count - 1; i >= 0; i--) {
				blessingIconObjs[i].startSlide(blessingIconDetailLocations[i].transform.position, true);
				yield return new WaitForSeconds(timeBetweenIconMovements);
			}

			yield return new WaitForSeconds(iconLerpTime / 2f);
		}
		else if (!SettingsManager.Instance.GetIsArcade()) {
			blackOverlay.SetActive(detail);

			for (int i = blessingIconObjs.Count - 1; i >= 0; i--) {
				blessingIconObjs[i].startSlide(blessingIconDefaultLocations[i].transform.position, false);
			}

			yield return new WaitForSeconds(iconLerpTime / 2f);
		}

		canToggleDetails = true;
	}

	private IEnumerator PatronDissolve(float dissolveTime)
    {
        while (myDissolveTimer < 1.2f)
        {
            myDissolveTimer += Time.deltaTime / dissolveTime;
            wallMaterial.SetFloat("_SwitVal", myDissolveTimer);
            yield return null;
        }
    }

    private Texture2D GeneratePerlinNoise(float scaleFactor)
    {
        Texture2D texture = new Texture2D(nPixWidth, nPixHeight);
        float randScale = Random.Range(nScaleMin, nScaleMax)/scaleFactor;
        float randOffset = Random.Range(nOffsetMin, nOffsetMax)/scaleFactor;

        for (int x = 0; x < nPixWidth; x++)
        {
            //convert x to perlin coordinates
            float xCoord = (float)x / nPixWidth * randScale + randOffset;

            for (int y = 0; y < nPixHeight; y++)
            {
                //convert y to perlin coordinates
                float yCoord = (float)y / nPixHeight * randScale + randOffset;
                float colSample = Mathf.PerlinNoise(xCoord, yCoord);
                Color color = new Color (colSample, colSample, colSample);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

	// this doesn't seem to sync up correctly with the map switch cooldown. I'm dividing cooldown by 2 to try to speed it up
	private IEnumerator arrowScaler(GameObject arrow) {
		arrow.transform.localScale = new Vector3(arrowMaxScale, arrowMaxScale, arrowMaxScale);

		float timer = 0;
		float scaleDelta = arrowMaxScale - 1;
		float newScale;

		while (timer < patronSwitchCooldown / 2) {
			newScale = arrowMaxScale - (scaleDelta * (timer / (patronSwitchCooldown / 2)));
			arrow.transform.localScale = new Vector3(newScale, newScale, newScale);

			yield return new WaitForEndOfFrame();
			timer += Time.deltaTime;
		}
		arrow.transform.localScale = new Vector3(1f, 1f, 1f);
	}
	#endregion

	#region Input Abstraction
	private bool getButtonDownInput(int actionId) {
		if (rewiredPlayerKey == PlayerIDs.player1 || !SettingsManager.Instance.GetIsSinglePlayer()) {
			if (rewiredPlayer.GetButtonDown(actionId)) {
				return true;
			}
		}
		else if(rewiredPlayerKey == PlayerIDs.player2) {
			if ((LoadoutManager.Instance.p1Ready || player1.controllers.hasMouse) 
				&& SettingsManager.Instance.GetIsSinglePlayer()
				&& player1.GetButtonDown(actionId)) {
				return true;
			}
		}
		return false;
	}

	private float getHorzAxisInput() {
		if (rewiredPlayerKey == PlayerIDs.player1 || !SettingsManager.Instance.GetIsSinglePlayer()) {
			return pInput.GetHorizNonRadialInput(rewiredPlayer);
		}
		else if (rewiredPlayerKey == PlayerIDs.player2) {
			if (LoadoutManager.Instance.p1Ready) {
				return pInput.GetHorizNonRadialInput(player1);
			}
		}
		return 0f;
	}

	#endregion

	#region Getters and Setters
	public int GetCurrentIndex() {
		return currentIndex;
	}

	public bool GetIsReady() {
		return isReady;
	}

	public void SetIsReady(bool _ready) {
		isReady = _ready;
		lm.unready(rewiredPlayerKey);
	}
	#endregion
}
