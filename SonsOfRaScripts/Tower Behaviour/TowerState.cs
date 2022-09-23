using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Rewired;
using System;

public class TowerState : MonoBehaviour {
	// ------------------ public variables ---------------------
	// enumerations
	//First two track if the tower is being placed or is already placed
	public enum tStates { hovering, building, placed };
	//public enum tTypes { archerTower, obelisk, sunTower, stasisTower};
	public enum tDimensions { _1x1, _3x1, _2x2 };

	// references
	public Material holoMat, mainMat;
	public ParticleSystem dustFx;
	public Sprite iconP1, iconP2;

	// gameplay values
	public Constants.towerType type;
	public tDimensions dimensions;
	public int tileCount;
	public tStates state = tStates.hovering;
	public int cost;
	public float influenceRange;
	public bool isColliding = false;
	public bool isOnGrid = false;
	public float constructTime = 2f;
	public string rewiredPlayerKey;
	public int killRewardGold;
	public List<STile> influenceTiles;

	// --------------- private variables ------------------
	// references
	private TowerHealthDisplay hpbar;
	private Image health, bdrop, highlight;
	private SoundManager s;
	private AudioSource src;
	private PlayerController pController;

    // gameplay values
    private bool isRotated = false;

    private Color holoColor;

    [ColorUsage(true,true)]
    [SerializeField] private Color p2StasisGlowColor;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string buildEvent;

	private void Awake() {
		if (state == tStates.hovering)
			GetComponent<Collider>().isTrigger = true;
	}

