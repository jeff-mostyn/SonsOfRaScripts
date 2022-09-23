using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntressSpawn : UnitBlessing_I {
	public override void Fire() {
		goOnCooldown();
	}
}
