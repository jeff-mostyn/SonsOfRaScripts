using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour {

	[System.Serializable]
	public struct LobbyGodEntry {
		public string name;
		public Patron godObject;
	}

	[System.Serializable]
	public struct LobbyMapEntry {
		public string name;
		public Sprite mapImage;
		public Constants.gameScenes mapCode;
	}

	private Player rewiredPlayer;

	[Header("UI Control")]
	[SerializeField] private float STICK_SENSITIVITY = 0.8f;
	[SerializeField] private float switchCooldown;
	public UnityEngine.EventSystems.EventSystem myEventSystem;
	private float switchTimer;
	private float horzMove, oldHorzMove;

	[Header("Dicts")]
	public List<LobbyGodEntry> gods;
	public List<LobbyMapEntry> maps;

	[Header("Host UI")]
	public GameObject p1UI;
	public CanvasGroup p1CanvasGroup;
	public TextMeshProUGUI hostName, hostGodName, mapName;
	public Image mapImage;
	public List<GameObject> hostButtons;
	public GameObject hostWall;
	public GameObject inviteButton;
	public GameObject publicButton, readyButton, leaveButton;
	public GameObject mapSelectButton, p1GodButton;

	[Header("Guest UI")]
	public GameObject p2UI;
	public CanvasGroup p2CanvasGroup;
	public TextMeshProUGUI guestName, guestGodName;
	public List<GameObject> guestButtons;
	public GameObject guestWall;
	public GameObject p2GodButton;

	[Header("General UI")]
	public TextMeshProUGUI countdownText;
	public GameObject RefreshLobbiesButton, CreateLobbiesButton, StartSearchButton;
	public GameObject lobbyWalls, listWalls;

	[Header("Bools")]
	public bool isHost;
	public bool isReady;

	[Header("List UI")]
	[SerializeField] private GameObject ListView;
	[SerializeField] private CanvasGroup ListCG;
	[SerializeField] private List<lobbyListing> lobbyEntries;
	[SerializeField] private GameObject scrollLeft, scrollRight;
	[SerializeField] private float refreshInterval;
	private int baseLobbiesIndex = 0;
	[SerializeField] private TextMeshProUGUI noLobbiesNotice;

	[Header("Search UI")]
	[SerializeField] private CanvasGroup SearchCG;
	[SerializeField] private TextMeshProUGUI searchText;
	[SerializeField] private GameObject cancelSearchButton;

	[Header("Lobby Scene UI Groups")]
	[SerializeField] private GameObject LobbyView;
	[SerializeField] private CanvasGroup LobbyCG;
	[SerializeField] private LobbyMenu lobbyMenu;

	#region Deity Wall Declarations
	[Header("Deity Wall")]
	private Material hostWallMaterial, guestWallMaterial;
	private Coroutine p1DissolveCour, p2DissolveCour = null;
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

	#endregion 

	private Steamworks.Data.Lobby lobby;

	private Coroutine countdown;

	#region System Functions
	private void Start() {
		rewiredPlayer = ReInput.players.GetPlayer(PlayerIDs.player1);

		CollectMaterials();

		switchTimer = 0;
		baseLobbiesIndex = 0;

		searchText.SetText(Lang.OnlineText[Lang.onlineText.searchingForLobbies][SettingsManager.Instance.language]);
		noLobbiesNotice.SetText(Lang.OnlineText[Lang.onlineText.noOpenLobbies][SettingsManager.Instance.language]);

		StartCoroutine(RegularListUpdate());

		horzMove = oldHorzMove = 0;
	}

	private void Update() {
		if (switchTimer > 0) {
			switchTimer -= Time.deltaTime;
		}

		// these buttons should be off if the player has a mouse
		p1GodButton.SetActive(!rewiredPlayer.controllers.hasMouse);
		p2GodButton.SetActive(!rewiredPlayer.controllers.hasMouse);
		mapSelectButton.SetActive(!rewiredPlayer.controllers.hasMouse);

		if (!rewiredPlayer.controllers.hasMouse && (myEventSystem.currentSelectedGameObject == null || !myEventSystem.currentSelectedGameObject.activeSelf)) {
			if (LobbyCG.alpha == 0) {
				myEventSystem.SetSelectedGameObject(CreateLobbiesButton);
				CreateLobbiesButton.GetComponentInChildren<TMPButtonHandler>().buttonSelectQuiet();
			}
			else {
				myEventSystem.SetSelectedGameObject(isHost ? p1GodButton : p2GodButton);
			}
		}
		else if (rewiredPlayer.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}

		HandleDeactivatedNavigations();

		if (LobbyCG.alpha == 1 && !rewiredPlayer.controllers.hasKeyboard) {
			if (!isReady) {

				oldHorzMove = horzMove;
				horzMove = rewiredPlayer.GetAxis(RewiredConsts.Action.MoveHorizontal);

				// reset timer to zero if the player releases the dpad or goes to double tap the stick
				if (Mathf.Abs(oldHorzMove) > STICK_SENSITIVITY && Mathf.Abs(horzMove) < STICK_SENSITIVITY
					|| rewiredPlayer.GetButtonDown(RewiredConsts.Action.DPadRight)
					|| rewiredPlayer.GetButtonDown(RewiredConsts.Action.DPadLeft)) {
					switchTimer = 0;
				}

				if (!SwitchOnCooldown() && (Mathf.Abs(horzMove) >= STICK_SENSITIVITY
						|| rewiredPlayer.GetButton(RewiredConsts.Action.DPadLeft) || rewiredPlayer.GetButton(RewiredConsts.Action.DPadRight))) {	// valid switch input
					if (horzMove > 0 || rewiredPlayer.GetButton(RewiredConsts.Action.DPadRight)) {	// moving right
						if (myEventSystem.currentSelectedGameObject == p1GodButton || myEventSystem.currentSelectedGameObject == p2GodButton) {	// has god name button selected
							IncrementGod();
							switchTimer = switchCooldown;
						}
						else if (isHost && myEventSystem.currentSelectedGameObject == mapSelectButton) {	// has map button selected
							IncrementMap();
							switchTimer = switchCooldown;
						}
					}
					else {	// moving left
						if (myEventSystem.currentSelectedGameObject == p1GodButton || myEventSystem.currentSelectedGameObject == p2GodButton) {	// god button selected
							DecrementGod();
							switchTimer = switchCooldown;
						}
						else if (isHost && myEventSystem.currentSelectedGameObject == mapSelectButton) {	// map button selected
							DecrementMap();
							switchTimer = switchCooldown;
						}
					}
				}

				// also B to leave
				if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.UIBack)) {
					Leave();
				}
			}
			else {  // player readied up
				if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.UIBack)) {
					Ready();
				}
			}
		}

		if (SearchCG.alpha > 0) {
			if (rewiredPlayer.controllers.hasMouse) {
				if (!cancelSearchButton.activeSelf) {
					cancelSearchButton.SetActive(true);
				}
			}
			else {
				if (rewiredPlayer.GetButtonDown(RewiredConsts.Action.UIBack)) {
					CancelSearch();
				}

				if (cancelSearchButton.activeSelf) {
					cancelSearchButton.SetActive(false);
				}
			}
		}
	}
	#endregion

	#region Lobby View Functions
	#region Lobby Startup
	private void Init(Steamworks.Data.Lobby _lobby) {
		isReady = false;
		lobby = _lobby;
		OnlineManager.Instance.currentLobby = _lobby;
		countdown = null;

		UpdateGodLocal(gods.FindIndex(g => g.name == (isHost ? lobby.GetData("hostGod") : lobby.GetData("guestGod"))));
		UpdateMapLocal(maps.FindIndex(m => m.name == lobby.GetData("mapName")));
		countdownText.gameObject.SetActive(false);
	}

	public void Create(Steamworks.Data.Lobby _lobby) {
		isHost = true;

		Init(_lobby);

		if (_lobby.MemberCount == 1) {
			if (!SettingsManager.Instance.DemoMode) {
				publicButton.SetActive(true);
			}
			else {
				publicButton.SetActive(false);
			}
		}

		foreach (GameObject g in hostButtons) {
			g.SetActive(true);
		}
		foreach (GameObject g in guestButtons) {
			g.SetActive(false);
		}

		if (!rewiredPlayer.controllers.hasKeyboard) {
			Navigation nav = readyButton.GetComponent<Button>().navigation;
			nav.selectOnUp = inviteButton.GetComponent<Button>();
			readyButton.GetComponent<Button>().navigation = nav;

			myEventSystem.SetSelectedGameObject(p1GodButton);
		}
	}

	public void Join(Steamworks.Data.Lobby _lobby) {
		isHost = false;

		Init(_lobby);

		publicButton.SetActive(false);

		foreach (GameObject g in hostButtons) {
			g.SetActive(false);
		}
		foreach (GameObject g in guestButtons) {
			g.SetActive(true);
		}

		if (!rewiredPlayer.controllers.hasKeyboard) {
			Navigation nav = readyButton.GetComponent<Button>().navigation;
			nav.selectOnUp = p1GodButton.GetComponent<Button>();
			readyButton.GetComponent<Button>().navigation = nav;

			myEventSystem.SetSelectedGameObject(p2GodButton);
		}
	}

	public void TakeOverLobby() {
		isHost = true;

		foreach (GameObject g in hostButtons) {
			g.SetActive(true);
		}

		foreach (GameObject g in guestButtons) {
			g.SetActive(false);
		}

		if (!SettingsManager.Instance.DemoMode) {
			publicButton.SetActive(true);
		}
		else {
			publicButton.SetActive(false);
		}

		if (isReady) {
			isReady = false;
			lobby.SetMemberData("ready", isReady ? "true" : "false");
		}

		UpdateGodLocal(gods.FindIndex(g => g.name == lobby.GetData("hostGod")));
	}

	public void ActivateUI(Steamworks.Data.Lobby lobby) {
		CollectMaterials();
		if (lobby.Owner.Name == Steamworks.SteamClient.Name) {
			Create(lobby);
		}
		else {
			Join(lobby);
		}
		Refresh();
	}
	#endregion

	#region UI Refresh
	public void Refresh() {
		if (lobby.MemberCount == 2) {
			inviteButton.SetActive(false);
			publicButton.SetActive(false);
			p2UI.SetActive(true);
		}
		else {
			inviteButton.SetActive(true);
			p2UI.SetActive(false);
		}

		UpdatePlayerNames();
		UpdateGods();
		UpdateMap();
		UpdateReady();

		// handle countdown when ready
		if (lobby.GetData("hostReady") == "true" && lobby.GetData("guestReady") == "true" && countdown == null) {
			StartCountdown();
		}
		else if ((lobby.GetData("hostReady") != "true" || lobby.GetData("guestReady") != "true") && countdown != null) {
			StopCountdown();
		}

		p2CanvasGroup.alpha = lobby.GetData("guestName") != "" ? 1f : 0f;
	}

	public void UpdatePlayerNames() {
		hostName.SetText(lobby.GetData("hostName"));
		guestName.SetText(lobby.GetData("guestName"));
	}

	public void UpdateGods() {
		int HostIndex = gods.FindIndex(g => g.name == lobby.GetData("hostGod"));
		int GuestIndex = gods.FindIndex(g => g.name == lobby.GetData("guestGod"));

		if (isHost 
			&& gods[GuestIndex].godObject.PatronBwIllustration != guestWallMaterial.GetTexture("_OverTex2") 
			&& gods[GuestIndex].godObject.PatronColorIllustration != guestWallMaterial.GetTexture("_OverTex2")) {
			setImagesByPatron(ref p2DissolveCour, guestWallMaterial, GuestIndex, lobby.GetData("guestReady") == "true");
			guestGodName.SetText(Lang.patronNames[gods[GuestIndex].godObject.patronID][SettingsManager.Instance.language]);
		}
		else if (gods[HostIndex].godObject.PatronBwIllustration != hostWallMaterial.GetTexture("_OverTex2")
			&& gods[HostIndex].godObject.PatronColorIllustration != hostWallMaterial.GetTexture("_OverTex2")) {
			setImagesByPatron(ref p1DissolveCour, hostWallMaterial, HostIndex, lobby.GetData("hostReady") == "true");
			hostGodName.SetText(Lang.patronNames[gods[HostIndex].godObject.patronID][SettingsManager.Instance.language]);
		}
	}

	public void UpdateMap() {
		if (!isHost) {
			int index = maps.FindIndex(g => g.name == lobby.GetData("mapName"));

			mapImage.sprite = maps[index].mapImage;
			mapName.SetText(Lang.MapNames[maps[index].mapCode][SettingsManager.Instance.language]);
		}
	}

	public void UpdateReady() {
		if (isHost) {
			foreach (GameObject b in hostButtons) {
				b.GetComponent<Button>().interactable = !isReady;
			}
		}
		else {
			foreach (GameObject b in guestButtons) {
				b.GetComponent<Button>().interactable = !isReady;
			}
		}
	}

	private void UpdateGodLocal(int index) {
		if (LobbyManager.Instance.inLobby && LobbyManager.Instance.lobbySearchRoutine == null) {
			if (isHost) {
				setImagesByPatron(ref p1DissolveCour, hostWallMaterial, index, isReady);
				hostGodName.SetText(Lang.patronNames[gods[index].godObject.patronID][SettingsManager.Instance.language]);
			}
			else {
				setImagesByPatron(ref p2DissolveCour, guestWallMaterial, index, isReady);
				guestGodName.SetText(Lang.patronNames[gods[index].godObject.patronID][SettingsManager.Instance.language]);
			}
		}
	}

	private void UpdateMapLocal(int index) {
		mapImage.sprite = maps[index].mapImage;
		mapName.SetText(Lang.MapNames[maps[index].mapCode][SettingsManager.Instance.language]);
	}
	#endregion

	#region Button Functions
	public void IncrementGod() {
		int currentIndex = gods.FindIndex(g => g.name == (isHost ? lobby.GetData("hostGod") : lobby.GetData("guestGod")));
		do {
			if (currentIndex < gods.Count - 1) {
				currentIndex++;
			}
			else {
				currentIndex = 0;
			}
		} while (ContentManager.Instance.isLocked(gods[currentIndex].name));

		lobby.SetMemberData("godName", gods[currentIndex].name);
		UpdateGodLocal(currentIndex);
		SoundManager.Instance.sound_godNext();
	}

	public void DecrementGod() {
		int currentIndex = gods.FindIndex(g => g.name == (isHost ? lobby.GetData("hostGod") : lobby.GetData("guestGod")));
		do {
			if (currentIndex > 0) {
				currentIndex--;
			}
			else {
				currentIndex = gods.Count - 1;
			}
		} while (ContentManager.Instance.isLocked(gods[currentIndex].name));

		lobby.SetMemberData("godName", gods[currentIndex].name);
		UpdateGodLocal(currentIndex);
		SoundManager.Instance.sound_godPrev();
	}

	public void IncrementMap() {
		int currentIndex = maps.FindIndex(m => m.name == lobby.GetData("mapName"));
		if (currentIndex < maps.Count - 1) {
			currentIndex++;
		}
		else {
			currentIndex = 0;
		}

		lobby.SetData("mapName", maps[currentIndex].name);
		UpdateMapLocal(currentIndex);
		SoundManager.Instance.sound_godNext();
	}

	public void DecrementMap() {
		int currentIndex = maps.FindIndex(m => m.name == lobby.GetData("mapName"));
		if (currentIndex > 0) {
			currentIndex--;
		}
		else {
			currentIndex = maps.Count - 1;
		}

		lobby.SetData("mapName", maps[currentIndex].name);
		UpdateMapLocal(currentIndex);
		SoundManager.Instance.sound_godNext();
	}

	public void Ready() {
		isReady = !isReady;
		lobby.SetMemberData("ready", isReady ? "true" : "false");

		if (isReady) {
			SoundManager.Instance.sound_GodSelect();
		}

		UpdateGodLocal(gods.FindIndex(g => g.name == (isHost ? lobby.GetData("hostGod") : lobby.GetData("guestGod"))));
	}

	public void Leave() {
		StopCountdown();
		LobbyManager.Instance.LeaveLobby();

		SoundManager.Instance.sound_back_menu();

		lobby.Leave();
		ClearWall();
		UpdateLobbiesList();

		myEventSystem.SetSelectedGameObject(null);
	}

	public void Invite() {
		LobbyManager.Instance.OpenInviteWindow(lobby);
	}

	public void SetLobbyPublic() {
		LobbyManager.Instance.SetPublic(lobby, publicButton);
	}
    #endregion

    #region Countdown and Game Prep
    private void StartCountdown() {
        countdown = StartCoroutine(Countdown());
        countdownText.gameObject.SetActive(true);
    }

    private void StopCountdown() {
		if (countdown != null) {
			StopCoroutine(countdown);
			countdown = null;
		}
        countdownText.gameObject.SetActive(false);
		readyButton.GetComponentInChildren<Button>().enabled = true;
		leaveButton.GetComponentInChildren<Button>().enabled = true;
	}

    private IEnumerator Countdown() {
		if (isHost && lobby.GetData("mapName") == "Random") {
			int mapIndex = Random.Range(1, maps.Count);
			lobby.SetData("randomMapNumber", mapIndex.ToString());

			Debug.Log("Index: " + mapIndex);
			Debug.Log("Map name: " + maps[mapIndex].name);
		}

		float timer = 4f;
		float oldTime = timer;
		int oldNumber;
		int newNumber = Mathf.Max(Mathf.CeilToInt(timer - 1), 1);

		while (timer > 0) {	
			countdownText.SetText(newNumber.ToString());

			if (timer < 1) {
				readyButton.GetComponentInChildren<Button>().enabled = false;
				leaveButton.GetComponentInChildren<Button>().enabled = false;
			}

            yield return null;
			oldTime = timer;
            timer -= Time.deltaTime;
			oldNumber = newNumber;
			newNumber = Mathf.Max(Mathf.CeilToInt(timer - 1), 1);

			if (newNumber != oldNumber) {
				SoundManager.Instance.sound_countDown();
			}
		}

        SetLoadouts();

        OnlineManager.Instance.waitForLoad = true;

        if (isHost) {
            OnlineManager.Instance.fakePlayerKey = PlayerIDs.player1;
        }
        else {
            OnlineManager.Instance.fakePlayerKey = PlayerIDs.player2;
        }

        OnlineManager.Instance.SendPacket(new PO_Handshake(OnlineManager.Instance.localLocation.ToString()));
		OnlineManager.Instance.StartPing();

		readyButton.GetComponentInChildren<Button>().enabled = true;
		leaveButton.GetComponentInChildren<Button>().enabled = true;

		if (lobby.GetData("mapName") != "Random") {
			LobbyManager.Instance.selectedMap = maps.Find(m => m.name == lobby.GetData("mapName")).mapCode;
		}
		else {
			LobbyManager.Instance.selectedMap = maps[int.Parse(lobby.GetData("randomMapNumber"))].mapCode;
		}

		SceneChangeManager.Instance.setNextSceneName(Constants.sceneNames[LobbyManager.Instance.selectedMap]);
        SceneChangeManager.Instance.LoadNextScene();
    }

    private void SetLoadouts() {
        LoadoutManager lm = LoadoutManager.Instance;

        // p1
        lm.p1Patron = gods.Find(g => g.name == lobby.GetData("hostGod")).godObject;

		// p2
		lm.p2Patron = gods.Find(g => g.name == lobby.GetData("guestGod")).godObject;

		// set colors
		PlayerColorPalette p1Palette, p2Palette;
		try {
			p1Palette = CustomizationManager.Instance.colorPalettes1.Find(x => x.name == lobby.GetData("hostColorName"));
		}
		catch {
			p1Palette = CustomizationManager.Instance.defaultP1Palette;
		}
		try {
			p2Palette = CustomizationManager.Instance.colorPalettes2.Find(x => x.name == lobby.GetData("guestColorName"));
		}
		catch {
			p2Palette = CustomizationManager.Instance.defaultP2Palette;
		}
		lm.SetColorPalettes(p1Palette.GetColorPalette(), p2Palette.GetColorPalette());


		// set portraits
		Sprite p1Portrait, p2Portrait;
		try {
			p1Portrait = CustomizationManager.Instance.portaits.Find(x => x.name == lobby.GetData("hostPortraitName"));
		}
		catch {
			p1Portrait = CustomizationManager.Instance.defaultPlayerPortrait;
		}
		try {
			p2Portrait = CustomizationManager.Instance.portaits.Find(x => x.name == lobby.GetData("guestPortraitName"));
		}
		catch {
			p2Portrait = CustomizationManager.Instance.defaultPlayerPortrait;
		}
		lm.SetPortraits(p1Portrait, p2Portrait);


		// set cosmetics
		GameObject p1Cosmetic, p2Cosmetic;
		try {
			p1Cosmetic = CustomizationManager.Instance.cosmetics.Find(x => x.name == lobby.GetData("hostCosmeticName"));
		}
		catch {
			p1Cosmetic = CustomizationManager.Instance.defaultCosmetic;
		}
		try {
			p2Cosmetic = CustomizationManager.Instance.cosmetics.Find(x => x.name == lobby.GetData("guestCosmeticName"));
		}
		catch {
			p2Cosmetic = CustomizationManager.Instance.defaultCosmetic;
		}
		lm.SetCosmetics(p1Cosmetic, p2Cosmetic);

		// clean up match manager if it exists
		if (GameObject.Find("MatchManager") != null) {
            Destroy(GameObject.Find("MatchManager"));
        }

        lm.setBlessingsByPatron();
    }
	#endregion

	#region Utility
	/// <summary>
	/// Check if the option switch timer is at 0 or not.
	/// </summary>
	/// <returns>Return true if the timer is finished and options can be switched</returns>
	private bool SwitchOnCooldown() {
        if (switchTimer > 0) {
            return true;
        }
        else {
            return false;
        }
    }

	private void CollectMaterials() {
		if (!hostWallMaterial) {
			//get image wall material, set it to new instance of
			Material tempMat = hostWall.GetComponent<Renderer>().material;
			hostWallMaterial = Instantiate(tempMat);
			hostWall.GetComponent<Renderer>().material = hostWallMaterial;
			tempMat = guestWall.GetComponent<Renderer>().material;
			guestWallMaterial = Instantiate(tempMat);
			guestWall.GetComponent<Renderer>().material = guestWallMaterial;
		}
	}

	public Steamworks.Data.Lobby GetLobby() {
		return lobby;
	}

	private void HandleDeactivatedNavigations() {
		if (!rewiredPlayer.controllers.hasMouse) {
			Navigation nav;
			if (!publicButton.activeSelf) {
				nav = mapSelectButton.GetComponent<Button>().navigation;
				nav.selectOnDown = inviteButton.GetComponent<Button>();
				mapSelectButton.GetComponent<Button>().navigation = nav;

				// only the invite button will ever go up to set public button
				// if invite isn't there, neither will set public
				nav = inviteButton.GetComponent<Button>().navigation;
				nav.selectOnUp = mapSelectButton.GetComponent<Button>();
				inviteButton.GetComponent<Button>().navigation = nav;
			}
			else {
				nav = mapSelectButton.GetComponent<Button>().navigation;
				nav.selectOnDown = publicButton.GetComponent<Button>();
				mapSelectButton.GetComponent<Button>().navigation = nav;

				nav = inviteButton.GetComponent<Button>().navigation;
				nav.selectOnUp = publicButton.GetComponent<Button>();
				inviteButton.GetComponent<Button>().navigation = nav;
			}

			// if invite button isnt there, neither will public button be
			if (!inviteButton.activeSelf) {
				nav = mapSelectButton.GetComponent<Button>().navigation;
				nav.selectOnDown = readyButton.GetComponent<Button>();
				mapSelectButton.GetComponent<Button>().navigation = nav;

				nav = readyButton.GetComponent<Button>().navigation;
				nav.selectOnUp = mapSelectButton.GetComponent<Button>();
				readyButton.GetComponent<Button>().navigation = nav;
			}
			else {
				if (publicButton.activeSelf) {
					nav = mapSelectButton.GetComponent<Button>().navigation;
					nav.selectOnDown = publicButton.GetComponent<Button>();
					mapSelectButton.GetComponent<Button>().navigation = nav;
				}
				else {
					nav = mapSelectButton.GetComponent<Button>().navigation;
					nav.selectOnDown = inviteButton.GetComponent<Button>();
					mapSelectButton.GetComponent<Button>().navigation = nav;
				}

				nav = readyButton.GetComponent<Button>().navigation;
				nav.selectOnUp = inviteButton.GetComponent<Button>();
				readyButton.GetComponent<Button>().navigation = nav;
			}
		}
	}
	#endregion
	#endregion

	#region List View Functions
	public async void UpdateLobbiesList() {
		lobbyListing listing;

		Steamworks.Data.Lobby[] availableLobbies = await LobbyManager.Instance.GetLobbies();

		try {
			if (availableLobbies.Length != 0) {
				Debug.Log("there are lobbies");
				List<Steamworks.Data.Lobby> availableLobbiesList = availableLobbies.ToList();
				availableLobbiesList.RemoveAll(l => l.MemberCount != 1);
				availableLobbiesList.RemoveAll(l => l.Owner.Name == SteamWorksManager.Instance.GetUsername());
				availableLobbiesList.RemoveAll(l => !l.Id.IsValid);
				availableLobbiesList.RemoveAll(l => l.GetData("applicationVersion") != Application.version);

				// make sure we dont display empty pages
				while (availableLobbiesList.Count <= baseLobbiesIndex * lobbyEntries.Count && baseLobbiesIndex > 0) {
					baseLobbiesIndex--;
				}

				for (int i = 0; i < lobbyEntries.Count; i++) {
					listing = lobbyEntries[i];

					if (availableLobbiesList == null || baseLobbiesIndex + i >= availableLobbiesList.Count) {
						listing.SetTextFieldValues("", "");
						listing.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
					}
					else {
						listing.SetTextFieldValues(availableLobbiesList[baseLobbiesIndex + i].GetData("hostName"), availableLobbiesList[baseLobbiesIndex + i].GetData("hostPingLocation"));
						listing.lobby = availableLobbiesList[baseLobbiesIndex + i];
						listing.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
					}

					listing.Refresh();
				}

				if (availableLobbiesList.Count > 0) {
					noLobbiesNotice.gameObject.GetComponent<CanvasGroup>().alpha = 0;
				}
				RefreshScrollButtons(availableLobbiesList.Count);
			}
		}
		catch {
			Debug.Log("there are not lobbies");
			baseLobbiesIndex = 0;
			for (int i = 0; i < lobbyEntries.Count; i++) {
				listing = lobbyEntries[i];

				listing.SetTextFieldValues("", "");

				listing.Refresh();
			}

			noLobbiesNotice.gameObject.GetComponent<CanvasGroup>().alpha = 1;
			RefreshScrollButtons(0);
		}
	}

	public void CreateLobby() {
		if (lobby.Id.IsValid) {
			LobbyManager.Instance.LeaveLobby();
			lobby.Leave();
		}

		SoundManager.Instance.sound_click();
		LobbyManager.Instance.CreateLobby();
	}

	public void ChangeBaseLobbyForListDisplay(int dir) {
		baseLobbiesIndex += lobbyEntries.Count * dir;

		UpdateLobbiesList();
	}

	private void RefreshScrollButtons(int lobbyCount) {
		if (baseLobbiesIndex == 0) {
			scrollLeft.SetActive(false);
		}
		else {
			scrollLeft.SetActive(true);
		}

		try { // there are lobbies
			if (baseLobbiesIndex + lobbyEntries.Count >= lobbyCount) {
				scrollRight.SetActive(false);
			}
			else {
				scrollRight.SetActive(true);
			}
		}
		catch {	// no lobbies
			scrollRight.SetActive(false);
		}
	}

	IEnumerator RegularListUpdate() {
		yield return new WaitForSeconds(1f);
		UpdateLobbiesList();

		while (true) {
			yield return new WaitForSeconds(refreshInterval);

			UpdateLobbiesList();
		}
	}

	public void StartLobbySearch() {
		LobbyManager.Instance.StartLobbySearch();

		SoundManager.Instance.sound_click();
		StartSearchButton.GetComponentInChildren<TMPButtonHandler>().buttonDeselect();
	}
	#endregion

	#region Search View Functions
	public void CancelSearch() {
		LobbyManager.Instance.CancelLobbySearch();
		SoundManager.Instance.sound_back_menu();

		if (!rewiredPlayer.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(StartSearchButton);
		}
	}
	#endregion

	#region Lobby Deity Wall
	public void setImagesByPatron(ref Coroutine dissolveCoroutine, Material wallMaterial, int index, bool ready) {
		Patron patron = gods[index].godObject;

		//target textures
		Texture nextText = ready ? patron.PatronColorIllustration.texture : patron.PatronBwIllustration.texture;
		if (nextText != wallMaterial.GetTexture("_OverTex2") && LobbyView.activeSelf) {
			if (dissolveCoroutine != null) {
				StopCoroutine(dissolveCoroutine);  //make sure coroutine isn't already running
			}
			//setting all material variables for the wall transition
			wallMaterial.SetFloat("_SwitVal", 0f);
			wallMaterial.SetFloat("_GlowThick", ready ? 0.03f : 0f);

			//previous target as base textures
			wallMaterial.SetTexture("_OverTex1", wallMaterial.GetTexture("_OverTex2"));
			wallMaterial.SetTexture("_NormMap1", wallMaterial.GetTexture("_NormMap2"));
			wallMaterial.SetTexture("_NormAlpha1", wallMaterial.GetTexture("_NormAlpha2"));

			wallMaterial.SetTexture("_OverTex2", nextText);
			wallMaterial.SetTexture("_NormMap2", patron.PatronNormalMap);
			wallMaterial.SetTexture("_NormAlpha2", patron.PatronNormalAlpha.texture);

			wallMaterial.SetTexture("_NoiseTex", GeneratePerlinNoise(3f));

			//Coroutine to go through dissolve effect, can change durration
			dissolveCoroutine = StartCoroutine(PatronDissolve(1f, wallMaterial));
		}

		if (isHost && lobby.MemberCount == 1) {
			guestWallMaterial.SetTexture("_OverTex1", OverlayTex2Plain);
			guestWallMaterial.SetTexture("_NormMap1", NormalTex2Plain);
			guestWallMaterial.SetTexture("_NormAlpha1", NormalAlpha2Plain);
			guestWallMaterial.SetTexture("_OverTex2", OverlayTex2Plain);
			guestWallMaterial.SetTexture("_NormMap2", NormalTex2Plain);
			guestWallMaterial.SetTexture("_NormAlpha2", NormalAlpha2Plain);
		}
	}

	private void ClearWall() {
		hostWallMaterial.SetTexture("_OverTex1", OverlayTex2Plain);
		hostWallMaterial.SetTexture("_NormMap1", NormalTex2Plain);
		hostWallMaterial.SetTexture("_NormAlpha1", NormalAlpha2Plain);
		hostWallMaterial.SetTexture("_OverTex2", OverlayTex2Plain);
		hostWallMaterial.SetTexture("_NormMap2", NormalTex2Plain);
		hostWallMaterial.SetTexture("_NormAlpha2", NormalAlpha2Plain);

		guestWallMaterial.SetTexture("_OverTex1", OverlayTex2Plain);
		guestWallMaterial.SetTexture("_NormMap1", NormalTex2Plain);
		guestWallMaterial.SetTexture("_NormAlpha1", NormalAlpha2Plain);
		guestWallMaterial.SetTexture("_OverTex2", OverlayTex2Plain);
		guestWallMaterial.SetTexture("_NormMap2", NormalTex2Plain);
		guestWallMaterial.SetTexture("_NormAlpha2", NormalAlpha2Plain);
	}

	private IEnumerator PatronDissolve(float dissolveTime, Material wallMaterial) {
		float myDissolveTimer = 0f;

		while (myDissolveTimer < 1.2f) {
			myDissolveTimer += Time.deltaTime / dissolveTime;
			wallMaterial.SetFloat("_SwitVal", myDissolveTimer);
			yield return null;
		}
	}

	private Texture2D GeneratePerlinNoise(float scaleFactor) {
		Texture2D texture = new Texture2D(nPixWidth, nPixHeight);
		float randScale = Random.Range(nScaleMin, nScaleMax) / scaleFactor;
		float randOffset = Random.Range(nOffsetMin, nOffsetMax) / scaleFactor;

		for (int x = 0; x < nPixWidth; x++) {
			//convert x to perlin coordinates
			float xCoord = (float)x / nPixWidth * randScale + randOffset;

			for (int y = 0; y < nPixHeight; y++) {
				//convert y to perlin coordinates
				float yCoord = (float)y / nPixHeight * randScale + randOffset;
				float colSample = Mathf.PerlinNoise(xCoord, yCoord);
				Color color = new Color(colSample, colSample, colSample);
				texture.SetPixel(x, y, color);
			}
		}
		texture.Apply();
		return texture;
	}

	#endregion
}
