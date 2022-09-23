using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_WellnessCheck : PacketObject {
	public PO_WellnessCheck() {
		type = packetType.weGoodBro;
	}
}
