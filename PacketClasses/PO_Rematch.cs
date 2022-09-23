using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Rematch : PacketObject {
	public string friendName;

	public PO_Rematch(string _friendName) {
		type = packetType.rematchRequest;
		friendName = _friendName;
	}
}
