using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Rewired;

public class TowerSpawner : MonoBehaviour {
    // ---------------- constants -----------------
    private float EVEN_DIMENSION_OFFSET = 0.5f;

    // ---------------- public variables ------------------
    public GameObject towerToSpawn = null;
    TowerState towerToSpawnState = null;
    public GameObject playerObject;

    public Squares sq;

    public ParticleSystem rangeIndicator;
    private ParticleSystem rangeFx;

    private PlayerController pController;
    LayerMask tileLayer;

    public string rewiredPlayerKey = "Player0";
    private float grace = .25f; //multiplier so placement isn't too rigid

    [Header("Square pulse setup")]
    [SerializeField] private float maxValue;
    [SerializeField] private float restValue;
    [SerializeField] private float pulseDuration;

    // ---------------- private variables -----------------
    private StatCollector stats;
    private List<STile> previewTiles;
    private float pulseValue;
    private Coroutine pulse;

    // Use this for initialization
    void Start() {
        stats = StatCollector.Instance;

        tileLayer = 1 << LayerMask.NameToLayer("Tile");

        rangeFx = Instantiate(rangeIndicator, transform.position, Quaternion.identity);
        rangeFx.transform.SetParent(transform);
        rangeFx.transform.localPosition = Vector3.zero;
        rangeFx.gameObject.SetActive(false);

        previewTiles = new List<STile>();
    }

    public void holdTower(GameObject tower) {
        if (towerToSpawn == null) {
            // start pulse
            pulse = StartCoroutine(PulseSquares());

            // hold tower
            towerToSpawn = Instantiate(tower);
            towerToSpawn.transform.SetParent(transform);
            towerToSpawnState = towerToSpawn.GetComponent<TowerState>();
            towerToSpawnState.SetPlayerController(pController);

            // change "hologram" location of tower based on dimensions
            if (towerToSpawnState.dimensions == TowerState.tDimensions._2x2) {
                towerToSpawn.transform.localPosition = new Vector3(EVEN_DIMENSION_OFFSET, 0f, EVEN_DIMENSION_OFFSET);
            }
            else {
                towerToSpawn.transform.localPosition = Vector3.zero;
            }

			// rotate if archer tower
			if (towerToSpawnState.type == Constants.towerType.archerTower) {
				
				if (rewiredPlayerKey == PlayerIDs.player1) {
					towerToSpawn.GetComponentInChildren<MeshRenderer>().transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
				}
				else {
					towerToSpawn.GetComponentInChildren<MeshRenderer>().transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				}
			}

            towerToSpawnState.rewiredPlayerKey = GetComponent<CursorMovement>().rewiredPlayerKey;

            // set playerKey for attacker scripts
            if (towerToSpawnState.type == Constants.towerType.archerTower || towerToSpawnState.type == Constants.towerType.obelisk)
                towerToSpawn.GetComponentInChildren<TowerAttacker>().rewiredPlayerKey = GetComponent<CursorMovement>().rewiredPlayerKey;
            else if (towerToSpawnState.type == Constants.towerType.stasisTower)
                towerToSpawn.GetComponentInChildren<StasisTowerAttacker>().rewiredPlayerKey = GetComponent<CursorMovement>().rewiredPlayerKey;
            else if (towerToSpawnState.type == Constants.towerType.sunTower)
                towerToSpawn.GetComponentInChildren<SunTowerAttacker>().rewiredPlayerKey = GetComponent<CursorMovement>().rewiredPlayerKey;

            //Create primitive cylinder to act as the attack range if tower can attack
            if (GetComponentInChildren<TowerAttacker>() || GetComponentInChildren<SunTowerAttacker>() || GetComponentInChildren<StasisTowerAttacker>()) {
                rangeFx.transform.localScale = new Vector3(towerToSpawn.GetComponentInChildren<CapsuleCollider>().radius * 2, towerToSpawn.GetComponentInChildren<CapsuleCollider>().radius * 2, towerToSpawn.GetComponentInChildren<CapsuleCollider>().radius * 2);
                rangeFx.transform.position = towerToSpawn.transform.position;   // adjust location of range to handle if towers are in a different position due to dimension
                rangeFx.gameObject.SetActive(true);
            }

            InfluenceTileDictionary.ColorTiles(GetComponent<CursorMovement>().rewiredPlayerKey);
        }
    }

    #region Rotation
    // Rotation of towers, these were called on PlayerController.cs 
    // Rotate the held tower 90 degrees right
    public void rotateTowerRight() {
        if (towerToSpawn) {
            towerToSpawn.gameObject.transform.Rotate(0, 90, 0);
            towerToSpawnState.FlipIsRotated();

            PreviewInfluence();
        }
    }

