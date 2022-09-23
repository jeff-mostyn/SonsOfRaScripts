using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecayEffect : MonoBehaviour {

	private float tickTime;
	private float tickDamage;
	private string id;
	List<UnitAI> targetsList;
	[SerializeField] private DamageOverTime decayDot;
	private float duration;

	// add entering unit to list of targets if they are the enemy
	private void OnTriggerEnter(Collider other) {
		if ((id == PlayerIDs.player1 && other.gameObject.tag == "P2Unit") || (id == PlayerIDs.player2 && other.gameObject.tag == "P1Unit")) {
			targetsList.Add(other.gameObject.GetComponent<UnitAI>());
			UnitAI ai = other.gameObject.GetComponent<UnitAI>();
			DamageOverTime dotInstance = Instantiate(decayDot, ai.gameObject.transform);
			dotInstance.ApplyEffect(id, ai, duration);
		}
	}

	// remove exiting unit from list of targets if they are the enemy
	private void OnTriggerExit(Collider other) {
		if ((id == PlayerIDs.player1 && other.gameObject.tag == "P2Unit") || (id == PlayerIDs.player2 && other.gameObject.tag == "P1Unit")) {
			UnitAI ai = other.gameObject.GetComponent<UnitAI>();
			BuffDebuff dot = ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.decayDot);
			if (dot) {
				dot.Cleanse();
			}
			targetsList.Remove(other.gameObject.GetComponent<UnitAI>());
		}
	}

	public void DeployBlessing(string playerID, float radius, float dur) {
		targetsList = new List<UnitAI>();
		id = playerID;
		SetDiameter(radius*2);
		duration = dur;
		Destroy(gameObject, dur); //Destroy after duration
	}

	//Sets range for both the collider and the particle or effect it is using, keep it at scale of 1
	private void SetDiameter(float radius) {
		gameObject.transform.localScale = new Vector3(radius, 0.1f, radius);
	}

	//remove the dot from any units that remain inside the area when it reaches full duration
	private void OnDestroy() {
		if (targetsList.Count != 0) {
			foreach (UnitAI ai in targetsList) {
				BuffDebuff dot = ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.decayDot);
				if (dot) {
					dot.Cleanse();
				}
			}
		}
	}
}
