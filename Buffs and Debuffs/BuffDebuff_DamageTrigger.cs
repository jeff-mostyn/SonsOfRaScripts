using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffDebuff_DamageTrigger : BuffDebuff
{
	public enum EffectRecipients { attacker, defender };
	public EffectRecipients recipient;

	public abstract void TriggerEffect(UnitAI defender, UnitAI attacker, float damage);

	void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent) {
				Cleanse();
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;

		duration = _duration;

		target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());
	}

	public override void Cleanse() {
		Destroy(gameObject);
	}
}
