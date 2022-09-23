using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveBlessing_MultiTarget_I : ActiveBlessing_I
{
	// ----------- declarations ---------------
	[Header("Multitarget Active Blessing Stats")]
	[SerializeField] protected int targetCount;
	[SerializeField] protected float timeBetweenTargets;
	protected List<Vector3> targets;
	protected List<GameObject> targeters;
	protected GameObject reticule = null;

	// ------------ abstract functions --------------
	public abstract override bool canFire(string pID, Vector3 height, float playerFavor, bool sendPacket = true);
	public abstract override void Fire();


	public void AddTarget(GameObject t, GameObject ret) {
		targets.Add(ret.transform.position);
		targeters.Add(t);
		if (!reticule) {
			reticule = ret;
		}
	}

	public void AddTarget(Vector3 position) {
		targets.Add(position);
	}

	public int GetTargetCount() {
		return targetCount;
	}

	public void ResetTargets() {
		// clean up data used for this fire
		foreach (GameObject obj in targeters) {
			Destroy(obj);
		}
		targeters.Clear();

		targets.Clear();
	}
}
