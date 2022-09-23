using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConquestPlayer_Human : ConquestPlayer
{
	protected Player player;

	[Header("Text to Fill")]
	public Text title;
	public Image mapImage;
	public TextMeshProUGUI mapHeader, modeHeader, modifierHeader;
	public TextMeshProUGUI mapName, modeName, modifierList, description;

	[Header("Map Images")]
	public Sprite dunesMap;
	public Sprite senetMap;
	public Sprite valleyMap;

	[Header("UI")]
	public Transform playerCamera;
	public GameObject detailsCanvas;
	[SerializeField] protected GameObject pauseUI;
	[SerializeField] protected EventSystem myEventSystem;
	[SerializeField] protected GameObject defaultPauseOption;
	[SerializeField] private GameObject MoveNodesPrompt;
	[SerializeField] private GameObject SeeNodeDetailsPrompt;
	[SerializeField] private GameObject PrepareForBattlePrompt;
	[SerializeField] private GameObject ReturnPrompt;
	[SerializeField] private GameObject FortifyPrompt;
	[SerializeField] private GameObject StartBattlePrompt;
	[SerializeField] private GameObject PurchaseBuffPrompt;
	[SerializeField] private GameObject EndTurnPrompt;
	[SerializeField] private Image fortifyButton, purchaseBuffButton;
	[SerializeField] private Text fortifyText, purchaseBuffText, endTurnText;
	[SerializeField] private PlayerColorPalette neutralColor;
	[SerializeField] private GameObject ActionsRemainingUI;
	[SerializeField] private TextMeshProUGUI ActionsRemainingText, ActionsRemainingNumber;
	public GameObject turnStartGraphic;
	public Image graphicFrame, colorBack;
	public TextMeshProUGUI turnStartText;

	[Header("Fortification Radial")]
	public GameObject fortificationRadial;
	public GameObject fortificationRadialSelector, fortificationRadialPointer;
	public TextMeshProUGUI fortificationName, fortificationTooltip, fortificationCost;
	public List<iconScaler> fortificationRadialIconScalers;
	private Vector3 originalFortificationRadialScale;
	
	[Header("Fight Buff Radial")]
	public GameObject fightBuffRadial;
	public GameObject fightBuffRadialSelector, fightBuffRadialPointer;
	public TextMeshProUGUI fightBuffName, fightBuffTooltip, fightBuffCost;
	public List<iconScaler> fightBuffRadialIconScalers;
	private Vector3 originalFightBuffRadialScale;

	[Header("General Radial")]
	public float radialScaleTime;
	private int zone = 0, oldZone = 0;
	private Color radialIconColor;

	[Header("FX")]
	public GameObject availbleNodePulse;

	#region System Functions
	void Start() {
		base.start();

		player = ReInput.players.GetPlayer(PlayerIDs.player1);

		if (currentNode == null) {
			currentNodeObject = startingNodeObject;
			currentNode = currentNodeObject.GetComponent<Node>();
		}

		SetModifiers();

		pInput = GetComponent<PlayerDirectionalInput>();
		ActivateButtonPrompts();
		originalFortificationRadialScale = fortificationRadial.transform.localScale;
		originalFightBuffRadialScale = fightBuffRadial.transform.localScale;
		radialIconColor = fortificationRadialIconScalers[0].gameObject.GetComponent<Image>().color;

#if !UNITY_XBOXONE
		if (!player.controllers.hasMouse) {
			myEventSystem.SetSelectedGameObject(defaultPauseOption);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else {
			myEventSystem.SetSelectedGameObject(null);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		nodeLayerMask = 1 << LayerMask.NameToLayer("ConquestNode");
#endif
	}

	void Update() {
		// handle pause menu and controller swapping
		if (!player.controllers.hasMouse && myEventSystem.currentSelectedGameObject == null) {
			myEventSystem.SetSelectedGameObject(defaultPauseOption);
		}
		else if (player.controllers.hasMouse && myEventSystem.currentSelectedGameObject != null) {
			myEventSystem.SetSelectedGameObject(null);
		}

		vertMove = pInput.GetVertNonRadialInput(player);
		horzMove = pInput.GetHorizNonRadialInput(player);

		if (!pauseUI.activeSelf && isTurn) {
			if (!moving) {
				if (fortificationRadial.activeSelf) {
					radialUpdate(fortificationRadial, fortificationRadialIconScalers, fortificationRadialSelector, fortificationRadialPointer, ConquestManager.Instance.fortificationCosts,
						ConquestManager.Instance.fortifications, fortificationName, fortificationTooltip, fortificationCost);
				}
				else if (fightBuffRadial.activeSelf) {
					radialUpdate(fightBuffRadial, fightBuffRadialIconScalers, fightBuffRadialSelector, fightBuffRadialPointer, ConquestManager.Instance.fightBuffCosts, ConquestManager.Instance.fightBuffs,
						fightBuffName, fightBuffTooltip, fightBuffCost);
				}
				else {
					nonRadialUpdate();
				}
			}
		}
		else {
			if (player.GetButtonDown(RewiredConsts.Action.Start) || player.GetButtonDown(RewiredConsts.Action.UIBack)) {
				pauseUI.SetActive(false);
			}
		}
	}
	#endregion

	#region Update State Functions
	private void nonRadialUpdate() {
		if (player.GetButtonDown(RewiredConsts.Action.Start) && !cameraMoving && !pauseUI.activeSelf) {
			pauseUI.SetActive(true);
		}

		if (lookingAtMap) {
			if (horzMove > moveSensitivity && currentNode.rightNode != null && currentNode.rightNode.GetComponent<Node>().canMove) {
				UpdateNode(currentNode.rightNode, currentNodeObject);
			}
			else if ((horzMove < moveSensitivity * -1) && currentNode.leftNode != null && currentNode.leftNode.GetComponent<Node>().canMove) {
				UpdateNode(currentNode.leftNode, currentNodeObject);
			}
			else if ((vertMove < moveSensitivity * -1) && currentNode.upNode != null && currentNode.upNode.GetComponent<Node>().canMove) {
				UpdateNode(currentNode.upNode, currentNodeObject);
			}
			else if (vertMove > moveSensitivity && currentNode.bottomNode != null && currentNode.bottomNode.GetComponent<Node>().canMove) {
				UpdateNode(currentNode.bottomNode, currentNodeObject);
			}

			//if (player.GetButtonDown(RewiredConsts.Action.EndConquestTurn)) {
			//	// apply hunker down buff
			//	EndTurn();
			//}
		}

		if (player.GetButtonDown(RewiredConsts.Action.Select) && !player.GetButtonDown(RewiredConsts.Action.LClick) && !cameraMoving) {
			if (lookingAtMap && currentNodeObject != startingNodeObject) //close up into node
			{
				cameraMoving = true;
				gameCamera.transform.parent = gameObject.transform;
				StartCoroutine(ChangeCamera(playerCamera));
				lookingAtMap = false;
				ActivateButtonPrompts();
			}
			else if (currentNode.canBattle) {
				detailsCanvas.SetActive(false);
				OpenRadial(fightBuffRadial, fightBuffRadialIconScalers, ConquestManager.Instance.fightBuffCosts, fightBuffRadialSelector);
			}
		}

		//if (player.GetButtonDown(RewiredConsts.Action.Fortify) && CanFortify()) {
		//	OpenRadial(fortificationRadial, fortificationRadialIconScalers, ConquestManager.Instance.fortificationCosts, fortificationRadialSelector);
		//}

		if (player.GetButtonDown(RewiredConsts.Action.LClick) && lookingAtMap) {
			ClickNodeMovement();
		}

		if (player.GetButtonDown(RewiredConsts.Action.UIBack) && (!cameraMoving && !lookingAtMap)) {
			detailsCanvas.SetActive(false);
			gameCamera.transform.parent = mapCamera.transform;
			StartCoroutine(ChangeCamera(mapCamera));
			lookingAtMap = true;
			ActivateButtonPrompts();
		}
	}

	private void radialUpdate(GameObject radial, List<iconScaler> radialIconScalers, GameObject selector, GameObject pointer, List<int> costs, List<ModifierManager.modifiers> mods,
		TextMeshProUGUI nameText, TextMeshProUGUI tooltipText, TextMeshProUGUI costText) {
		if (player.controllers.hasMouse) {
			Vector2 relativeMousePosition = Vector2.zero;
			if (radial == fortificationRadial) {
				Vector2 currentMousePosition = Input.mousePosition;
				relativeMousePosition = currentMousePosition - new Vector2(radial.transform.position.x, radial.transform.position.y);
			}
			else {
				Vector2 currentMousePosition = Input.mousePosition;
				relativeMousePosition = currentMousePosition - new Vector2(gameCamera.WorldToScreenPoint(radial.transform.position).x, gameCamera.WorldToScreenPoint(radial.transform.position).y);
			}
			horzMove = relativeMousePosition.x;
			vertMove = -relativeMousePosition.y;
		}

		oldZone = zone;

		if (Mathf.Abs(horzMove) >= moveSensitivity || Mathf.Abs(vertMove) >= moveSensitivity
			|| Mathf.Sqrt(Mathf.Pow(horzMove, 2) + Mathf.Pow(vertMove, 2)) >= moveSensitivity) {
			float angle = Mathf.Atan2(vertMove, horzMove) * Mathf.Rad2Deg;

			// zone
			zone = getZone(angle);

			if (oldZone != zone && zone != 0) {
				if (oldZone != 0) {
					radialIconScalers[oldZone - 1].scaleDown();
				}
				radialIconScalers[zone-1].scaleUp();
				SoundManager.Instance.sound_radialHover();
			}

			updateRadialVisuals(angle, selector, pointer, mods, costs, nameText, tooltipText, costText);

			// attempt to purchase
			if (player.GetButtonDown(RewiredConsts.Action.Select) && influence >= costs[zone - 1]) {
				if (radial == fortificationRadial) { // purchase fortification
					currentNode.FortifyNode(ConquestManager.Instance.fortifications[zone - 1], true);
					influence -= costs[zone - 1];
					influenceDisplay.text = influence.ToString();

					CloseRadial(radial, radialIconScalers);
					ActivateButtonPrompts();
				}
				else {
					// add fight buff to list of modifiers
					ModifierManager.Instance.SetFightBuff(ConquestManager.Instance.fightBuffs[zone - 1]);
					influence -= costs[zone - 1];
					influenceDisplay.text = influence.ToString();

					StartBattle();
				}
			}
			else if (player.GetButtonDown(RewiredConsts.Action.Select) && influence < costs[zone - 1]) {
				// cant afford, play purchase error
				gameCamera.gameObject.GetComponent<cameraShake>().ShakeTheCamera(0.08f, 0.1f);
				SoundManager.Instance.sound_UIDeny();
			}

			// Proceed to match if we are selecting fight buffs
			//if (radial == fightBuffRadial && player.GetButtonDown(RewiredConsts.Action.StartConquestMatch)) {
			//	StartBattle();
			//}

		}
		else {
			zone = 0;

			pointer.SetActive(false);
			selector.SetActive(false);
		}

		if (player.GetButtonDown(RewiredConsts.Action.UIBack)) {
			CloseRadial(radial, radialIconScalers);
			if (radial == fightBuffRadial) {
				detailsCanvas.SetActive(true);
			}
		}
	}

	private void StartBattle() {
		SetLoadouts();
		ConquestManager.Instance.SavePlayers();
		ConquestManager.Instance.combatNodeName = currentNode.name;
		ConquestManager.Instance.nodeOwned = currentNode.owner != "";
		ModifierManager.Instance.playerOwnedNode = false;
		if (currentNode.owner != "") {
			SetEncounterQuipSource(ConquestManager.Instance.playerPatrons[currentNode.owner]);
		}

		scm.setNextSceneName(Constants.sceneNames[currentNode.nodeDetails.map]);
		scm.LoadNextScene(true);
	}

	#region Radial Effect Stuff
	private void OpenRadial(GameObject radial, List<iconScaler> radialIconScalers, List<int> costs, GameObject selector) {
		radial.SetActive(true);

		oldZone = 0;
		zone = 0;
		for (int i = 0; i < radialIconScalers.Count; i++) {
			radialIconScalers[i].SetScaleMin();
		}

		for (int i=0; i<radialIconScalers.Count; i++) {
			radialIconScalers[i].gameObject.GetComponent<Image>().color = influence >= costs[i] ? radialIconColor : Color.gray;
		}

		StartCoroutine(ScaleRadialWorker(true, radial, radial == fortificationRadial ? originalFortificationRadialScale : originalFightBuffRadialScale));
		if (radial == fortificationRadial) {
			StartCoroutine(FadeRadialIn());
			StartCoroutine(ChangeCameraFOVWithMenu(42f));
		}
	}

	private void CloseRadial(GameObject radial, List<iconScaler> radialIconScalers) {
		StartCoroutine(ScaleRadialWorker(false, radial, radial == fortificationRadial ? originalFortificationRadialScale : originalFightBuffRadialScale));
		if (radial == fortificationRadial) {
			StartCoroutine(ChangeCameraFOVWithMenu(40f));
		}
	}

	private int getZone(float angle) {
		if (angle >= Constants.RADIAL_TOP3_MIN && angle < Constants.RADIAL_TOP3_MAX) {
			return 1;
		}
		else if (angle >= Constants.RADIAL_MID3_MIN && angle < Constants.RADIAL_MID3_MAX) {
			return 2;
		}
		else {
			return 3;
		}
	}

	private void updateRadialVisuals(float angle, GameObject selector, GameObject pointer, List<ModifierManager.modifiers> mods, List<int> costs,
		TextMeshProUGUI nameText, TextMeshProUGUI tooltipText, TextMeshProUGUI costText) {
		pointer.SetActive(true);
		selector.SetActive(true);

		// point selector
		float zRotation = 0f;
		if (zone == 1) {
			zRotation = Constants.RADIAL_P1_TOP;
		}
		else if (zone == 2) {
			zRotation = Constants.RADIAL_P1_MID;
		}
		else {
			zRotation = Constants.RADIAL_P1_BOT;
		}

		if (selector == fortificationRadialSelector) {
			selector.transform.eulerAngles = new Vector3(selector.transform.rotation.x, selector.transform.rotation.y, zRotation);
			pointer.transform.eulerAngles = new Vector3(pointer.transform.rotation.x, pointer.transform.rotation.y, -angle);
		}
		else {
			selector.transform.localRotation = Quaternion.Euler(new Vector3(selector.transform.rotation.x, selector.transform.rotation.y, zRotation));
			pointer.transform.localRotation = Quaternion.Euler(new Vector3(pointer.transform.rotation.x, pointer.transform.rotation.y, -angle));
			ActivateButtonPrompts(zone - 1);
		}

		// set text
		nameText.SetText(Lang.ModifierNames[mods[zone - 1]][SettingsManager.Instance.language]);
		tooltipText.SetText(Lang.ConquestBuffTooltips[mods[zone - 1]][SettingsManager.Instance.language]);
		costText.SetText(costs[zone - 1].ToString());
	}

	IEnumerator ScaleRadialWorker(bool grow, GameObject radial, Vector3 originalRadialScale) {
		if (grow) {
			radial.transform.localScale = Vector3.zero;
		}
		else {
			radial.transform.localScale = originalRadialScale;
		}

		float elapsedTime = 0;

		while (elapsedTime < radialScaleTime) {
			if (grow) {
				radial.transform.localScale = Vector3.Lerp(Vector3.zero, originalRadialScale, Mathf.SmoothStep(0.0f, 1.0f, elapsedTime / radialScaleTime));
			}
			else {
				radial.transform.localScale = Vector3.Lerp(originalRadialScale, Vector3.zero, Mathf.SmoothStep(0.0f, 1.0f, elapsedTime / radialScaleTime));
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		if (grow) {
			radial.transform.localScale = originalRadialScale;
		}
		else {
			radial.transform.localScale = Vector3.zero;
			radial.SetActive(false);
		}
	}

	IEnumerator FadeRadialIn() {
		Image[] imageChildren = fortificationRadial.GetComponentsInChildren<Image>();
		Text[] textChildren = fortificationRadial.GetComponentsInChildren<Text>();
		Color newColor;
		float elapsedTime = 0f;

		while (elapsedTime < radialScaleTime) {
			foreach (Image child in imageChildren) {
				newColor = child.color;
				newColor.a = Mathf.Lerp(0, 1, Mathf.SmoothStep(0, 1, Mathf.Pow(elapsedTime/radialScaleTime, 2)));
				child.color = newColor;
			}
			foreach (Text child in textChildren) {
				newColor = child.color;
				newColor.a = Mathf.Lerp(0, 1, Mathf.SmoothStep(0, 1, Mathf.Pow(elapsedTime / radialScaleTime, 2)));
				child.color = newColor;
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

	IEnumerator ChangeCameraFOVWithMenu(float target) {
		float currentFOV = gameCamera.fieldOfView;
		float elapsedTime = 0f;

		while (elapsedTime < radialScaleTime) {
				gameCamera.fieldOfView = Mathf.Lerp(currentFOV, target, Mathf.SmoothStep(0, 1, elapsedTime / radialScaleTime));

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		gameCamera.fieldOfView = target;
	}
	#endregion
	#endregion

	public void ActivateButtonPrompts(int zone = -1) {
		MoveNodesPrompt.SetActive(lookingAtMap);
		SeeNodeDetailsPrompt.SetActive(lookingAtMap && currentNodeObject != startingNodeObject && !fortificationRadial.activeSelf);
		PrepareForBattlePrompt.SetActive(!lookingAtMap && currentNode.canBattle && !fightBuffRadial.activeSelf);
		ReturnPrompt.SetActive(!lookingAtMap || fortificationRadial.activeSelf);
		EndTurnPrompt.SetActive(lookingAtMap && !fortificationRadial.activeSelf);
		StartBattlePrompt.SetActive(fightBuffRadial.activeSelf);
		PurchaseBuffPrompt.SetActive(fightBuffRadial.activeSelf || fortificationRadial.activeSelf);
		FortifyPrompt.SetActive(lookingAtMap && !fortificationRadial.activeSelf);

		fortifyButton.color = CanFortify() ? Color.white : Color.gray;
		fortifyText.color = CanFortify() ? Color.white : Color.gray;

		endTurnText.text = actionPoints > 0 ? Lang.ButtonPrompts[Lang.buttonPrompts.defendNode][SettingsManager.Instance.language] : Lang.ButtonPrompts[Lang.buttonPrompts.endTurn][SettingsManager.Instance.language];

		if (zone != -1) {
			purchaseBuffButton.color = influence >= ConquestManager.Instance.fightBuffCosts[zone] ? Color.white : Color.gray;
			purchaseBuffText.color = influence >= ConquestManager.Instance.fightBuffCosts[zone] ? Color.white : Color.gray;
		}
	}

	#region Set Match Details
	public void SetSprite(Constants.gameScenes map) {
		if (map == Constants.gameScenes.mapDunes) {
			mapImage.sprite = dunesMap;
		}
		else if (map == Constants.gameScenes.mapSenet) {
			mapImage.sprite = senetMap;
		}
		else {
			mapImage.sprite = valleyMap;
		}
	}

	public void SetLoadouts(ConquestPlayer opponent = null) {
		if (opponent == null) {
			if (currentNode.owner != gameObject.name && currentNode.owner != "") {
				lm.p2Patron = GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().patron;
				lm.p2Colors = GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().palette.GetColorPalette();
				lm.setBlessingsByPatron();
			}
			else if (currentNode.owner == "") {
				lm.p2Patron = patron;
				lm.p2Colors = neutralColor.GetColorPalette();
				lm.setBlessingsByPatron();
			}
		}
		else {
			lm.p2Patron = opponent.patron;
			lm.p2Colors = opponent.palette.GetColorPalette();
			lm.setBlessingsByPatron();
		}
	}

	public void SetDifficulty() {
		SettingsManager.Instance.SetDifficulty((int)currentNode.nodeDetails.mode);
	}

	public void SetEncounterQuipSource(Patron enemy) {
		if (currentNode.GetComponent<Node>().nodeDetails.quipType == ConquestManager.enemyQuipSources.unspecified || currentNode.GetComponent<Node>().owner == gameObject.name) {
			if (ConquestManager.Instance.hasEncountered[enemy.patronID]) {
				ConquestManager.Instance.encounterNumber = ConquestManager.enemyQuipSources.random;
			}
			else {
				ConquestManager.Instance.encounterNumber = ConquestManager.enemyQuipSources.first;
				ConquestManager.Instance.hasEncountered[enemy.patronID] = true;
			}
		}
		else {
			ConquestManager.Instance.encounterNumber = currentNode.GetComponent<Node>().nodeDetails.quipType;
		}
	}

	public void ChangeDetails() {
		title.text = currentNode.nodeDetails.name;
		SetSprite(currentNode.nodeDetails.map);

		mapHeader.SetText(Lang.MenuText[Lang.menuText.map][SettingsManager.Instance.language] + ":");
		modeHeader.SetText(Lang.MenuText[Lang.menuText.mode][SettingsManager.Instance.language] + ":");
		modifierHeader.SetText(Lang.MenuText[Lang.menuText.modifiers][SettingsManager.Instance.language] + ":");

		mapName.SetText(Lang.MapNames[currentNode.nodeDetails.map][SettingsManager.Instance.language]);
		modeName.SetText(Lang.NodeDifficulties[currentNode.nodeDetails.mode][SettingsManager.Instance.language]);
		string modifierListText = "";
		for(int i=0; i < currentNode.nodeDetails.modifiers.Count; i++) {
			if (i != 0) {
				modifierListText += "\n";
			}
			modifierListText += Lang.ModifierNames[currentNode.nodeDetails.modifiers[i]][SettingsManager.Instance.language];
		}
		modifierList.SetText(modifierListText);

		//description.text = currentNode.nodeDetails.description;
	}
	#endregion

	#region Nodes
	protected override void UpdateNode(GameObject newNode, GameObject oldNode) {
		base.UpdateNode(newNode, currentNodeObject);
		ActionsRemainingText.SetText(Lang.MenuText[Lang.menuText.remainingActions][SettingsManager.Instance.language] + ": ");
		ActionsRemainingNumber.SetText(actionPoints.ToString());
		ActionsRemainingNumber.fontSize = ActionsRemainingText.fontSize;

		SetLoadouts();
		SetDifficulty();
		SetModifiers();
		ChangeDetails();
		ActivateButtonPrompts();
	}

	protected void ClickNodeMovement() {
		Ray cameraRay = gameCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		RaycastHit[] collisions = Physics.RaycastAll(cameraRay, Mathf.Infinity, nodeLayerMask);

		if (collisions.Length == 1 && collisions[0].collider.gameObject.GetComponent<Node>().canMove) {
			if (collisions[0].collider.gameObject == currentNodeObject) {
				cameraMoving = true;
				gameCamera.transform.parent = gameObject.transform;
				StartCoroutine(ChangeCamera(playerCamera));
				lookingAtMap = false;
				ActivateButtonPrompts();
			}
			else {
				UpdateNode(collisions[0].collider.gameObject, currentNodeObject);
			}
		}
		else if (collisions.Length > 1) {
			Transform closest = collisions[0].collider.transform;

			float smallestDist = Vector3.Distance(gameCamera.transform.position, closest.transform.position);

			foreach (RaycastHit hit in collisions) {
				float lastDist = Vector3.Distance(gameCamera.transform.position, hit.collider.transform.position);
				if (lastDist < smallestDist) {
					closest = hit.collider.transform;
					smallestDist = lastDist;
				}
			}
			if (closest.gameObject == currentNodeObject) {
				cameraMoving = true;
				gameCamera.transform.parent = gameObject.transform;
				StartCoroutine(ChangeCamera(playerCamera));
				lookingAtMap = false;
			}
			else {
				UpdateNode(closest.gameObject, currentNodeObject);
			}
		}
	}

	IEnumerator ChangeCamera(Transform newCameraPos) {
		float elapsedTime = 0;

		Vector3 startPos = gameCamera.transform.position;
		Quaternion startRot = gameCamera.transform.rotation;
		while (elapsedTime < moveTime) {
			gameCamera.transform.position = Vector3.Lerp(startPos, newCameraPos.position, Mathf.SmoothStep(0.0f, 1.0f, (elapsedTime / moveTime)));
			gameCamera.transform.rotation = Quaternion.Lerp(startRot, newCameraPos.rotation, Mathf.SmoothStep(0.0f, 1.0f, (elapsedTime / moveTime)));

			elapsedTime += Time.deltaTime;
			yield return null;
		}
		gameCamera.transform.position = newCameraPos.position;
		gameCamera.transform.rotation = newCameraPos.rotation;

		if (!lookingAtMap) {
			ChangeDetails();
			detailsCanvas.SetActive(true);
		}

		cameraMoving = false;

		yield return null;
	}
	#endregion

	#region Turns
	public override void StartTurn() {
		// play any kind of turn fx, change ui, etc.
		isTurn = true;
		turnHighlightRing.SetActive(true);

		foreach (GameObject g in ConquestManager.Instance.nodesList) {
			Node n = g.GetComponent<Node>();
			n.canBattle = false;
			n.canMove = false;
		}

		foreach (CanvasGroupFade fade in GetComponentsInChildren<CanvasGroupFade>()) {
			if (fade.gameObject.GetComponent<CanvasGroup>().alpha == 0) {
				fade.FadeIn();
			}
		}

		base.StartTurn();
		MarkUsableNodes(currentNode, actionPoints);

		ActionsRemainingText.SetText(Lang.MenuText[Lang.menuText.remainingActions][SettingsManager.Instance.language] + ": ");
		ActionsRemainingNumber.SetText(actionPoints.ToString());
		ActionsRemainingNumber.fontSize = ActionsRemainingText.fontSize;
		TakeTurn();
	}

	public override void TakeTurn() {
		ConquestManager.Instance.CenterCameraOnPlayer(this);
	}

	public override void FinishTurn() {
		ResetAfterCombat();

		StartCoroutine(PlayOutFinishTurn());
	}

	IEnumerator PlayOutFinishTurn() {
		actionPoints = 0;
		ActionsRemainingNumber.text = actionPoints.ToString();

		if (ConquestManager.Instance.playerVictory) {
			Victorious();
		}
		else {
			Defeat();
		}

		yield return new WaitForSeconds(3);

		EndTurn();
	}

	public override void EndTurn() {
		foreach (CanvasGroupFade fade in GetComponentsInChildren<CanvasGroupFade>()) {
			fade.FadeOut();
		}

		base.EndTurn();
		ActionsRemainingNumber.text = actionPoints.ToString();
	}
	#endregion

	#region Victory and Defeat
	public override void Victorious() {
		combatEfficiency += 0.05f;

		MoveToCenterOfNode(currentNode.transform.position);

		if (currentNode.owner != gameObject.name && currentNode.fortified) {
			currentNode.UnfortifyNode();
		}

		string enemyName = currentNode.owner;
		currentNode.owner = gameObject.name;
		currentNode.GetComponent<Node>().FillPaths(playerColor, gameObject.name);
        currentNode.GetComponent<Node>().SpawnNodeParticle(this);

		if (currentNode.GetComponent<Node>().nodeDetails.quipType == ConquestManager.enemyQuipSources.penultimate && enemyName != "") {
			currentNode.GetComponent<Node>().nodeDetails.quipType = ConquestManager.enemyQuipSources.unspecified;
		}

		if (enemyName != "") {
			GameObject.Find(enemyName).GetComponent<ConquestPlayer>().Defeat();
			ConquestManager.Instance.CheckDefeat(enemyName);
		}

		ConquestManager.Instance.playerVictory = false;

		ConquestManager.Instance.CheckPlayerVictory();
	}
	#endregion

	#region Visuals
	public void DisplayTurnGraphic(int playerIndex) {
		StartCoroutine(StartTurnGraphic(playerIndex));
	}

	IEnumerator StartTurnGraphic(int playerIndex) {
		float fadeInTime = 1f;
		Color backColor = ConquestManager.Instance.players[playerIndex].GetComponent<ConquestPlayer>().playerColor;

		turnStartGraphic.SetActive(true);

		// fade in
		float alpha = 0f;
		float elapsedTime = 0f;
		while (alpha < 1) {
			colorBack.color = new Color(backColor.r * .75f, backColor.g * .75f, backColor.b * .75f, alpha * alpha * alpha);
			graphicFrame.color = new Color(1f, 1f, 1f, Mathf.Log10(alpha) + 1);
			turnStartText.color = new Color(1f, 1f, 1f, alpha);

			alpha = Mathf.Lerp(0f, 1f, Mathf.SmoothStep(0f, 1f, elapsedTime / fadeInTime));
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		yield return new WaitForSeconds(1f);

		alpha = 1f;
		elapsedTime = 0f;
		while (alpha > 0) {
			colorBack.color = new Color(backColor.r * .75f, backColor.g * .75f, backColor.b * .75f, alpha * alpha * alpha);
			graphicFrame.color = new Color(1f, 1f, 1f, Mathf.Log10(alpha) + 1);
			turnStartText.color = new Color(1f, 1f, 1f, alpha);

			alpha = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, elapsedTime / fadeInTime));
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		turnStartGraphic.SetActive(false);
	}

	public override void SetKeepAesthetics() {
		SetHeadPosition();
		SetCursor();

		// set up keep colors
		Material keepMat = keep.GetComponent<MeshRenderer>().material;
		keepMat.SetColor("_PalCol1", LoadoutManager.Instance.getPaletteColor(0, PlayerIDs.player1));
		keepMat.SetColor("_PalCol2", LoadoutManager.Instance.getPaletteColor(1, PlayerIDs.player1));

		Material headMat = keepHead.GetComponent<MeshRenderer>().material;
		headMat.SetColor("_PalCol1", LoadoutManager.Instance.getPaletteColor(0, PlayerIDs.player1));
		headMat.SetColor("_PalCol2", LoadoutManager.Instance.getPaletteColor(1, PlayerIDs.player1));

		Material cursorMat = cursor.GetComponent<MeshRenderer>().material;
		cursorMat.SetColor("_PalCol1", LoadoutManager.Instance.getPaletteColor(0, PlayerIDs.player1));
		cursorMat.SetColor("_PalCol2", LoadoutManager.Instance.getPaletteColor(1, PlayerIDs.player1));
	}
	#endregion
}
