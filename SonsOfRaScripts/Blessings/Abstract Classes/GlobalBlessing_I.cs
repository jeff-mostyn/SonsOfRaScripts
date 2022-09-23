using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GlobalBlessing_I : Blessing {

	public string playerID;

	public virtual bool canFire(float favor, bool sendPacket = true) {
		if (favor >= cost && !isOnCd 
			&& (LivingUnitDictionary.dict[PlayerIDs.player1].Count > 0 || LivingUnitDictionary.dict[PlayerIDs.player2].Count > 0)) {
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
