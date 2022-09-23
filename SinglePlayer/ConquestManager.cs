using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class ConquestManager : MonoBehaviour {
	public enum enemyQuipSources { first, random, penultimate, final, unspecified }
	public static ConquestManager Instance;

	public List<GameObject> nodesList = new List<GameObject>();
	public bool playerVictory = false;
	private Color playerColor, enemy1Color, enemy2Color, enemy3Color;

	[Header("Node Generation")]
	public List<Constants.gameScenes> availableMaps = new List<Constants.gameScenes> {
		Constants.gameScenes.mapDunes,
		Constants.gameScenes.mapSenet,
		Constants.gameScenes.mapValley,
	};

	[Header("Node Bonus Stats")]
	public List<ModifierManager.modifiers> fortifications;
	public List<int> fortificationCosts;
	public List<ModifierManager.modifiers> fightBuffs;
	public List<int> fightBuffCosts;
	public List<ModifierManager.modifiers> bonusModifiers;

	[Header("UI")]
	public float fillTime;

	[Header("Enemy Encounter Details")]
	public bool nodeOwned;
	public enemyQuipSources encounterNumber;
	public Dictionary<Constants.patrons, bool> hasEncountered = new Dictionary<Constants.patrons, bool> {
		{ Constants.patrons.Anubis, false },
		{ Constants.patrons.Isis, false },
		{ Constants.patrons.Ra, false },
		{ Constants.patrons.Set, false },
	};
	public float fortifcationAutoResolveModifier, fightBuffAutoResolveModifier, hunkerDownAutoResolveModifier;

	[Header("Player Data")]
	public int turn = 0;
	public List<GameObject> players;
	public string playerName, enemy1Name, enemy2Name, enemy3Name;
	public string combatNodeName = "";
	public List<Patron> patronList;
	public Dictionary<string, bool> defeated = new Dictionary<string, bool>();
	public bool playerWonConquest;
	public Dictionary<string, Patron> playerPatrons;
	public float bonusOwnedValue = 0.1f;
	public float modifierValue = 0.1f;
	private bool gameOver;

	[Header("Timer")]
	public float timer;
	private Coroutine timerCoroutine = null;

	[Header("Camera")]
	public GameObject cameraObject;

	#region System Functions
	void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
			timer = 0;
		}
		else {
			Destroy(this);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if ((scene.name == Constants.sceneNames[Constants.gameScenes.mainMenu] || scene.name == Constants.sceneNames[Constants.gameScenes.patronSelectConquest]) && gameObject) {
			SceneManager.sceneLoaded -= OnSceneLoaded;
			Destroy(gameObject);
		}
		else if (scene.name == Constants.sceneNames[Constants.gameScenes.conquest1]) {
			LoadNodes();

			players[0] = GameObject.Find(playerName);
			players[1] = GameObject.Find(enemy1Name);
			players[2] = GameObject.Find(enemy2Name);
			players[3] = GameObject.Find(enemy3Name);

			// set patrons of each player
			players[0].GetComponent<ConquestPlayer>().patron = playerPatrons[playerName];
			players[1].GetComponent<ConquestPlayer>().patron = playerPatrons[enemy1Name];
			players[2].GetComponent<ConquestPlayer>().patron = playerPatrons[enemy2Name];
			players[3].GetComponent<ConquestPlayer>().patron = playerPatrons[enemy3Name];

			LoadPlayers();

			// handle colors of nodes and pictures
			SetupConquestOnLoad();
			SavePlayers();

			cameraObject.transform.parent = players[turn].GetComponent<ConquestPlayer>().mapCamera.transform;
			cameraObject.transform.localPosition = Vector3.zero;
			players[turn].GetComponent<ConquestPlayer>().FinishTurn();
		}

		if (scene.name == Constants.sceneNames[Constants.gameScenes.mapDunes]
			|| scene.name == Constants.sceneNames[Constants.gameScenes.mapSenet]
			|| scene.name == Constants.sceneNames[Constants.gameScenes.mapValley]) {

			timerCoroutine = StartCoroutine(runTimer());
		}
		else if (timerCoroutine != null) {
			StopCoroutine(timerCoroutine);
		}
	}

	private void Start() {
		defeated = new Dictionary<string, bool>();
		gameOver = false;

		SettingsManager.Instance.ResetCustomGameSettings();

		playerName = players[0].name;
		enemy1Name = players[1] ? players[1].name : "";
		enemy2Name = players[2] ? players[2].name : "";
		enemy3Name = players[3] ? players[3].name : "";

		LoadPlayers();

		string path = Application.persistentDataPath + Constants.conquestFilePath + playerName + ".data";

		// set patrons of each player
		playerPatrons = new Dictionary<string, Patron>();
		if (File.Exists(path))
		{
			Debug.Log("file exists");
			players[0].GetComponent<ConquestPlayer>().patron = SetPatronContinue(players[0]);
			playerPatrons.Add(playerName, players[0].GetComponent<ConquestPlayer>().patron);			
			//Debug.Log("p1 patron is: " + SetPatronContinue(players[0]).ToString());

			players[1].GetComponent<ConquestPlayer>().patron = SetPatronContinue(players[1]);
			playerPatrons.Add(enemy1Name, players[1].GetComponent<ConquestPlayer>().patron);			

			players[2].GetComponent<ConquestPlayer>().patron = SetPatronContinue(players[2]);
			playerPatrons.Add(enemy2Name, players[2].GetComponent<ConquestPlayer>().patron);			

			players[3].GetComponent<ConquestPlayer>().patron = SetPatronContinue(players[3]);
			playerPatrons.Add(enemy3Name, players[3].GetComponent<ConquestPlayer>().patron);

			LoadoutManager.Instance.p1Patron = players[0].GetComponent<ConquestPlayer>().patron;
		}
		else
		{
			playerPatrons.Add(playerName, LoadoutManager.Instance.p1Patron);
			patronList.Remove(LoadoutManager.Instance.p1Patron);
			players[0].GetComponent<ConquestPlayer>().patron = playerPatrons[playerName];

			playerPatrons.Add(enemy1Name, patronList[UnityEngine.Random.Range(0, patronList.Count)]);
			patronList.Remove(playerPatrons[enemy1Name]);
			players[1].GetComponent<ConquestPlayer>().patron = playerPatrons[enemy1Name];

			playerPatrons.Add(enemy2Name, patronList[UnityEngine.Random.Range(0, patronList.Count)]);
			patronList.Remove(playerPatrons[enemy2Name]);
			players[2].GetComponent<ConquestPlayer>().patron = playerPatrons[enemy2Name];

			playerPatrons.Add(enemy3Name, patronList[UnityEngine.Random.Range(0, patronList.Count)]);
			patronList.Remove(playerPatrons[enemy3Name]);
			players[3].GetComponent<ConquestPlayer>().patron = playerPatrons[enemy3Name];
		}

		players[0].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
		players[0].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
		players[0].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[playerName].BustColor;
		defeated.Add(players[0].name, false);

		players[1].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", enemy1Color);
		players[1].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
		players[1].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[enemy1Name].BustColor;
		defeated.Add(players[1].name, false);

		players[2].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", enemy2Color);
		players[2].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
		players[2].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[enemy2Name].BustColor;
		defeated.Add(players[2].name, false);

		players[3].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", enemy3Color);
		players[3].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
		players[3].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[enemy3Name].BustColor;
		defeated.Add(players[3].name, false);

		LoadNodes();
		SetupConquestOnLoad();
		GenerateNodeBonuses();
		AssignNodeMaps();

		//turn = UnityEngine.Random.Range(0, players.Count);
		turn = 0;

		for (int i = 0; i<players.Count; i++) {
			players[i].GetComponent<ConquestPlayer>().isTurn = turn == i;
		}

		GameObject.Find("Conquest Player").GetComponent<ConquestPlayer_Human>().DisplayTurnGraphic(Instance.turn);
		Instance.players[Instance.turn].GetComponent<ConquestPlayer>().StartTurn();

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	IEnumerator runTimer() {
		while (true) {
			timer += Time.deltaTime;
			yield return null;
		}
	}
	#endregion

	#region Node Saving/Loading
	public void SavePlayers() {
		for(int i=0; i<players.Count; i++) {
			players[i].GetComponent<ConquestPlayer>().SavePlayer();
		}
	}

	public void LoadPlayers() {
		for (int i = 0; i < players.Count; i++) {
			players[i].GetComponent<ConquestPlayer>().LoadPlayer();
		}
	}

	public Patron SetPatronContinue(GameObject player)
	{
		LoadPlayers();
		foreach(Patron p in patronList)
		{			
			if(p.patronID.ToString() == player.GetComponent<ConquestPlayer>().patronName)
			{
				//Debug.Log("patronID: " + p.patronID.ToString() + " and " + player.GetComponent<ConquestPlayer>().patronName);
				return p;
			}
		}
		return null;
	}

	public void SaveNodes()
    {
        foreach(GameObject node in nodesList)
        {
            SaveSystem.SaveNodeData(node.GetComponent<Node>(), node.name);
        }
    }

    public void LoadNodes() {
        foreach (GameObject node in nodesList) {
            NodeData data = SaveSystem.LoadNodeData(node.name);
			if (data != null) {
				node.GetComponent<Node>().owner = data.owner;
				if (data.fortified) {
					node.GetComponent<Node>().FortifyNode((ModifierManager.modifiers)data.fortification, false);
				}
				else {
					node.GetComponent<Node>().UnfortifyNode();
				}
				node.GetComponent<Node>().hunkeredDown = data.hunkeredDown;
				node.GetComponent<Node>().nodeDetails = data.details;
			}
        }
    }
	#endregion

	public void GenerateNodeBonuses() {
		Debug.Log("generate bonus");
		List<GameObject> bonusNodes = nodesList.FindAll(n => n.GetComponent<Node>().nodeDetails.bonusNode);

		foreach (GameObject n in bonusNodes) {
			Node nodeScript = n.GetComponent<Node>();
			nodeScript.nodeDetails.bonusMod = bonusModifiers[UnityEngine.Random.Range(0, bonusModifiers.Count)];
			nodeScript.nodeDetails.modifiers.Add(nodeScript.nodeDetails.bonusMod);
		}
	}

	private void AssignNodeMaps() {
		Node n;
		foreach (GameObject g in nodesList) {
			n = g.GetComponent<Node>();
			n.nodeDetails.map = availableMaps[UnityEngine.Random.Range(0, availableMaps.Count)];
		}
	}

	private void SetupConquestOnLoad() {
		// set colors
		playerColor = players[0].GetComponent<ConquestPlayer>().playerColor;
		enemy1Color = players[1].GetComponent<ConquestPlayer>().playerColor;
		enemy2Color = players[2].GetComponent<ConquestPlayer>().playerColor;
		enemy3Color = players[3].GetComponent<ConquestPlayer>().playerColor;

		// set god faces
		players[0].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[playerName].BustColor;
		players[1].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[enemy1Name].BustColor;
		players[2].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[enemy2Name].BustColor;
		players[3].GetComponent<ConquestPlayer>().godFaceImage.sprite = playerPatrons[enemy3Name].BustColor;

		// set keep looks
		for (int i = 0; i < players.Count; i++) {
			players[i].GetComponent<ConquestPlayer>().SetKeepAesthetics();
		}

		// handle colors
		players[0].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().ColorPath(playerColor, playerName, Node.nodeDirection.none);
		if (players[1].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().owner == enemy1Name) {
			players[1].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().ColorPath(enemy1Color, enemy1Name, Node.nodeDirection.none);
		}
		else {
			Debug.Log("defeat");
			CheckDefeat(players[1].name);
			GrayOutPicture(players[1].GetComponent<ConquestPlayer>().playerImageObj,
				players[1].GetComponent<ConquestPlayer>().godFaceImage,
				playerPatrons[players[1].name]);
		}

		if (players[2].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().owner == enemy2Name) {
			players[2].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().ColorPath(enemy2Color, enemy2Name, Node.nodeDirection.none);
		}
		else if (defeated.ContainsKey(enemy2Name) && defeated.ContainsKey(enemy2Name)) {
			CheckDefeat(players[2].name);
		}

		if (players[3].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().owner == enemy3Name) {
			players[3].GetComponent<ConquestPlayer>().startingNodeObject.GetComponent<Node>().ColorPath(enemy3Color, enemy3Name, Node.nodeDirection.none);
		}
		else if (defeated.ContainsKey(enemy3Name) && defeated.ContainsKey(enemy3Name)) {
			CheckDefeat(players[3].name);
		}

		// make sure all nodes are colored, even if not connected to home
		foreach (GameObject g in nodesList) {
			Node n = g.GetComponent<Node>();
			if (n.owner != "" && g.GetComponent<MeshRenderer>().material.GetColor("_ForeColor") == Color.white) {
				n.ColorPath(
					n.owner == playerName ? playerColor
						: n.owner == enemy1Name ? enemy1Color
							: n.owner == enemy2Name ? enemy2Color
								: enemy3Color,
					n.owner, Node.nodeDirection.none);
			}
		}

		cameraObject = players[0].GetComponent<ConquestPlayer>().gameCamera.gameObject;
	}

	#region Victory and Defeat
	public void GrayOutPicture(GameObject picture, Image face, Patron god) {
		foreach (Image i in picture.GetComponentsInChildren<Image>()) {
			i.color = new Color(i.color.grayscale, i.color.grayscale, i.color.grayscale);
		}
		face.sprite = god.BustGrayscale;
	}

	public void CheckDefeat(string playerName) {
		GameObject playerObject = playerName == players[0].name ? players[0]
			: playerName == players[1].name ? players[1]
				: playerName == players[2].name ? players[2]
					: players[3];
		ConquestPlayer player = playerObject.GetComponent<ConquestPlayer>();

		if (player.startingNodeObject.GetComponent<Node>().owner != playerObject.name) {
			Debug.Log(playerObject.name);
			defeated[playerObject.name] = true;
			player.SavePlayer();
			List<GameObject> playerNodes = nodesList.FindAll(g => g.GetComponent<Node>().owner == playerObject.name);

			foreach (GameObject g in playerNodes) {
				g.GetComponent<Node>().owner = "";
			}
			foreach(GameObject g in playerNodes) {
				g.GetComponent<Node>().ColorPath(Color.white, "", Node.nodeDirection.none);
			}

			GrayOutPicture(player.playerImageObj, player.godFaceImage, playerPatrons[playerObject.name]);
			playerObject.SetActive(false);
		}
	}

	public void CheckDefeatDemo(string playerName) //checkdefeat to remove players, but dont save
	{
		GameObject playerObject = playerName == players[0].name ? players[0]
			: playerName == players[1].name ? players[1]
				: playerName == players[2].name ? players[2]
					: players[3];
		ConquestPlayer player = playerObject.GetComponent<ConquestPlayer>();

		if (player.startingNodeObject.GetComponent<Node>().owner != playerObject.name)
		{
			Debug.Log(playerObject.name);
			defeated[playerObject.name] = true;
			//player.SavePlayer();
			List<GameObject> playerNodes = nodesList.FindAll(g => g.GetComponent<Node>().owner == playerObject.name);

			foreach (GameObject g in playerNodes)
			{
				g.GetComponent<Node>().owner = "";
			}
			foreach (GameObject g in playerNodes)
			{
				g.GetComponent<Node>().ColorPath(Color.white, "", Node.nodeDirection.none);
			}

			GrayOutPicture(player.playerImageObj, player.godFaceImage, playerPatrons[playerObject.name]);
			playerObject.SetActive(false);
		}
	}

	public void CheckPlayerVictory() {
		if (defeated[players[1].name] && defeated[players[2].name] && defeated[players[3].name]) {
			StartCoroutine(EndGameSequence(true));
		}
	}

	public void CheckPlayerDefeat() {
		if (defeated[players[0].name]) {
			Debug.Log("defeated");
			StartCoroutine(EndGameSequence(false));
		}
	}

	IEnumerator EndGameSequence(bool playerVictory) {
		gameOver = true;

		yield return new WaitForSeconds(5f);

		playerWonConquest = playerVictory;

		SceneChangeManager.Instance.setNextSceneName(Constants.sceneNames[Constants.gameScenes.conquestPostGame]);
		SceneChangeManager.Instance.LoadNextScene();
	}
	#endregion

	#region Turns
	public void NextTurn() {
		Debug.Log("next turn");
		if (!gameOver) {
			IncrementTurn();

			players[0].GetComponent<ConquestPlayer_Human>().DisplayTurnGraphic(turn);

			players[turn].GetComponent<ConquestPlayer>().StartTurn();
		}
	}

	public void IncrementTurn() {
		Debug.Log("end turn " + turn);

		if (turn < 3) {
			turn++;
		}
		else {
			turn = 0;
		}

		Debug.Log("start turn " + turn);

		if (defeated[players[turn].name] && defeated.ContainsValue(false)) {
			Debug.Log("skip turn");
			IncrementTurn();
		}
	}

	public void CenterCameraOnPlayer(ConquestPlayer p) {
		cameraObject.transform.parent = players[turn].GetComponent<ConquestPlayer>().mapCamera.transform;
		StartCoroutine(MoveCamera(p));
	}

	IEnumerator MoveCamera(ConquestPlayer p) {
		p.moving = true;
		var startPos = cameraObject.transform.localPosition;
		var timer = 0.0f;

		while (timer <= 1.5f) {
			cameraObject.transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, Mathf.SmoothStep(0.0f, 1.0f, timer));

			timer += Time.deltaTime;
			yield return null;
		}
		transform.localPosition = Vector3.zero;

		p.moving = false;
	}
	#endregion
}
