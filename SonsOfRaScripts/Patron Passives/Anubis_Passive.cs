using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anubis_Passive : StaticUnitPassive
{
	[Header("Anubis Passive Stats")]
	[SerializeField] private int unitDeathGoldBonus;
	[SerializeField] private float unitDeathFavorChance;

	private void Awake() {
		SonsOfRa.Events.GameEvents.UnitDie += TriggerUnitDeathEffect;
	}

	public override void TriggerUnitDeathEffect(UnitAI unit, string unitPlayerId, Constants.damageSource damageSource) {
		if (unitPlayerId == rewiredPlayerKey) {
			pController.addGold(unitDeathGoldBonus);

			float randNum = Random.Range(0f, 1f);
			if (randNum < unitDeathFavorChance) {
				pController.addFavor(1);
			}
		}
	}

	private void OnDestroy() {
		SonsOfRa.Events.GameEvents.UnitDie -= TriggerUnitDeathEffect;
	}
}
