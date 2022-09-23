using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LaneBlessing_I : Blessing
{
	public string playerID;
	protected string lane;
	protected bool laneSet = false;

	public abstract override void Fire();

	public virtual bool CanFire(float favor, bool sendPacket = true) {
		if (laneSet && favor >= cost && !isOnCd) {
            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                SendPacket();
            }

            Fire();
			return true;
		}
		else {
			return false;
		}
	}

	public void SetLane(string l) {
		lane = l;
		laneSet = true;
	}

    protected abstract void SendPacket();
}
