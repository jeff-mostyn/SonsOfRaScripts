using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialSelctor : MonoBehaviour
{
	// ---------------- public variables ---------------------
	// references
	public Player p;
	public Player p2;

	// gameplay values
	public string rewiredPlayerKey = "Player0";
	public float selectionMoveCooldown = 2;
	public float moveSensitivity = 0.8f;
	public float mapSwitchCooldown;
	public bool selected = false;

	// --------------- nonpublic variables -------------------
	// references
	private SceneChangeManager sc;
	private GameManager g;
	[SerializeField] private Text tutorialTextBox;
	[SerializeField] private List<GameObject> tutorialWindows;
    [SerializeField] private List<GameObject> menuOptions;
    [SerializeField] private EventSystem myEventSystem;
    [SerializeField] private Image upArrow, downArrow;
	private PlayerDirectionalInput pInput;

	// gameplay values
	private float vertMove = 0f;
	private float mapSwitchCooldownTimer;
	[SerializeField] private float arrowMaxScale;
	public int currentIndex = 0;
	private int tutorialCount;
	private bool isSwitchOnCooldown = false;

	[Header("Canvas References")]
	public GameObject MainMenuCanvas;
	public GameObject HowToMenuCanvas;
	[SerializeField] private GameObject DefaultMenuOption;

	// Use this for initialization
	void Start() {
		sc = SceneChangeManager.Instance;
		g = GameManager.Instance;
		p = ReInput.players.GetPlayer(PlayerIDs.player1);
        p2 = ReInput.players.GetPlayer(PlayerIDs.player2);

        tutorialCount = tutorialWindows.Count;

		updateTextVisuals();

		pInput = GetComponent<PlayerDirectionalInput>();

		HowToMenuCanvas.SetActive(false);
	}

	// Update is called once per frame
	void Update() {
		//make sure timer is updated (WHY WASN'T THIS HERE BEFORE!?!)
		switchOnCooldown();

		if (p.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}
		else if (!p.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(menuOptions[currentIndex].gameObject);
		}

		vertMove = pInput.GetVertNonRadialInput(p);

		if (vertMove < moveSensitivity * (-1) && !switchOnCooldown()) {
			StartCoroutine(arrowScaler(upArrow.gameObject));
			if (currentIndex > 0) {
				currentIndex--;
			}

			mapSwitchCooldownTimer = mapSwitchCooldown;
			updateTextVisuals();

            myEventSystem.SetSelectedGameObject(menuOptions[currentIndex]);
		}
		else if (vertMove > moveSensitivity && !switchOnCooldown()) {
			StartCoroutine(arrowScaler(downArrow.gameObject));
			if (currentIndex < tutorialCount - 1) {
				currentIndex++;
			}

			mapSwitchCooldownTimer = mapSwitchCooldown;
			updateTextVisuals();

            myEventSystem.SetSelectedGameObject(menuOptions[currentIndex]);
        }
		else if (p.GetButtonDown(RewiredConsts.Action.UIBack) || p2.GetButtonDown(RewiredConsts.Action.UIBack)) {
			ReturnToMainMenu();
		}
	}

	private void OnEnable() {
		GetComponentInParent<CanvasGroup>().alpha = 1;
		if (p == null) {
			p = ReInput.players.GetPlayer(PlayerIDs.player1);
		}
		if (!p.controllers.hasKeyboard) {
			myEventSystem.SetSelectedGameObject(DefaultMenuOption);
			currentIndex = 0;
		}
	}

	private bool switchOnCooldown() {
		if (mapSwitchCooldownTimer > 0) {
			mapSwitchCooldownTimer -= Time.deltaTime;
			return true;
		}
		else
			return false;
	}

	private void updateTextVisuals() {
		for (int i = 0; i < tutorialWindows.Count; i++) {
			if (i == currentIndex) {
				tutorialWindows[i].SetActive(true);
			}
			else {
				tutorialWindows[i].SetActive(false);
			}
		}
	}

	public void setCurrentIndex(int index) {
		currentIndex = index;
		updateTextVisuals();
	}

	// this doesn't seem to sync up correctly with the map switch cooldown. I'm dividing cooldown by 2 to try to speed it up
	private IEnumerator arrowScaler(GameObject arrow) {
		arrow.transform.localScale = new Vector3(arrowMaxScale, arrowMaxScale, arrowMaxScale);

		float timer = 0;
		float scaleDelta = arrowMaxScale - 1;
		float newScale;

		while (timer < mapSwitchCooldown / 2) {
			newScale = arrowMaxScale - (scaleDelta * (timer / (mapSwitchCooldown / 2)));
			arrow.transform.localScale = new Vector3(newScale, newScale, newScale);

			yield return new WaitForEndOfFrame();
			timer += Time.deltaTime;
		}
		arrow.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public void ReturnToMainMenu() {
		GetComponentInParent<CanvasGroup>().alpha = 0;
		MainMenuCanvas.SetActive(true);
		HowToMenuCanvas.SetActive(false);
	}
}
