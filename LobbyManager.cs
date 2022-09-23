#if !UNITY_XBOXONE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour {
	private enum searchMode { close, mid, far };

	public static LobbyManager Instance;

	[Header("Match and Game Things")]
	public Constants.gameScenes selectedMap;

	[Header("Refresh")]
	[SerializeField] private float updateFrequency;
	private float updateCountdown;

	[Header("Lobby Search Parameters")]
	public float searchDuration;
	public float timeBetweenQueries;
	public Coroutine lobbySearchRoutine;

	[Header("Notification Messaging")]
	[SerializeField] private GameObject NotificationWindow;
	[SerializeField] private LobbyNotification notificationScript;
	[SerializeField] private float messageActiveTime, gameCloseTime;

	// Lobby Scene UI Groups
	private GameObject LobbyView, ListView, SearchView;
	private LobbyMenu lobbyMenu;

	private Coroutine RematchCountdownCoroutine = null;

	private MenuScripts menuScripts;

	public bool inLobby = false;
	public Steamworks.Data.Lobby myLobby;
	private Friend opponent;

	[Header("Lobby Data")]
	public bool opponentRematch = false;
	public bool selfRematch = false;


	// THIS IS THE LOBBY DATA THAT IS CURRENTLY BEING STORED
	/* General Lobby Data:
     * hostName
     * hostGod
	 * hostColorName
	 * hostPortraitName
	 * hostCosmeticName
     * hostReady
	 * hostLobby
     * guestname
     * guestGod
	 * guestColorName
	 * guestPortraitName
	 * guestCosmeticName
     * guestReady
	 * guestLobby
     * mapName
	 * hostPingLocation
	 * applicationVersion
	 * randomMapNumber
     * 
     * Member Data
     * name
     * godName
	 * colorName
	 * portraitName
	 * cosmeticName
     * ready
	 * lobby
    */

	#region Event Handlers
	void OnLobbyCreated_handler(Result res, Steamworks.Data.Lobby lobby) {
		Debug.Log("created lobby");
		if (myLobby.Id.IsValid) {
			myLobby.Leave();
			inLobby = false;
		}

		lobby.SetData("hostName", SteamClient.Name);
		lobby.SetData("hostGod", "Ra");
		lobby.SetData("hostColorName", CustomizationManager.Instance.p1PaletteName);
		lobby.SetData("hostPortraitName", CustomizationManager.Instance.playerPortraitName);
		lobby.SetData("hostCosmeticName", CustomizationManager.Instance.cosmeticName);
		lobby.SetData("hostReady", "false");
		lobby.SetData("hostLobby", "false");
		lobby.SetData("hostRematch", "false");
		lobby.SetData("guestName", "");
		lobby.SetData("guestGod", "Ra");
		lobby.SetData("guestReady", "false");
		lobby.SetData("guestLobby", "false");
		lobby.SetData("guestRematch", "false");
		lobby.SetData("mapName", "Senet");
		lobby.SetData("hostPingLocation", OnlineManager.Instance.localLocation.ToString());
		lobby.SetData("applicationVersion", Application.version);

		inLobby = true;
		myLobby = lobby;

		if (SettingsManager.Instance.DemoMode || lobbySearchRoutine != null) {
			myLobby.SetPublic();
		}
		else {
			myLobby.SetPrivate();
		}

		// if we're not doing a lobby search, turn on lobby UI
		if (lobbySearchRoutine == null) {
			SwitchToLobbyView();
		}

		lobbyMenu.Create(lobby);
		lobbyMenu.Refresh();
	}

	void OnLobbyEntered_handler(Steamworks.Data.Lobby lobby) {
		Debug.Log("Joined lobby owned by " + lobby.Owner.Name);
		inLobby = true;
		myLobby = lobby;

		lobby.SetMemberData("name", SteamClient.Name);
		lobby.SetMemberData("godName", "Ra");
		lobby.SetMemberData("colorName", lobby.Owner.Name == SteamClient.Name ? CustomizationManager.Instance.p1PaletteName : CustomizationManager.Instance.p2PaletteName);
		lobby.SetMemberData("portraitName", CustomizationManager.Instance.playerPortraitName);
		lobby.SetMemberData("cosmeticName", CustomizationManager.Instance.cosmeticName);
		lobby.SetMemberData("ready", "false");

		if (lobbySearchRoutine == null) {
			SwitchToLobbyView();
		}

		if (!AmILobbyOwner(lobby)) {
			lobbyMenu.Join(lobby);
			OnlineManager.Instance.SetOpponent(lobby.Owner);
		}

		lobbyMenu.Refresh();
	}

	void OnLobbyMemberLeave_handler(Steamworks.Data.Lobby lobby, Friend friend) {
		HandlePlayerLeaveLobby(lobby, friend);
	}

	void OnLobbyMemberDisconnected_handler(Steamworks.Data.Lobby lobby, Friend friend) {
		HandlePlayerLeaveLobby(lobby, friend);
	}

	private void HandlePlayerLeaveLobby(Steamworks.Data.Lobby lobby, Friend friend) {
		OnlineManager.Instance.partnerLocation = null;
		bool takeOverLobby = false;

		// here we will need to clear text data and if necessary, promotion to host
		// if guest leaves, only clear guest data
		// if host leaves, clear everything, and reset lobby menu so guest can be made host
		if (lobby.GetData("hostName") != SteamClient.Name) {
			lobby.SetData("hostGod", lobby.GetData("guestGod"));
			lobby.SetData("hostName", SteamClient.Name);
			lobby.SetData("hostPingLocation", OnlineManager.Instance.localLocation.ToString());
			lobby.SetData("applicationVersion", Application.version);
			lobby.SetMemberData("colorName", CustomizationManager.Instance.p1PaletteName);
			lobby.SetData("hostColorName", CustomizationManager.Instance.p1PaletteName);
			lobby.SetData("hostPortraitName", CustomizationManager.Instance.playerPortraitName);
			lobby.SetData("hostCosmeticName", CustomizationManager.Instance.cosmeticName);
			takeOverLobby = true;
		}
		lobby.SetData("hostReady", "false");
		lobby.SetData("hostLobby", "false");
		lobby.SetData("guestName", "");
		lobby.SetData("guestGod", "Ra");
		lobby.SetData("guestReady", "false");
		lobby.SetData("guestLobby", "false");

		if (SceneManager.GetActiveScene().name == Constants.sceneNames[Constants.gameScenes.lobby]) {
			Debug.Log(friend.Name + " left the lobby");
			Debug.Log("There are " + lobby.MemberCount + " people in this lobby");
			Debug.Log("Lobby owner is " + lobby.Owner.Name);
			lobby.SetPublic();

			if (takeOverLobby) {
				lobbyMenu.TakeOverLobby();
			}

			lobbyMenu.Refresh();

			DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.opponentLeaveLobby][SettingsManager.Instance.language], friend.Name));
		}
		else if (SceneManager.GetActiveScene().name == Constants.sceneNames[Constants.gameScenes.postGame]) {
			// display message
			DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.opponentLeaveLobby][SettingsManager.Instance.language], friend.Name));

			SteamNetworking.CloseP2PSessionWithUser(friend.Id);

			if (RematchCountdownCoroutine != null) {
				StopCoroutine(RematchCountdownCoroutine);
				notificationScript.SetText(Lang.OnlineNotifications[Lang.onlineNotifications.opponentLeaveRematchCancel][SettingsManager.Instance.language]);
			}

			// disable rematch button
			GameObject.Find("PostGameMenuController").GetComponent<PostGameMenuController>().DisableButtonsOnOpponentLeave();
			opponentRematch = false;
			selfRematch = false;

			// leave lobby
			lobby.Leave();
			inLobby = false;
		}
		else if (SceneManager.GetActiveScene().name == Constants.sceneNames[Constants.gameScenes.load]) {
			DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.opponentLeaveLobby][SettingsManager.Instance.language], friend.Name));
			SteamNetworking.CloseP2PSessionWithUser(friend.Id);
			lobby.Leave();
			inLobby = false;
			StartCoroutine(GoToMainMenuAfterDelay());
		}
		else {
			//DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.opponentLeaveGame][SettingsManager.Instance.language], friend.Name));
			SteamNetworking.CloseP2PSessionWithUser(friend.Id);
			//StartCoroutine(ExitGameAfterDelay(lobby.GetData("hostName") == SteamClient.Name));
			StartDisconnectSequence();
			lobby.Leave();
			inLobby = false;
		}
	}

	void OnLobbyMemberJoined_handler(Steamworks.Data.Lobby lobby, Friend friend) {
		Debug.Log(friend.Name + " has joined the lobby");
		Debug.Log("There are " + lobby.MemberCount + " people in this lobby");

		lobby.SetData("guestName", friend.Name);

		lobby.SetPrivate();

		if (AmILobbyOwner(lobby)) {
			OnlineManager.Instance.SetOpponent(friend);
		}

		if (lobbySearchRoutine != null) {
			StopCoroutine(lobbySearchRoutine);
			lobbySearchRoutine = null;
			SwitchToLobbyView();
		}

		lobbyMenu.Refresh();
	}

	void OnChatMessage_handler(Steamworks.Data.Lobby lobby, Friend friend, string msg) {
		Debug.Log(friend.Name + ": " + msg);
	}

	void OnLobbyDataChanged_handler(Steamworks.Data.Lobby lobby) {
		lobbyMenu.Refresh();
	}

	void OnLobbyMemberDataChanged_handler(Steamworks.Data.Lobby lobby, Friend friend) {
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestGod" : "hostGod", lobby.GetMemberData(friend, "godName"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestName" : "hostName", lobby.GetMemberData(friend, "name"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestReady" : "hostReady", lobby.GetMemberData(friend, "ready"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestRematch" : "hostRematch", lobby.GetMemberData(friend, "rematch"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestLobby" : "hostLobby", lobby.GetMemberData(friend, "lobby"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestColorName" : "hostColorName", lobby.GetMemberData(friend, "colorName"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestPortraitName" : "hostPortraitName", lobby.GetMemberData(friend, "portraitName"));
		lobby.SetData(friend.Name != lobby.Owner.Name ? "guestCosmeticName" : "hostCosmeticName", lobby.GetMemberData(friend, "cosmeticName"));

		if (SceneManager.GetActiveScene().name == Constants.sceneNames[Constants.gameScenes.postGame]) {
			if (lobby.MemberCount != 2) {  // if there are not two people, set everything rematch to false
				opponentRematch = false;
				selfRematch = false;
			}

			// the other player has returned to the lobby, but not me
			if (((lobby.GetData("hostLobby") == "true" && !AmILobbyOwner(lobby)) || (lobby.GetData("guestLobby") == "true" && AmILobbyOwner(lobby)))
				&& lobby.GetData("hostLobby") != lobby.GetData("guestLobby")) {

				DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.opponentReturnToLobby][SettingsManager.Instance.language], friend.Name));
				GameObject.Find("PostGameMenuController").GetComponent<PostGameMenuController>().DisableButtonsOnOpponentLeave();
				if (RematchCountdownCoroutine != null) {
					StopCoroutine(RematchCountdownCoroutine);
				}
			}
		}
	}
	#endregion

	#region System Functions
	private void Awake() {
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;

			SteamMatchmaking.OnLobbyCreated += OnLobbyCreated_handler;
			SteamMatchmaking.OnLobbyEntered += OnLobbyEntered_handler;
			SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave_handler;
			SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected_handler;
			SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined_handler;
			SteamMatchmaking.OnChatMessage += OnChatMessage_handler;
			SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged_handler;

			// this will be used for the players updating their gods
			SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged_handler;

			SceneManager.sceneLoaded += OnSceneLoaded;

			lobbyMenu = GameObject.Find("GeneralCanvas").GetComponent<LobbyMenu>();
			LobbyView = GameObject.Find("LobbyView");
			ListView = GameObject.Find("ListView");
			SearchView = GameObject.Find("SearchView");

			StartCoroutine(UpdatePingLocation());
		}
		else {
			Destroy(gameObject);
		}
	}

	void Start() {
		updateCountdown = updateFrequency;
		menuScripts = GetComponent<MenuScripts>();

		GetLobbies();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		selfRematch = false;
		opponentRematch = false;

		if (scene.name == Constants.sceneNames[Constants.gameScenes.postGame] || scene.name == Constants.sceneNames[Constants.gameScenes.lobby]) {
			myLobby.SetMemberData("ready", "false");
			myLobby.SetMemberData("lobby", "false");

			if (myLobby.GetData("hostName") == SteamClient.Name) {
				myLobby.SetData("hostReady", "false");
				myLobby.SetData("hostLobby", "false");

				myLobby.SetData("guestReady", "false");
				myLobby.SetData("guestLobby", "false");
			}
		}

		if (scene.name == Constants.sceneNames[Constants.gameScenes.lobby]) {
			lobbyMenu = GameObject.Find("GeneralCanvas").GetComponent<LobbyMenu>();
			LobbyView = GameObject.Find("LobbyView");
			ListView = GameObject.Find("ListView");
			SearchView = GameObject.Find("SearchView");

			if (inLobby) {
				Debug.Log("in lobby");
				SwitchToLobbyView();
			}
			else {
				SwitchToListView();
				SteamNetworkingUtils.InitRelayNetworkAccess();
			}
		}

		if (notificationScript != null) {
			notificationScript.ForceTurnOff();
		}
	}

	private void OnDestroy() {
		SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated_handler;
		SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered_handler;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave_handler;
		SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected_handler;
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined_handler;
		SteamMatchmaking.OnChatMessage -= OnChatMessage_handler;
		SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged_handler;
		SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged_handler;

		if (RematchCountdownCoroutine != null) {
			StopCoroutine(RematchCountdownCoroutine);
		}
	}
	#endregion

	#region UI Functions
	public void SwitchToLobbyView() {
		// turn off list view
		ListView.GetComponent<CanvasGroup>().alpha = 0f;
		ListView.GetComponent<CanvasGroup>().interactable = false;
		ListView.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// turn on lobby view
		LobbyView.GetComponent<CanvasGroup>().alpha = 1f;
		LobbyView.GetComponent<CanvasGroup>().interactable = true;
		LobbyView.GetComponent<CanvasGroup>().blocksRaycasts = true;

		// turn off search view
		SearchView.GetComponent<CanvasGroup>().alpha = 0f;
		SearchView.GetComponent<CanvasGroup>().interactable = false;
		SearchView.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// switch lobby walls
		lobbyMenu.listWalls.SetActive(false);
		lobbyMenu.lobbyWalls.SetActive(true);

		lobbyMenu.ActivateUI(myLobby);
	}

	public void SwitchToListView() {
		// turn on list view
		ListView.GetComponent<CanvasGroup>().alpha = 1f;
		ListView.GetComponent<CanvasGroup>().interactable = true;
		ListView.GetComponent<CanvasGroup>().blocksRaycasts = true;

		// turn on lobby view
		LobbyView.GetComponent<CanvasGroup>().alpha = 0f;
		LobbyView.GetComponent<CanvasGroup>().interactable = false;
		LobbyView.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// turn off search view
		SearchView.GetComponent<CanvasGroup>().alpha = 0f;
		SearchView.GetComponent<CanvasGroup>().interactable = false;
		SearchView.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// switch lobby walls
		lobbyMenu.listWalls.SetActive(true);
		lobbyMenu.lobbyWalls.SetActive(false);
	}

	public void SwitchToSearchView() {
		// turn on list view
		ListView.GetComponent<CanvasGroup>().alpha = 0f;
		ListView.GetComponent<CanvasGroup>().interactable = false;
		ListView.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// turn off lobby view
		LobbyView.GetComponent<CanvasGroup>().alpha = 0f;
		LobbyView.GetComponent<CanvasGroup>().interactable = false;
		LobbyView.GetComponent<CanvasGroup>().blocksRaycasts = false;

		// turn on search view
		SearchView.GetComponent<CanvasGroup>().alpha = 1f;
		SearchView.GetComponent<CanvasGroup>().interactable = true;
		SearchView.GetComponent<CanvasGroup>().blocksRaycasts = true;

		// switch lobby walls
		lobbyMenu.listWalls.SetActive(true);
		lobbyMenu.lobbyWalls.SetActive(false);
	}
	#endregion

	#region Lobby Search
	public void CancelLobbySearch() {
		StopCoroutine(lobbySearchRoutine);
		lobbySearchRoutine = null;
		lobbyMenu.Leave();
		SwitchToListView();
	}

	public void StartLobbySearch() {
		if (lobbySearchRoutine == null) {
			SwitchToSearchView();

			lobbySearchRoutine = StartCoroutine(LobbySearchLoop());

			CreateLobby();
			myLobby.SetPublic();
		}
	}

	IEnumerator LobbySearchLoop() {
		float timeElapsed = 0f;

		while (timeElapsed <= searchDuration) {
			LobbySearcher(timeElapsed < searchDuration / 3 ? searchMode.close : timeElapsed < (2 * searchDuration) / 3 ? searchMode.mid : searchMode.far);

			yield return new WaitForSeconds(timeBetweenQueries);
			timeElapsed += timeBetweenQueries;
		}

		CancelLobbySearch();
		yield return null;
	}

	private async void LobbySearcher(searchMode mode) {
		Steamworks.Data.LobbyQuery query = SteamMatchmaking.LobbyList.WithMaxResults(100).WithSlotsAvailable(1).FilterDistanceClose();
		Task<Steamworks.Data.Lobby[]> queryTask = query.RequestAsync();
		Steamworks.Data.Lobby[] availableLobbies = await queryTask;

		if (availableLobbies == null && mode > searchMode.close) {
			query = SteamMatchmaking.LobbyList.WithMaxResults(100).WithSlotsAvailable(1).FilterDistanceFar();
			queryTask = query.RequestAsync();
			availableLobbies = await queryTask;

			if (availableLobbies == null && mode > searchMode.mid) {
				query = SteamMatchmaking.LobbyList.WithMaxResults(100).WithSlotsAvailable(1).FilterDistanceWorldwide() ;
				queryTask = query.RequestAsync();
				availableLobbies = await queryTask;
			}
		}
		else if (availableLobbies != null) {
			Debug.Log("found " + availableLobbies.Length + " lobbies");
		}

		if (availableLobbies != null) {
			ChooseLobby(availableLobbies, mode);
		}
	}

	private void ChooseLobby(Steamworks.Data.Lobby[] availableLobbies, searchMode mode) {
		List<Steamworks.Data.Lobby> lobbiesList = availableLobbies.ToList();
		lobbiesList.RemoveAll(lobby => lobby.Id == myLobby.Id);
		lobbiesList.RemoveAll(l => !l.Id.IsValid);
		lobbiesList.RemoveAll(l => Steamworks.Data.NetPingLocation.TryParseFromString(l.GetData("hostPingLocation")) == null);
		lobbiesList.RemoveAll(l => l.GetData("applicationVersion") != Application.version);
		lobbiesList.RemoveAll(l => SteamNetworkingUtils.EstimatePingTo((Steamworks.Data.NetPingLocation)Steamworks.Data.NetPingLocation.TryParseFromString(l.GetData("hostPingLocation"))) > 
			(mode == searchMode.close || mode == searchMode.mid ? Constants.ping_fair : Constants.ping_bad));

		if (lobbiesList.Count > 0) {
			StopCoroutine(lobbySearchRoutine);
			lobbySearchRoutine = null;
			myLobby.SetPrivate();
			myLobby.SetJoinable(false);
			lobbyMenu.Leave();

			lobbiesList[0].Join();
			SwitchToLobbyView();
		}
	}
	#endregion

	#region Lobby Interaction
	public async Task<Steamworks.Data.Lobby[]> GetLobbies() {
        Steamworks.Data.LobbyQuery query = SteamMatchmaking.LobbyList.WithMaxResults(100).WithSlotsAvailable(1);
        Task<Steamworks.Data.Lobby[]> queryTask = query.RequestAsync();
		Steamworks.Data.Lobby[] availableLobbies = await queryTask;

		return availableLobbies;
    }

    public async void CreateLobby() {
        await SteamMatchmaking.CreateLobbyAsync(2);
    }

	public async Task<bool> CanJoinLobby(Steamworks.Data.Lobby lobby) {
		Steamworks.Data.Lobby[] availableLobbies = await GetLobbies();
		try {
			if (availableLobbies.ToList().FindIndex(l => l.Id == lobby.Id) != -1 && lobby.Id.IsValid) {
				return true;
			}
			return false;
		}
		catch {
			return false;
		}
	}

	public void DisplayNotification(string message) {
		notificationScript.DisplayMessage(message, messageActiveTime);
	}

    public async void JoinLobby(SteamId lobbyId) {
        await SteamMatchmaking.JoinLobbyAsync(lobbyId);
    }

    public void VoteRematch() {
		selfRematch = true;
		if (opponentRematch) {
			OnlineManager.Instance.SendPacket(new PO_Rematch(SteamClient.Name));
			DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.rematchCountdown][SettingsManager.Instance.language], 3));
			RematchCountdownCoroutine = StartCoroutine(RematchCountdown());
		}
		else {
			OnlineManager.Instance.SendPacket(new PO_Rematch(SteamClient.Name));
		}
    }

	public void handleRematchRequest(string friendName) {
		opponentRematch = true;

		if (selfRematch) {  // if we already sent a request, start the match
			DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.rematchCountdown][SettingsManager.Instance.language], 3));
			RematchCountdownCoroutine = StartCoroutine(RematchCountdown());
		}
		else { // if we have not
			DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.opponentRematchRequest][SettingsManager.Instance.language], friendName));
		}
	}

	public void InformReturnToLobby() {
		myLobby.SetMemberData("lobby", "true");
	}

    public void LeaveLobby() {
		Debug.Log("leaving a lobby with " + myLobby.MemberCount + " members");

		inLobby = false;

		myLobby.SetMemberData("name", "");
		myLobby.SetMemberData("godName", "Ra");
		myLobby.SetMemberData("ready", "false");

		SwitchToListView();
    }

	public void SetPublic(Steamworks.Data.Lobby l, GameObject callingButton) {
		l.SetPublic();
		callingButton.SetActive(false);
	}
	#endregion

	#region PostGame and Game End Functions
	public void StopRematchCountdown() {
		if (RematchCountdownCoroutine != null) {
			StopCoroutine(RematchCountdownCoroutine);
			if (notificationScript != null) {
				notificationScript.ForceTurnOff();
			}

			GameObject.Find("PostGameMenuController").GetComponent<PostGameMenuController>().EnableButtonsOnRematchCancel();
		}
	}

    private IEnumerator RematchCountdown() {
		if (lobbyMenu.isHost && lobbyMenu.GetLobby().GetData("mapName") == "Random") {
			int mapIndex = Random.Range(1, lobbyMenu.maps.Count);
			lobbyMenu.GetLobby().SetData("randomMapNumber", mapIndex.ToString());

			Debug.Log("Index: " + mapIndex);
			Debug.Log("Map name: " + lobbyMenu.maps[mapIndex].name);
		}
		float timer = 3f;

		GameObject.Find("PostGameMenuController").GetComponent<PostGameMenuController>().DisableButtonsOnRematchCountdown();

		while (timer > 0) {
            notificationScript.SetText(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.rematchCountdown][SettingsManager.Instance.language], Mathf.CeilToInt(timer).ToString()));

            yield return null;
            timer -= Time.deltaTime;
        }

		OnlineManager.Instance.SendPacket(new PO_Handshake(OnlineManager.Instance.localLocation.ToString()));

		GameObject.Find("PostGameMenuController").GetComponent<PostGameMenuController>().EnableButtonsOnRematchCancel();

		// account for random
		if (lobbyMenu.GetLobby().GetData("mapName") == "Random") {
			selectedMap = lobbyMenu.maps[int.Parse(lobbyMenu.GetLobby().GetData("randomMapNumber"))].mapCode;
		}

		SceneChangeManager.Instance.setNextSceneName(Constants.sceneNames[selectedMap]);
        SceneChangeManager.Instance.LoadNextScene();
    }

	private IEnumerator GoToMainMenuAfterDelay() {
		yield return new WaitForSeconds(gameCloseTime);

		SceneChangeManager.Instance.setNextSceneName(Constants.sceneNames[Constants.gameScenes.mainMenu]);
		SceneChangeManager.Instance.LoadNextScene();
	}

	private IEnumerator ExitGameAfterDelay(bool playerOne) {
        yield return new WaitForSeconds(gameCloseTime);

		GameManager.Instance.WinOnOpponentDisconnect(playerOne);
    }

	public void StartDisconnectSequence() {
		StartCoroutine(DisconnectAfterConnectionFailure());
	}

	private IEnumerator DisconnectAfterConnectionFailure() {
		DisplayNotification(string.Format(Lang.OnlineNotifications[Lang.onlineNotifications.connectionFailure][SettingsManager.Instance.language], 3));

		yield return new WaitForSeconds(gameCloseTime);

		InfluenceTileDictionary.NukeDictionary();

		menuScripts.MainMenu();
	}
	#endregion

	public void OpenInviteWindow(Steamworks.Data.Lobby _lobby) {
		SteamFriends.OpenGameInviteOverlay(_lobby.Id);
	}

	private IEnumerator UpdatePingLocation() {
		while (true) {
			if (OnlineManager.Instance != null) {
				OnlineManager.Instance.localLocation = SteamNetworkingUtils.LocalPingLocation;
				if (myLobby.Id.IsValid && myLobby.GetData("hostName") == SteamClient.Name) {
					myLobby.SetData("hostPingLocation", OnlineManager.Instance.localLocation.ToString());
				}
			}
			yield return new WaitForSeconds(5f);
		}
	}

	#region Getters and Setters
	public bool AmILobbyOwner(Steamworks.Data.Lobby lobby) {
		return lobby.Owner.Id == SteamClient.SteamId;
	}
	#endregion
}
#endif
