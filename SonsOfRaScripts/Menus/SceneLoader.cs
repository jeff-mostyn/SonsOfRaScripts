using System.Collections;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {

    public string nextScene;
	private AsyncOperation asyncLoad;
	[SerializeField] private Image controlDiagram;
	[SerializeField] private GameObject continuePrompt;
	[SerializeField] private GameObject loadingText;

	private Player player;

	SceneChangeManager sc;

	// Use this for initialization
	void Start () {
		player = ReInput.players.GetPlayer(PlayerIDs.player1);
		sc = SceneChangeManager.Instance;
		if (sc.getIntermediarySceneName() != "") { // an intermediary scene is to play
			nextScene = sc.getIntermediarySceneName();
			sc.clearIntermediarySceneName();
			StartCoroutine(LoadYourAsyncScene());
		}
		else {	// normal behavior
			nextScene = sc.getNextSceneName();
			StartCoroutine(LoadYourAsyncScene());
		}

		// unfilter music
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("paused", 0);
		MusicManager.Instance.MusicFade(true);
	}

	IEnumerator LoadYourAsyncScene()
    {
		// The Application loads the Scene in the background as the current Scene runs.
		// This is particularly good for creating loading screens.
		// You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
		// a sceneBuildIndex of 1 as shown in Build Settings.

		controlDiagram.gameObject.SetActive(sc.displayInfoAndPrompt);

		asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextScene);
		if (sc.displayInfoAndPrompt || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance != null && OnlineManager.Instance.waitForLoad)) {
			asyncLoad.allowSceneActivation = false;
		}

		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
        {
			if (asyncLoad.progress >= 0.9f && sc.displayInfoAndPrompt && !asyncLoad.allowSceneActivation) {
				continuePrompt.SetActive(true);
				loadingText.SetActive(false);
                
                if (player.GetButtonDown(RewiredConsts.Action.Select)) {
                    asyncLoad.allowSceneActivation = true;
                }
			}
            else if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation && OnlineManager.Instance != null && SettingsManager.Instance.GetIsOnline()) {
                if (!OnlineManager.Instance.selfSceneLoad) {
                    Debug.Log("Im ready");
                    Send_Ready();
                    OnlineManager.Instance.selfSceneLoad = true;
                    OnlineManager.Instance.waitForLoad = false;
                }

                if (OnlineManager.Instance.opponentSceneLoad && OnlineManager.Instance.selfSceneLoad) {
                    asyncLoad.allowSceneActivation = true;
                    OnlineManager.Instance.waitForLoad = false;
                }
            }

            yield return null;
		}
	}

    public void Send_Ready() {
        PacketObject obj = new PO_LoadSig();

        OnlineManager.Instance.SendPacket(obj);
    }
}
