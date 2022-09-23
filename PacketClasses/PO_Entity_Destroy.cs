using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Entity_Destroy : PO_Entity {
	public string ownerPlayerKey;

	public PO_Entity_Destroy(packetType _type, int _entityId, string _ownerPlayerKey) {
		type = _type;
		entityID = _entityId;
		ownerPlayerKey = _ownerPlayerKey;
	}
}
