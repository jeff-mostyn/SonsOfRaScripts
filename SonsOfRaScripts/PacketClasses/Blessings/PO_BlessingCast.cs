using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_BlessingCast : PacketObject
{
    public Blessing.blessingID blessingId;
    public string rewiredPlayerKey;
}
