using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTime : BuffDebuff
{
	[Header("Values")]
	[SerializeField] private float tickDamage;
	[SerializeField] private float tickTime;
	private float timeUntilTick = 0f;

	void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			timeUntilTick -= Time.deltaTime;
			if (timeUntilTick <= 0) {
				timeUntilTick = tickTime;
				target.takeDamage(tickDamage, Constants.damageSource.blessing);
			}
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
