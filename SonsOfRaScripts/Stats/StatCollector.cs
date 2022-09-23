using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;


public class StatCollector : MonoBehaviour {
	// ---------------------- public variables -----------------------
	public static StatCollector Instance;

	// --------------------- private variables -----------------------
	// references
	private GameManager g;

	// misc. stats
	private float startTime;
	private float endTime;

	private List<StatRecording> recordings;
	private string filename;

	private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}

	// used to set up collector for a new game, called by GameManager
	public void initialize() {
		g = GameManager.Instance;

		if (recordings == null) {
			recordings = new List<StatRecording>();
		}
		recordings.Add(new StatRecording());

		startTime = Time.time;
	}

	public void FlushRecordings() {
		recordings.Clear();
	}

	public void saveRecording() {
		// moved these two out of initialize because initialize was getting called before players were set up
		for (int i = 0; i < recordings.Count; i++) {
			recordings[i].p1Patron = g.player1Controller.patron;
			recordings[i].p2Patron = g.player2Controller.patron;

			recordings[i].gameLength = endTime - startTime;

			// Create file path to stat saving location if it does not exist
			string directoryPath = Application.persistentDataPath.ToString() + Constants.statFilePath;
			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			}

			// create file name, then file path
			filename = System.DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss") + ".sorgame";
			string filepath = Path.Combine(directoryPath, filename);

			//File.Create(filepath);
			FileStream save = new FileStream(filepath, FileMode.Create);

			BinaryFormatter binForm = new BinaryFormatter();
			try {
				binForm.Serialize(save, recordings[i]);
			}
			catch (SerializationException e) {
				Debug.Log("Failed to serialize. Reason: " + e.Message);
				throw;
			}
			finally {
				save.Close();
			}
		}
	}

	#region Recording
	public void recordGoldEarned(string pID, float goldAmt) {
		if (pID == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1GoldEarned += goldAmt;
		}
		else {
			recordings[recordings.Count-1].p2GoldEarned += goldAmt;
		}
	}

	public void recordFavorEarned(string pID, float favorAmt) {
		if (pID == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1FavorEarned += favorAmt;
		}
		else {
			recordings[recordings.Count-1].p2FavorEarned += favorAmt;
		}
	}

	public void recordUnitsSpawned(string pID, int unitID) {
		if (unitID <= 3) {	// we're only recording the core 4 units
			if (pID == PlayerIDs.player1) {
				recordings[recordings.Count-1].p1UnitsSpawned++;
				recordings[recordings.Count-1].p1UnitTypesSpawned[unitID]++;
			}
			else {
				recordings[recordings.Count-1].p2UnitsSpawned++;
				recordings[recordings.Count-1].p2UnitTypesSpawned[unitID]++;
			}
		}
	}

	public void recordTowersSpawned(string pID, int towerID) {
		if (pID == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1TowersSpawned++;
			recordings[recordings.Count-1].p1TowerTypesSpawned[towerID]++;
		}
		else {
			recordings[recordings.Count-1].p2TowersSpawned++;
			recordings[recordings.Count-1].p2TowerTypesSpawned[towerID]++;
		}
	}

	public void recordUnitGold(string pID, float goldAmt) {
		if (pID == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1UnitGold += goldAmt;
		}
		else {
			recordings[recordings.Count-1].p2UnitGold += goldAmt;
		}
	}

	public void recordTowerGold(string pID, float goldAmt) {
		if (pID == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1TowerGold += goldAmt;
		}
		else {
			recordings[recordings.Count-1].p2TowerGold += goldAmt;
		}
	}

	public void recordBlessingUse(string pID, int blessingID) {
		if (pID == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1BlessingsUsed++;
			recordings[recordings.Count-1].p1BlessingUses[blessingID]++;
		}
		else {
			recordings[recordings.Count-1].p2BlessingsUsed++;
			recordings[recordings.Count-1].p2BlessingUses[blessingID]++;
		}
	}

	public void recordEndTime() {
		endTime = Time.time;
	}

	public void recordWinner(string playerKey) {
		if (playerKey == PlayerIDs.player1) {
			recordings[recordings.Count-1].winner = 0;
		}
		else {
			recordings[recordings.Count-1].winner = 1;
		}
	}

	public void recordExpansionSpawned(string playerKey, Constants.expansionType exp) {
		if (playerKey == PlayerIDs.player1) {
			recordings[recordings.Count-1].p1Expansions.Add(exp);
		}
		else {
			recordings[recordings.Count-1].p2Expansions.Add(exp);
		}
	}
	#endregion

	// ---------------- getters and setters -----------------
	public StatRecording GetRecording(int i) {
		return recordings[i];
	}

	public List<StatRecording> GetRecordings() {
		return recordings;
	}

    public StatRecording GetCurrentRecording() {
        return recordings[recordings.Count - 1];
    }
}
