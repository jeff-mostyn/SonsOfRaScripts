using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_BattleHardenedSync : PO_Entity
{
	public string ownerPlayerKey;
    public string teamPlayerKey;
    public int stackCount;

    public PO_BattleHardenedSync(int _entityID, string _ownerPlayerKey, string _teamPlayerKey, int _stackCount) {
        type = packetType.battleHardenedSync;
		ownerPlayerKey = _ownerPlayerKey;
        teamPlayerKey = _teamPlayerKey;

        entityID = _entityID;
		stackCount = _stackCount;
    }
}
