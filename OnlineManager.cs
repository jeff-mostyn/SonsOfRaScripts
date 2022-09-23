using System.Collections;
using System.Collections.Generic;
using Steamworks;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class OnlineManager : MonoBehaviour {
    public static OnlineManager Instance;

    public Friend opponent { get; private set; }
    public bool waitForLoad = false, opponentSceneLoad = false, selfSceneLoad = false;

    public string fakePlayerKey;

    public Steamworks.Data.Lobby currentLobby;
    public SteamId opponentId;

	[Header("Sync Frequencies")]
	public float syncPollRate;
    public float unitPollRate;

	[Header("Ping Stuff")]
	public int currentPingTime = -1;
	private Coroutine pingCoroutine = null;
	public Steamworks.Data.NetPingLocation? localLocation, partnerLocation;
	[SerializeField] private GameObject PingCanvas;

	[Header("Wellness Check")]
	public float disconnectTime;
	public float wellnessCheckFrequency;
	private float disconnectTimer, wellnessCheckTimer;
	private bool checkForMessages = false;
	private bool stopSending = false;
	private bool disconnected = false;


	#region Event Handlers
	void OnP2PSessionRequest_handler(SteamId steamid) {
        // If we want to let this steamid talk to us
        SteamNetworking.AcceptP2PSessionWithUser(steamid);
		opponentId = steamid;
    }

   void OnP2PConnectionFailed_handler (SteamId steamid, P2PSessionError error) {
        SteamNetworking.CloseP2PSessionWithUser(steamid);
        Debug.LogError(error);
		partnerLocation = null;
		currentPingTime = -1;

		if (Constants.mapNames.ContainsValue(SceneManager.GetActiveScene().name)) {
			LobbyManager.Instance.StartDisconnectSequence();
		}
		if (pingCoroutine != null) {
			StopCoroutine(pingCoroutine);
			pingCoroutine = null;
		}
	}
#endregion

	private void Awake() {
        if (Instance == null) {
            DontDestroyOnLoad(gameObject); // Don't destroy this object
            Instance = this;

            fakePlayerKey = "";

            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest_handler;
            SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailed_handler;

            SceneManager.sceneLoaded += OnSceneLoaded;

			SteamNetworking.AllowP2PPacketRelay(true);
		}
        else {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        Constants.gameScenes currentScene = Constants.sceneCodes[scene.name];

		disconnected = false;

		if (opponentSceneLoad == selfSceneLoad) {
            opponentSceneLoad = false;
            selfSceneLoad = false;
        }

		// this should never exist in the main menu
		if (currentScene == Constants.gameScenes.mainMenu) {
			checkForMessages = false;
			stopSending = true;
			Destroy(gameObject);
		}
		else {
			localLocation = SteamNetworkingUtils.LocalPingLocation;
			Debug.Log("Ping Location: " + localLocation.ToString());

			if (Constants.mapNames.ContainsKey(currentScene)) {
				SendPacket(new PO_Handshake(localLocation.ToString()));
				checkForMessages = true;
				stopSending = false;
				wellnessCheckTimer = wellnessCheckFrequency;
				disconnectTimer = disconnectTime;
			}
			else {
				checkForMessages = false;
				stopSending = true;
			}
		}
    }

    // Update is called once per frame
    void Update() {
        while (SteamNetworking.IsP2PPacketAvailable()) {
            var packet = SteamNetworking.ReadP2PPacket();
            if (packet.HasValue) {
                HandleMessageFrom(packet.Value.SteamId, packet.Value.Data);
            }
        }

//		if (checkForMessages) {
//			wellnessCheckTimer -= Time.deltaTime;
//			if (wellnessCheckTimer <= 0 && !stopSending) {
//				SendPacket(new PO_WellnessCheck());
//				wellnessCheckTimer = wellnessCheckFrequency;
//			}

//			disconnectTimer -= Time.deltaTime;
//			if (disconnectTimer <= 0 && !disconnected && LobbyManager.Instance != null) {
//				// disconnect from the game
//				if (opponent.Id.IsValid) {
//					SteamNetworking.CloseP2PSessionWithUser(opponent.Id);
//				}
//				LobbyManager.Instance.StartDisconnectSequence();
//				disconnected = true;
//			}
//		}

//#if UNITY_EDITOR
//		if (Input.GetKeyDown(KeyCode.H)) {
//			stopSending = !stopSending;
//		}
//#endif
	}

	private void OnDestroy() {
        SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest_handler;
        SteamNetworking.OnP2PConnectionFailed -= OnP2PConnectionFailed_handler;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region PacketHandling
    public void SendPacket(PacketObject p) {
        BinaryFormatter formatter = new BinaryFormatter();
        byte[] bytes;

        using (MemoryStream stream = new MemoryStream()) {
            formatter.Serialize(stream, p);
            bytes = stream.ToArray();

            try {
                SteamNetworking.SendP2PPacket(opponent.Id, bytes, bytes.Length);
            }
            catch {
                Debug.LogError("Unable to send P2P packet to " + opponent.Name);
            }
        }
    }

    void HandleMessageFrom(SteamId steamid, byte[] data) {
        PacketObject obj;

        // parse data into packet object
        using (MemoryStream stream = new MemoryStream(data)) {
            BinaryFormatter formatter = new BinaryFormatter();
            obj = (PacketObject)formatter.Deserialize(stream);
        }

		SelectPacketOperation(obj, steamid);
    }

	private void SelectPacketOperation(PacketObject obj, SteamId steamid) {
		switch (obj.type) {
			case PacketObject.packetType.cluster:
				HandlePacketCluster((PO_Cluster)obj, steamid);
				break;
			case PacketObject.packetType.handshake:
				HandleHandshake((PO_Handshake)obj, steamid);
				break;
			case PacketObject.packetType.loadSignal:
				HandleLoadSignal(steamid);
				break;
			case PacketObject.packetType.timerSync:
				HandleTimerSync((PO_TimerSync)obj);
				break;
			case PacketObject.packetType.statSync:
				HandleStatSync((PO_StatSync)obj);
				break;
			case PacketObject.packetType.currencySync:
				HandleCurrencySync((PO_CurrencySync)obj);
				break;
			case PacketObject.packetType.unitQueue:
				HandleUnitQueue((PO_UnitQueue)obj);
				break;
			case PacketObject.packetType.unitSpawn:
				HandleUnitSpawn((PO_UnitSpawn)obj);
				break;
			case PacketObject.packetType.unitSync:
				HandleUnitSync((PO_UnitSync)obj);
				break;
			case PacketObject.packetType.unitDestroy:
				HandleUnitDestroy((PO_Entity_Destroy)obj);
				break;
			case PacketObject.packetType.catapultSync:
				HandleCatapultSync((PO_CatapultSync)obj);
				break;
			case PacketObject.packetType.unitTargetSync:
				HandleUnitTargetSync((PO_UnitTargetSync)obj);
				break;
			case PacketObject.packetType.catapultTargetSync:
				HandleCatapultTargetSync((PO_UnitTargetSync)obj);
				break;
			case PacketObject.packetType.towerSpawn:
				HandleTowerSpawn((PO_TowerSpawn)obj);
				break;
			case PacketObject.packetType.towerTargetSync:
				HandleTowerTargetSync((PO_TowerTargetSync)obj);
				break;
			case PacketObject.packetType.towerSync:
				HandleTowerSync((PO_TowerSync)obj);
				break;
			case PacketObject.packetType.towerDestroy:
				HandleTowerDestroy((PO_Entity_Destroy)obj);
				break;
			case PacketObject.packetType.blessingCast:
				HandleBlessingCast((PO_BlessingCast)obj);
				break;
			case PacketObject.packetType.playerUnlock:
				HandlePlayerUnlock((PO_PlayerUnlock)obj);
				break;
			case PacketObject.packetType.playerUpgrade:
				HandlePlayerUpgrade((PO_PlayerUpgrade)obj);
				break;
			case PacketObject.packetType.keepDamage:
				HandleKeepDamage((PO_KeepDamage)obj);
				break;
			case PacketObject.packetType.battleHardenedSync:
				HandleBattleHardenedSync((PO_BattleHardenedSync)obj);
				break;
			case PacketObject.packetType.rematchRequest:
				LobbyManager.Instance.handleRematchRequest(((PO_Rematch)obj).friendName);
				break;
			case PacketObject.packetType.weGoodBro:
				disconnectTimer = disconnectTime;
				break;
			default:
				break;
		}
	}

	private void HandleHandshake(PO_Handshake packet, SteamId steamid) {
		partnerLocation = Steamworks.Data.NetPingLocation.TryParseFromString(packet.pingLocation);
		Debug.Log("Partner Ping Location: " + partnerLocation.ToString());
	}

	private void HandlePacketCluster(PO_Cluster packet, SteamId steamid) {
		for (int i = 0; i < packet.cluster.Length; i++) {
			SelectPacketOperation(packet.cluster[i], steamid);
		}
	}

    private void HandleLoadSignal(SteamId sender) {
        opponentSceneLoad = true;
    }

    private void HandleTimerSync(PO_TimerSync packet) {
		try {
			GameManager.Instance.timer.SetTimeRemaining(packet.timeRemaining);
		}
		catch {
			Debug.LogWarning("Timer not found. Likely GameManager did not exist at time of function call");
		}
    }

    private void HandleCurrencySync(PO_CurrencySync packet) {
        GameManager.Instance.HandleCurrencySync(packet);
    }

    private void HandleStatSync(PO_StatSync packet) {
        ((Human_PlayerController)GameManager.Instance.player2Controller).SyncStats(packet.stats);
    }

    #region Units
    private void HandleUnitQueue(PO_UnitQueue packet) {
        UnitSpawner uSpawner = packet.rewiredPlayerKey == PlayerIDs.player1 ? GameObject.Find("Player1").GetComponent<UnitSpawner>() : GameObject.Find("Player2").GetComponent<UnitSpawner>();

        uSpawner.addToSpawnQueue(packet.unitTypeIndex, packet.lane, packet.entityID);
    }

    private void HandleUnitSpawn(PO_UnitSpawn packet) {
        UnitSpawner uSpawner = packet.rewiredPlayerKey == PlayerIDs.player1 ? GameObject.Find("Player1").GetComponent<UnitSpawner>() : GameObject.Find("Player2").GetComponent<UnitSpawner>();

        uSpawner.SpawnUnit(packet.unitTypeIndex, packet.lane, new Vector3(packet.spawnPosition[0], packet.spawnPosition[1], packet.spawnPosition[2]), packet.nextWaypoint, packet.entityID);
    }

    private void HandleUnitSync(PO_UnitSync packet) {
        try {
            UnitAI_Infantry ai = LivingUnitDictionary.dict[packet.ownerPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<UnitAI_Infantry>();

            if (ai.gameObject != null) {
                ai.Sync(packet.enemyEntityID, packet.position, packet.armor, packet.blockStacks, packet.movementSpeedModifier, packet.attackSpeedModifier, packet.damageModifier, packet.health, packet.shield, packet.nextWaypoint);
            }
        }
        catch {
            Debug.LogWarning("there was a problem finding the AI for unit sync");
        }
    }

    private void HandleCatapultSync(PO_CatapultSync packet) {
        try {
            UnitAI_Catapult ai = LivingUnitDictionary.dict[packet.ownerPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<UnitAI_Catapult>();

            if (ai.gameObject != null) {
                ai.Sync(packet.enemyEntityID, packet.position, packet.armor, packet.blockStacks, packet.movementSpeedModifier, packet.attackSpeedModifier, packet.damageModifier, packet.health, packet.shield, packet.waypointIndex);
            }
        }
        catch {
            Debug.LogWarning("there was a problem finding the AI for catapult sync");
        }
    }

    private void HandleUnitTargetSync(PO_UnitTargetSync packet) {
        try {
            UnitAI_Catapult ai = LivingUnitDictionary.dict[packet.ownerPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<UnitAI_Catapult>();

			try {
				if (ai.gameObject != null) {
					ai.TargetSync(packet.enemyEntityID);
				}
			}
			catch {
				Debug.LogWarning("Problem syncing target for infantry unit");
			}
        }
        catch {
            Debug.LogWarning("there was a problem finding the AI for unit target sync");
        }
    }

	private void HandleCatapultTargetSync(PO_UnitTargetSync packet) {
		try {
			UnitAI_Infantry ai = LivingUnitDictionary.dict[packet.ownerPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<UnitAI_Infantry>();

			try {
				if (ai.gameObject != null) {
					ai.TargetSync(packet.enemyEntityID);
				}
			}
			catch {
				Debug.LogWarning("Problem syncing target for catapult");
			}
		}
		catch {
			Debug.LogWarning("there was a problem finding the AI for unit target sync");
		}
	}

	private void HandleBattleHardenedSync(PO_BattleHardenedSync packet) {
		try {
			UnitAI_Infantry ai = LivingUnitDictionary.dict[packet.ownerPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<UnitAI_Infantry>();
			BattleHardened buff = (BattleHardened)ai.activeEffects.Find(x => x.type == BuffDebuff.BuffsAndDebuffs.battleHardened);

			if (buff != null) {
				buff.SetStacks(packet.stackCount);
			}
		}
		catch {
			Debug.LogWarning("there was a problem finding the AI for unit target sync");
		}
	}

	private void HandleUnitDestroy(PO_Entity_Destroy packet) {
		try {
			GameObject unit = null;
			try {
				unit = LivingUnitDictionary.dict[packet.ownerPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID);
			}
			catch {
				unit = null;
			}

			if (unit != null) {
				unit.GetComponent<UnitAI>().Die();
				Debug.Log("unit destroyed");
			}
		}
		catch {
			Debug.LogWarning("Unit was not found in dictionary or could not be destroyed. Should already have been destroyed.");
		}
	}
	#endregion

	#region Towers
	private void HandleTowerSync(PO_TowerSync packet) {
        try {
            TowerState tState = LivingTowerDictionary.dict[packet.rewiredPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<TowerState>();

            if (tState.gameObject != null) {
                tState.Sync(packet.health, packet.shield, packet.stunned, packet.power, packet.attackSpeed);
            }
        }
        catch {
            Debug.LogWarning("there was a problem finding the Tower State for tower sync");
        }
    }

    private void HandleTowerTargetSync(PO_TowerTargetSync packet) {
        try {
            TowerState tState = LivingTowerDictionary.dict[packet.rewiredPlayerKey].Find(u => u.GetComponent<EntityIdentifier>().ID == packet.entityID).GetComponent<TowerState>();

            if (tState.gameObject != null) {
                tState.TargetSync(packet.enemyEntityID);
            }
        }
        catch {
            Debug.LogWarning("there was a problem finding the Tower State for tower target sync");
        }
    }

    private void HandleTowerSpawn(PO_TowerSpawn packet) {
        Human_PlayerController pController = packet.rewiredPlayerKey == PlayerIDs.player1 ? (Human_PlayerController)GameManager.Instance.player1Controller : (Human_PlayerController)GameManager.Instance.player2Controller;

        pController.ForcePlaceTower(packet.towerType, packet.entityID, packet.tileName, packet.yRotation);
    }

	private void HandleTowerDestroy(PO_Entity_Destroy packet) {
		try {
			GameObject tower = LivingTowerDictionary.dict[packet.ownerPlayerKey].Find(t => t.GetComponent<EntityIdentifier>().ID == packet.entityID);

			//Destroy(tower);
			tower.GetComponentInChildren<TowerHealth>().Die();
			Debug.Log("tower destroyed");
		}
		catch {
			Debug.LogWarning("Tower was not found in dictionary or could not be destroyed. Should already have been destroyed.");
		}
	}
	#endregion

	private void HandleBlessingCast(PO_BlessingCast packet) {
        Human_PlayerController pController = packet.rewiredPlayerKey == PlayerIDs.player1 ? (Human_PlayerController)GameManager.Instance.player1Controller : (Human_PlayerController)GameManager.Instance.player2Controller;

        pController.ForceBlessingUse(packet);
    }

    private void HandleKeepDamage(PO_KeepDamage packet) {
        KeepManager k = packet.rewiredPlayerKey == PlayerIDs.player1 ? GameObject.Find("Player1").GetComponentInChildren<KeepManager>() : GameObject.Find("Player2").GetComponentInChildren<KeepManager>();

        k.takeDamage(packet.damage);
    }

    private void HandlePlayerUnlock(PO_PlayerUnlock packet) {
        Human_PlayerController pController = packet.rewiredPlayerKey == PlayerIDs.player1 ? (Human_PlayerController)GameManager.Instance.player1Controller : (Human_PlayerController)GameManager.Instance.player2Controller;

        pController.SyncUnlock(packet.unlockedItem);
    }

    private void HandlePlayerUpgrade(PO_PlayerUpgrade packet) {
        Human_PlayerController pController = packet.rewiredPlayerKey == PlayerIDs.player1 ? (Human_PlayerController)GameManager.Instance.player1Controller : (Human_PlayerController)GameManager.Instance.player2Controller;

        pController.SyncUpgrade(packet.expansion);
    }
	#endregion

	#region Getters and Setters
	public void SetOpponent(Friend _opponent) {
        opponent = _opponent;
    }

    public bool GetIsHost() {
        return fakePlayerKey == PlayerIDs.player1;
    }
	#endregion

	#region Ping
	public void StartPing() {
		if (pingCoroutine == null) {
			pingCoroutine = StartCoroutine(PingLoop());
		}
	}

	private IEnumerator PingLoop() {
		while (true) {
			if (partnerLocation != null) {
				currentPingTime = SteamNetworkingUtils.EstimatePingTo((Steamworks.Data.NetPingLocation)partnerLocation);
			}
			else {
				currentPingTime = -1;
			}
			PingCanvas.GetComponent<CanvasGroup>().alpha = currentPingTime != -1 ? 1 : 0;
			yield return new WaitForSeconds(3f);
		}
	}
	#endregion
}