    private void Start()
    {
        //matCountCorrection is meant to ignore range indicator object when assigning materials
        //this checks if the tower type doesn't have the range indicator and lowers correction to 0

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        List<Renderer> rendererList = renderers.ToList();
        rendererList.RemoveAll(x => x.GetType() == typeof(ParticleSystemRenderer));

        for (int i = 0; i < rendererList.Count; i++)
        {// -matCountCorrection accounts for range indicator object
            if (rendererList[i].tag != "KeepMat") //check if you want to keep this material
                rendererList[i].material = holoMat;
        }

        if (rewiredPlayerKey == PlayerIDs.player1)
        {
            // Make Color
            holoColor = new Color(1f, 0.7f, 0.7f, 0.5f);
        }
        else
        {
            // Make Color
            holoColor = new Color(0.7f, 0.7f, 1f, 0.5f);
        }

        // Change holo mat Color
        if (gameObject.GetComponent<Renderer>() != null)
            gameObject.GetComponent<Renderer>().material.color = holoColor;
        else
        {
            for (int i = 0; i < rendererList.Count; i++) // -matCountCorrection accounts for range indicator object
                rendererList[i].material.color = new Color(holoColor.r, holoColor.g, holoColor.b, 0.3f);
        }

        hpbar = GetComponentInChildren<TowerHealthDisplay>();
        health = hpbar.healthBar;
        health.enabled = false;
        bdrop = hpbar.backdrop;
        bdrop.enabled = false;
		highlight = hpbar.healthBarHighlight;
        highlight.enabled = false;

        //set up main material colors
        LoadoutManager l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
        mainMat = Instantiate(mainMat);
        mainMat.SetColor("_PalCol1", l.getPaletteColor(0, rewiredPlayerKey));
        mainMat.SetColor("_PalCol2", l.getPaletteColor(1, rewiredPlayerKey));

        //Set up glow color for p2 stasis tower
        if (rewiredPlayerKey == PlayerIDs.player2 && type == Constants.towerType.stasisTower)
        {
            mainMat.SetColor("_EmisColor", p2StasisGlowColor);
        }

        s = SoundManager.Instance;
        src = GetComponent<AudioSource>();

    }

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Tile" && other.gameObject.GetComponent<STile>().isSpawnable(rewiredPlayerKey))
			isOnGrid = true;
	}

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "Tile" && other.gameObject.GetComponent<STile>().isSpawnable(rewiredPlayerKey))
			isOnGrid = false;
	}

	public void realTower() {
		state = tStates.placed;

		//Add to tower dictionary
		LivingTowerDictionary.dict[rewiredPlayerKey].Add(gameObject);

		// trigger spawn event
		SonsOfRa.Events.GameEvents.InvokeTowerSpawn(gameObject);

		if (type == Constants.towerType.archerTower || type == Constants.towerType.obelisk) {
			GetComponentInChildren<TowerAttacker>().checkEnemiesInRadius();
			GetComponentInChildren<TowerAttacker>().StartDetection();
		}
		else if (type == Constants.towerType.sunTower) {
			GetComponentInChildren<SunTowerAttacker>().checkEnemiesInRadius();
			GetComponentInChildren<SunTowerAttacker>().StartDetection();

            //start sun tower spin
            gameObject.GetComponentInChildren<Animator>().SetBool("Spin", true);
            //fix FX materials
            GetComponentInChildren<SunTowerAttacker>().FixFXMaterials();
        }
		else if (type == Constants.towerType.stasisTower) {
			GetComponentInChildren<StasisTowerAttacker>().checkEnemiesInRadius();
			GetComponentInChildren<StasisTowerAttacker>().StartDetection();
		}
    }

	private IEnumerator buildingTower() {
		state = tStates.building;

		ModelDrop drop = gameObject.GetComponentInChildren<ModelDrop>();
		drop.ObjectDrop();

		yield return new WaitForSeconds(constructTime / 7);

		// This all sets the material to be rendered opaque after it builds
		if (gameObject.GetComponent<Renderer>() != null) {
            GetComponent<Renderer>().material = mainMat;
        }
		else {
			for (int i = 0; i < (gameObject.GetComponentsInChildren<Renderer>()).Length; i++) {
				if (GetComponentsInChildren<Renderer>()[i].gameObject.tag != "KeepMat") { //check if you want to keep this material (sorry jeffery, I know you don't like hacky code)
                    GetComponentsInChildren<Renderer>()[i].material = mainMat;
                }
			}
		}

		if (type == Constants.towerType.archerTower) {
			GameObject archer = GetComponentInChildren<TowerAttacker>().orientedObject;
			Material mat = pController.gameObject.GetComponentInChildren<UnitSpawner>().unitMat[0];
			archer.GetComponentsInChildren<MeshRenderer>()[0].material = mat;
			archer.GetComponentsInChildren<MeshRenderer>()[1].material = mat;
			archer.GetComponentsInChildren<SkinnedMeshRenderer>()[0].material = mat;
		}

		// Tower build sound
		GameManager.Instance.CameraParent.GetComponentInChildren<cameraShake>().ShakeTheCamera(0.105f, 0.125f);

		// Tower dust spray
		ParticleSystem ps = Instantiate(dustFx, transform);
		ps.transform.localPosition = Vector3.zero;
		ps.transform.localRotation = Quaternion.identity;
		//StartCoroutine(destroyParticles(ps));

		yield return new WaitForSecondsRealtime(0.2f);

		// Controller buzz
		pController.ControllerVibration(0, 0.65f, 0.10f);

		health.enabled = true; //hp bar appears when tower is actually active, slightly after placement
		bdrop.enabled = true;
		highlight.enabled = true;

        realTower();
	}

	public void towerBuilderHelper() {
		sound_towerDrop();
		StartCoroutine(buildingTower());
	}

	private IEnumerator invalidLocation() {
		// turn purplish
		if (gameObject.GetComponent<Renderer>() != null)
			gameObject.GetComponent<Renderer>().material.color = new Color(.67f, 0, .67f);
		else {
			for (int i = 0; i < (gameObject.GetComponentsInChildren<Renderer>()).Length; i++)
				gameObject.GetComponentsInChildren<Renderer>()[i].material.color = new Color(.67f, 0, .67f);
		}

		yield return new WaitForSeconds(.5f);//Flash for half a second

		// return to translucent
		if (gameObject.GetComponent<Renderer>() != null)
			gameObject.GetComponent<Renderer>().material.color = holoColor;
		else {
			for (int i = 0; i < (gameObject.GetComponentsInChildren<Renderer>()).Length; i++)
				gameObject.GetComponentsInChildren<Renderer>()[i].material.color = new Color(holoColor.r, holoColor.g, holoColor.b, .3f);
		}
	}

	private void OnDestroy() {
		// Give player who destroyed it gold
		if (state == tStates.placed) {
			if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
				OnlineManager.Instance.SendPacket(new PO_Entity_Destroy(PacketObject.packetType.towerDestroy, GetComponent<EntityIdentifier>().ID, rewiredPlayerKey));
			}

			if (rewiredPlayerKey == "Player0") {
				GameManager.Instance.player2Controller.addGold(killRewardGold);
			}
			else {
				GameManager.Instance.player1Controller.addGold(killRewardGold);
			}
		}
	}

	#region effect functions
	public void sound_towerDrop() {
		FMOD.Studio.EventInstance build = FMODUnity.RuntimeManager.CreateInstance(buildEvent);
		build.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		build.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		build.start();
		build.release();
	}
	#endregion

	#region helper functions
	private IEnumerator destroyParticles(ParticleSystem particles) {
		yield return new WaitUntil(delegate { return !particles.GetComponent<ParticleSystem>().IsAlive(); });

		Destroy(particles.gameObject);
	}

	public void invalidLocationHelper() {
		StartCoroutine(invalidLocation());
	}

	private bool checkCollision() {
		return isColliding;
	}

	private bool checkIsOnGrid() {
		return isOnGrid;
	}

	public bool isValidSpot() {
		return !checkCollision() && checkIsOnGrid();
	}

	public int getCost() {
		return cost;
	}
	#endregion

	#region Getters and Setters
	public void SetPlayerController(PlayerController p) {
		pController = p;
	}

	public void FlipIsRotated() {
		isRotated = !isRotated;
	}

	public bool GetIsRotated() {
		return isRotated;
	}
    #endregion

    #region Online Functions
    public void TargetSync(int[] unitEntityIDs) {
        string playerKey = rewiredPlayerKey == PlayerIDs.player1 ? PlayerIDs.player2 : PlayerIDs.player1;

        if (type == Constants.towerType.archerTower || type == Constants.towerType.obelisk) {
            GetComponentInChildren<TowerAttacker>().SetTargetsList(unitEntityIDs);
        }
        else if (type == Constants.towerType.sunTower) {
            GetComponentInChildren<SunTowerAttacker>().SetTargetsList(unitEntityIDs);
        }
        else if (type == Constants.towerType.stasisTower) {
            GetComponentInChildren<StasisTowerAttacker>().SetTargetsList(unitEntityIDs);
        }
    }

    public void Sync(float _health, float _shield, bool _stunned, float _power, float _attackspeed = -1) {
        TowerHealth th = GetComponentInChildren<TowerHealth>();

        if (type == Constants.towerType.stasisTower) {
            StasisTowerAttacker ta = GetComponentInChildren<StasisTowerAttacker>();
            ta.isStunned = _stunned;
            ta.speedReductionPercent = _power;
        }
        else if (type == Constants.towerType.sunTower) {
            SunTowerAttacker ta = GetComponentInChildren<SunTowerAttacker>();
            ta.isStunned = _stunned;
            ta.damage = _power;
            ta.attackSpeed = _attackspeed;
        }
        else {
            TowerAttacker ta = GetComponentInChildren<TowerAttacker>();
            ta.isStunned = _stunned;
            ta.damage = _power;
            ta.attackSpeed = _attackspeed;
        }

        th.shield = _shield;
        th.health = _health;
        //th.TakeDamage(0);
    }

    public void SendTargetSync(int[] unitEntityIDs) {
        PO_TowerTargetSync packet = new PO_TowerTargetSync(GetComponent<EntityIdentifier>().ID, rewiredPlayerKey, unitEntityIDs);

        OnlineManager.Instance.SendPacket(packet);
    }

	public void SendSync() {
		if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
			OnlineManager.Instance.SendPacket(GetSyncPacket());
		}
	}

    public PacketObject GetSyncPacket() {
        TowerHealth th = GetComponentInChildren<TowerHealth>();
        bool isStunned;
        float power, attackSpeed;

        if (type == Constants.towerType.stasisTower) {
            StasisTowerAttacker ta = GetComponentInChildren<StasisTowerAttacker>();
            isStunned = ta.isStunned;
            power = ta.speedReductionPercent;
            attackSpeed = -1;
        }
        else if (type == Constants.towerType.sunTower) {
            SunTowerAttacker ta = GetComponentInChildren<SunTowerAttacker>();
            isStunned = ta.isStunned;
            power = ta.damage;
            attackSpeed = ta.attackSpeed;
        }
        else {
            TowerAttacker ta = GetComponentInChildren<TowerAttacker>();
            isStunned = ta.isStunned;
            power = ta.damage;
            attackSpeed = ta.attackSpeed;
        }

		return new PO_TowerSync(GetComponentInChildren<EntityIdentifier>().ID, rewiredPlayerKey, th.health, th.shield, isStunned, power, attackSpeed);
	}
    #endregion
}
