using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitBlessing_I : Blessing {
	// ----------- public variables -------------
	// spawned unit
	public Constants.unitType unitTypeToSpawn;
    
	// gameplay values
	public bool allLanesSpawn;

	// ---------- nonpublic variables -----------
	protected UnitSpawner uSpawner = null;

	public abstract override void Fire();

	public virtual bool canFire(float favor) {
		if (favor >= cost && !isOnCd) {
			return true;
		}
		else {
			return false;
		}
	}
}
