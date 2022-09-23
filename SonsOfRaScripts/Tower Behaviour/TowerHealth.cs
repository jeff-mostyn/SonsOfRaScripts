using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TowerHealth : MonoBehaviour {
    public float health;
    public float shield;
    public ParticleSystem rubbleFx;
    public GameObject attacker;
    LayerMask tileLayer;
	private TowerState myState;
	bool isDead;

    Animator towerAni;
	// Use this for initialization

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string dieEvent;

	private Vector3 sizeOfTower;
    void Start ()
    {
		isDead = false;
        //Need this here becuase size is 0,0,0 if called in Ondestroy
        sizeOfTower = gameObject.GetComponent<Collider>().bounds.extents;

        //set up animator
        towerAni = gameObject.GetComponentInChildren<Animator>();
		myState = GetComponent<TowerState>();
    }
	
	// Update is called once per frame
	void Update () {
        tileLayer = 1 << LayerMask.NameToLayer("Tile");

		if (health <= 0 && !isDead) {
			Die();
		}
    }

    //Function for other entities to do damage to the tower
    public void TakeDamage(float damage) {
		if (health > 0 && myState.state == TowerState.tStates.placed
			&& (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()))) {
			if ((shield - damage) > 0) {
				shield -= damage;
			}
			else {
				damage -= shield;
				shield = 0;
				health -= damage;
			}

			myState.SendSync();

			if (health <= 0 && !isDead) {
				Die();
			}
		}
    }

	public void Die() {
		if (!isDead) {
			isDead = true;
			health = 0;
			towerAni.SetTrigger("TowerDeath"); //death animtion

			sound_die();

			// Tower rubble
			ParticleSystem ps = Instantiate(rubbleFx, transform);
			ps.transform.localPosition = Vector3.zero;
			ps.transform.localRotation = Quaternion.identity;
			attacker.SetActive(false);
			StartCoroutine(destroy(ps));
			GetComponentInChildren<Canvas>().enabled = false;

			UnMarkTiles();
		}
	}

    public float getHealth() {
		return health;
	}

	private void sound_die() {
		FMOD.Studio.EventInstance die = FMODUnity.RuntimeManager.CreateInstance(dieEvent);
		die.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		die.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		die.start();
		die.release();
	}

	private IEnumerator destroy(ParticleSystem particles) {
		yield return new WaitUntil(delegate { return particles == null; });

		// Invoke Event
		SonsOfRa.Events.GameEvents.InvokeTowerDie();
		Destroy(gameObject);
	}

	private void UnMarkTiles() {
		if (myState.state == TowerState.tStates.placed) {
			//Remove the tower from the list
			LivingTowerDictionary.dict[myState.rewiredPlayerKey].Remove(gameObject);

			// update tile ownership counts
			foreach (STile t in myState.influenceTiles) {
				if (!t.isRoad && !t.isDefault) {
					t.RemoveTower(myState.rewiredPlayerKey);
				}
			}

			//code to mark the tiles as free
			if (!myState.GetIsRotated()) {
				Collider[] colList = Physics.OverlapBox(transform.position, 
					new Vector3(sizeOfTower.x * .98f, sizeOfTower.y * .98f, sizeOfTower.z * .98f), Quaternion.identity, tileLayer);
				colList.ToList().ForEach(x => x.gameObject.GetComponent<STile>().isEmpty = true);
				colList.ToList().ForEach(x => x.gameObject.GetComponent<STile>().colorCheck());
			}
			else {
				Collider[] colList = Physics.OverlapBox(transform.position,
					new Vector3(sizeOfTower.z * .98f, sizeOfTower.y * .98f, sizeOfTower.x * .98f), Quaternion.identity, tileLayer);
				colList.ToList().ForEach(x => x.gameObject.GetComponent<STile>().isEmpty = true);
				colList.ToList().ForEach(x => x.gameObject.GetComponent<STile>().colorCheck());
			}

			////Need to adject which tiles the player can use once a tower is destroyed
			//List<Collider> inflRange;
			//if (!myState.GetIsRotated()) {   // not rotated, normal box
			//	inflRange = Physics.OverlapBox(gameObject.transform.position,
			//							new Vector3(sizeOfTower.x + myState.influenceRange * .98f,
			//							sizeOfTower.y + myState.influenceRange * .98f,
			//							sizeOfTower.z + myState.influenceRange * .98f),
			//		Quaternion.identity, tileLayer).ToList();
			//}
			//else {  // rotated, flip x and z sizes
			//	inflRange = Physics.OverlapBox(gameObject.transform.position,
			//							new Vector3(sizeOfTower.z + myState.influenceRange * .98f,
			//							sizeOfTower.y + myState.influenceRange * .98f,
			//							sizeOfTower.x + myState.influenceRange * .98f),
			//		Quaternion.identity, tileLayer).ToList();
			//}

			//// reassign ownership and correctly mark tiles
			//string playerKey = myState.rewiredPlayerKey;
			//foreach (Collider col in inflRange) {
			//	STile tile = col.GetComponent<STile>();
			//	tile.RemoveTower(playerKey);
			//	if (!tile.isDefault && tile.GetOwnerID() == playerKey) {
			//		if (tile.GetTowerCount(playerKey) == 0) {
			//			InfluenceTileDictionary.RemoveTileFromDict(tile.gameObject, tile.GetOwnerID());

			//			if (tile.IsContested()) {
			//				if (tile.GetOwnerID() == PlayerIDs.player1) {
			//					tile.SetOwner(PlayerIDs.player2);
			//				}
			//				else {
			//					tile.SetOwner(PlayerIDs.player1);
			//				}
			//				tile.UnsetIsContested();
			//				InfluenceTileDictionary.AddTileToDict(tile.gameObject);
			//			}
			//			else {
			//				tile.SetOwner("");

			//				// dont do this when resolving contested tiles
			//				// it colors things after they've been set to clear
			//				tile.colorCheck();
			//			}
			//		}
			//		else {  // tile not changing ownership
			//			tile.colorCheck();
			//		}
			//	}
			//}
		}
	}
}
