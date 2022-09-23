using SonsOfRa.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPassive : PatronPassive
{
	[SerializeField] private List<BuffDebuff> passiveBuffs;

	private void Awake() {
		GameEvents.UnitSpawn += ApplyBuff;
	}

	public void ApplyBuff(UnitAI u) {
		if (u.GetTeamPlayerKey() == rewiredPlayerKey) {
			foreach (BuffDebuff buff in passiveBuffs) {
				GameObject buffInstance = Instantiate(buff.gameObject, u.gameObject.transform);
				buffInstance.GetComponent<BuffDebuff>().ApplyEffect(rewiredPlayerKey, u);
			}
		}
	}

	private void OnDestroy() {
		GameEvents.UnitSpawn -= ApplyBuff;
	}
}
