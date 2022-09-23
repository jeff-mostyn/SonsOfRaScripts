using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour {

	// ---------------- public variables ---------------------
	// references
	private Player p;
	private SceneChangeManager sc;
	private GameManager g;
	private Camera mainCam;

	[Header("Carousel UI")]
	public Constants.gameScenes[] mapCodes;
	public GameObject[] mapVisuals;
	public Sprite[] mapDiagrams;
	public List<Transform> mapObjectPositions;
	[SerializeField] private GameObject demoLock;
	[SerializeField] private TextMeshProUGUI selectedMapName;
	[SerializeField] private List<Image> mapDiagramImages;
	[SerializeField] private Vector3 selectedDiagramScale;
	[SerializeField] private float diagramSideTranslation;

	[Header("Other UI")]
	[SerializeField] private GameObject StartButton;
	[SerializeField] private GameObject BackButton, SelectButton, BackController, SelectController;

	[Header("Gameplay Values")]
	public float selectionMoveCooldown = 2;
	public float moveSensitivity = 0.8f;
	public float mapSwitchCooldown;
	[SerializeField] private float transitionTime;
	private PlayerDirectionalInput pInput;
	public LayerMask mapObjectDetectionLayer;

	// gameplay values
	private float horzMove = 0f;
    private float vertMove = 0f;
	private float mapSwitchCooldownTimer;
	private int currentIndex = 0;
	private int mapCount;
	private bool isSwitchOnCooldown = false;

    //Temp fix, anim play only once
    bool FirstAnim = true;

	#region System Functions
	private void Awake() {
		SonsOfRa.Events.GeneralEvents.ControllerAssignmentChange += SwapVisibleButtons;
	}

	void Start () {
		sc = SceneChangeManager.Instance;
		g = GameManager.Instance;
        p = ReInput.players.GetPlayer(PlayerIDs.player1);
		mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();

		mapCount = mapCodes.Length;

		//FirstAnim = false;

		selectedMapName.SetText(Lang.MapNames[mapCodes[currentIndex]][SettingsManager.Instance.language]);
		for (int i = 0; i < mapDiagrams.Length; i++) {
			mapDiagramImages[i].sprite = mapDiagrams[i];
		}

		// instantiate the map objects
		int tempIndex;
		for (int i = -2; i < mapObjectPositions.Count-2; i++) {
			if (i < 0) {
				tempIndex = currentIndex + i < 0 ? currentIndex + i + mapVisuals.Length : currentIndex + i;
			}
			else if (i > 0) {
				tempIndex = currentIndex + i > mapVisuals.Length ? currentIndex + i - mapVisuals.Length : currentIndex + i;
			}
			else {
				tempIndex = currentIndex;
			}
			GameObject tmp = Instantiate(mapVisuals[tempIndex]);
			tmp.transform.SetParent(mapObjectPositions[i + 2]);
			tmp.transform.localPosition = Vector3.zero;
			tmp.transform.localScale = Vector3.one;

			// disable colliders for objects that are not the center one
			if (i != 0) {
				foreach (Collider c in mapObjectPositions[i+2].GetComponentsInChildren<Collider>()) {
					c.enabled = false;
				}
			}
			else {
				foreach (Collider c in mapObjectPositions[i+2].GetComponentsInChildren<Collider>()) {
					c.enabled = true;
				}
			}
		}

		pInput = GetComponent<PlayerDirectionalInput>();

		AdjustMapDiagrams();

		SwapVisibleButtons();
    }

    void Update() {
		//make sure timer is updated (WHY WASN'T THIS HERE BEFORE!?!)
		isSwitchOnCooldown = switchOnCooldown();

		horzMove = pInput.GetHorizNonRadialInput(p);

		if (horzMove > moveSensitivity && !isSwitchOnCooldown) {
			switchMapOnInput(1);
		}
		else if (horzMove < moveSensitivity * (-1) && !isSwitchOnCooldown) {
			switchMapOnInput(-1);
		}

		if (p.GetButtonDown(RewiredConsts.Action.Select) && !p.GetButtonDown(RewiredConsts.Action.LClick)) {
			selectMap();
		}
		else if (p.GetButtonDown(RewiredConsts.Action.UIBack)) { //going back to main menu
			ReturnToPatronSelect();
        }

		if (p.controllers.hasMouse) {
			Ray cameraRay = mainCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
			RaycastHit[] collisions = Physics.RaycastAll(cameraRay, Mathf.Infinity, mapObjectDetectionLayer);
			StartButton.gameObject.SetActive(collisions.Length > 0);

			if (collisions.Length > 0) {
				foreach (Transform t in mapObjectPositions) {
					if (!t.GetComponentsInChildren<Collider>().ToList().Contains(collisions[0].collider)) {
						Outline tmpOutline = t.gameObject.GetComponentInChildren<Outline>();
						Color tmpColor = new Color(tmpOutline.OutlineColor.r, tmpOutline.OutlineColor.g, tmpOutline.OutlineColor.b, 0.0f);
						tmpOutline.OutlineColor = tmpColor;
					}
				}

				Outline outlineScript = collisions[0].collider.gameObject.GetComponentInParent<Outline>();
				Color outlineColor = new Color(outlineScript.OutlineColor.r, outlineScript.OutlineColor.g, outlineScript.OutlineColor.b, 1.0f);
				outlineScript.OutlineColor = outlineColor;
			}
			else {
				foreach (Transform t in mapObjectPositions) {
					Outline outlineScript = t.gameObject.GetComponentInChildren<Outline>();
					Color outlineColor = new Color(outlineScript.OutlineColor.r, outlineScript.OutlineColor.g, outlineScript.OutlineColor.b, 0.0f);
					outlineScript.OutlineColor = outlineColor;
				}
			}
		}
		else {
			StartButton.gameObject.SetActive(false);

			foreach (Transform t in mapObjectPositions) {
				Outline outlineScript = t.gameObject.GetComponentInChildren<Outline>();
				Color outlineColor = new Color(outlineScript.OutlineColor.r, outlineScript.OutlineColor.g, outlineScript.OutlineColor.b, 0.0f);
				outlineScript.OutlineColor = outlineColor;
			}
		}
	}

	private void OnDestroy() {
		SonsOfRa.Events.GeneralEvents.ControllerAssignmentChange -= SwapVisibleButtons;
	}
	#endregion

	public void SwitchMapOnIndex(int index) {
		StartCoroutine(SwitchToMap(index));
	}

	IEnumerator SwitchToMap(int index) {
		selectedMapName.SetText(Lang.MapNames[mapCodes[index]][SettingsManager.Instance.language]);
		AdjustMapDiagrams(index);
		if (index > currentIndex) {
			for (int i = currentIndex; i < index; i++) {
				switchMapOnInputHandler(1, false);
				yield return new WaitForSeconds(transitionTime);
			}
		}
		else {
			for (int i = currentIndex; i > index; i--) {
				switchMapOnInputHandler(-1, false);
				yield return new WaitForSeconds(transitionTime);
			}
		}
		yield return null;
	}

	public void switchMapOnInput(int direction) {
		switchMapOnInputHandler(direction);
	}

	public void switchMapOnInputHandler(int direction, bool updateUI = true) {
		if (direction == 1) {
			SoundManager.Instance.sound_godNext();
			if (currentIndex < mapCount - 1) {
				currentIndex++;
			}
			else if (currentIndex == mapCount - 1) {
				currentIndex = 0;
			}
		}
		else {
			SoundManager.Instance.sound_godPrev();
			if (currentIndex > 0) {
				currentIndex--;
			}
			else if (currentIndex == 0) {
				currentIndex = mapCount - 1;
			}
		}
		mapSwitchCooldownTimer = mapSwitchCooldown;
		updateMapVisuals(direction, transitionTime, updateUI);

#if !UNITY_XBOXONE
		// make sure button is deslected so attempt to ready doesn't trigger patron switch again
		EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
#endif
	}

	public void selectMap() {
		if (CanSelect()) {
			LoadoutManager.Instance.SetMap(Constants.sceneNames[mapCodes[currentIndex]]);

			SoundManager.Instance.sound_click();

			// load next Scene
			sc.setNextSceneName(Constants.sceneNames[mapCodes[currentIndex]]);
			sc.LoadNextScene(true);
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

	private void updateMapVisuals(float direction, float time, bool updateUI) {
		if (updateUI) {
			UpdateUI(direction);
		}

		StartCoroutine(MapObjectMover(direction, time));
	}

	private void UpdateUI(float direction) {
		selectedMapName.SetText(Lang.MapNames[mapCodes[currentIndex]][SettingsManager.Instance.language]);

		AdjustMapDiagrams();
	}

	private void AdjustMapDiagrams(int index = -1) {
		bool passedSelectedIndex = false;

		int targetIndex = index == -1 ? currentIndex : index;

		for (int i = 0; i < mapDiagramImages.Count; i++) {
			if (i == targetIndex) {
				((RectTransform)mapDiagramImages[i].transform).anchoredPosition = Vector3.zero;
				mapDiagramImages[i].transform.localScale = selectedDiagramScale;
				passedSelectedIndex = true;
			}
			else {
				((RectTransform)mapDiagramImages[i].transform).anchoredPosition = passedSelectedIndex ? new Vector3(diagramSideTranslation, 0f, 0f) : new Vector3(-diagramSideTranslation, 0f, 0f);
				mapDiagramImages[i].transform.localScale = Vector3.one;
			}
		}
	}

	private bool CanSelect() {
		if (SettingsManager.Instance.DemoMode && !Constants.MapAvailableInDemo[mapCodes[currentIndex]]) {
			return false;
		}
		else {
			return true;
		}
	}

	public void ReturnToPatronSelect() {
		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.patronSelect]);
		sc.LoadNextScene();
	}

	private IEnumerator MapObjectMover(float direction, float time) {
		if (direction > 0) {    // right input, move carousel from right to left
			Destroy(mapObjectPositions[0].transform.GetChild(0).gameObject);
			
			// give children of transforms new parents: the transform to the left
			for (int i = 1; i < mapObjectPositions.Count; i++) {
				mapObjectPositions[i].transform.GetChild(0).SetParent(mapObjectPositions[i-1]);
			}

			// instantiate new object, put it at child of element 4
			int tempindex;

			if (currentIndex == mapVisuals.Length-1) {	// current selection is the top end, offscreen right should be index 1
				tempindex = 1;
			}
			else if (currentIndex == mapVisuals.Length-2) { // current selection is one below top end, offscreen right should be index 
				tempindex = 0;
			}
			else {
				tempindex = currentIndex + 2;
			}

			GameObject tmp = Instantiate(mapVisuals[tempindex]);
			tmp.transform.SetParent(mapObjectPositions[mapObjectPositions.Count - 1], false);
			tmp.transform.localPosition = Vector3.zero;
		}
		else {  // left input, move carousel from left to right
			Destroy(mapObjectPositions[mapObjectPositions.Count - 1].transform.GetChild(0).gameObject);

			// give children of transforms new parents: the transform to the right
			for (int i = mapObjectPositions.Count - 2; i >= 0; i--) {
				mapObjectPositions[i].transform.GetChild(0).SetParent(mapObjectPositions[i + 1]);
			}

			// instantiate new object, put it at child of element 0
			int tempindex;

			if (currentIndex == 0) {    // current selection is the bottom end, offscreen left should be 1 below top end
				tempindex = mapVisuals.Length - 2; 
			}
			else if (currentIndex == 1) { // current selection is one above bottom end, offscreen left should be top end
				tempindex = mapVisuals.Length - 1;
			}
			else {
				tempindex = currentIndex - 2;
			}

			GameObject tmp = Instantiate(mapVisuals[tempindex]);
			tmp.transform.SetParent(mapObjectPositions[0], false);
			tmp.transform.localPosition = Vector3.zero;
		}

		// find distances between where objects currently are and their new parents, lerp them that distance over duration of transitionTime
		List<Vector3> distancesToMove = new List<Vector3>();
		for (int i = 0; i < mapObjectPositions.Count; i++) {
			// disable colliders for objects that are not the selected one
			foreach(Collider c in mapObjectPositions[i].GetComponentsInChildren<Collider>()) {
				c.enabled = false;
			}

			distancesToMove.Add(mapObjectPositions[i].transform.GetChild(0).transform.localPosition);
		}
		float t = 0f;
		while (t < time) {
			for (int i=0; i<mapObjectPositions.Count; i++) {
				mapObjectPositions[i].transform.GetChild(0).transform.localPosition = Vector3.Lerp(distancesToMove[i], Vector3.zero, t / time);
			}

			t += Time.deltaTime;
			yield return null;
		}
		for (int i = 0; i < mapObjectPositions.Count; i++) {
			mapObjectPositions[i].transform.GetChild(0).transform.localPosition = Vector3.zero;
		}

		// enable collider for raycast detection on center object
		foreach (Collider c in mapObjectPositions[2].GetComponentsInChildren<Collider>()) {
			c.enabled = true;
		}

		// Determine if map is selectable due to demo mode
		if (CanSelect()) {
			demoLock.SetActive(false);
		}
		else {
			demoLock.SetActive(true);
		}

		yield return null;
	}

	private void SwapVisibleButtons() {
		SelectButton.SetActive(p.controllers.hasKeyboard);
		BackButton.SetActive(p.controllers.hasKeyboard);
		SelectController.SetActive(!p.controllers.hasKeyboard);
		BackController.SetActive(!p.controllers.hasKeyboard);
	}
}
