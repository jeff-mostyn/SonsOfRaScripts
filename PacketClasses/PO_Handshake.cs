using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Handshake : PacketObject
{
	public string pingLocation;

    public PO_Handshake(string _pingLocation) {
        type = packetType.handshake;
		pingLocation = _pingLocation;
    }
}
