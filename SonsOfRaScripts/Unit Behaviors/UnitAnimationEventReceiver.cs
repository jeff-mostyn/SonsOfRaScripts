using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationEventReceiver : MonoBehaviour {

	UnitAI uAI;

    void Start() {
		uAI = GetComponentInParent<UnitAI>();
    }

    public void Attack() {
		uAI.Attack();
	}

	public void AttackChain(AnimationEvent e) {
		uAI.AttackChain(e.intParameter);
	}

	public void Draw() {
		if (uAI.type == Constants.unitType.archer) {
			((UnitAI_Archer)uAI).sound_draw();
		}
	}

	public void Reload() {
		if (uAI.type == Constants.unitType.catapult) {
			((UnitAI_Catapult)uAI).sound_reload();
		}
	}

	public void AttackSound() {
		uAI.sound_attack();
	}
}
