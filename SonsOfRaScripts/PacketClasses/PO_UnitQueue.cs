using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_UnitQueue : PO_Entity {
    public string rewiredPlayerKey;
    public string lane;
    public int unitTypeIndex;

    public PO_UnitQueue(string _rewiredPlayerKey, string _lane, int _unitTypeIndex, int _entityID) {
        type = packetType.unitQueue;
        entityID = _entityID;
        rewiredPlayerKey = _rewiredPlayerKey;
        lane = _lane;
        unitTypeIndex = _unitTypeIndex;        
    }
}
