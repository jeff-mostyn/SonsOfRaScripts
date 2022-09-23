using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Rewired;
using UnityEngine.UI;

public class TutorialVideoController : MonoBehaviour
{
	[SerializeField] private float buttonHoldToSkipTime;
	private float buttonHoldCounter = 0;
	[SerializeField] private Image circle;

	private Player p1, p2;
	private VideoPlayer vp;

	// Start is called before the first frame update
	void Start() {
		vp = GetComponent<VideoPlayer>();
		vp.loopPointReached += EndReached;

		p1 = ReInput.players.GetPlayer(PlayerIDs.player1);
		p2 = ReInput.players.GetPlayer(PlayerIDs.player2);
	}

	// Update is called once per frame
	void Update() {
		if (buttonHoldCounter >= buttonHoldToSkipTime) {
			vp.Stop();
			SceneChangeManager.Instance.LoadNextScene();
		}
		else if (p1.GetButton(RewiredConsts.Action.Select) || p2.GetButton(RewiredConsts.Action.Select)) {
			buttonHoldCounter += Time.deltaTime;
			circle.fillAmount = buttonHoldCounter / buttonHoldToSkipTime;
		}
		else {
			buttonHoldCounter = 0;
			circle.fillAmount = buttonHoldCounter / buttonHoldToSkipTime;
		}
	}

	void EndReached(VideoPlayer MyVp) {
		SceneChangeManager.Instance.LoadNextScene();
	}
}
