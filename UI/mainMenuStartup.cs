using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Video;

public class mainMenuStartup : MonoBehaviour
{
	[Header("Main Menu Pieces")]
	[SerializeField] private GameObject backgroundImage;
	[SerializeField] private CanvasGroup titleCG;
	[SerializeField] private List<CanvasGroup> mainButtonCanvasGroups;

	[Header("Splash and Intro")]
	[SerializeField] private CanvasGroup overlayCG;
	[SerializeField] private GameObject overlay;
	[SerializeField] private GameObject splashVideoObj;
	[SerializeField] private PostProcessingBehaviour postprocessing;

	[Header("Timing Values")]
	[SerializeField] private float backgroundImageScrollTime;
	[SerializeField] private float postTitleDelayTime;
	[SerializeField] private float menuItemDelayTime;
	[SerializeField] private float menuItemFadeTime;
	[SerializeField] private float panelAndTitleSlideTime;
	[SerializeField] private float titleSitTime;

	[Header("Position Values")]
	[SerializeField] private float backPanelCenterX;
	[SerializeField] private float backPanelCenterY;
	[SerializeField] private Vector3 titleStartPos;
	[SerializeField] private Vector3 titleStartScale;

	private Coroutine introCoroutine;
	private Player p;
	public bool IntroInProgress;

	#region System Functions
	void Start() {
		introCoroutine = null;
		p = ReInput.players.GetPlayer(0);

		if (SettingsManager.Instance.PlayIntro) {
			postprocessing.enabled = false;

			backgroundImage.transform.localScale = new Vector3(1f, 0f, 1f);
			titleCG.alpha = 0;
			foreach (CanvasGroup cg in mainButtonCanvasGroups) {
				cg.alpha = 0f;
			}

			SoundManager.Instance.gameObject.SetActive(false);

			((RectTransform)titleCG.gameObject.transform).anchoredPosition = titleStartPos;
			titleCG.gameObject.transform.localScale = titleStartScale;
		}
		else {
			splashVideoObj.GetComponent<VideoPlayer>().Stop();
			splashVideoObj.GetComponent<GoToMainMenu>().enabled = false;
			splashVideoObj.SetActive(false);
			overlayCG.alpha = 0;
			overlay.SetActive(false);
			postprocessing.enabled = true;
		}
	}

	private void Update() {
		if (p.GetAnyButtonDown() && introCoroutine != null) {
			StopAllCoroutines();
			introCoroutine = null;

			SkipIntro();
		}
	}
	#endregion

	public void PlayStartup() {
		ContentManager.Instance.SetPlayerUIAlpha(1f);

		SettingsManager.Instance.PlayIntro = false;

		overlayCG.alpha = 1f;

		splashVideoObj.GetComponent<VideoPlayer>().Stop();
		splashVideoObj.GetComponent<GoToMainMenu>().enabled = false;
		splashVideoObj.SetActive(false);
		postprocessing.enabled = true;

		introCoroutine = StartCoroutine(QuickIntro());

		// run anything that needs to run after splash screen
		PostStartup();
	}

	// all the coroutines
	#region Intro
	private IEnumerator QuickIntro() {
		yield return new WaitForSeconds(0.5f);

		MusicManager.Instance.PlayOnCommand(MusicManager.Instance.sceneMusicFMOD.Find(x => x.scene == Constants.gameScenes.mainMenu).musicEvent);
		StartCoroutine(FadeItem(titleCG, true));

		yield return new WaitForSeconds(1.65f);

		StartCoroutine(LerpPositionUI((RectTransform)titleCG.gameObject.transform, titleStartPos, Vector3.zero, panelAndTitleSlideTime));
		StartCoroutine(LerpScaleUI((RectTransform)titleCG.gameObject.transform, titleStartScale, Vector3.one, panelAndTitleSlideTime));

		yield return new WaitForSeconds(.55f);

		StartCoroutine(ScrollBackgroundImage());
		StartCoroutine(FadeItem(overlayCG, false, (menuItemDelayTime * 6) + menuItemFadeTime));
		foreach (CanvasGroup cg in mainButtonCanvasGroups) {
			yield return new WaitForSeconds(menuItemDelayTime);
			StartCoroutine(FadeItem(cg, true));
		}

		yield return new WaitForSeconds(0.2f);

		overlay.SetActive(false);
		SoundManager.Instance.gameObject.SetActive(true);

		yield return null;

		WrapUp();
	}

