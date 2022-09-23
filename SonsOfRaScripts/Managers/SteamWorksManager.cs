#if !UNITY_XBOXONE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamWorksManager : MonoBehaviour {
	const int SonsOfRaAppID = 1134020;
	public enum appIds { mainGame = 1134020, demo = 1267420, beta = 1280710 };

	public static SteamWorksManager Instance;

	[Header("Online Settings")]
	[SerializeField] public bool PlayOnline;
	[SerializeField] public bool LaunchThroughSteam;
	public appIds appIdForBuild;

	private bool active;

	public bool isJoiningLobby;
	private Steamworks.Data.Lobby pendingLobby;

	public bool isJoiningFromClose;
	private SteamId pendingLobbyId;

	MenuScripts menuScripts;
	//private Coroutine joinLobbyCoroutine;

	[Header("Progression Stuff")]
	[SerializeField] private int serverLevel = int.MaxValue;
	[SerializeField] private int serverExperience = int.MaxValue;
	[SerializeField] private int serverGamesWon = int.MaxValue;
	public IEnumerable<Steamworks.Data.Achievement> achievements;

	#region Event Handlers
	void OnGameLobbyJoinRequested_handler(Steamworks.Data.Lobby lobby, SteamId id) {
		// join lobby right away if in lobby menu
		if (SceneManager.GetActiveScene().name == Constants.sceneNames[Constants.gameScenes.lobby]) {
			if (LobbyManager.Instance.inLobby && LobbyManager.Instance.myLobby.Id != lobby.Id) {
				LobbyManager.Instance.myLobby.Leave();
				LobbyManager.Instance.LeaveLobby();
			}

			lobby.Join();
		}
		else if (SettingsManager.Instance.GetIsOnline()) {   // if lobby manager is active, we are either in a game or in post game online
			pendingLobby = lobby;

			// we will need a confirmation in here

			ConfirmExit();	// ultimately we'll get redirected so it doesnt matter where we go, but this prevents a double redirect
		}
		else if (SettingsManager.Instance.GetIsConquest() 
			&& SceneManager.GetActiveScene().name != Constants.sceneNames[Constants.gameScenes.patronSelectConquest]
			&& SceneManager.GetActiveScene().name != Constants.sceneNames[Constants.gameScenes.conquestPostGame]) {    // we're in conquest, need to prompt about losing unsaved progress
			pendingLobby = lobby;

			ConfirmExit();	// need to replace this with confirmation prompt
		}
		else {  // quickplay, local play, or menus, no worries just go, but need to handle scene transition
			isJoiningLobby = true;
			pendingLobby = lobby;
			menuScripts.GoToLobbyMenu();
		}
	}

	void OnGameOverlayActivated_handler(bool active) {
		if (active) {
			Debug.Log("Steam Overlay has been activated");
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("paused", 1);
			if (!SettingsManager.Instance.GetIsOnline()) {
				Time.timeScale = 0;
			}
		}
		else {
			Debug.Log("Steam Overlay has been deactivated");
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("paused", 0);
			Time.timeScale = 1;
		}
	}

	void OnUserStatsReceived_handler(SteamId steamid, Result result) {
		//Debug.Log("user stats have been received");

		int newLevel = SteamUserStats.GetStatInt("level");
		if (newLevel > serverLevel) {
			ContentManager.Instance.LevelUp(newLevel);
		}

		if (serverLevel == int.MaxValue || newLevel > serverLevel) {
			Debug.Log("current level is " + serverLevel + " and new level is " + newLevel);
			serverLevel = newLevel;
		}
		serverExperience = SteamUserStats.GetStatInt("experience");
		serverGamesWon = SteamUserStats.GetStatInt("onlineMatchesWon");
		achievements = SteamUserStats.Achievements;

		ContentManager.Instance.SetUpPlayerUI();
		ContentManager.Instance.CheckUnlocksOnLevelUp(serverLevel);
		ContentManager.Instance.CheckUnlocksOnGameWon(serverGamesWon);
		ContentManager.Instance.CheckForAchievementUnlocks();
	}

	void OnUserStatsStored_handler(Result result) {
		Debug.Log("User stats have been stored to Steam");
		SteamUserStats.RequestCurrentStats();
	}

	void OnUserStatsUnloaded_handler(SteamId steamid) {
		Debug.Log("user stats unloaded, requesting again");

		SteamUserStats.RequestCurrentStats();
	}
	#endregion

	#region System Functions
	private void Awake() {
		// There can only be one
		active = false;
		if (Instance == null && PlayOnline) {
			DontDestroyOnLoad(gameObject);
			Instance = this;
			isJoiningLobby = false;

			SceneManager.sceneLoaded += OnSceneLoaded;

			// on first run, initialize Steam connection
			if (LaunchThroughSteam) {
				try {
					if (SteamClient.RestartAppIfNecessary((AppId)1134020)) {
						Application.Quit();
						return;
					}
				}
				catch (System.DllNotFoundException e) {
					Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

					Application.Quit();
					return;
				}
			}
			else {
				try {
					//SteamClient.Init(SonsOfRaAppID);
					SteamClient.Init((uint)appIdForBuild);

					SteamUserStats.OnAchievementProgress += UpdateAchievements;
					SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested_handler;
					SteamFriends.OnGameOverlayActivated += OnGameOverlayActivated_handler;

					SteamUserStats.OnUserStatsReceived += OnUserStatsReceived_handler;
					SteamUserStats.OnUserStatsStored += OnUserStatsStored_handler;
					SteamUserStats.OnUserStatsUnloaded += OnUserStatsUnloaded_handler;

					SteamUserStats.RequestCurrentStats();
					active = true;
				}
				catch (System.Exception e) {
					Debug.LogError(e.ToString());
				}
			}
		}
		else {
			Destroy(gameObject);
		}
	}

	void Start() {
		Debug.Log(SteamClient.Name);

		menuScripts = GetComponent<MenuScripts>();
	}

	void Update() {
		SteamClient.RunCallbacks();
	}

	private void OnDisable() {
		if (active) {
			SteamClient.Shutdown();
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Debug.Log("Ping Location: " + SteamNetworkingUtils.LocalPingLocation.ToString());
		if (isJoiningLobby) {
			if (LobbyManager.Instance.gameObject && LobbyManager.Instance.inLobby) { // we were in an online match, leave the lobby. This is here to make sure we dont leave in the middle of an online match, but in the next scene in flow
				LobbyManager.Instance.myLobby.Leave();
				LobbyManager.Instance.LeaveLobby();
			}

			if (scene.name == Constants.sceneNames[Constants.gameScenes.lobby]) { // we were in a local non online flow and went straight to the lobby scene
				pendingLobby.Join();
				isJoiningLobby = false;
			}
			else if (scene.name != Constants.sceneNames[Constants.gameScenes.load]) { // send us to the lobby menu, fire the other conditional in this function
				menuScripts.GoToLobbyMenu();    
			}
		}
		else if (isJoiningFromClose && scene.name == Constants.sceneNames[Constants.gameScenes.lobby]) {
			try {
				LobbyManager.Instance.JoinLobby(pendingLobbyId);
			}
			catch {
				Debug.Log("Error joining lobby async");
			}
			isJoiningFromClose = false;
		}
	}
	#endregion

	#region Achievements
	private void UpdateAchievements(Steamworks.Data.Achievement ach, int currentProgress, int progress) {
		// An achievement has been unlocked
		if (ach.State || (currentProgress == 0 && progress == 0)) {
			Debug.Log($"{ach.Name} WAS UNLOCKED!");
			// unlock any content associated with the achievement
			ContentManager.Instance.CheckUnlocksOnAchievementEarn(ach.Identifier);
		}
	}

	private IEnumerator TrySendStats() {
		bool sendSuccess = false;
		int attempts = 0;
		while (!sendSuccess && attempts < 10) {
			sendSuccess = SteamUserStats.StoreStats();
			yield return new WaitForSeconds(10);
			Debug.Log(sendSuccess ? "Stat storage SUCCEEDED" : "Stat storage FAILED");
		}
	}
    #endregion

    #region Accessors
    public string GetUsername() {
        return SteamClient.Name;
    }

	public int GetLevel() {
		return serverLevel;
	}

	public int GetExperience() {
		return serverExperience;
	}

	public bool GetIsOnline() {
		return active;
	}
	#endregion

	#region Handling Invites
	private void DisplayExitConfirmation(bool turnOn) {
		
	}

	public void ConfirmExit() {
		isJoiningLobby = true;
		menuScripts.GoToLobbyMenu();
	}

	public void DenyExit() {
		isJoiningLobby = false;
		DisplayExitConfirmation(false);
	}

	public void ParseCommandLineLobbyInvite(string arg) {
		ulong lobbyIdNumber = ulong.Parse(arg);
		pendingLobbyId = lobbyIdNumber;
		isJoiningFromClose = true;

		menuScripts.GoToLobbyMenu();
	}
	#endregion
}
#endif
