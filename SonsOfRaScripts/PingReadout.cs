using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PingReadout : MonoBehaviour
{
	private TextMeshProUGUI text;
	private Coroutine pingReadoutUpdateRoutine;

	private void Awake() {
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void Start() {
		text = GetComponent<TextMeshProUGUI>();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (this) {
			Constants.gameScenes code = Constants.sceneCodes[scene.name];

			if (Constants.mapNames.ContainsKey(code)) {
				pingReadoutUpdateRoutine = StartCoroutine(PingReadoutUpdate());
			}
			else {
				if (pingReadoutUpdateRoutine != null) {
					StopCoroutine(pingReadoutUpdateRoutine);
				}
			}
		}
	}

	private IEnumerator PingReadoutUpdate() {
		while (true) {
			yield return new WaitForSeconds(0.1f);
			text.SetText(Lang.MenuText[Lang.menuText.ping][SettingsManager.Instance.language] + " " + OnlineManager.Instance.currentPingTime);
			yield return null;
		}
	}

	private void OnDestroy() {
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
