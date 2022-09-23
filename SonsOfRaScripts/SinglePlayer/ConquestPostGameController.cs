using Rewired;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConquestPostGameController : MonoBehaviour
{
	private Player p;

	[Header("References")]
	public TMPro.TextMeshProUGUI EndStateText;
	public Image BackDrop;
	public TMPro.TextMeshProUGUI timerHeader;
	public GameObject timerDisplay;
	public Sprite victoryBackDrop;
	public Sprite defeatBackDrop;
	[SerializeField] private GameObject spinner;
	public UnityEngine.EventSystems.EventSystem myEventSystem;
	[SerializeField] private GameObject DefaultMenuOption;

	[Header("Leaderboard")]
	[SerializeField] private List<leaderboardEntry> entries;
	[SerializeField] private leaderboardEntry playerEntry;
	[SerializeField] private Color highlightColor;
	private Steamworks.Data.Leaderboard boardRef;
	private int currentPage;

	// constants
	const float SECONDS_IN_HOUR = 3600;
	const float SECONDS_IN_MINUTE = 60;

	// Start is called before the first frame update
	void Start()
    {
		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		currentPage = 1;

		HandleUIDisplay();
		HandleTimer();
		UpdateLeaderboardOnLoad(ArcadeManager.Instance.GetArcadeCompletionTime());
		HandleAchievements();
		HandleExperience();

#if !UNITY_XBOXONE
		if (!p.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
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
		if (p.GetButtonDown(RewiredConsts.Action.RBumperUI) && boardRef.EntryCount > currentPage * 5) {
		//if (p.GetButtonDown(RewiredConsts.Action.RBumperUI) && boardRef.EntryCount > currentPage) {
			currentPage += 1;
			UpdateLeaderboardUI(boardRef, currentPage);
		}
		else if (p.GetButtonDown(RewiredConsts.Action.RBumperUI) &&currentPage > 1) {
			currentPage -= 1;
			UpdateLeaderboardUI(boardRef, currentPage);
		}

		if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
			myEventSystem.currentSelectedGameObject.GetComponentInChildren<TMPButtonHandler>().buttonSelect();
		}
	}

	private void HandleUIDisplay() {
		if (SettingsManager.Instance.GetIsArcade()) {
			if (!ArcadeManager.Instance.failArcade) {
				EndStateText.SetText(Lang.MenuText[Lang.menuText.victory][SettingsManager.Instance.language]);
				BackDrop.sprite = victoryBackDrop;
			}
			else {
				EndStateText.SetText(Lang.MenuText[Lang.menuText.defeat][SettingsManager.Instance.language]);
				BackDrop.sprite = defeatBackDrop;
			}

			timerHeader.SetText(Lang.MenuText[Lang.menuText.yourTime][SettingsManager.Instance.language]);
			timerDisplay.SetActive(!ArcadeManager.Instance.failArcade);
			timerHeader.gameObject.SetActive(!ArcadeManager.Instance.failArcade);
		}
	}

	private void HandleTimer() {
		if (SettingsManager.Instance.GetIsArcade() && !ArcadeManager.Instance.failArcade) {
			timerDisplay.GetComponent<TMPro.TextMeshProUGUI>().SetText(GetFormattedTime(ArcadeManager.Instance.GetArcadeCompletionTime()));
		}
	}

	private string GetFormattedTime(float seconds) {
		int hours = 0;
		int minutes = 0;

		if (seconds > SECONDS_IN_HOUR) {
			hours = (int)Mathf.Floor(seconds / SECONDS_IN_HOUR);
			seconds -= hours * SECONDS_IN_HOUR;
		}
		if (seconds > SECONDS_IN_MINUTE) {
			minutes = (int)Mathf.Floor(seconds / SECONDS_IN_MINUTE);
			seconds -= minutes * SECONDS_IN_MINUTE;
		}

		return hours.ToString("0") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
	}

	private async void UpdateLeaderboardOnLoad(float time) {
		spinner.SetActive(true);
		Steamworks.Data.Leaderboard? timeLeaderBoard = await SteamUserStats.FindLeaderboardAsync("Arcade Leaderboard");

		if (timeLeaderBoard != null) {
			// add score
			boardRef = (Steamworks.Data.Leaderboard)timeLeaderBoard;
			if (!ArcadeManager.Instance.failArcade) {
				Debug.Log("submitting player time");
				await boardRef.SubmitScoreAsync((int)time);
			}

			// update UI for leaderboard
			UpdateLeaderboardUI(boardRef, currentPage);
		}
		else {
			foreach(leaderboardEntry e in entries) {
				e.gameObject.GetComponent<CanvasGroup>().alpha = 0;
			}
			spinner.SetActive(false);
		}
	}

	private async void UpdateLeaderboardUI(Steamworks.Data.Leaderboard board, int pageNumber) {
		Steamworks.Data.LeaderboardEntry[] fetchedEntries = await board.GetScoresAsync(5, pageNumber);
		Steamworks.Data.LeaderboardEntry[] fetchedPlayerEntry = await board.GetScoresAroundUserAsync(0, 0);

		spinner.SetActive(false);

		// fill out top 5
		for (int i = 0; i<entries.Count; i++) {
			if (i < fetchedEntries.Length) {
				entries[i].gameObject.GetComponent<CanvasGroup>().alpha = 1;

				entries[i].rank.SetText(fetchedEntries[i].GlobalRank.ToString() + ".");
				entries[i].name.SetText(fetchedEntries[i].User.Name);
				entries[i].time.SetText(GetFormattedTime(fetchedEntries[i].Score));

				entries[i].rank.color = fetchedEntries[i].User.Id == SteamClient.SteamId ? highlightColor : Color.white;
				entries[i].name.color = fetchedEntries[i].User.Id == SteamClient.SteamId ? highlightColor : Color.white;
				entries[i].time.color = fetchedEntries[i].User.Id == SteamClient.SteamId ? highlightColor : Color.white;
			}
			else {
				entries[i].gameObject.GetComponent<CanvasGroup>().alpha = 0;
			}
		}

		// fill out user position
		if (fetchedPlayerEntry != null) {
			playerEntry.gameObject.GetComponent<CanvasGroup>().alpha = 1;
			playerEntry.rank.SetText(fetchedPlayerEntry[0].GlobalRank.ToString() + ".");
			playerEntry.name.SetText(fetchedPlayerEntry[0].User.Name);
			playerEntry.time.SetText(GetFormattedTime(fetchedPlayerEntry[0].Score));
		}
		else {
			playerEntry.gameObject.GetComponent<CanvasGroup>().alpha = 0;
		}
	}

	private void HandleAchievements() {
		Debug.Log("hit achievement function");
		if (SettingsManager.Instance.GetIsArcade() && !ArcadeManager.Instance.failArcade) {
			Debug.Log("Attempting to unlock achievement");
			Patron p = ArcadeManager.Instance.GetPlayerPatron(PlayerIDs.player1);
			AchievementManager.Instance.CheckForArcadeCompletion(p);
		}
	}

	private void HandleExperience() {
		int experienceGained = 0;
		if (SettingsManager.Instance.GetIsArcade()) {
			if (!ArcadeManager.Instance.failArcade) {
				experienceGained += ContentManager.Instance.arcadeWinXP + (ArcadeManager.Instance.GetLives() * ContentManager.Instance.arcadeCompletionLifeBonus);
			}
			else {
				experienceGained = ContentManager.Instance.arcadeLosePerLevelXP * ArcadeManager.Instance.GetCurrentStage();
			}
		}

		ContentManager.Instance.GainExperience(experienceGained);
	}
}