    //Rotate the tower 90 degrees left
    public void rotateTowerLeft() {
        if (towerToSpawn) {
            towerToSpawn.transform.Rotate(0, -90, 0);
            towerToSpawnState.FlipIsRotated();

            PreviewInfluence();
        }
    }
    #endregion

    public void clearTower() {
        if (towerToSpawn != null) {
            Destroy(towerToSpawn);
            towerToSpawn = null;
            rangeFx.gameObject.SetActive(false);

            StopCoroutine(pulse);
            pulseValue = 0.3f;
        }
    }

    //Make sure we have a tower held and its not colliding with other towers
    //Will change it to use a racyast probably
    bool checkCreate() {
        bool temp = false;
        if (towerToSpawn != null && towerToSpawn.GetComponent<TowerState>().isValidSpot())
            temp = true;

        return temp;
    }

    public bool trySpawn(int entityId = -1) {
        if (pController.getGold() >= towerToSpawnState.getCost() || entityId != -1) {
            Vector3 boundBox = new Vector3(towerToSpawn.GetComponent<Collider>().bounds.extents.x * grace,
                                                    towerToSpawn.GetComponent<Collider>().bounds.extents.y,
                                                    towerToSpawn.GetComponent<Collider>().bounds.extents.z * grace);
            Collider[] colList = Physics.OverlapBox(towerToSpawn.transform.position, boundBox, Quaternion.identity, tileLayer);
            if (colList.Length != 0 && !colList.Any(x => x.gameObject.GetComponent<STile>().isRoad)
                && !colList.Any(x => x.gameObject.GetComponent<STile>().neutralExpansionTile)) {    // no roads, no expansion tiles
                                                                                                    // this might be redundant in some ways
                                                                                                    //Finding the closest tile to the center of the tower(i.e. cursor position)
                Collider closest = colList[0];
                float smallestDist = Vector3.Distance(transform.position, closest.transform.position);

                foreach (Collider col in colList) {
                    float lastDist = Vector3.Distance(transform.position, col.transform.position);
                    if (lastDist < smallestDist) {
                        closest = col;
                        smallestDist = lastDist;
                    }
                }

                //check spot at where tower will spawn to make sure it will not overlap with anything else(.4 because its a radius and don't want to grab tiles touching  it)
                colList = Physics.OverlapBox(towerToSpawn.transform.position, towerToSpawn.GetComponent<Collider>().bounds.extents * .8f, Quaternion.identity, tileLayer);

                //Check and make sure none of the tiles are false for can spawn and that none of them are roads
                if (!colList.Any(x => !x.GetComponent<STile>().isSpawnable(pController.rewiredPlayerKey) || x.GetComponent<STile>().isRoad) 
					&& colList.Length == towerToSpawnState.tileCount) {
					// if its from online, the player may not have enough gold because of a mismatch. spend what the player has in that case
                    pController.spendGold(pController.getGold() >= towerToSpawnState.getCost() ? towerToSpawnState.getCost() : pController.getGold());//Spend cost

                    if (SettingsManager.Instance.GetIsOnline() && entityId == -1) {
                        entityId = GameManager.Instance.GetNextEntityID();

                        CursorMovement cursor = gameObject.GetComponent<CursorMovement>();

                        PO_TowerSpawn packet = new PO_TowerSpawn(rewiredPlayerKey, towerToSpawnState.type, cursor.current.name, (int)towerToSpawn.transform.rotation.eulerAngles.y, entityId);
                        OnlineManager.Instance.SendPacket(packet);
                    }

                    towerToSpawn.GetComponent<EntityIdentifier>().ID = entityId;

                    // record stats
                    stats.recordTowersSpawned(rewiredPlayerKey, (int)towerToSpawnState.type);    // record construction of towers for stats
                    stats.recordTowerGold(rewiredPlayerKey, towerToSpawnState.getCost());	// record gold spent on towers for stats

                    //Setting the tower to the tile in world space
                    towerToSpawn.transform.SetParent(null);

                    //Get the bounds of the renderer component
                    Renderer rend = towerToSpawn.GetComponentInChildren<Renderer>();
                    Vector3 center = rend.bounds.center;
                    float radius = rend.bounds.extents.y;

                    //Offset y location if pivot point isn't at the center of tower model itself
                    if (rend.bounds.center.y == transform.position.y) {
                        towerToSpawn.transform.position = new Vector3(closest.transform.position.x,
                            closest.transform.position.y + radius,
                            closest.transform.position.z);
                    }
                    else
                        towerToSpawn.transform.position = closest.transform.position;

                    // offset spawn location depending on dimensions of tower
                    if (towerToSpawnState.dimensions == TowerState.tDimensions._2x2) {
                        towerToSpawn.transform.position = new Vector3(towerToSpawn.transform.position.x + EVEN_DIMENSION_OFFSET, towerToSpawn.transform.position.y, towerToSpawn.transform.position.z + EVEN_DIMENSION_OFFSET);
                    }

                    //Mark non road and non empty nearby tiles as spawnable by player 1f is the cell size
                    List<Collider> inflRange = Physics.OverlapBox(towerToSpawn.transform.position,
                        new Vector3(towerToSpawn.GetComponent<Collider>().bounds.extents.x + towerToSpawnState.influenceRange * .98f,
                                    towerToSpawn.GetComponent<Collider>().bounds.extents.y,
                                    towerToSpawn.GetComponent<Collider>().bounds.extents.z + towerToSpawnState.influenceRange * .98f),
                        Quaternion.identity, tileLayer).ToList();


					// Set variables of tile corresponding to owner and what is in range of it 
					List<STile> influenceTiles = new List<STile>();
                    foreach (Collider col in inflRange) {
                        STile tile = col.GetComponent<STile>();
						if (!tile.isRoad && !tile.isDefault) {
							influenceTiles.Add(tile);
							tile.AddTower(rewiredPlayerKey);
                        }
                    }
					towerToSpawnState.influenceTiles = influenceTiles;

					//Start the construction
					towerToSpawnState.towerBuilderHelper();

                    towerToSpawn = null;
                    rangeFx.gameObject.SetActive(false);

                    //Mark all the spaces the tower will occupy to be true
                    foreach (Collider col in colList)
                        col.GetComponent<STile>().isEmpty = false;

                    return true;
                }
                else {
                    Debug.LogWarning("At least one tile under tower not owned by player");
                    pController.purchaseErrorFeedback("other");
					towerToSpawnState.invalidLocationHelper();
                    return false;
                }
            }
            else {
                Debug.LogWarning("There is a road under the tower");
                pController.purchaseErrorFeedback("other");
				towerToSpawnState.invalidLocationHelper();
                return false;
            }
        }
        else {
            Debug.LogWarning("Not enough gold");
            pController.purchaseErrorFeedback("gold");
			towerToSpawnState.invalidLocationHelper();
            return false;
        }
    }

