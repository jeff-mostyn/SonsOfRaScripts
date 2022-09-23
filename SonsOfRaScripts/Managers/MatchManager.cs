using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
	// static ref
	public static MatchManager Instance;

	[Header("Round UI Elements")]
	[SerializeField] private Image stoneBackdrop;
	[SerializeField] private TextMeshProUGUI roundText;
	[SerializeField] private List<GameObject> roundCirclesShared, roundCircles1, roundCircles2, p1Circles, p2Circles;

	private int p1Wins = 0;
	private int p2Wins = 0;

	private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;

			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		else {
			Destroy(gameObject);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Constants.gameScenes code = Constants.sceneCodes[scene.name];

		//if (!Constants.mapNames.ContainsKey(code)) {
		//	Destroy(gameObject);
		//}
		if (code == Constants.gameScenes.mainMenu) {
			Destroy(gameObject);
		}
	}

	private void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	#region UI Functions
	public void RoundDisplay() {
		roundText.SetText(Lang.MenuText[Lang.menuText.round][SettingsManager.Instance.language] + " " + GetRound());
	}

	public void SetupRoundCircles() {
		p1Circles = new List<GameObject>();
		p2Circles = new List<GameObject>();

		for (int i = 0; i < (SettingsManager.Instance.GetBestOf() / 2 + 1); i++) {
			if (p1Circles.Count == 0) {
				p1Circles.Add(roundCircles1[roundCircles1.Count - 1 - i]);
				p2Circles.Add(roundCircles2[roundCircles2.Count - 1 - i]);
			}
			else {
				p1Circles.Insert(0, roundCircles1[roundCircles1.Count - 1 - i]);
				p2Circles.Insert(0, roundCircles2[roundCircles2.Count - 1 - i]);
			}
		}

		for (int i = 0; i < SettingsManager.Instance.GetBestOf(); i++) {
			roundCirclesShared[i].SetActive(true);
		}

		for (int i = 0; i < p1Wins; i++) {
			p1Circles[i].transform.GetChild(0).gameObject.SetActive(true);
		}
		for (int i = 0; i < p2Wins; i++) {
			p2Circles[i].transform.GetChild(1).gameObject.SetActive(true);
		}
	}
	#endregion

	#region Getters and Setters
	public int GetP1Wins() {
		return p1Wins;
	}

	public void IncrementP1Wins() {
		p1Wins++;
	}

	public int GetP2Wins() {
		return p2Wins;
	}

	public void IncrementP2Wins() {
		p2Wins++;
	}

	public int GetRound() {
		return p1Wins + p2Wins + 1;
	}
	#endregion

	#region Helper Functions
	public void Fade(float fadeTime, bool fadeIn, bool roundTitle, bool roundCounters) {
		RoundDisplay();
		StartCoroutine(FadeHelper(fadeTime, fadeIn, roundTitle, roundCounters));
	}

	private IEnumerator FadeHelper(float fadeTime, bool fadeIn, bool roundTitle, bool roundCounters) {
		float counter = 0;
		while (counter <= fadeTime) {
			counter += Time.deltaTime;

			if (fadeIn) {
				if (roundTitle) {
					stoneBackdrop.color = Color.Lerp(Color.clear, Color.white, counter / fadeTime);
					roundText.color = Color.Lerp(Color.clear, Color.white, counter / fadeTime);
				}

				if (roundCounters) {
					for (int i = 0; i < SettingsManager.Instance.GetBestOf(); i++) {
						roundCirclesShared[i].GetComponent<Image>().color = Color.Lerp(Color.clear, Color.white, counter / fadeTime);
					}
					for (int i = 0; i < p1Circles.Count; i++) {
						p1Circles[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.red, counter / fadeTime);
						p2Circles[i].transform.GetChild(1).gameObject.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.blue, counter / fadeTime);
					}
				}

			}
			else {
				if (roundTitle) {
					stoneBackdrop.color = Color.Lerp(Color.white, Color.clear, counter / fadeTime);
					roundText.color = Color.Lerp(Color.white, Color.clear, counter / fadeTime);
				}

				if (roundCounters) {
					for (int i = 0; i < SettingsManager.Instance.GetBestOf(); i++) {
						roundCirclesShared[i].GetComponent<Image>().color = Color.Lerp(Color.white, Color.clear, counter / fadeTime);
					}
					for (int i = 0; i < p1Circles.Count; i++) {
						p1Circles[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.Lerp(Color.red, Color.clear, counter / fadeTime);
						p2Circles[i].transform.GetChild(1).gameObject.GetComponent<Image>().color = Color.Lerp(Color.blue, Color.clear, counter / fadeTime);
					}
				}
			}

			yield return null;
		}
	}

	public void ResetCounterVisual() {
		for (int i = 0; i < SettingsManager.Instance.GetBestOf(); i++) {
			roundCirclesShared[i].GetComponent<Image>().color = Color.clear;
			roundCirclesShared[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.clear;
			roundCirclesShared[i].transform.GetChild(1).gameObject.GetComponent<Image>().color = Color.clear;
		}
	}
	#endregion
}
