using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NonSceneMusicCues { Anubis, Isis, Ra, Set, Sekhmet, Amalgam, Victory, Defeat, StoryVictory, SuddenDeath };

[System.Serializable]
public struct SceneMusicFMOD {
	public Constants.gameScenes scene;
	[FMODUnity.EventRef]
	public string musicEvent;
	public bool previousTrackFadeOut;
	public bool fadeAfter;
}

[System.Serializable]
public struct NonSceneMusic {
	public NonSceneMusicCues cue;
	[FMODUnity.EventRef]
	public string musicEvent;
	public bool previousTrackFadeOut;
	public bool fadeAfter;
}

public class MusicManager : MonoBehaviour
{	
	public static MusicManager Instance;

	[Header("Volume and Fade")]
	[SerializeField] private float volume;
	[SerializeField] private float fadeTimeShort, fadeTimeLong;
	private float fadeTime;
	private bool isFadeActive = false;
	FMOD.Studio.EventInstance musicState;
	FMOD.Studio.PLAYBACK_STATE musicStatePlaybackState;
	string currentEvent;

	[Header("Conditional Music")]
	public string nonSceneMusicEvent = "";

	// variables related to playing music of a certain scene
	private Constants.gameScenes currentSceneMusic;
	public List<SceneMusicFMOD> sceneMusicFMOD;
	public List<NonSceneMusic> nonSceneMusic;
	private int sceneMusicIndex;

	private void Awake() {
		// If there is no instance of this class, set it.
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	private void Start() {
		SceneManager.sceneLoaded += OnSceneLoaded;
		float vol;
		musicState.setVolume(SettingsManager.Instance.GetMusicVolumeFloat() * 2);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Constants.gameScenes currentScene = Constants.sceneCodes[scene.name];

		StopAllCoroutines();  // make sure that nothing is currently fading

		if (nonSceneMusicEvent == "") {
			// this will change the music playing if the current scene has a specific track that is supposed to play AND
			// 1. the nonlooping player is playing music and that clip is not the same as the track to be played OR
			// 2. the looping player is playing music and that clip is not the same as the track to be played OR
			// 3. no music is currently playing
			sceneMusicIndex = sceneMusicFMOD.FindIndex(s => s.scene == currentScene);
			// if the new scene has a dedicated music track
			if (sceneMusicIndex != -1 && sceneMusicFMOD[sceneMusicIndex].musicEvent != currentEvent) {
				// if emitter is playing
				musicState.getPlaybackState(out musicStatePlaybackState);
				Debug.Log("music state " + musicStatePlaybackState.ToString());
				if (musicStatePlaybackState != FMOD.Studio.PLAYBACK_STATE.STOPPED && musicStatePlaybackState != FMOD.Studio.PLAYBACK_STATE.STOPPING) {
					// if current playing music is what is still supposed to play
					if (currentEvent == sceneMusicFMOD[sceneMusicIndex].musicEvent) {
						
					}
					else {  // new music to play
						Debug.Log("playing new music");
						musicState.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
						PlayEmitter(sceneMusicFMOD[sceneMusicIndex].musicEvent);
					}
				}
				else {
					PlayEmitter(sceneMusicFMOD[sceneMusicIndex].musicEvent);
				}
			} 
		}
		else if (scene.name != Constants.sceneNames[Constants.gameScenes.load]){
			PlayEmitter(nonSceneMusicEvent);
			nonSceneMusicEvent = "";
		}
	}

	private void OnDestroy() {
		musicState.release();
	}

	public void PlayOnCommand(string musicEvent) {
		PlayEmitter(musicEvent);
	}

	private void PlayEmitter(string musicEvent) {
		musicState = FMODUnity.RuntimeManager.CreateInstance(musicEvent);
		musicState.start();
		currentEvent = musicEvent;
	}

	public bool IsPlaying() {
		musicState.getPlaybackState(out musicStatePlaybackState);
		return musicStatePlaybackState != FMOD.Studio.PLAYBACK_STATE.STOPPED && musicStatePlaybackState != FMOD.Studio.PLAYBACK_STATE.STOPPING;
	}

	public void crossfade(/*string newEvent,*/ int winLoseParameter) {
		musicState.setParameterByName("gameState", winLoseParameter);
		currentEvent = "";
	}

	public void PlaySuddenDeath() {
		musicState.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		PlayEmitter(nonSceneMusic.Find(x => x.cue == NonSceneMusicCues.SuddenDeath).musicEvent);
		ForceMusicReload();
	}

	public void SetupNonSceneMusic(NonSceneMusicCues musicCue) {
		int index = nonSceneMusic.FindIndex(s => s.cue == musicCue);
		if (index != -1) {
			nonSceneMusicEvent = nonSceneMusic[index].musicEvent;
		}
	}

	public void ForceMusicReload() {
		currentEvent = "";
	}

	#region Music Fade Effect

	public void MusicFade(bool shortFade) {
		Constants.gameScenes nextScene = Constants.sceneCodes[SceneChangeManager.Instance.getNextSceneName()];
		int nextSceneMusicIndex = sceneMusicFMOD.FindIndex(s => s.scene == nextScene);
		Constants.gameScenes currentScene = Constants.sceneCodes[SceneManager.GetActiveScene().name];
		int currentSceneMusicIndex = sceneMusicFMOD.FindIndex(s => s.scene == currentScene);

		// checks if the next scene will have a new music track
		if (SceneChangeManager.Instance.getNextSceneName() != ""
			&& nextSceneMusicIndex != -1
			&& currentScene == Constants.gameScenes.load
			&& currentEvent != sceneMusicFMOD[nextSceneMusicIndex].musicEvent) {
			if ((nextSceneMusicIndex != -1 && sceneMusicFMOD[nextSceneMusicIndex].previousTrackFadeOut)
				|| (currentSceneMusicIndex != -1 && sceneMusicFMOD[currentSceneMusicIndex].fadeAfter)) {
				Debug.Log("fade out");
				musicState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}
		}
	}
	#endregion
}
