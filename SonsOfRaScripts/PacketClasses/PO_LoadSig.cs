using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_LoadSig : PacketObject
{
    public PO_LoadSig() {
        type = packetType.loadSignal;
    }
}
