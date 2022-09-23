using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBeamObj : MonoBehaviour {
	private float tickTime;
	private float tickDamage;
	[SerializeField] private string id;
	private float _radius;
	List<UnitAI> unitTargets;
	List<TowerHealth> towerTargets;

	// add entering unit to list of targets if they are the enemy
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("Unit")) {
			UnitAI targetAI = other.GetComponent<UnitAI>();
			if (targetAI.GetTeamPlayerKey() == PlayerIDs.GetOpponentPID(id)) {
				unitTargets.Add(targetAI);
			}
		}
		else if (other.gameObject.tag == "Tower") {
			TowerState tState = other.GetComponent<TowerState>();
			if (tState.rewiredPlayerKey == PlayerIDs.GetOpponentPID(id)) {
				towerTargets.Add(other.GetComponent<TowerHealth>());
			}
		}
	}

	// remove exiting unit from list of targets if they are the enemy
	private void OnTriggerExit(Collider other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("Unit")) {
			UnitAI targetAI = other.GetComponent<UnitAI>();
			if (unitTargets.Contains(targetAI)) {
				unitTargets.Remove(targetAI);
			}
		}
		else if (other.gameObject.tag == "Tower") {
			TowerHealth tHealth = other.GetComponent<TowerHealth>();
			if (towerTargets.Contains(tHealth)) {
				towerTargets.Remove(tHealth);
			}
		}
	}

	public void DeployBlessing(string playerID, float radius, float dur, float _tickTime, float _tickDamage) {
		unitTargets = new List<UnitAI>();
		towerTargets = new List<TowerHealth>();

		id = playerID;
		SetRadius(radius);
		tickTime = _tickTime;
		tickDamage = _tickDamage;

		StartCoroutine("ApplyDOT", dur);
		Destroy(gameObject, dur + 5f); //Destroy after duration
	}

	//Sets range for both the collider and the particle or effect it is using, keep it at scale of 1
	private void SetRadius(float radius) {
		_radius = radius;
		gameObject.transform.localScale = new Vector3(radius, 1, radius);
	}

	private IEnumerator ApplyDOT(float dur) {
		while (gameObject) {
			unitTargets.RemoveAll(u => u == null);
			towerTargets.RemoveAll(t => t == null);

			unitTargets.ForEach(u => u.takeDamage(Mathf.Round(tickDamage), Constants.damageSource.blessing));
			towerTargets.ForEach(t => t.TakeDamage(Mathf.Round(tickDamage)));

			yield return new WaitForSeconds(tickTime);
		}
	}
}
