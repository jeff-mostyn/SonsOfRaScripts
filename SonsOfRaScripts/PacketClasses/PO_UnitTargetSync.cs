using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_UnitTargetSync : PO_Entity
{
	public string ownerPlayerKey;
    public string teamPlayerKey;
    public int enemyEntityID;

    public PO_UnitTargetSync(packetType _type, int _entityID, string _ownerPlayerKey, string _teamPlayerKey, int _enemyEntityID) {
        type = _type;
		ownerPlayerKey = _ownerPlayerKey;
        teamPlayerKey = _teamPlayerKey;

        entityID = _entityID;
        enemyEntityID = _enemyEntityID;
    }
}
