using Rewired;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class GoToMainMenu : MonoBehaviour
{
    //[SerializeField]
    private float timeBeforeLoad;
    private VideoPlayer vp;
	private Player p;

	[SerializeField] private mainMenuStartup Startup;

    // Start is called before the first frame update
    void Start()
    {
		p = ReInput.players.GetPlayer(0);
		vp = GetComponent<VideoPlayer>();
        vp.loopPointReached += EndReached;
		if (SettingsManager.Instance.PlayIntro) {
			Startup.IntroInProgress = true;
		}
	}

    // Update is called once per frame
    void Update() {
		if (Startup.IsOverlayOn() && vp.time > 0.5) {
			Startup.TurnOffOverlay();
		}

        if (p.GetAnyButtonDown() || ReInput.controllers.Keyboard.GetAnyButtonDown() || ReInput.controllers.Mouse.GetAnyButtonDown()) {
            vp.Stop();
			EndReached(vp);
        }
    }

    void EndReached(VideoPlayer MyVp) {
		Startup.PlayStartup();
    }
}
