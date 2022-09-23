using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour {
	// ------------------- public variables -----------------------
	[Header("References")]
	public List<GameObject> units;
	public List<Transform> spawnpoints;
    public List<Material> catapultMat;
	public List<Material> unitMat;
    public List<Image> topQueueImageLocations;
	public List<Image> midQueueImageLocations;
	public List<Image> botQueueImageLocations;
	[SerializeField] private Text topOverflowText, midOverflowText, botOverflowText;
	public List<Sprite> unitQueueIcons;

	[Header("Values")]
	public List<float> unitBuildTime;
	public float scaleUpTime;
	public float topBuildCountdown;
	public float midBuildCountdown;
	public float botBuildCountdown;
	public float overshield;
	public float fightBuffUnitDamageBoost;
	public float fightBuffDiscount = 0;

	// ------------------ private variables -----------------------
	// references
	private StatCollector stats;
    private Material embPriestMat;
    [SerializeField] Texture lvl2EmisMap;
    private PlayerController playerController;

	// gameplay values
	private Queue<int> topLaneQueue, midLaneQueue, botLaneQueue;
    private Queue<int> topLaneIdQueue, midLaneIdQueue, botLaneIdQueue;
    private List<Sprite> topLaneImages;
	private List<Sprite> midLaneImages;
	private List<Sprite> botLaneImages;
	private string pID;
	private Dictionary<Constants.unitType, float> unitGoldCosts;
	[SerializeField] private int trainingGrounds = 0;

	#region System Functions
	private void Start() {
		stats = StatCollector.Instance;

		pID = playerController.rewiredPlayerKey;

		topLaneQueue = new Queue<int>();
		midLaneQueue = new Queue<int>();
		botLaneQueue = new Queue<int>();

        topLaneIdQueue = new Queue<int>();
        midLaneIdQueue = new Queue<int>();
        botLaneIdQueue = new Queue<int>();

        topLaneImages = new List<Sprite>();
		midLaneImages = new List<Sprite>();
		botLaneImages = new List<Sprite>();

		for (int i = 0; i<topLaneImages.Count; i++) {
			topQueueImageLocations[i].gameObject.SetActive(false);
			midQueueImageLocations[i].gameObject.SetActive(false);
			botQueueImageLocations[i].gameObject.SetActive(false);
		}

		// set up a gold cost dictionary for ease of access
		unitGoldCosts = new Dictionary<Constants.unitType, float>();
		for (int i = 0; i<units.Count; i++) {
			UnitAI tempAI = units[i].GetComponent<UnitAI>();
			if (tempAI.getCost() != 0) {
				unitGoldCosts.Add(units[i].GetComponent<UnitAI>().type, (int)Mathf.Ceil(units[i].GetComponent<UnitAI>().getCost() * (1-fightBuffDiscount)));
			}
		}
		unitGoldCosts.Add(Constants.unitType.embalmPriest, 0);
		unitGoldCosts.Add(Constants.unitType.mummy, 0);
		unitGoldCosts.Add(Constants.unitType.huntress, 0);

		//set up main material colors
		LoadoutManager l;
		if (SettingsManager.Instance.GetIsTutorial())
		{
			l = GameObject.Find("TutorialManager(Clone)").GetComponent<TutorialManager>().tutorialLoadout.GetComponent<LoadoutManager>();
		}
		else
		{
			l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
		}
		// base catapult mat
        catapultMat[0] = Instantiate(catapultMat[0]);
        catapultMat[0].SetColor("_PalCol1", l.getPaletteColor(0, pID));
        catapultMat[0].SetColor("_PalCol2", l.getPaletteColor(1, pID));
		// level 1 catapult mat
		catapultMat[1] = Instantiate(catapultMat[1]);
		catapultMat[1].SetColor("_PalCol1", l.getPaletteColor(2, pID));
		catapultMat[1].SetColor("_PalCol2", l.getPaletteColor(0, pID));
		// level 2 catapult mat
		catapultMat[2] = Instantiate(catapultMat[2]);
		catapultMat[2].SetColor("_PalCol1", l.getPaletteColor(2, pID));
		catapultMat[2].SetColor("_PalCol2", l.getPaletteColor(1, pID));

		unitMat[0] = Instantiate(unitMat[0]);
        unitMat[0].SetColor("_PalCol1", l.getPaletteColor(0, pID));
        unitMat[0].SetColor("_PalCol2", l.getPaletteColor(1, pID));
        //level 1 unit mat
        unitMat[1] = Instantiate(unitMat[1]);
        unitMat[1].SetColor("_PalCol1", l.getPaletteColor(2, pID));
        unitMat[1].SetColor("_PalCol2", l.getPaletteColor(0, pID));
        //level 2 unit mat
        unitMat[2] = Instantiate(unitMat[2]);
        unitMat[2].SetColor("_PalCol1", l.getPaletteColor(2, pID));
        unitMat[2].SetColor("_PalCol2", l.getPaletteColor(1, pID));
        //level 2 emissive element
        //unitMat[2].SetColor("_EmisColor", l.getPaletteColor(1, pID));
        //unitMat[2].SetTexture("_EmisMap", lvl2EmisMap);
    }

	private void Update() {
		if (topLaneQueue.Count > 0) {
			if (topBuildCountdown <= 0) {
				SpawnUnit(topLaneQueue.Dequeue(), "top", topLaneIdQueue.Dequeue());
				topLaneImages.RemoveAt(0);

				if (topLaneQueue.Count > 0) {
					topBuildCountdown = unitBuildTime[topLaneQueue.Peek()];

					// populate images
					for (int i = 0; i < 6; i++) {
						if (i < topLaneImages.Count) {
							topQueueImageLocations[i].sprite = topLaneImages[i];
							topQueueImageLocations[i].gameObject.SetActive(true);
						}
						else {
							topQueueImageLocations[i].sprite = null;
							topQueueImageLocations[i].gameObject.SetActive(false);
						}
					}

					topQueueImageLocations[6].sprite = topLaneImages[0];
					topQueueImageLocations[6].gameObject.SetActive(true);
				}
				else {
					topQueueImageLocations[6].gameObject.SetActive(false);
				}
			}
			topBuildCountdown -= Time.deltaTime;
		}
		if (midLaneQueue.Count > 0) {
			if (midBuildCountdown <= 0) {
				SpawnUnit(midLaneQueue.Dequeue(), "mid", midLaneIdQueue.Dequeue());
				midLaneImages.RemoveAt(0);

				if (midLaneQueue.Count > 0) {
					midBuildCountdown = unitBuildTime[midLaneQueue.Peek()];

					// populate images
					for (int i = 0; i < 6; i++) {
						if (i < midLaneImages.Count) {
							midQueueImageLocations[i].sprite = midLaneImages[i];
							midQueueImageLocations[i].gameObject.SetActive(true);
						}
						else {
							midQueueImageLocations[i].sprite = null;
							midQueueImageLocations[i].gameObject.SetActive(false);
						}			
					}

					midQueueImageLocations[6].sprite = midLaneImages[0];
					midQueueImageLocations[6].gameObject.SetActive(true);
				}
				else {
					midQueueImageLocations[6].gameObject.SetActive(false);
				}
			}
			midBuildCountdown -= Time.deltaTime;
		}
		if (botLaneQueue.Count > 0) {
			if (botBuildCountdown <= 0) {
				SpawnUnit(botLaneQueue.Dequeue(), "bot", botLaneIdQueue.Dequeue());
				botLaneImages.RemoveAt(0);

				if (botLaneQueue.Count > 0) {
					botBuildCountdown = unitBuildTime[botLaneQueue.Peek()];

					// populate images
					for (int i = 0; i < 6; i++) {
						if (i < botLaneImages.Count) {
							botQueueImageLocations[i].sprite = botLaneImages[i];
							botQueueImageLocations[i].gameObject.SetActive(true);
						}
						else {
							botQueueImageLocations[i].sprite = null;
							botQueueImageLocations[i].gameObject.SetActive(false);
						}
					}

					botQueueImageLocations[6].sprite = botLaneImages[0];
					botQueueImageLocations[6].gameObject.SetActive(true);
				}
				else {
					botQueueImageLocations[6].gameObject.SetActive(false);
				}
			}
			botBuildCountdown -= Time.deltaTime;
		}

		if (topLaneQueue.Count > 6) {
			topOverflowText.gameObject.SetActive(true);
			topOverflowText.text = "+" + (topLaneQueue.Count - 6).ToString();
		}
		else {
			topOverflowText.gameObject.SetActive(false);
		}
		if (midLaneQueue.Count > 6) {
			midOverflowText.gameObject.SetActive(true);
			midOverflowText.text = playerController.rewiredPlayerKey == PlayerIDs.player1 ? "+" + (midLaneQueue.Count - 6).ToString()
				: (midLaneQueue.Count - 6).ToString() + "+";
		}
		else {
			midOverflowText.gameObject.SetActive(false);
		}
		if (botLaneQueue.Count > 6) {
			botOverflowText.gameObject.SetActive(true);
			botOverflowText.text = "+" + (botLaneQueue.Count - 6).ToString();
		}
		else {
			botOverflowText.gameObject.SetActive(false);
		}
	}

	#endregion

	#region Spawning Functions
	public void SpawnUnit(int unitID, string lane, int entityId) {
		int index = 0;
        Vector3 rot = new Vector3(0, -90, 0);//This will be the rotation of the unit on spawn depedant on lane.

        if (lane == "mid") {
            index = 1;
            if (pID == PlayerIDs.player1) {
                rot = new Vector3(0, 0, 0);
            }
            else if (pID == PlayerIDs.player2) {
                rot = new Vector3(0, -180, 0);
            }
        }
        else if (lane == "bot") {
            index = 2;
            rot = new Vector3(0, 90, 0);
        }

        GameObject unitToSpawn = Instantiate(units[unitID], spawnpoints[index].position, Quaternion.identity);
		UnitAI unitToSpawnAI = unitToSpawn.GetComponent<UnitAI>();
		unitToSpawn.GetComponent<EntityIdentifier>().ID = entityId;

		unitToSpawn.transform.localScale = new Vector3(0, 0, 0);
		StartCoroutine(SpawnScale(unitToSpawn));

		stats.recordUnitsSpawned(pID, unitID); // record spawning of unit for stats

		unitToSpawn.transform.eulerAngles = rot;//Sets rotation of gameobject

		// set unitMovement values
		UnitMovement unitToSpawnMovement = unitToSpawn.GetComponent<UnitMovement>();
		unitToSpawnMovement.SetupSpawn(lane, pID);

		List<GameObject> uList = new List<GameObject>();
        if(LivingUnitDictionary.dict.TryGetValue(pID, out uList)) {
            uList.Add(unitToSpawn);            
        }

		AssignUnitMat(unitToSpawn);
		TrainingGroundStatChange(unitToSpawn);

		unitToSpawnAI.addShield(overshield);
		unitToSpawnAI.damageModifier += fightBuffUnitDamageBoost;
		unitToSpawnAI.ownerPlayerKey = pID;
	}

	public void SpawnUnit(int unitID, string lane, Vector3 spawnPos, int nextWaypointIndex, int entityId) {
		Vector3 rot = new Vector3(0, -90, 0);//This will be the rotation of the unit on spawn depedant on lane.

		if (lane == "mid") {
			if (pID == PlayerIDs.player1) {
				rot = new Vector3(0, 0, 0);
			}
			else if (pID == PlayerIDs.player2) {
				rot = new Vector3(0, -180, 0);
			}
		}
		else if (lane == "bot") {
			rot = new Vector3(0, 90, 0);
		}

		GameObject unitToSpawn = Instantiate(units[unitID], spawnPos, Quaternion.identity);
		UnitAI unitToSpawnAI = unitToSpawn.GetComponent<UnitAI>();
		unitToSpawn.GetComponent<EntityIdentifier>().ID = entityId;

        unitToSpawn.transform.localScale = new Vector3(0, 0, 0);
		StartCoroutine(SpawnScale(unitToSpawn));

		stats.recordUnitsSpawned(pID, unitID); // record spawning of unit for stats

		unitToSpawn.transform.eulerAngles = rot;//Sets rotation of gameobject

		// set unitMovement values
		UnitMovement unitToSpawnMovement = unitToSpawn.GetComponent<UnitMovement>();
		unitToSpawnMovement.SetupSpawn(lane, pID, nextWaypointIndex);

		List<GameObject> uList = new List<GameObject>();
		if (LivingUnitDictionary.dict.TryGetValue(pID, out uList)) {
			uList.Add(unitToSpawn);
		}

		AssignUnitMat(unitToSpawn);
		TrainingGroundStatChange(unitToSpawn);

		unitToSpawnAI.addShield(overshield);
		unitToSpawnAI.damageModifier += fightBuffUnitDamageBoost;
		unitToSpawnAI.ownerPlayerKey = pID;
	}
	#endregion

	private void AssignUnitMat(GameObject unitToSpawn) {
		// assign material
		if (unitToSpawn.GetComponent<UnitAI>().getType() == Constants.unitType.catapult) {
			MeshRenderer[] renderers = unitToSpawn.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < renderers.Length; i++) {
				renderers[i].material = catapultMat[trainingGrounds];
			}
			SkinnedMeshRenderer[] skinnedrenderers = unitToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < skinnedrenderers.Length; i++) {
				skinnedrenderers[i].material = catapultMat[trainingGrounds];
			}
		}
		else {
			unitToSpawn.GetComponentsInChildren<MeshRenderer>()[0].material = unitMat[trainingGrounds];
			if (unitToSpawn.GetComponent<UnitAI>().getType() != Constants.unitType.mummy) {	// mummies only have one mesh renderer
				unitToSpawn.GetComponentsInChildren<MeshRenderer>()[1].material = unitMat[trainingGrounds];
			}
			unitToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>()[0].material = unitMat[trainingGrounds];
		}
	}
	
	private void TrainingGroundStatChange(GameObject unitToSpawn) {
		UnitAI uAI = unitToSpawn.GetComponent<UnitAI>();
		uAI.armor -= Constants.TRAINING_GROUND_ARMOR_BOOST * trainingGrounds;
		uAI.adjustDamageModifier(Constants.TRAINING_GROUND_ATK_BOOST * trainingGrounds);
	}

	public void addToSpawnQueue(int i, string lane, int entityId = -1) {
		GameObject unitToQueue = units[i];
		UnitAI unitToQueueAI = (UnitAI)unitToQueue.GetComponent<UnitAI>();

		// adds unit to spawn queue if you have enough gold, spends gold equal to cost of said unit
		if (playerController.getGold() >= unitGoldCosts[unitToQueueAI.type] || entityId != -1) {
            // send packet if we're online, and if the local player is the one spawning
            // the local player doesnt specify entity id when they add to queue
            if (SettingsManager.Instance.GetIsOnline() && entityId == -1) {
                entityId = GameManager.Instance.GetNextEntityID();

                PO_UnitQueue unitQueuePacket = new PO_UnitQueue(playerController.rewiredPlayerKey, lane, i, entityId);
                OnlineManager.Instance.SendPacket(unitQueuePacket);
            }

            if (lane == "top") {
				topLaneImages.Add(unitQueueIcons[i]);
				topLaneQueue.Enqueue(i);
                topLaneIdQueue.Enqueue(entityId);
				if (topLaneQueue.Count == 1) {
					topBuildCountdown = unitBuildTime[i];
					topQueueImageLocations[0].gameObject.SetActive(true);
					topQueueImageLocations[0].sprite = topLaneImages[0];

					topQueueImageLocations[6].gameObject.SetActive(true);
					topQueueImageLocations[6].sprite = topLaneImages[0];
				}
				else if (topLaneQueue.Count <= 6) {
					topQueueImageLocations[topLaneQueue.Count - 1].gameObject.SetActive(true);
					topQueueImageLocations[topLaneQueue.Count - 1].sprite = topLaneImages[topLaneQueue.Count - 1];
				}
			}
			else if (lane == "mid") {
				midLaneImages.Add(unitQueueIcons[i]);
				midLaneQueue.Enqueue(i);
                midLaneIdQueue.Enqueue(entityId);
                if (midLaneQueue.Count == 1) {
					midBuildCountdown = unitBuildTime[i];
					midQueueImageLocations[0].gameObject.SetActive(true);
					midQueueImageLocations[0].sprite = midLaneImages[0];

					midQueueImageLocations[6].gameObject.SetActive(true);
					midQueueImageLocations[6].sprite = midLaneImages[0];
				}
				else if (midLaneQueue.Count <= 6) {
					midQueueImageLocations[midLaneQueue.Count - 1].gameObject.SetActive(true);
					midQueueImageLocations[midLaneQueue.Count - 1].sprite = midLaneImages[midLaneQueue.Count - 1];
				}
			}
			else {
				botLaneImages.Add(unitQueueIcons[i]);
				botLaneQueue.Enqueue(i);
                botLaneIdQueue.Enqueue(entityId);
                if (botLaneQueue.Count == 1) {
					botBuildCountdown = unitBuildTime[i];
					botQueueImageLocations[0].gameObject.SetActive(true);
					botQueueImageLocations[0].sprite = botLaneImages[0];

					botQueueImageLocations[6].gameObject.SetActive(true);
					botQueueImageLocations[6].sprite = botLaneImages[0];
				}
				else if (botLaneQueue.Count <= 6) {
					botQueueImageLocations[botLaneQueue.Count - 1].gameObject.SetActive(true);
					botQueueImageLocations[botLaneQueue.Count - 1].sprite = botLaneImages[botLaneQueue.Count - 1];
				}
			}
			// spend gold. if the player does not have enough, (there is a mismatch between sides in online play is the only situation where this would happen) it spends all they have
			playerController.spendGold(playerController.getGold() >= unitGoldCosts[unitToQueueAI.type] ? unitGoldCosts[unitToQueueAI.type] : playerController.getGold());

			stats.recordUnitGold(pID, unitGoldCosts[unitToQueueAI.type]);	// record gold spent on unit for stats
		}
		else { // not enough money
			if (playerController.rewiredPlayerKey == PlayerIDs.player1 || !SettingsManager.Instance.GetIsSinglePlayer()) {
				GetComponent<PlayerController>().purchaseErrorFeedback("gold");
			}
			else {
				GetComponent<AI_PlayerController>().purchaseErrorFeedback("gold");
			}
		}
	}

	public void ClearQueue() {
		topLaneQueue.Clear();
		midLaneQueue.Clear();
		botLaneQueue.Clear();
	}

	private IEnumerator SpawnScale(GameObject unit) {
        float scalePerSecond = 1 / scaleUpTime;

		while (unit && unit.transform.localScale.x < 1) {
			float scaleToAdd = scalePerSecond * Time.deltaTime;
			unit.transform.localScale = new Vector3(unit.transform.localScale.x + scaleToAdd,
				unit.transform.localScale.y + scaleToAdd,
				unit.transform.localScale.z + scaleToAdd);
			yield return null;
		}

		if (unit) unit.transform.localScale = Vector3.one;
	}

	public void SetPlayerController(PlayerController p) {
		playerController = p;
	}

	#region accessors
	public int peekLane(Constants.radialCodes code) {
		if (code == Constants.radialCodes.top && topLaneQueue.Count > 0) {
			return topLaneQueue.Peek();
		}
		else if (code == Constants.radialCodes.mid && midLaneQueue.Count > 0) {
			return midLaneQueue.Peek();
		}
		else if (botLaneQueue.Count > 0){
			return botLaneQueue.Peek();
		}
		else {
			return -1;
		}
	}

	public float remainingTime(Constants.radialCodes code) {
		if (code == Constants.radialCodes.top && topLaneQueue.Count > 0) {
			return topBuildCountdown;
		}
		else if (code == Constants.radialCodes.mid && midLaneQueue.Count > 0) {
			return midBuildCountdown;
		}
		else if (code == Constants.radialCodes.bot && botLaneQueue.Count > 0) {
			return botBuildCountdown;
		}
		else {
			return 0;
		}
	}

	public float getUnitCost(Constants.unitType type) {
		return unitGoldCosts[type];
	}

	public void IncrementTrainingGroundCount() {
		trainingGrounds++;
	}
	#endregion
}
