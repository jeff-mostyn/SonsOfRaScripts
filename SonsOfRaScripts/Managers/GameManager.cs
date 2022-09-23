using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {

	// singleton reference
	public static GameManager Instance;

	public GameObject p1;
	public GameObject p2;
    public GameObject CameraParent; //this is parent object inside camera group, not the overall group object
	public List<GameObject> blessings1Buttons, blessings1ButtonBacks, blessings1ButtonsMasked, blessings1ButtonBacksMasked, blessings2Buttons, blessings2ButtonBacks, blessings2ButtonsMasked, blessings2ButtonBacksMasked;
    public bool gameStarted = false;
    public bool timerSynced = false;
	private float gameTimer;
	public bool gameOver = false;
	public GameObject FireLanesManager;

	//public GameObject p1Radial, p2Radial;

	public LoadoutManager loadoutManager;
	public GameObject tutorialManager;
	private SceneChangeManager sc;
	private StatCollector stats;
	private SettingsManager settings;
	[SerializeField] private GameObject matchManagerObj;
	private MatchManager m;

	// currency variables
	[Header("Currency Variables")]
	public int gp5;
	public float p1BonusGp5 = 0;
	public float p2BonusGp5 = 0;
	public float p1BonusFp5 = 0;
	public float p2BonusFp5 = 0;
	private float currencyUpdateTimeRemaining;
	private const float currencyUpdateSeconds = 0.25f;
	public float spinTime;
	private bool p1SpinGold, p1SpinFavor, p2SpinGold, p2SpinFavor;

	private enum menus {gold1, favor1, gold2, favor2};
	private GameObject p1Menu;
	private GameObject p2Menu;

	// for these, 0 is the player's current amounts of gold/favor, 1 and 2 are the costs in the radial
	[Header("Currency References")]
	public TextMeshProUGUI gold1;
	public TextMeshProUGUI favor1;
	public TextMeshProUGUI gold2;
	public TextMeshProUGUI favor2;
	public GameObject favor2Icon;

	public Dictionary<string, int> keepBonusHealth = new Dictionary<string, int> { { PlayerIDs.player1, 0 }, { PlayerIDs.player2, 0 } };

    //Victory Stuff (maybe move into it's own game Object
	[Header("Game UI References")]
    public GameObject VictoryCanvas;
	[SerializeField] private TextMeshProUGUI victoryUIText;
	[SerializeField] private GameObject CutawayCanvas;
	[SerializeField] private GameObject CutawayCamera;
	[SerializeField] private GameObject player2ButtonPrompts;
	public Timer timer;

    public PlayerController player1Controller;
	public PlayerController player2Controller;

	private const int submenuItemCount = 4;

    [Header("UI Feedback Variables")]
    public Color uiErrorColor;
    public float uiErrorFadeTime;
    private float colorTimer = 0f;
    private Color p1TextColor;
    private Color p2TextColor;

	[Header("AI Control")]
	public bool AIUseBlessings = true;

    [Header("Entity Stuff")]
    public int entityIdNextUsable = 0;

	#region System Functions
	private void Awake() {
		// There can only be one
		if (Instance == null) {
			Instance = this;

			SonsOfRa.Events.GameEvents.KeepTakeDamage += AddFavorOnKeepDamage;
			SonsOfRa.Events.GameEvents.UnitDie += AddFavorOnUnitDie;
		}
		else {
			Instance = this;
			Destroy(this);
		}

		if (FindObjectsOfType<PoolManager>().Length > 1) {
			Destroy(gameObject);
		}		
	}

	// Use this for initialization
	void Start () {
		if (SettingsManager.Instance.GetIsTutorial())
		{
			player2Controller = p2.GetComponent<AI_PlayerController>();
			Instantiate(tutorialManager);
		}
		if (SettingsManager.Instance.GetIsTutorial())
		{
			GameObject tl = Instantiate(GameObject.Find("TutorialManager(Clone)").GetComponent<TutorialManager>().tutorialLoadout);
			loadoutManager = tl.GetComponent<LoadoutManager>();
		}
		else
		{
			loadoutManager = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
		}

		timer = GetComponentInChildren<Timer>();

		sc = SceneChangeManager.Instance;
		stats = StatCollector.Instance;
		settings = SettingsManager.Instance;
		m = MatchManager.Instance;

		gameOver = false;

		// Use timer if the players choose to
		GetComponent<Timer>().enabled = settings.GetUseTimer();

		player1Controller = p1.GetComponent<PlayerController>();
		
		p2.GetComponent<Human_PlayerController>().enabled = !SettingsManager.Instance.GetIsSinglePlayer();
		p2.GetComponent<PlayerDirectionalInput>().enabled = !SettingsManager.Instance.GetIsSinglePlayer();
		p2.GetComponent<RadialVisuals>().enabled = !SettingsManager.Instance.GetIsSinglePlayer();
		player2ButtonPrompts.SetActive(!SettingsManager.Instance.GetIsSinglePlayer());
		p2.GetComponent<AI_PlayerController>().enabled = SettingsManager.Instance.GetIsSinglePlayer();
		p2.GetComponent<AI_ThreatDetection>().enabled = SettingsManager.Instance.GetIsSinglePlayer();
		p2.GetComponent<AI_BlessingHandler>().enabled = SettingsManager.Instance.GetIsSinglePlayer();
		p2.GetComponent<AI_Unlocks>().enabled = SettingsManager.Instance.GetIsSinglePlayer();
		if (SettingsManager.Instance.GetIsSinglePlayer()) {
			player2Controller = p2.GetComponent<AI_PlayerController>();
		}
		else {
			player2Controller = p2.GetComponent<Human_PlayerController>();
		}		

        if (SettingsManager.Instance.GetIsConquest())
        {
            ModifierManager.Instance.ApplyModifiers();
			ModifierManager.Instance.ApplyFortifications();
			ModifierManager.Instance.ApplyFightBuffs();
			AIUseBlessings = ConquestManager.Instance.nodeOwned;
        }		

		if (!AIUseBlessings) {
			favor2.gameObject.SetActive(false);
			favor2Icon.gameObject.SetActive(false);
		}

		currencyUpdateTimeRemaining = currencyUpdateSeconds;
		gold1.SetText(player1Controller.startingGold.ToString());
		gold2.SetText(player2Controller.startingGold.ToString());

		favor1.SetText(player1Controller.startingFavor.ToString());
		favor2.SetText(player2Controller.startingFavor.ToString());

		setupUIForLoadouts();

        p1TextColor = gold1.color;
        p2TextColor = gold2.color;

		stats.initialize();

		gameTimer = 0f;

        if (SettingsManager.Instance.GetIsOnline()) {
            if (OnlineManager.Instance.fakePlayerKey == PlayerIDs.player1) {
                entityIdNextUsable = 0;
            }
            else {
                entityIdNextUsable = 1;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		currencyUpdateTimeRemaining -= Time.deltaTime;

		// every half second, update the players' gold and favor counts
		if (currencyUpdateTimeRemaining <= 0.0) {
			currencyUpdateTimeRemaining = currencyUpdateSeconds;
			displayGold();
			displayFavor();
		}

        // add to the players' gold and favor counts based on the gp5 set and base expansions
        if (gameStarted) {
            currencyGain();
			gameTimer += Time.deltaTime;
		}		
	}

	private void OnDestroy() {
		SonsOfRa.Events.GameEvents.KeepTakeDamage -= AddFavorOnKeepDamage;
		SonsOfRa.Events.GameEvents.UnitDie -= AddFavorOnUnitDie;
	}
	#endregion

	// -------------------------- Public functions -----------------------------------
	#region EndGame Functions
	public void EndGame(string losingPlayer, GameObject keep) {
		gameOver = true;

		if (SettingsManager.Instance.GetIsArcade()) {
			ArcadeManager.Instance.AddGameTimeToTimer(gameTimer);
		}

		StartCoroutine(EndGameHelper(losingPlayer, keep));
	}

    private IEnumerator EndGameHelper(string losingPlayer, GameObject keep) {
        GameObject.Find("PauseController").GetComponent<PauseGame>().DisablePause = true; //stop players from pausing game

		CutawayCamera.GetComponent<CutawayCamController>().CloseCutawayGameEnd();

        MatchManager.Instance.ResetCounterVisual();

		// stop AI from continuing to do things
		if (SettingsManager.Instance.GetIsSinglePlayer()) {
            p2.GetComponent<AI_PlayerController>().Stop();
        }

        // wipe player queues and stop all player actions
        p1.GetComponent<UnitSpawner>().ClearQueue();
        p2.GetComponent<UnitSpawner>().ClearQueue();
		((Human_PlayerController)player1Controller).clearTowerAndUncolor();
		if (!SettingsManager.Instance.GetIsSinglePlayer()) {
			((Human_PlayerController)player2Controller).clearTowerAndUncolor();
		}

        victoryUIText.SetText(Lang.MenuText[Lang.menuText.victory][SettingsManager.Instance.language]);

		// play victory/defeat
        if (SettingsManager.Instance.GetIsSinglePlayer()) {
			if (losingPlayer == PlayerIDs.player1) {
				MusicManager.Instance.crossfade(/*MusicManager.Instance.nonSceneMusic[MusicManager.Instance.nonSceneMusic.FindIndex(s => s.cue == NonSceneMusicCues.Defeat)].musicEvent,*/ 2);
			}
			else {
				MusicManager.Instance.crossfade(/*MusicManager.Instance.nonSceneMusic[MusicManager.Instance.nonSceneMusic.FindIndex(s => s.cue == NonSceneMusicCues.Victory)].musicEvent,*/ 1);
			}
        }
		else if (SettingsManager.Instance.GetIsOnline()) {
			if ((OnlineManager.Instance.GetIsHost() && losingPlayer == PlayerIDs.player2)
				|| (!OnlineManager.Instance.GetIsHost() && losingPlayer == PlayerIDs.player1)) {
				MusicManager.Instance.crossfade(/*MusicManager.Instance.nonSceneMusic[MusicManager.Instance.nonSceneMusic.FindIndex(s => s.cue == NonSceneMusicCues.Victory)].musicEvent,*/ 1);
			}
			else {
				MusicManager.Instance.crossfade(/*MusicManager.Instance.nonSceneMusic[MusicManager.Instance.nonSceneMusic.FindIndex(s => s.cue == NonSceneMusicCues.Defeat)].musicEvent,*/ 2);
			}
		}
        else {
            MusicManager.Instance.crossfade(/*MusicManager.Instance.nonSceneMusic[MusicManager.Instance.nonSceneMusic.FindIndex(s => s.cue == NonSceneMusicCues.Victory)].musicEvent,*/ 1);
        }

        CameraParent.GetComponentInChildren<cameraShake>().ShakeTheCamera(0.125f, .25f);
        yield return new WaitForSeconds(.25f);
        CameraParent.GetComponentInChildren<cameraShake>().ShakeTheCamera(0.05f, 2.85f);
        yield return new WaitForSeconds(.35f);

        // fade out UI
        foreach (CanvasGroupFade fade in p1.GetComponentsInChildren<CanvasGroupFade>()) {
            fade.FadeOut();
        }
        foreach (CanvasGroupFade fade in p2.GetComponentsInChildren<CanvasGroupFade>()) {
            fade.FadeOut();
        }
        GetComponentInChildren<CanvasGroupFade>().FadeOut();
        GameObject.Find("Main Camera").GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));

        //Animation
        //if player 1 loses, player 1 plays lose anims and player 2 plays victory anims
        if (losingPlayer == PlayerIDs.player1) {
            stats.recordEndTime();  //	record end time for stats

            stats.recordWinner(PlayerIDs.player2);  // record player 1 as winner for stats

            foreach (GameObject unit in LivingUnitDictionary.dict[PlayerIDs.player1]) {
                if (unit) {
                    unit.GetComponent<UnitAI>().MyTeamLost();
                }
            } foreach (GameObject unit in LivingUnitDictionary.dict[PlayerIDs.player2]) {
                if (unit) {
                    unit.GetComponent<UnitAI>().MyTeamWon();
                }
            }

            if (SettingsManager.Instance.GetIsSinglePlayer()) {
                victoryUIText.SetText(Lang.MenuText[Lang.menuText.defeat][SettingsManager.Instance.language]);
            }
            else if (SettingsManager.Instance.GetIsOnline()) {
                if (OnlineManager.Instance.GetIsHost()) {
                    victoryUIText.SetText(Lang.MenuText[Lang.menuText.defeat][SettingsManager.Instance.language]);
                }
                else {
                    victoryUIText.SetText(Lang.MenuText[Lang.menuText.victory][SettingsManager.Instance.language]);
                }
            }
        }
        //if player 2 loses, player 2 plays lose anims and player 1 plays victory anims
        if (losingPlayer == PlayerIDs.player2) {
            stats.recordWinner(PlayerIDs.player1);  // record player 2 as winner for stats

            foreach (GameObject unit in LivingUnitDictionary.dict[PlayerIDs.player2]) {
                if (unit) {
                    unit.GetComponent<UnitAI>().MyTeamLost();
                }
            } foreach (GameObject unit in LivingUnitDictionary.dict[PlayerIDs.player1]) {
                if (unit) {
                    unit.GetComponent<UnitAI>().MyTeamWon();
                }
            }

            if (SettingsManager.Instance.GetIsOnline()) {
                if (OnlineManager.Instance.GetIsHost()) {
                    victoryUIText.SetText(Lang.MenuText[Lang.menuText.victory][SettingsManager.Instance.language]);
                }
                else {
                    victoryUIText.SetText(Lang.MenuText[Lang.menuText.defeat][SettingsManager.Instance.language]);
                }
            }
        }

        // if online, request steam stats and send game stats to p2 if the host
        if (SettingsManager.Instance.GetIsOnline()) {
			Steamworks.SteamUserStats.RequestCurrentStats();

			if (OnlineManager.Instance.GetIsHost()) {
				OnlineManager.Instance.SendPacket(new PO_StatSync(PlayerIDs.player2, stats.GetCurrentRecording()));
			}
        }

        VictoryCanvas.GetComponent<VictoryUIcontroller>().fadeAndSwitchCameras(CameraParent.GetComponent<endgameCameraMove>(), losingPlayer);

        if (!(SettingsManager.Instance.GetIsOnline() && !OnlineManager.Instance.GetIsHost())) {
            stats.saveRecording();  // start the process of saving the recording
        }

		//Get rid of Normal Game UI and normal game controls, sanitize dictionary
        player1Controller.enabled = false;
		player2Controller.enabled = false;
		InfluenceTileDictionary.NukeDictionary();

		yield return new WaitForSeconds(1.5f); // wait 5 for animation to play
		CameraParent.GetComponentInChildren<cameraShake>().ShakeTheCamera(0.125f, 2.25f);
		yield return new WaitForSeconds(2.25f);

		if (settings.GetMatchPlay()) {
			// increment winner's win count
			if (losingPlayer == PlayerIDs.player1) {
				m.IncrementP2Wins();
			}
			else {
				m.IncrementP1Wins();
			}

			// nobody has won yet, reload scene
			if (m.GetP1Wins() < (settings.GetBestOf() / 2) + 1 && m.GetP2Wins() < (settings.GetBestOf() / 2) + 1) {
				sc.setNextSceneName(sc.getNextSceneName());
				sc.LoadNextScene();
			}
			else {	// one player has won
				VictoryCanvas.GetComponent<VictoryUIcontroller>().showVictoryMenu(losingPlayer);

				// match is over
				Destroy(m.gameObject);

				yield return new WaitForSeconds(6f);

				GetComponent<MenuScripts>().StatScreen();
			}
		}
		else {
			VictoryCanvas.GetComponent<VictoryUIcontroller>().showVictoryMenu(losingPlayer);

			// match is over
			Destroy(m.gameObject);

			yield return new WaitForSeconds(6f);

            if (SettingsManager.Instance.GetIsConquest()) {
				ConquestManager.Instance.playerVictory = losingPlayer == PlayerIDs.player2;
				ConquestManager.Instance.nodesList = new List<GameObject>();

				Destroy(StatCollector.Instance.gameObject);

				sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.conquest1]);
				sc.LoadNextScene();	
            }
			else if (ArcadeManager.Instance)
			{
				if (losingPlayer == PlayerIDs.player1)
				{
					ArcadeManager.Instance.victory = false;
				}
				else
				{
					ArcadeManager.Instance.victory = true;
				}				
				sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.arcade]);
				sc.LoadNextScene();
			}
            else {
				GetComponent<MenuScripts>().StatScreen();
			}
		}
    }
	#endregion

	#region Cost Feedback
	public void CostFeedback(string playerID, string errorType) {
        colorTimer = 0f;
        StartCoroutine(CostFeedbackUpdate(playerID, errorType));
    }

    private IEnumerator CostFeedbackUpdate(string playerID, string errorType) {
        if (playerID == "Player0") {
            while (colorTimer < uiErrorFadeTime) //Timer to track color transition
            {
                colorTimer += Time.deltaTime;
                if (errorType == "gold" || errorType == "both") { //should the gold change?
                    gold1.color = Color.Lerp(uiErrorColor, Color.white, colorTimer); //lerp color from error to normal
				}
                if (errorType == "favor" || errorType == "both") { //should the favor change?
                    favor1.color = Color.Lerp(uiErrorColor, Color.white, colorTimer);
				}
                yield return null;
            }
        }
		else { //ADD TO IF WE DO MORE THAN 2 PLAYERS
            while (colorTimer < uiErrorFadeTime) {
                colorTimer += Time.deltaTime;
                if (errorType == "gold" || errorType == "both") {
                    gold2.color = Color.Lerp(uiErrorColor, Color.white, colorTimer);
				}
                if (errorType == "favor" || errorType == "both") {
                    favor2.color = Color.Lerp(uiErrorColor, Color.white, colorTimer);
				}
                yield return null;
            }
        }
    }
	#endregion

	// -------------------------- Passive private functions --------------------------
	#region Currency
	public void AddStartingGoldAll(float amount) {
		player1Controller.startingGold += amount;
		player2Controller.startingGold += amount;
	}

	public void AddStartingFavorAll(float amount) {
		player1Controller.startingFavor += amount;
		player2Controller.startingFavor += amount;
	}

	private void AddFavorOnUnitDie(UnitAI unit, string ownerPlayerKey, Constants.damageSource source) {
		try {
			player1Controller.addFavor(unit.ownerPlayerKey == PlayerIDs.player1 ? unit.favor : unit.favor * 2);
			player2Controller.addFavor(unit.ownerPlayerKey == PlayerIDs.player1 ? unit.favor * 2 : unit.favor);
		}
		catch {
			Debug.Log("player objects deleted before able to add favor");
		}
	}

	private void AddFavorOnKeepDamage(KeepManager keep, UnitAI unit = null) {
		try {
			if (unit != null) {
				player1Controller.addFavor(keep.rewiredPlayerKey == PlayerIDs.player1 ? 2 : 1);
				player2Controller.addFavor(keep.rewiredPlayerKey == PlayerIDs.player1 ? 1 : 2);
			}
		}
		catch {
			Debug.Log("player objects deleted before able to add favor");
		}
	}

	private void currencyGain() {
		player1Controller.addGold(((gp5 + (Constants.MINE_GP5 * player1Controller.GetMineCount()) + p1BonusGp5) / 5.0f) * Time.deltaTime);
		player1Controller.addFavor(((Constants.TEMPLE_FP5 * player1Controller.GetTempleCount() + p1BonusFp5) / 5.0f) * Time.deltaTime);
		if (player1Controller.getFavor() < 0) {
			player1Controller.SetFavor(0);
		}

		player2Controller.addGold(((gp5 + (Constants.MINE_GP5 * player2Controller.GetMineCount()) + player2Controller.GetExtraGold() + p2BonusGp5) / 5.0f) * Time.deltaTime);
		player2Controller.addFavor(((Constants.TEMPLE_FP5 * player2Controller.GetTempleCount() + p2BonusFp5) / 5.0f) * Time.deltaTime);
		if (player2Controller.getFavor() < 0) {
			player2Controller.SetFavor(0);
		}
	}

    public void HandleCurrencySync(PO_CurrencySync packet) {
        player1Controller.setGold(packet.p1Gold);
        player1Controller.SetFavor(packet.p1Favor);

        player2Controller.setGold(packet.p2Gold);
        player2Controller.SetFavor(packet.p2Favor);
    }

	private void displayGold() {
		if (!p1SpinGold) {
			gold1.SetText(((int)player1Controller.getGold() - (int)player1Controller.getGold() % 10).ToString());
		}
		if (!p2SpinGold) {
			gold2.SetText(((int)player2Controller.getGold() - (int)player2Controller.getGold() % 10).ToString());
		}
    }

	private void displayFavor() {
		if (!p1SpinFavor) {
			favor1.SetText(((int)Math.Floor(player1Controller.getFavor())).ToString());
		}
		if (!p2SpinFavor) {
			favor2.SetText(((int)Math.Floor(player2Controller.getFavor())).ToString());
		}
	}

	public void SpinCurrency(string playerID, bool gold, float amountSpent, float startingAmount) {
		StartCoroutine(SpinCurrencyWorker(playerID, gold, amountSpent, startingAmount));
	}

	IEnumerator SpinCurrencyWorker(string playerID, bool gold, float amountSpent, float startingAmount) {
		TextMeshProUGUI txt;
		float finalAmount;
		if (playerID == PlayerIDs.player1) {
			txt = gold ? gold1 : favor1;
			finalAmount = gold ? player1Controller.getGold() : player1Controller.getFavor();
			if (gold) {
				p1SpinGold = true;
			}
			else {
				p1SpinFavor = true;
			}
		}
		else {
			txt = gold ? gold2 : favor2;
			finalAmount = gold ? player2Controller.getGold() : player2Controller.getFavor();
			if (gold) {
				p2SpinGold = true;
			}
			else {
				p2SpinFavor = true;
			}
		}

		float currencyPerSecond = amountSpent / spinTime;
		float current = startingAmount;
		float elapsedTime = 0f;
		float frameTime;

		while (elapsedTime < spinTime) {
			frameTime = Time.deltaTime;
			txt.SetText(((int)current).ToString());
			current -= currencyPerSecond * frameTime;

			elapsedTime += frameTime;
			yield return null;
		}

		txt.SetText(((int)finalAmount).ToString());

		if (playerID == PlayerIDs.player1) {
			if (gold) {
				p1SpinGold = false;
			}
			else {
				p1SpinFavor = false;
			}
		}
		else {
			if (gold) {
				p2SpinGold = false;
			}
			else {
				p2SpinFavor = false;
			}
		}
	}
	#endregion

	#region Online Stuff
	public int GetNextEntityID() {
        int nextId = entityIdNextUsable;
        entityIdNextUsable += 2;
        return nextId;
    }

	public void WinOnOpponentDisconnect(bool playerOne) {
		if (!gameOver) {
			if (playerOne) {
				player2Controller.gameObject.GetComponentInChildren<KeepManager>().takeDamage(int.MaxValue);
			}
			else {
				player1Controller.gameObject.GetComponentInChildren<KeepManager>().takeDamage(int.MaxValue);
			}
		}
	}

	public void StartRegularSync() {
		StartCoroutine(RegularSync());
	}
	
	private IEnumerator RegularSync() {
		List<PacketObject> packetsList = new List<PacketObject>();
		while (true) {
			packetsList.Add(new PO_TimerSync(timer.GetTimeRemaining()));
			packetsList.Add(new PO_CurrencySync(PacketObject.packetType.currencySync, (int)player1Controller.getGold(), (int)player1Controller.getFavor(), (int)player2Controller.getGold(), (int)player2Controller.getFavor()));
			foreach (GameObject u in LivingUnitDictionary.dict[PlayerIDs.player1]) {
				PacketObject packet = null;
				try {
					packet = u.GetComponent<UnitAI>().GetSyncPacket();
				}
				catch {
					Debug.LogWarning("Catching error getting unit sync packet");
				}
				if (packet != null) {
					packetsList.Add(packet);
				}
			}
			foreach (GameObject u in LivingUnitDictionary.dict[PlayerIDs.player2]) {
				PacketObject packet = null;
				try {
					packet = u.GetComponent<UnitAI>().GetSyncPacket();
				}
				catch {
					Debug.LogWarning("Catching error getting unit sync packet");
				}
				if (packet != null) {
					packetsList.Add(packet);
				}
			}
			foreach (GameObject t in LivingTowerDictionary.dict[PlayerIDs.player1]) {
				PacketObject packet = null;
				try {
					packet = t.GetComponent<TowerState>().GetSyncPacket();
				}
				catch {
					Debug.LogWarning("Catching error getting tower sync packet");
				}
				if (packet != null) {
					packetsList.Add(packet);
				}
			}
			foreach (GameObject t in LivingTowerDictionary.dict[PlayerIDs.player2]) {
				PacketObject packet = null;
				try {
					packet = t.GetComponent<TowerState>().GetSyncPacket();
				}
				catch {
					Debug.LogWarning("Catching error getting tower sync packet");
				}
				if (packet != null) {
					packetsList.Add(packet);
				}
			}

			OnlineManager.Instance.SendPacket(new PO_Cluster(packetsList.ToArray()));

			packetsList.Clear();

			yield return new WaitForSeconds(OnlineManager.Instance.syncPollRate);
		}
	}
	#endregion

	// Inserts images into the UI buttons based on which blessings are active for each player
	private void setupUIForLoadouts() {
		for (int i = 0; i < blessings1Buttons.Count; i++) {
			blessings1Buttons[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player0").GetComponent<Blessing>().icon;
			blessings1ButtonBacks[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player0").GetComponent<Blessing>().icon;
			blessings1ButtonsMasked[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player0").GetComponent<Blessing>().icon;
			blessings1ButtonBacksMasked[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player0").GetComponent<Blessing>().icon;

			blessings2Buttons[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player1").GetComponent<Blessing>().icon;
			blessings2ButtonBacks[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player1").GetComponent<Blessing>().icon;
			blessings2ButtonsMasked[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player1").GetComponent<Blessing>().icon;
			blessings2ButtonBacksMasked[i].GetComponent<Image>().sprite = loadoutManager.getBlessingAssignment(i, "Player1").GetComponent<Blessing>().icon;
		}
	}

	public void ScorchLanes() {
		FireLanesManager.SetActive(true);
	}
}
