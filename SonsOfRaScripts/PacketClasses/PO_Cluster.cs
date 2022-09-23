using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Cluster : PacketObject
{
	public PacketObject[] cluster;

    public PO_Cluster(PacketObject[] _cluster) {
        type = packetType.cluster;
		cluster = _cluster;
    }
}
