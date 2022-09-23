using SonsOfRa.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour {
	#region Declarations
	public enum gamemodes { conquest, quickplay, localMulti, tutorial, online, arcade };
	public enum targetFrameRates { Uncapped = -1, Monitor = 0, Thirty = 30, Sixty = 60, OneTwenty = 120, OneFortyFour = 144 };

	[SerializeField] private gamemodes selectedGameMode;

	public static SettingsManager Instance;

	public bool PlayOnline = true;
	public bool LaunchThroughSteam = false;
	public bool DemoMode = false;
	[SerializeField] private bool ExpoMode = false;
	public bool RecordingMode = false;
	public bool PlayIntro = true;
	public bool UnlockEverything = false;
	public bool AchievementsForReal = false;
	public Lang.language language;
	private bool fileLoaded;

	#region Global Game Options
	private FullScreenMode fsMode;
	private float musicVolume;
	private float fxVolume;
	[SerializeField]
	private int resolutionx, resolutiony;
	private int qualitySetting;
	private targetFrameRates frameRate;

	// file save
	private SettingsRecording recording;
	private string filename = "player.prefs";
	private int keyboardUserIndex;
	#endregion

	#region Custom Game Options
	// default values
	private bool isSinglePlayer = true;
	private bool defaultUseTimer = true;
	private int defaultRoundTimeMinutes = 7;
	private int defaultKeepHealth = 8;
	private bool defaultMatchPlay = false;
	private int defaultBestOf = 3;

	// editable values
	private bool useTimer = true;
	private int matchLengthMinutes = 7;
	private int keepHealth = 8;
	private bool matchPlay = false;
	private int bestOf = 3;

	private int quickDiffLevel = 1;
	#endregion

	#region Mins, Maxes, Values
	// Mins and Maxes
	private int minMatchLength = 1;
	private int maxMatchLength = 15;
	private int minKeepHealth = 1;
	private int maxKeepHealth = 20;
	private int minBestOf = 3;
	private int maxBestOf = 9;
	private float minVolume = 0f;
	private float maxVolume = 200f;
	#endregion
	#endregion

	private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this objects
			Instance = this;

			SceneManager.sceneLoaded += OnSceneLoaded;

			fileLoaded = false;
		}
		else {
			Destroy(gameObject);
		}
	}

	private void Start() {
		LoadPlayerPreferences();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Application.runInBackground = GetIsOnline();
	}

	public void LoadPlayerPreferences() {
		try {
			byte[] bytes = FileIO.Load(filename);
			using (MemoryStream stream = new MemoryStream(bytes)) {
				BinaryFormatter binRead = new BinaryFormatter();
				recording = (SettingsRecording)binRead.Deserialize(stream);
			}
		}
		catch (Exception e) {
			Debug.LogWarning("No preferences file found!");
			recording = new SettingsRecording();
			recording.Initialize(Screen.currentResolution.width, Screen.currentResolution.height, fsMode, Constants.apiLanguageMapping.ContainsKey(Steamworks.SteamApps.GameLanguage) ? Constants.apiLanguageMapping[Steamworks.SteamApps.GameLanguage] : Lang.language.English);
		}

		musicVolume = recording.musicVolume;
		fxVolume = recording.fxVolume;

		FMOD.Studio.Bus musicBus;
		musicBus = FMODUnity.RuntimeManager.GetBus("Bus:/Music");
		musicBus.setVolume(SettingsManager.Instance.GetMusicVolumeFloat());

		FMOD.Studio.Bus sfxBus;
		sfxBus = FMODUnity.RuntimeManager.GetBus("Bus:/SFX");
		sfxBus.setVolume(SettingsManager.Instance.GetFXVolumeFloat());

		fsMode = recording.fsMode;
		language = (Lang.language)recording.language;

		resolutionx = recording.resX;
		resolutiony = recording.resY;
		if (fsMode == FullScreenMode.Windowed) {
			Screen.SetResolution(recording.resX, recording.resY, false);
		}
		else {
			Screen.SetResolution(recording.resX, recording.resY, true);
		}

		qualitySetting = recording.qualitySetting;
		QualitySettings.SetQualityLevel(qualitySetting);

		frameRate = (targetFrameRates)recording.frameRate;
		Application.targetFrameRate = frameRate == 0 ? Screen.currentResolution.refreshRate : (int)frameRate;

		fileLoaded = true;
	}

	public void SavePlayerPreferences() {
		bool changingLanguage = false;

		recording.musicVolume = musicVolume;
		recording.fxVolume = fxVolume;

		if (recording.language != (int)language) {
			changingLanguage = true;
		}
		recording.language = (int)language;

#if !UNITY_XBOXONE
		recording.fsMode = fsMode;
		recording.resX = resolutionx;
		recording.resY = resolutiony;
		recording.qualitySetting = qualitySetting;
		recording.frameRate = (int)frameRate;

		Camera.main.ResetAspect();
		//if (resolutionx/resolutiony >= 1.7) Camera.main.aspect = 1.7f;
#endif

		using (MemoryStream stream = new MemoryStream()) {
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, recording);
			byte[] bytes = stream.ToArray();

			FileIO.Save(bytes, filename, true);
		}

		if (changingLanguage) {
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	public string GetFilePath(string _filename) {
		// Create file path to stat saving location if it does not exist
		string directoryPath = Application.persistentDataPath.ToString() + Constants.settingsFilePath;
		if (!Directory.Exists(directoryPath)) {
			Directory.CreateDirectory(directoryPath);
		}

		// create file name, then file path
		return Path.Combine(directoryPath, _filename);
	}

	public void ResetCustomGameSettings() {
		useTimer = defaultUseTimer;
		matchLengthMinutes = defaultRoundTimeMinutes;
		keepHealth = defaultKeepHealth;
		matchPlay = defaultMatchPlay;
		bestOf = defaultBestOf;
	}

	#region Getters and Setters
	public bool GetUseTimer() {
		return useTimer;
	}

	public bool GetDefaultUseTimer() {
		return defaultUseTimer;
	}

	public void SetUseTimer(bool _tf) {
		useTimer = _tf;
	}

	public int GetRoundTimeMinutes() {
		return matchLengthMinutes;
	}

	public int GetDefaultRoundTimeMinutes() {
		return defaultRoundTimeMinutes;
	}

	public void SetRoundTimeMinutes(int min) {
		matchLengthMinutes = min;
	}

	public int GetKeepHealth() {
		return keepHealth;
	}

	public int GetDefaultKeepHealth() {
		return defaultKeepHealth;
	}

	public void SetKeepHealth(int health) {
		keepHealth = health;
	}

	public bool GetMatchPlay() {
		return matchPlay;
	}

	public bool GetDefaultMatchPlay() {
		return defaultMatchPlay;
	}

	public void SetMatchPlay(bool _matchPlay) {
		matchPlay = _matchPlay;
	}

	public int GetBestOf() {
		return bestOf;
	}

	public int GetDefaultBestOf() {
		return defaultBestOf;
	}

	public void SetBestOf(int _bestOf) {
		bestOf = _bestOf;
	}

	public bool GetIsSinglePlayer() {
		return (selectedGameMode == gamemodes.quickplay || selectedGameMode == gamemodes.arcade || selectedGameMode == gamemodes.tutorial);
	}

	public bool GetIsConquest() {
		return selectedGameMode == gamemodes.conquest;
	}

	public bool GetIsArcade() {
		return selectedGameMode == gamemodes.arcade;
	}

	public bool GetIsQuickplay() {
		return selectedGameMode == gamemodes.quickplay;
	}

	public bool GetIsLocalMulti() {
		return selectedGameMode == gamemodes.localMulti;
	}

	public bool GetIsOnline() {
		return selectedGameMode == gamemodes.online;
	}

	public bool GetIsTutorial() {
		return selectedGameMode == gamemodes.tutorial;
	}

	public void SetGameMode(gamemodes mode) {
		selectedGameMode = mode;
	}

	public void SetDifficulty(int level) {
		quickDiffLevel = level;
	}

	public int GetDifficulty() {
		return quickDiffLevel;
	}
	#endregion

	#region Global Setting Getters and Setters
	public bool getIsExpoMode() {
		return ExpoMode;
	}

	public bool getIsDemoMode() {
		return DemoMode;
	}

	public float GetMusicVolume() {
		return musicVolume;
	}

	public float GetMusicVolumeFloat() {
		return musicVolume / 100f;
	}

	public void SetMusicVolume(float _musicVolume) {
		musicVolume = _musicVolume;
	}

	public float GetFXVolume() {
		return fxVolume;
	}

	public float GetFXVolumeFloat() {
		return fxVolume / 100f;
	}

	public void SetFXVolume(float _fxVolume) {
		fxVolume = _fxVolume;
	}

	public int GetResolution(int i) {
		return i == 0 ? resolutionx : resolutiony;
	}

	public void SetResolution(int _resx, int _resy) {
		resolutionx = _resx;
		resolutiony = _resy;
	}

	public int GetQualityLevel() {
		return qualitySetting;
	}

	public void SetQualityLevel(int _quality) {
		qualitySetting = _quality;
	}

	public FullScreenMode GetFullscreen() {
		return fsMode;
	}

	public void SetFullScreen(FullScreenMode f) {
		fsMode = f;
	}

	public targetFrameRates GetFrameRate() {
		return frameRate;
	}

	public void SetFrameRate(int f) {
		frameRate = (targetFrameRates)f;
	}
	#endregion

	#region Get Mins and Maxes
	public int GetMinMatchLength() {
		return minMatchLength;
	}

	public int GetMaxMatchLength() {
		return maxMatchLength;
	}

	public int GetMinKeepHealth() {
		return minKeepHealth;
	}

	public int GetMaxKeepHealth() {
		return maxKeepHealth;
	}

	public int GetMinBestOf() {
		return minBestOf;
	}

	public int GetMaxBestOf() {
		return maxBestOf;
	}

	public float GetMinVolume() {
		return minVolume;
	}

	public float GetMaxVolume() {
		return maxVolume;
	}

	public bool GetFileLoaded() {
		return fileLoaded;
	}
	#endregion
}
