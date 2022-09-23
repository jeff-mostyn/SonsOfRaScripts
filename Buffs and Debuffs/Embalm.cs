using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Embalm : BuffDebuff {
	[Header("Effect Values")]
	[SerializeField] protected float embalmDuration;

	// ----------- public variables -------------
	// references
	[Header("References")]
	public GameObject mummy;
	public string playerKey;
	public Material P1UnitMat, P2UnitMat;

	// ----------- private variables ------------
	// references
	[SerializeField] private GameObject fxLarge, fxSmall;
	private GameObject activeFx;

	// gameplay values
	private string mummySpawnLane;
	private bool spawningMummy = false;
	private Material unitMat;
	private GameObject player;
	private UnitSpawner uSpawner;

	private void Update() {
		if (target != null) {
			if (target.getHealth() <= 0 && !spawningMummy) {
				SpawnMummy();
			}

			timeElapsed += Time.deltaTime;
			if (timeElapsed > embalmDuration && !spawningMummy && !isPermanent) {
                Cleanse();
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		if (!t.is_debuffEmbalm) {
			target = t;
			target.is_debuffEmbalm = true;

			mummySpawnLane = target.gameObject.GetComponent<UnitMovement>().lane;
			playerKey = _playerKey;

			target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());

			// instantiate particles
			if (target.getType() == Constants.unitType.spearman || target.getType() == Constants.unitType.mummy) {
				activeFx = Instantiate(fxSmall, target.gameObject.transform.GetChild(0));
			}
			else {
				activeFx = Instantiate(fxLarge, target.gameObject.transform.GetChild(0));
			}
			activeFx.transform.localPosition = Vector3.zero;

			if (playerKey == PlayerIDs.player1) {
				unitMat = P1UnitMat;
				player = GameObject.Find("Player1"); // this is horrendous
			}
			else {
				unitMat = P2UnitMat;
				player = GameObject.Find("Player2"); // this is too
			}

			uSpawner = player.GetComponent<UnitSpawner>();
		}
	}

	private void SpawnMummy() {
		Destroy(activeFx);

		if (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost())) {
			spawningMummy = true;

			GameObject targetObj = target.gameObject;
			int mummyEntityId = GameManager.Instance.GetNextEntityID();

			if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
				float[] spawnPosArray = {targetObj.transform.position.x, targetObj.transform.position.y, targetObj.transform.position.z};
				OnlineManager.Instance.SendPacket(new PO_UnitSpawn(playerKey, mummySpawnLane, spawnPosArray, targetObj.GetComponent<UnitMovement>().GetWaypointIndex(), (int)Constants.unitType.mummy, mummyEntityId));
			}

			uSpawner.SpawnUnit((int)Constants.unitType.mummy, mummySpawnLane, targetObj.transform.position, targetObj.GetComponent<UnitMovement>().GetWaypointIndex(), mummyEntityId);

			spawningMummy = false;
		}

		Destroy(gameObject);
	}

	public override void Cleanse() {
		if (!spawningMummy) {	// idk if this is the best way to handle this
			target.is_debuffEmbalm = false;
			Destroy(activeFx);
			Destroy(gameObject);
		}
	}
}