    public void PreviewInfluence() {
        if (towerToSpawn) {
            // empty list of preview tiles
            ClearPreview();

            // establish new list of preview tiles
            List<Collider> inflRange = Physics.OverlapBox(towerToSpawn.transform.position,
                new Vector3(towerToSpawn.GetComponent<Collider>().bounds.extents.x + towerToSpawn.GetComponent<TowerState>().influenceRange * .98f,
                            towerToSpawn.GetComponent<Collider>().bounds.extents.y,
                            towerToSpawn.GetComponent<Collider>().bounds.extents.z + towerToSpawn.GetComponent<TowerState>().influenceRange * .98f),
                Quaternion.identity, tileLayer).ToList();

            foreach (Collider col in inflRange) {
                previewTiles.Add(col.gameObject.GetComponent<STile>());
            }

            // tell preview tiles they are being previewed
            for (int i = 0; i < previewTiles.Count; i++) {
                previewTiles[i].SetPreviewFlag(rewiredPlayerKey);
                previewTiles[i].ColorForPreview();
            }
        }
    }

    public void ClearPreview() {
        if (previewTiles.Count > 0) {
            // unpreview tiles
            for (int i = 0; i < previewTiles.Count; i++) {
                previewTiles[i].UnsetPreviewFlag(rewiredPlayerKey);
                previewTiles[i].ColorForPreview();
            }
            previewTiles.Clear();
        }
    }

    public void SetPlayerController(PlayerController p) {
        pController = p;
    }

    public PlayerController GetPlayerController() {
        return pController;
    }

    #region Pulse squares
    IEnumerator PulseSquares() {
        float timer = 0f;
        float angle = 0f;
        //yield return new WaitForSeconds(pulseDowntime);
        while (true) {
            while (timer < pulseDuration) {
                pulseValue = ((maxValue - restValue) / 2 * Mathf.Cos(angle - Mathf.PI)) + restValue;

                angle = Mathf.Lerp(0f, 2*Mathf.PI, timer/pulseDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            angle = 0f;
        }
    }

    public float GetPulseValue() {
        return pulseValue;
    }

    public bool isHoldingTower() {
        return towerToSpawn != null;
    }
    #endregion
}