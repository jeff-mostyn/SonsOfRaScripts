using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChangeManager : MonoBehaviour {

	public static SceneChangeManager Instance;
	public string intermediarySceneName = "", nextSceneName;
	public bool displayInfoAndPrompt;

	void Awake () {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	public void LoadNextScene(bool displayInfo = false) {
		displayInfoAndPrompt = displayInfo;
		UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.sceneNames[Constants.gameScenes.load]);
	}
	
	public string getNextSceneName() {
		return nextSceneName;
	}

	public void setNextSceneName(string _name) {
		nextSceneName = _name;
	}

	/// <summary>
	/// Use for loading scenes where there is an automatic scene in between, such as a video
	/// </summary>
	/// <param name="_next">The next formal scene to be played</param>
	/// <param name="_intermediary">The automatic scene to go to before the next scene</param>
	public void setNextScenenameWithIntermediary(string _next, string _intermediary) {
		nextSceneName = _next;
		intermediarySceneName = _intermediary;
	}

	public string getIntermediarySceneName() {
		return intermediarySceneName;
	}

	public void clearIntermediarySceneName() {
		intermediarySceneName = "";
	}
}
