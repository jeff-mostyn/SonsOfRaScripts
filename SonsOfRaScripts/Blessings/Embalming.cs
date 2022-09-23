using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Embalming : UnitBlessing_I {
	public override void Fire() {
		goOnCooldown();
	}
}
