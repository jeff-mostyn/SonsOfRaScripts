using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ra_Passive : StaticUnitPassive
{
	[Header("Ra Passive Stats")]
	[SerializeField] private int unitKillGoldBonus;

	private void Awake() {
		SonsOfRa.Events.GameEvents.UnitDie += TriggerUnitDeathEffect;
	}

	public override void TriggerUnitDeathEffect(UnitAI unit, string unitPlayerId, Constants.damageSource damageSource) {
		if (unitPlayerId != rewiredPlayerKey 
			&& (damageSource == Constants.damageSource.unit || damageSource == Constants.damageSource.blessing)) {
			pController.addGold(unitKillGoldBonus);
		}
	}

	private void OnDestroy() {
		SonsOfRa.Events.GameEvents.UnitDie -= TriggerUnitDeathEffect;
	}
}
