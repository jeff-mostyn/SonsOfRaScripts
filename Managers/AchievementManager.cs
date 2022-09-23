using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementManager : MonoBehaviour
{
	public static AchievementManager Instance;

	#region Single Game Progress Stats
	[Header("Single Game Progress Stats")]
	[SerializeField] private int battleHardenedMaxStacks;
	[SerializeField] private int mummySpawns;
	[SerializeField] private int protectionMaxStacks;
	private List<UnitAI> battleHardenedMaxStackUnits, protectionMaxStackUnits;
	[SerializeField] private int solarFlareKills;
	#endregion

	#region System Functions
	private void Awake() {
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this objects
			Instance = this;

			SceneManager.sceneLoaded += OnSceneLoaded;

			SonsOfRa.Events.GameEvents.BuffStackChange += CheckForStackMax;
			SonsOfRa.Events.GameEvents.KeepTakeDamage += CheckForKeepDamageAchievements;
			SonsOfRa.Events.GameEvents.MeterUnlock += CheckForAllUnlocks;
			SonsOfRa.Events.GameEvents.MeterUpgrade += CheckForAllUnlocks;
			SonsOfRa.Events.GameEvents.UnitSpawn += CheckForMummySpawn;
			SonsOfRa.Events.GameEvents.UnitDie += CheckForSolarFlareKills;

			SteamUserStats.OnAchievementProgress += ReceiveAchievementCallbacks;
		}
		else {
			Destroy(gameObject);
		}
	}

	void Start() {
		ResetSingleGameProgress();
    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		ResetSingleGameProgress();
	}

	private void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;

		SonsOfRa.Events.GameEvents.BuffStackChange -= CheckForStackMax;
		SonsOfRa.Events.GameEvents.KeepTakeDamage -= CheckForKeepDamageAchievements;
		SonsOfRa.Events.GameEvents.MeterUnlock -= CheckForAllUnlocks;
		SonsOfRa.Events.GameEvents.MeterUpgrade -= CheckForAllUnlocks;
		SonsOfRa.Events.GameEvents.UnitSpawn -= CheckForMummySpawn;
		SonsOfRa.Events.GameEvents.UnitDie -= CheckForSolarFlareKills;

		SteamUserStats.OnAchievementProgress -= ReceiveAchievementCallbacks;
	}
	#endregion

	private void ResetSingleGameProgress() {
		battleHardenedMaxStacks = 0;
		mummySpawns = 0;
		protectionMaxStacks = 0;
		solarFlareKills = 0;

		battleHardenedMaxStackUnits = new List<UnitAI>();
		protectionMaxStackUnits = new List<UnitAI>();
	}

	#region Event Receivers
	private void CheckForMummySpawn(UnitAI unit) {
		if (unit.type == Constants.unitType.mummy) {
			if (SettingsManager.Instance.GetIsSinglePlayer() && SettingsManager.Instance.GetDifficulty() == 2) {    // check for hard mode single player
				if (unit.ownerPlayerKey == PlayerIDs.player1) {
					mummySpawns++;
				}
			}
			else if (SettingsManager.Instance.GetIsOnline()) { // check for online
				if (OnlineManager.Instance.GetIsHost() && unit.ownerPlayerKey == PlayerIDs.player1) {
					mummySpawns++;
				}
				else if (!OnlineManager.Instance.GetIsHost() && unit.ownerPlayerKey == PlayerIDs.player2) {
					mummySpawns++;
				}
			}

			if (mummySpawns > Constants.Anubis_Mummy_Spawn_count && !IsAchievementUnlocked("Anubis_Mummy_Spawn")) {
				UnlockAchievement("Anubis_Mummy_Spawn");
			}
		}
	}

	private void CheckForStackMax(UnitAI unit, BuffDebuff buff, int currentStacks, int maxStacks) {
		if (currentStacks == maxStacks	// we've reached max stacks on this unit
			&& ((SettingsManager.Instance.GetIsSinglePlayer() && SettingsManager.Instance.GetDifficulty() == 2)	// single player and hard ai
				|| (SettingsManager.Instance.GetIsOnline()	// online
					&& (OnlineManager.Instance.GetIsHost() && unit.ownerPlayerKey == PlayerIDs.player1 || !OnlineManager.Instance.GetIsHost() && unit.ownerPlayerKey == PlayerIDs.player2)))) { // check which team the player controls

			// we'll clean the lists just in case
			battleHardenedMaxStackUnits.RemoveAll(u => u == null);
			protectionMaxStackUnits.RemoveAll(u => u == null);

			if (buff.type == BuffDebuff.BuffsAndDebuffs.battleHardened && !battleHardenedMaxStackUnits.Contains(unit)) {
				battleHardenedMaxStacks++;
				battleHardenedMaxStackUnits.Add(unit);
				if (battleHardenedMaxStacks > Constants.Sekhmet_BattleHardened_count && !IsAchievementUnlocked("Sekhmet_BattleHardened")) {
					UnlockAchievement("Sekhmet_BattleHardened");
				}
			}
			else if (buff.type == BuffDebuff.BuffsAndDebuffs.protection && !protectionMaxStackUnits.Contains(unit)) {
				protectionMaxStacks++;
				protectionMaxStackUnits.Add(unit);
				if (protectionMaxStacks > Constants.Isis_Block_Stacks_count && !IsAchievementUnlocked("Isis_Block_Stacks")) {
					UnlockAchievement("Isis_Block_Stacks");
				}
			}
		}
	}

	private void CheckForDefeatByOwnUnit(KeepManager keep, UnitAI unit = null) {
		if (keep.health <= 0 && unit != null) {
			bool dealtFinalDamageByOwnUnit = false;

			if (SettingsManager.Instance.GetIsSinglePlayer() && SettingsManager.Instance.GetDifficulty() == 2) {    // check for hard mode single player
				if (keep.rewiredPlayerKey == PlayerIDs.player2 && unit.ownerPlayerKey == PlayerIDs.player2) {
					dealtFinalDamageByOwnUnit = true;
				}
			}
			else if (SettingsManager.Instance.GetIsOnline()) { // check for online
				if (OnlineManager.Instance.GetIsHost() && keep.rewiredPlayerKey == PlayerIDs.player2 && unit.ownerPlayerKey == PlayerIDs.player2) {
					dealtFinalDamageByOwnUnit = true;
				}
				else if (!OnlineManager.Instance.GetIsHost() && keep.rewiredPlayerKey == PlayerIDs.player1 && unit.ownerPlayerKey == PlayerIDs.player1) {
					dealtFinalDamageByOwnUnit = true;
				}
			}

			if (dealtFinalDamageByOwnUnit && !IsAchievementUnlocked("Set_Betrayal_Win")) {
				UnlockAchievement("Set_Betrayal_Win");
			}
		}
	}

	private void CheckForKeepDamageAchievements(KeepManager keep, UnitAI unit = null) {
		// sudden death online achievement logic
		if (SettingsManager.Instance.GetIsOnline()
			&& keep.health <= 0
			&& GameManager.Instance.timer.IsSuddenDeath()
			&& ((OnlineManager.Instance.GetIsHost() && keep.rewiredPlayerKey == PlayerIDs.player2)
				|| (!OnlineManager.Instance.GetIsHost() && keep.rewiredPlayerKey == PlayerIDs.player1))
			&& !IsAchievementUnlocked("Sudden_Death_Online")) {
			UnlockAchievement("Sudden_Death_Online");
		}

		// defeat by own unit logic
		if (keep.health <= 0 && unit != null) {
			bool dealtFinalDamageByOwnUnit = false;

			if (SettingsManager.Instance.GetIsSinglePlayer() && SettingsManager.Instance.GetDifficulty() == 2) {    // check for hard mode single player
				if (keep.rewiredPlayerKey == PlayerIDs.player2 && unit.ownerPlayerKey == PlayerIDs.player2) {
					dealtFinalDamageByOwnUnit = true;
				}
			}
			else if (SettingsManager.Instance.GetIsOnline()) { // check for online
				if (OnlineManager.Instance.GetIsHost() && keep.rewiredPlayerKey == PlayerIDs.player2 && unit.ownerPlayerKey == PlayerIDs.player2) {
					dealtFinalDamageByOwnUnit = true;
				}
				else if (!OnlineManager.Instance.GetIsHost() && keep.rewiredPlayerKey == PlayerIDs.player1 && unit.ownerPlayerKey == PlayerIDs.player1) {
					dealtFinalDamageByOwnUnit = true;
				}
			}

			if (dealtFinalDamageByOwnUnit && !IsAchievementUnlocked("Set_Betrayal_Win")) {
				UnlockAchievement("Set_Betrayal_Win");
			}
		}
	}

	private void CheckForSolarFlareKills(UnitAI unit, string teamPlayerKey, Constants.damageSource source) {
		// Ra's only unit damaging blessing is Solar Flare, so if a unit on the other team against a Ra player dies by a blessing, they have to have died by solar flare
		if (SettingsManager.Instance.GetIsSinglePlayer() && SettingsManager.Instance.GetDifficulty() == 2) {
			if (GameManager.Instance.player1Controller.patron == Constants.patrons.Ra && teamPlayerKey == PlayerIDs.player2
				&& source == Constants.damageSource.blessing) {
				solarFlareKills++;
			}
		}
		else if (SettingsManager.Instance.GetIsOnline()
			&& ((GameManager.Instance.player1Controller.patron == Constants.patrons.Ra && teamPlayerKey == PlayerIDs.player2 && OnlineManager.Instance.GetIsHost())
				|| (GameManager.Instance.player2Controller.patron == Constants.patrons.Ra && teamPlayerKey == PlayerIDs.player1 && !OnlineManager.Instance.GetIsHost()))
			&& source == Constants.damageSource.blessing) {
			solarFlareKills++;
		}

		if (solarFlareKills > Constants.Ra_Solar_Flare_Kills_Count && !IsAchievementUnlocked("Ra_SolarFlare_Kills")) {
			UnlockAchievement("Ra_SolarFlare_Kills");
		}
	}

	private void CheckForAllUnlocks(PlayerController playerController, Meter meter) {
		if (meter.AllPurchased() 
			&& SettingsManager.Instance.GetIsOnline() 
			&& ((OnlineManager.Instance.GetIsHost() && playerController.rewiredPlayerKey == PlayerIDs.player1)
				|| (!OnlineManager.Instance.GetIsHost() && playerController.rewiredPlayerKey == PlayerIDs.player2))
			&& !IsAchievementUnlocked("All_Unlocks_Online")) {
			UnlockAchievement("All_Unlocks_Online");
		}
	}

	public void CheckForArcadeCompletion(Patron p) {
		if (!IsAchievementUnlocked(p.name + "_Arcade")) {
			UnlockAchievement(p.name + "_Arcade");
		}

		if (ArcadeManager.Instance != null && ArcadeManager.Instance.GetLives() == 3) {
			if (!IsAchievementUnlocked("Perfect_Arcade")) {
				UnlockAchievement("Perfect_Arcade");
			}
		}
	}

	private void ReceiveAchievementCallbacks(Steamworks.Data.Achievement ach, int currentProgress, int maxProgress) {
		Debug.Log(string.Format("Received progress on achievement \"{0}\"", ach.Name));
		if (currentProgress == 0 && maxProgress == 0) {
			Debug.Log(string.Format("Achievement with alias \"{0}\" has been unlocked on Steam", ach.Name));
		}
	}
	#endregion

	#region Steamworks Integration
	public bool IsAchievementUnlocked(string name) {
		try {
			Steamworks.Data.Achievement searchedAchievement = SteamWorksManager.Instance.achievements.First(ach => ach.Identifier == name);
			return searchedAchievement.State;
		}
		catch {
			Debug.LogWarning(string.Format("Achievement with alias \"{0}\" was not found", name));
			return false;
		}
	}

	private void UnlockAchievement(string name) {
		if (SettingsManager.Instance.AchievementsForReal) {
			try {
				Steamworks.Data.Achievement searchedAchievement = SteamWorksManager.Instance.achievements.First(ach => ach.Identifier == name);
				searchedAchievement.Trigger();

				Debug.Log(string.Format("Achievement with alias \"{0}\" has been unlocked locally", name));
				SteamUserStats.StoreStats();
			}
			catch {
				Debug.LogWarning(string.Format("Achievement with alias \"{0}\" was not found", name));
			}
		}
		else {
			Debug.Log(string.Format("Achievement with alias \"{0}\" would have been unlocked", name));
		}
	}
	#endregion
}