	private IEnumerator ScrollBackgroundImage() {
		float timer = 0f;
		float yScale = 0f;

		while (timer < backgroundImageScrollTime) {
			yScale = Mathf.Lerp(0f, 1f, Mathf.SmoothStep(0f, 1f, timer / backgroundImageScrollTime));

			backgroundImage.transform.localScale = new Vector3(1f, yScale, 1f);

			yield return null;
			timer += Time.deltaTime;
		}

		backgroundImage.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	private IEnumerator FadeItem(CanvasGroup cg, bool fadeIn, float fadeTime = -1) {
		float timer = 0f;
		float a = 0f;

		fadeTime = fadeTime == -1 ? menuItemFadeTime : fadeTime;

		float start = fadeIn ? 0f : 1f;
		float end = fadeIn ? 1f : 0f;

		while (timer < fadeTime) {
			a = Mathf.Lerp(start, end, Mathf.SmoothStep(0f, 1f, timer / menuItemFadeTime));

			cg.alpha = a;

			yield return null;
			timer += Time.deltaTime;
		}

		cg.alpha = end;
	}

	private IEnumerator LerpPositionUI(RectTransform slidingItem, Vector3 startPos, Vector3 endPos, float slideTime) {
		float timer = 0f;
		Vector3 tempPos;

		while (timer <= slideTime) {
			tempPos = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, timer / menuItemFadeTime));

			slidingItem.anchoredPosition = tempPos;

			yield return null;
			timer += Time.deltaTime;
		}

		slidingItem.anchoredPosition = endPos;
	}

	private IEnumerator LerpScaleUI(RectTransform slidingItem, Vector3 startScale, Vector3 endScale, float slideTime) {
		float timer = 0f;
		Vector3 tempScale;

		while (timer < slideTime) {
			tempScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, timer / menuItemFadeTime));

			slidingItem.localScale = tempScale;

			yield return null;
			timer += Time.deltaTime;
		}

		slidingItem.localScale = endScale;
	}

	private void SkipIntro() {
		((RectTransform)titleCG.gameObject.transform).anchoredPosition = Vector3.zero;
		titleCG.alpha = 1f;
		titleCG.transform.localScale = Vector3.one;
		backgroundImage.transform.localScale = Vector3.one;

		foreach (CanvasGroup cg in mainButtonCanvasGroups) {
			cg.alpha = 1f;
		}

		overlayCG.alpha = 0f;
		overlay.SetActive(false);

		SoundManager.Instance.gameObject.SetActive(true);
		if (!MusicManager.Instance.IsPlaying()) {
			MusicManager.Instance.PlayOnCommand(MusicManager.Instance.sceneMusicFMOD.Find(x => x.scene == Constants.gameScenes.mainMenu).musicEvent);
		}

		WrapUp();
	}
	#endregion

	public void PostStartup() {
		ContentManager.Instance.SetPlayerUIAlpha(1f);
#if !UNITY_XBOXONE
		ReadArgs();
#endif
	}

	public void WrapUp() {
		IntroInProgress = false;
	}

	private void ReadArgs() {
		string[] args = System.Environment.GetCommandLineArgs();

		string input = "";
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == "+connect_lobby" && args.Length > i + 1) {
				input = args[i + 1];
				if (SettingsManager.Instance.PlayOnline) {
					SteamWorksManager.Instance.ParseCommandLineLobbyInvite(input);
				}
			}
		}
	}

	#region Accessors
	public bool IsOverlayOn() {
		return overlayCG.alpha > 0;
	}

	public void TurnOffOverlay() {
		overlayCG.alpha = 0f;
	}
	#endregion
}
