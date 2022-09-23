using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffDebuff : MonoBehaviour {

	// ----------- public variables -------------
	// enumerations
	public enum BuffsAndDebuffs { embalm, traitor, hastened, empowerBuff, decayDot, sandstormDot, recoveryHeal, stasisSlow, immune,
		setPassiveHaste, overheal, damageAbsorb, sekhmetUnitInspire, battleHardened, siphon, battleRage, stunned, protection };
	public enum BuffOrDebuff { buff, debuff };

	// references
	public BuffsAndDebuffs type;
	public BuffOrDebuff effectClassification;
	public UnitAI target;

	// gameplay values
    [Header("Duration")]
	public bool isPermanent;
	//public float duration;
	protected float duration;
	protected float timeElapsed = 0;

	public abstract void ApplyEffect(string _playerKey, UnitAI t, float _duration = -1);
	public abstract void Cleanse(); // this is for any circumstance in which an effect would be ended early

	protected virtual void OnDestroy() {
		if (target) {
			target.RemoveEffectFromList(this);
		}
	}

	public void RefreshDuration() {
		timeElapsed = 0;
	}
}
