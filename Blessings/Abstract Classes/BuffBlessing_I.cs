using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffBlessing_I : Blessing {
	
	public string playerID;

    public virtual bool canFire(string rewiredPlayerKey, float favor, bool sendPacket = true)
    {
        playerID = rewiredPlayerKey;
		if (favor >= cost && !isOnCd) {
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

	public abstract override void Fire();

    protected abstract void SendPacket();
}
