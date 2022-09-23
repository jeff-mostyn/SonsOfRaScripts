using SonsOfRa.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContentManager : MonoBehaviour {
	public enum lockTypes { level, achievement, gamesWon, drop };
	public enum lockRarity { common, uncommon, rare };

	private string filename = "content.save";

	[System.Serializable]
	public struct itemLock {		
		public string inGameIdentifier;
		public string identifier;
		public bool locked;
		public lockTypes type;
		[Tooltip("Any numeric value associated with something being unlocked. Could be levels, games played, etc.")]
		public int number;
		public string achievementName;
		public lockRarity rarity;
	}

	public static ContentManager Instance;

	public List<itemLock> lockedContent;

	[Header("Progression")]
	public float xpMultiplier;
	public int baseLevelXP;
	public float levelXPIncreaseFactor;
	public int onlineWinXP, skirmishEasyWinXP, skirmishMediumWinXP, skirmishHardWinXP, localPlayXP, arcadeWinXP, onlinePlayXP, skirmishPlayXP, arcadeLosePerLevelXP;
	[Header("Bonus XP")]
	public int onlinePerfectXP;
	public int arcadeCompletionLifeBonus;

	[Header("Drops")]
	[SerializeField] private float uncommonChance;
	[SerializeField] private float rareChance;

	[Header("General UI")]
	[SerializeField] private CanvasGroup LevelCanvasGroup;
	[SerializeField] private CanvasGroup PlayerCanvasGroup;

	[Header("Level Up UI")]
	public Image levelBar;
	public float levelBarFillTime, levelBarNewLevelWaitTime;
	[SerializeField] private CanvasGroup notificationGroup;
	[SerializeField] private PlayerColorPalette paletteUnlock;
	[SerializeField] private GameObject nonPaletteUnlock;
	[SerializeField] private Image nonPaletteUnlockImage;
	[SerializeField] private TextMeshProUGUI LevelNumber, LevelTitle;
	bool waitingForBarFill;
	[SerializeField] private float fadeInTime, fadeOutTime, scaleUpTime, scaleWaitTime;
	[SerializeField] private TextMeshProUGUI currentLevelNum, nextLevelNum, XPNum;

	[Header("Player UI")]
	[SerializeField] private TextMeshProUGUI playerName;
	[SerializeField] private TextMeshProUGUI levelCounter;
	[SerializeField] private Image experienceBar;
	[SerializeField] private Image playerIcon;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string experienceGainEvent;
	FMOD.Studio.EventInstance fxState;

	#region System Functions
	private void Awake() {
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;

			SceneManager.sceneLoaded += OnSceneLoaded;

			LoadUnlocks();
		}
		else {
			Destroy(gameObject);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		StopAllCoroutines();
		notificationGroup.alpha = 0;

		if (scene.name == Constants.sceneNames[Constants.gameScenes.conquestPostGame] || scene.name == Constants.sceneNames[Constants.gameScenes.postGame]) {
			LevelCanvasGroup.alpha = 1f;
		}
		else {
			LevelCanvasGroup.alpha = 0f;
		}

		if (scene.name == Constants.sceneNames[Constants.gameScenes.mainMenu] && !SettingsManager.Instance.PlayIntro) {
			PlayerCanvasGroup.alpha = 1f;
		}
		else {
			PlayerCanvasGroup.alpha = 0f;
		}

		waitingForBarFill = true;
		paletteUnlock.gameObject.transform.localScale = Vector3.zero;
		nonPaletteUnlock.gameObject.transform.localScale = Vector3.zero;

		// stop XP gain sfx on changing scenes;
		FMOD.Studio.PLAYBACK_STATE state;
		fxState.getPlaybackState(out state);
		if (state != FMOD.Studio.PLAYBACK_STATE.STOPPED && state != FMOD.Studio.PLAYBACK_STATE.STOPPING) {
			fxState.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			fxState.release();
		}
	}

	private void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
	#endregion

	#region Unlocks
	/// <summary>
	/// Load a string list of unlocks from the local (and from cloud eventually). 
	/// </summary>
	public void LoadUnlocks() {
		for (int i = 0; i<lockedContent.Count; i++) {
			itemLock tempLock = lockedContent[i];
			tempLock.locked = true;
			lockedContent[i] = tempLock;
		}

		try {
			byte[] bytes = FileIO.Load(filename);
			using (MemoryStream stream = new MemoryStream(bytes)) {
				BinaryFormatter binRead = new BinaryFormatter();
				List<string> unlocks = (List<string>)binRead.Deserialize(stream);

				foreach (string s in unlocks) {
					UnlockItem(s, false);
				}

				SaveUnlocks();
			}
		}
		catch (Exception e) {
			Debug.LogWarning("No content file found!");
			SaveUnlocks();
		}
	}

	/// <summary>
	/// This will save a string list of unlocks to the local (and to cloud eventually).
	/// </summary>
	public void SaveUnlocks() {
		if (!SettingsManager.Instance.UnlockEverything) {
			Debug.Log("saving unlocks");
			List<string> unlocks = new List<string>();
			foreach (itemLock l in lockedContent) {
				if (!l.locked) {
					unlocks.Add(l.identifier);
				}
			}

			using (MemoryStream stream = new MemoryStream()) {
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, unlocks);
				byte[] bytes = stream.ToArray();

				FileIO.Save(bytes, filename, true);
			}
		}
	}

	/// <summary>
	/// See if a given item is locked
	/// </summary>
	/// <param name="identifier">The In-Game name for the item whose status is being checked</param>
	/// <returns>The locked/unlocked status of the requested item</returns>
	public bool isLocked(string identifier) {
		int index = lockedContent.FindIndex(x => x.inGameIdentifier == identifier);
		if (index == -1 || SettingsManager.Instance.UnlockEverything) {
			return false;	
		}
		else {
			return lockedContent[index].locked;
		}
	}

	/// <summary>
	/// Call when the player levels up. Check the content list for anything unlocked at or before that level;
	/// </summary>
	/// <param name="_level">The player's current level</param>
	public void CheckUnlocksOnLevelUp(int _level) {
		Debug.Log("checking for level unlocks at level " + _level);
		foreach (itemLock i in lockedContent) {
			if (i.type == lockTypes.level && i.number <= _level && i.locked) {
				UnlockItem(i.identifier, false);
			}
		}

		SaveUnlocks();
	}

	/// <summary>
	/// Call when the player finishes a game. Check the content list for anything unlocked at or before that game count;
	/// </summary>
	/// <param name="_level">The player's current level</param>
	public void CheckUnlocksOnGameWon(int _games) {
		foreach (itemLock i in lockedContent) {
			if (i.type == lockTypes.gamesWon && i.number <= _games && i.locked) {
				UnlockItem(i.identifier);
			}
		}
	}

	/// <summary>
	/// Call when the player earns an achievement to unlock any in-game items associated with it
	/// </summary>
	/// <param name="_name">The name of the unlocked achievement</param>
	public void CheckUnlocksOnAchievementEarn(string _name) {
		int index = lockedContent.FindIndex(x => x.type == lockTypes.achievement && x.achievementName == _name);
		if (index != -1 && lockedContent[index].locked) {
			UnlockItem(lockedContent[index].identifier);
		}
	}

	private void UnlockItem(string _identifier, bool save = true) {
		int index = lockedContent.FindIndex(x => x.identifier == _identifier);
		if (index != -1) {
			itemLock tempLock = lockedContent[index];
			tempLock.locked = false;
			lockedContent[index] = tempLock;

			if (save) {
				SaveUnlocks();
			}

			Debug.Log("You unlocked " + tempLock.inGameIdentifier + "!");
		}
	}

	public void CheckForAchievementUnlocks() {
		foreach (itemLock item in lockedContent) {
			if (item.type == lockTypes.achievement && item.locked && SteamWorksManager.Instance.achievements.Any(ach => ach.State && ach.Identifier == item.achievementName)) {
				UnlockItem(item.identifier, false);
			}
		}

		SaveUnlocks();
	}

	public string UnlockRandomDrop() {
		List<itemLock> drops = lockedContent.Where(item => item.locked && item.type == lockTypes.drop).ToList();
		List<itemLock> commonDrops = drops.Where(item => item.locked && item.type == lockTypes.drop && item.rarity == lockRarity.common).ToList();
		List<itemLock> uncommonDrops = drops.Where(item => item.locked && item.type == lockTypes.drop && item.rarity == lockRarity.uncommon).ToList();
		List<itemLock> rareDrops = drops.Where(item => item.locked && item.type == lockTypes.drop && item.rarity == lockRarity.rare).ToList();

		float roll = UnityEngine.Random.Range(0f, 1f);
		int itemRoll;
		itemLock unlockedItem;
		
		if (roll < rareChance && rareDrops.Count > 0) {
			unlockedItem = rareDrops[UnityEngine.Random.Range(0, rareDrops.Count())];
			UnlockItem(unlockedItem.identifier);
			return unlockedItem.inGameIdentifier;
		}
		else if (roll < rareChance + uncommonChance && uncommonDrops.Count > 0) {
			unlockedItem = uncommonDrops[UnityEngine.Random.Range(0, uncommonDrops.Count())];
			UnlockItem(unlockedItem.identifier);
			return unlockedItem.inGameIdentifier;
		}
		else if (commonDrops.Count > 0) {
			unlockedItem = commonDrops[UnityEngine.Random.Range(0, commonDrops.Count())];
			UnlockItem(unlockedItem.identifier);
			return unlockedItem.inGameIdentifier;
		}
		return "";
	}
	#endregion

	#region Experience and Leveling
	public int GetXPForCurrentLevel(int level) {
		int runningTotal = 0;
		for (int i = 2; i <= level; i++) {
			if (i == 2) {
				runningTotal += baseLevelXP;
			}
			else {
				runningTotal += level <= 51 ? (int)(baseLevelXP + (baseLevelXP * levelXPIncreaseFactor * (i-2))) : 350;
			}
		}

		return runningTotal;
	}

	public int GetXPForNextLevel(int level) {
		int XP = GetXPForCurrentLevel(level) + (level <= 51 ? (int)(baseLevelXP + (baseLevelXP * levelXPIncreaseFactor * (level - 1))) : 350);
		Debug.Log("XP needed for level " + (level + 1) + " is " + XP);
		return XP;
	}

	public int GetPlayGameXP() {
		if (SettingsManager.Instance.GetIsQuickplay() || SettingsManager.Instance.GetIsTutorial()) {
			return skirmishPlayXP;
		}
		else if (SettingsManager.Instance.GetIsOnline()) {
			return onlinePlayXP;
		}
		else if (SettingsManager.Instance.GetIsLocalMulti()) {
			return localPlayXP;
		}
		else {
			return 0;
		}
	}

	public int GetWinGameXP() {
		if (SettingsManager.Instance.GetIsQuickplay()) {
			if (SettingsManager.Instance.GetDifficulty() == 0) {
				return skirmishEasyWinXP;
			}
			else if (SettingsManager.Instance.GetDifficulty() == 1) {
				return skirmishMediumWinXP;
			}
			else {
				return skirmishHardWinXP;
			}
		}
		else if (SettingsManager.Instance.GetIsOnline()) {
			return onlineWinXP;
		}
		else {
			return 0;
		}
	}

	public void LevelUp(int level) {
		Debug.Log("registering a level up");
		string unlock = UnlockRandomDrop();
		Debug.Log("unlocked " + unlock);
		StartCoroutine(DisplayLevelUpUI(level, unlock));
		CheckUnlocksOnLevelUp(level);
	}
	#endregion

	#region XP and Levels UI
	public void GainExperience(int experienceGained) {
		experienceGained = (int)(experienceGained * xpMultiplier);

		// HERE IS WHERE THE EXP THING WILL HAPPEN
		// get current xp/level
		int currentXp = Steamworks.SteamUserStats.GetStatInt("experience");
		int currentLevel = Steamworks.SteamUserStats.GetStatInt("level");
		int nextLevelXP = GetXPForNextLevel(currentLevel);

		currentLevelNum.SetText(currentLevel.ToString());
		nextLevelNum.SetText((currentLevel + 1).ToString());
		XPNum.SetText(currentXp.ToString() + " | " + nextLevelXP.ToString());

		// for the bar
		// subtract the previous level's xp from current to get how much we have towards next level
		int currentLevelXPForBar = currentXp - GetXPForCurrentLevel(currentLevel);
		Debug.Log("XP acquired for level " + (currentLevel + 1) + " before game is: " + currentLevelXPForBar);

		// subtract current level's xp from next level's xp to get how much we still need for next level
		int totalXPNeededForNextLevel = GetXPForNextLevel(currentLevel) - GetXPForCurrentLevel(currentLevel);
		Debug.Log("Total XP needed for level " + (currentLevel + 1) + " is: " + totalXPNeededForNextLevel);
		
		// figure out current percentage, set bar fill to that
		float percentOfNextLevel = totalXPNeededForNextLevel == 0 ? (float)currentLevelXPForBar / totalXPNeededForNextLevel : 1;
		levelBar.fillAmount = percentOfNextLevel;

		// add expierience gained to current xp
		int newXP = currentXp + experienceGained;
		Debug.Log("XP acquired for level " + (currentLevel + 1) + " after game is: " + newXP + " out of " + GetXPForNextLevel(currentLevel) + " needed.");

		Steamworks.SteamUserStats.SetStat("experience", newXP);

		// bar fill-up
		StartCoroutine(FillExperienceBar(currentLevel, currentXp, newXP));
		// find percentage of new current xp vs next level req
		// call coroutine to fill bar to the new percent.
		// that coroutine will roll over to the next level, and fill past.

		// send that data to the stats in steamworks
		while (newXP > GetXPForNextLevel(currentLevel)) {
			currentLevel++;
		}
		Steamworks.SteamUserStats.SetStat("level", currentLevel);

		Steamworks.SteamUserStats.StoreStats();
		Steamworks.SteamUserStats.RequestCurrentStats();
	}

	public void ResetXP() {
		Steamworks.SteamUserStats.ResetAll(true); // true = wipe achivements too
		Steamworks.SteamUserStats.StoreStats();
		Steamworks.SteamUserStats.RequestCurrentStats();
	}

	private IEnumerator FillExperienceBar(int level, float currentExperience, float newExperienceTotal) {
		float timer = 0f;
		float startingExperience = currentExperience;
		float fill = levelBar.fillAmount;

		float currentLevelXPForBar;
		float XPNeededForNextLevel;

		// start audio
		fxState = FMODUnity.RuntimeManager.CreateInstance(experienceGainEvent);
		fxState.start();

		while (currentExperience < newExperienceTotal) {
			// lerp value
			currentExperience = Mathf.Lerp(startingExperience, newExperienceTotal, timer / levelBarFillTime);

			XPNum.SetText(((int)currentExperience).ToString() + " | " + GetXPForNextLevel(level).ToString());

			// calculate bar percent
			currentLevelXPForBar = currentExperience - GetXPForCurrentLevel(level);
			XPNeededForNextLevel = GetXPForNextLevel(level) - GetXPForCurrentLevel(level);
			fill = Mathf.Min(currentLevelXPForBar / XPNeededForNextLevel, 1f);

			fxState.setParameterByName("level", currentLevelXPForBar / XPNeededForNextLevel);

			// handle level up
			if (fill >= 1f) {
				level++;
				waitingForBarFill = false;

				// stop level gain sound and play meter fill
				fxState.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				fxState.release();
				SoundManager.Instance.sound_MeterFill();

				yield return new WaitForSeconds(levelBarNewLevelWaitTime);

				currentLevelNum.SetText(level.ToString());
				nextLevelNum.SetText((level + 1).ToString());

				// start level gain sound again
				fxState = FMODUnity.RuntimeManager.CreateInstance(experienceGainEvent);
				fxState.start();

				fill = 0f;
			}

			levelBar.fillAmount = fill;

			timer += Time.deltaTime;
			yield return null;
		}

		// clean it up, set it at final value
		currentLevelXPForBar = newExperienceTotal - GetXPForCurrentLevel(level);
		XPNeededForNextLevel = GetXPForNextLevel(level) - GetXPForCurrentLevel(level);
		fill = Mathf.Min(currentLevelXPForBar / XPNeededForNextLevel, 1f);

		// stop audio
		fxState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		fxState.release();

		yield return null;
	}

	public void RecordGamesPlayed() {
		int gamesWon = Steamworks.SteamUserStats.GetStatInt("onlineMatchesWon");
		statDisplay stats = GameObject.Find("StatDisplay").GetComponent<statDisplay>();
		string godNameString = "_" + stats.GetPlayerGodName(OnlineManager.Instance.fakePlayerKey);

		bool wongame = stats.GetWinner() == PlayerIDs.player1 && OnlineManager.Instance.GetIsHost() || stats.GetWinner() == PlayerIDs.player2 && !OnlineManager.Instance.GetIsHost();

		if (wongame) {
			gamesWon++;
			Steamworks.SteamUserStats.SetStat("onlineMatchesWon", gamesWon);
			Steamworks.SteamUserStats.AddStat("matchesWon" + godNameString, 1);
		}
		else {
			Steamworks.SteamUserStats.AddStat("onlineMatchesLost", 1);
			Steamworks.SteamUserStats.AddStat("matchesLost" + godNameString, 1);
		}

		Steamworks.SteamUserStats.StoreStats();
		Steamworks.SteamUserStats.RequestCurrentStats();
	}

	private IEnumerator DisplayLevelUpUI(int level, string unlockedItemName) {
		LevelTitle.SetText(Lang.OnlineText[Lang.onlineText.playerLevel][SettingsManager.Instance.language]);
		LevelNumber.SetText(level.ToString());
		GameObject scalingObject = null;

		if (unlockedItemName != "") {
			if (unlockedItemName.ToLower().Contains("palette")) {
				nonPaletteUnlock.SetActive(false);
				paletteUnlock.gameObject.SetActive(true);
				PlayerColorPalette unlockedPalette;
				scalingObject = paletteUnlock.gameObject;

				unlockedPalette = unlockedItemName.Contains("1") ? CustomizationManager.Instance.colorPalettes1.FirstOrDefault(x => x.name == unlockedItemName)
					: CustomizationManager.Instance.colorPalettes2.FirstOrDefault(x => x.name == unlockedItemName);

				if (unlockedPalette) {
					paletteUnlock.AssignColorPalette(unlockedPalette);
				}
			}
			else {
				Sprite itemSprite = null;
				nonPaletteUnlock.SetActive(true);
				paletteUnlock.gameObject.SetActive(false);
				scalingObject = nonPaletteUnlock;

				if (unlockedItemName.ToLower().Contains("cos")) {
					itemSprite = CustomizationManager.Instance.cosmetics.FirstOrDefault(x => x.name == unlockedItemName).GetComponent<keepCosmetic>().thumbnail;
				}
				else {
					itemSprite = CustomizationManager.Instance.portaits.FirstOrDefault(x => x.name == unlockedItemName);
				}

				nonPaletteUnlockImage.sprite = itemSprite;
			}
		}

		yield return new WaitUntil(() => !waitingForBarFill);

		StartCoroutine(FadeCanvas(false, notificationGroup));

		yield return new WaitForSeconds(scaleWaitTime);

		if (unlockedItemName != "") {
			StartCoroutine(ScaleUp(scalingObject));
		}

		yield return new WaitForSeconds(levelBarNewLevelWaitTime - scaleWaitTime - fadeOutTime);

		StartCoroutine(FadeCanvas(true, notificationGroup));
	}

	IEnumerator FadeCanvas(bool fadeOut, CanvasGroup cg) {
		float elapsedTime = 0;
		float time = fadeOut ? fadeOutTime : fadeInTime;
		while (elapsedTime < time) {
			if (fadeOut) {
				cg.alpha = Mathf.Lerp(1, 0, (elapsedTime / time));
			}
			else {
				cg.alpha = Mathf.Lerp(0, 1, (elapsedTime / time));
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		cg.alpha = fadeOut ? 0f : 1f;
		yield return null;
	}

	IEnumerator ScaleUp(GameObject g) {
		float elapsedTime = 0;
		float scale = 0;
		while (elapsedTime < scaleUpTime) {
			scale = Mathf.Lerp(0f, 1f, (elapsedTime / scaleUpTime));
			g.transform.localScale = new Vector3(scale, scale, scale);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		g.transform.localScale = Vector3.one;
		yield return null;
	}
	#endregion

	#region PlayerUI
	public void SetUpPlayerUI() {
		playerName.SetText(SteamWorksManager.Instance.GetUsername());
		levelCounter.SetText(SteamWorksManager.Instance.GetLevel().ToString());

		int currentXP = SteamWorksManager.Instance.GetExperience();
		float nextLevelXP = GetXPForNextLevel(SteamWorksManager.Instance.GetLevel());
		float currentLevelXP = GetXPForCurrentLevel(SteamWorksManager.Instance.GetLevel());
		experienceBar.fillAmount = Mathf.Max(0f, (currentXP - currentLevelXP) / (nextLevelXP - currentLevelXP));

		SetPlayerIcon(CustomizationManager.Instance.playerPortrait);
	}

	public void SetPlayerIcon(Sprite icon) {
		playerIcon.sprite = icon;
	}

	public void SetPlayerUIAlpha(float alpha) {
		PlayerCanvasGroup.alpha = alpha;
	}
	#endregion
}
