using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	#region variables and declarations
	private int MAX_ACTIVE_SOUND_COUNT = 5;
	public static SoundManager Instance;

	// ------------- public variables ----------------
	// references
	[Header("Player Sounds")]
	public AudioClip conquestNodeCaptureSound;
	public AudioClip timerSound;

	// gameplay values
	[Header("Player Sound Volumes")]
	public float conquestNodeCaptureMaxVolume;
	public float timerSoundMaxVolume;

	public float panMagnitude;

	[Header("UI")]
	[FMODUnity.EventRef] [SerializeField] private string hoverEvent;
	[FMODUnity.EventRef] [SerializeField] private string clickEvent;
	[FMODUnity.EventRef] [SerializeField] private string backMenuEvent;
	[FMODUnity.EventRef] [SerializeField] private string radialHoverEvent;
	[FMODUnity.EventRef] [SerializeField] private string radialClickEvent;
	[FMODUnity.EventRef] [SerializeField] private string countdown;
	[FMODUnity.EventRef] [SerializeField] private string godNext;
	[FMODUnity.EventRef] [SerializeField] private string godPrev;
	[FMODUnity.EventRef] [SerializeField] private string meterUnlock;
	[FMODUnity.EventRef] [SerializeField] private string meterFillEvent;
	[FMODUnity.EventRef] [SerializeField] private string godSelectEvent;
	[FMODUnity.EventRef] [SerializeField] private string UIDenyEvent;

	#endregion

	private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	#region player sounds
	public void conquestNodeCapture(AudioSource src) {
		StartCoroutine(playConquestNodeCaptureSound(src));
	}

	private IEnumerator playConquestNodeCaptureSound(AudioSource src) {
		src.volume = conquestNodeCaptureMaxVolume * SettingsManager.Instance.GetFXVolumeFloat() * 2;
		src.clip = conquestNodeCaptureSound;

		src.panStereo = 0;
		src.Play();

		yield return null;
	}

	public void timerCountdownSound(AudioSource src)
	{
		StartCoroutine(playTimeSound(src));
	}

	private IEnumerator playTimeSound(AudioSource src)
	{
		src.volume = timerSoundMaxVolume * SettingsManager.Instance.GetFXVolumeFloat() * 2;
		src.clip = timerSound;

		src.panStereo = 0;
		src.Play();

		yield return null;
	}
	#endregion

	#region UI Sounds
	public void sound_hover() {
		FMOD.Studio.EventInstance hover = FMODUnity.RuntimeManager.CreateInstance(hoverEvent);
		hover.start();
		hover.release();
	}

	public void sound_click() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(clickEvent);
		click.start();
		click.release();
	}

	public void sound_back_menu() {
		FMOD.Studio.EventInstance back = FMODUnity.RuntimeManager.CreateInstance(backMenuEvent);
		back.start();
		back.release();
	}

	public void sound_radialClick() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(radialClickEvent);
		click.start();
		click.release();
	}

	public void sound_radialHover() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(radialHoverEvent);
		click.start();
		click.release();
	}

	public void sound_countDown() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(countdown);
		click.start();
		click.release();
	}

	public void sound_godNext() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(godNext);
		click.start();
		click.release();
	}

	public void sound_godPrev() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(godPrev);
		click.start();
		click.release();
	}

	public void sound_MeterUnlock() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(meterUnlock);
		click.start();
		click.release();
	}

	public void sound_MeterFill() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(meterFillEvent);
		click.start();
		click.release();
	}

	public void sound_GodSelect() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(godSelectEvent);
		click.start();
		click.release();
	}

	public void sound_UIDeny() {
		FMOD.Studio.EventInstance click = FMODUnity.RuntimeManager.CreateInstance(UIDenyEvent);
		click.start();
		click.release();
	}
	#endregion
}
