using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VictoryUIcontroller : MonoBehaviour {

    //individual Victory sprites
    public Sprite victoryWallBlue;
    public Sprite victoryWallRed;
    public Sprite victoryHeadBlue;
    public Sprite victoryHeadRed;

    public Color p1VictTextColor;
    public Color p2VictTextColor;

    public float delayMenuTime;

    private Animator myAnim;

    public GameObject blackOverlay;
    public GameObject victHeadLeft;
    public GameObject victHeadRight;
    public GameObject victWall;
    private TextMeshProUGUI victText;
    public EventSystem myEventSystem;
    public GameObject FirstMenuSelect;

	public float fadeTime;

    // Use this for initialization
    void Start () {
        //disable objects if not already
        blackOverlay.SetActive(false);
        victHeadLeft.SetActive(false);
        victHeadRight.SetActive(false);
        victWall.SetActive(false);

        myAnim = GameObject.Find("VictoryGroup").GetComponent<Animator>();
        victText = victWall.GetComponentInChildren<TextMeshProUGUI>();
    }

    //2.5 seconds for animation to play
    public void showVictoryMenu(string losingPlayer)
    {
		//make sure objects are enabled
		StartCoroutine(fadeToBlack());
		victHeadLeft.SetActive(true);
        victHeadRight.SetActive(true);
        victWall.SetActive(true);

        //add a variable in function declaration for type of victory, if else to assign text color, head sprite, wall sprite
        if(losingPlayer == PlayerIDs.player2)
        {
            victHeadLeft.GetComponent<Image>().sprite = victoryHeadRed;
            victHeadRight.GetComponent<Image>().sprite = victoryHeadRed;
            victWall.GetComponent<Image>().sprite = victoryWallRed;
            victText.color = p1VictTextColor;
        }
        else if (losingPlayer == PlayerIDs.player1)
        {
            victHeadLeft.GetComponent<Image>().sprite = victoryHeadBlue;
            victHeadRight.GetComponent<Image>().sprite = victoryHeadBlue;
            victWall.GetComponent<Image>().sprite = victoryWallBlue;
            victText.color = p2VictTextColor;
        }

        myAnim.SetTrigger("playVictAnim");

        StartCoroutine("victMenuAppear");
    }

	public void fadeAndSwitchCameras(endgameCameraMove camScript, string loser) {
		StartCoroutine(fadeAndSwitchCamerasWorker(camScript, loser));
	}

    //Add delay to menus appearing, can add more elements (fade in with alpha, wait for input)
    IEnumerator victMenuAppear()
    {
        yield return new WaitForSeconds(delayMenuTime);

        //need timed
        //victMenu.SetActive(true);

        //it seems a scene can only have one event system, so we need to reuse pause and change first select
        myEventSystem.firstSelectedGameObject = FirstMenuSelect;
        StartCoroutine("highlightBtn");
    }

    //set default position in menu
    IEnumerator highlightBtn() {
        myEventSystem.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        myEventSystem.SetSelectedGameObject(myEventSystem.firstSelectedGameObject);
    }

	IEnumerator fadeToBlack() {
		blackOverlay.SetActive(true);
		Image overlayImage = blackOverlay.GetComponent<Image>();
		float fadeCount = 0;
		float alphaPercent = 0;

		yield return new WaitForSeconds(2f);

		while (overlayImage.color.a < 1) {
			alphaPercent = Mathf.Lerp(0, 1, fadeCount / fadeTime);
			overlayImage.color = new Color(0, 0, 0, alphaPercent);
			fadeCount += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator fadeAndSwitchCamerasWorker(endgameCameraMove camScript, string loser) {
		blackOverlay.SetActive(true);
		Image overlayImage = blackOverlay.GetComponent<Image>();
		float fadeCount = 0;
		float alphaPercent = 0;

		float time = .35f;

		while (overlayImage.color.a < 1) {
			alphaPercent = Mathf.Lerp(0, 1, fadeCount / time);
			overlayImage.color = new Color(0, 0, 0, alphaPercent);
			fadeCount += Time.deltaTime;
			yield return null;
		}

		overlayImage.color = new Color(0f, 0f, 0f, 1f);

		camScript.StartRotation(loser);

		yield return new WaitForSeconds(0.1f);

		fadeCount = 0;
		alphaPercent = 0;
		while (overlayImage.color.a > 0) {
			alphaPercent = Mathf.Lerp(1, 0, fadeCount / time);
			overlayImage.color = new Color(0, 0, 0, alphaPercent);
			fadeCount += Time.deltaTime;
			yield return null;
		}
	}
}
