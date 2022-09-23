using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_TriggerZone : MonoBehaviour
{
	private AI_PlayerController pController;
	private Constants.radialCodes lane;
	[SerializeField] private float radius;
	[SerializeField] private float panicSpawnRefresh;
	[SerializeField] private float postSpawnPanicSpawnRefresh;
	private float panicSpawnCooldown = 0;
	private LayerMask unitLayer;


    // Start is called before the first frame update
    void Start() {
		GetComponent<SphereCollider>().radius = radius;
		unitLayer = 1 << LayerMask.NameToLayer("Unit");
	}

	private void Update() {
		if (panicSpawnCooldown > 0) {
			panicSpawnCooldown -= Time.deltaTime;
		}
		else {
			DetectUnits(radius);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (panicSpawnCooldown <= 0 && other.gameObject.GetComponent<UnitAI>() 
			&& other.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey() != pController.rewiredPlayerKey) {
			DetectUnits(radius * 1.5f);
		}
	}

	private void DetectUnits(float rad) {
		bool successfulSpawn = false;
		Collider[] collisions = Physics.OverlapSphere(transform.position, rad, unitLayer);
		List<Collider> collisionsList = collisions.ToList();
		collisionsList.RemoveAll(x => x.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey() == pController.rewiredPlayerKey);

		Constants.unitType focusUnit = Constants.unitType.spearman;

		if (collisionsList.Find(u => u.gameObject.GetComponent<UnitAI>().type == Constants.unitType.archer)) {
			focusUnit = Constants.unitType.archer;
		}
		else if (collisionsList.Find(u => u.gameObject.GetComponent<UnitAI>().type == Constants.unitType.catapult)) {
			focusUnit = Constants.unitType.catapult;
		}
		else if (collisionsList.Find(u => u.gameObject.GetComponent<UnitAI>().type == Constants.unitType.embalmPriest)) {
			focusUnit = Constants.unitType.embalmPriest;
		}
		else if (collisionsList.Find(u => u.gameObject.GetComponent<UnitAI>().type == Constants.unitType.mummy)) {
			focusUnit = Constants.unitType.mummy;
		}
		else if (collisionsList.Find(u => u.gameObject.GetComponent<UnitAI>().type == Constants.unitType.shieldbearer)) {
			focusUnit = Constants.unitType.shieldbearer;
		}

		if (collisionsList.Count > 0 && panicSpawnCooldown <= 0) {
			if (pController.PanicSpawn(lane, collisions.Length, focusUnit)) {
				Debug.Log("spawning panic units");
				panicSpawnCooldown = postSpawnPanicSpawnRefresh;
				successfulSpawn = true;
			}
		}
		else if (collisionsList.Count == 0 && panicSpawnCooldown <= 0) {
			panicSpawnCooldown = panicSpawnRefresh;
		}
	}

	public void SetUp(AI_PlayerController p, Constants.radialCodes _lane) {
		pController = p;
		lane = _lane;
	}
}
