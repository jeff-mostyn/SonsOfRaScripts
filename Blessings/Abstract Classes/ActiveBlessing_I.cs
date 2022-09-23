using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveBlessing_I : Blessing {
	[Header("Active Blessing Stats")]
	public float radius;	
	public float secondaryRange;
    public float power;

	public LayerMask unitLayer;
    public LayerMask towerLayer;

	[Header("Active Blessing Targeter References")]
	public ParticleSystem P1targeter;
	public ParticleSystem P2targeter;

	public abstract bool canFire(string pID, Vector3 height, float playerFavor, bool sendPacket = true);
    public abstract override void Fire();
}
