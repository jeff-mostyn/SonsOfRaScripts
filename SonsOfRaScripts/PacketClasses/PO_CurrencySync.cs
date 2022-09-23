using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_CurrencySync : PacketObject
{
    public int p1Gold, p1Favor, p2Gold, p2Favor;

    public PO_CurrencySync(packetType _type, int _p1Gold, int _p1Favor, int _p2Gold, int _p2Favor) {
        type = _type;
        p1Gold = _p1Gold;
        p1Favor = _p1Favor;
        p2Gold = _p2Gold;
        p2Favor = _p2Favor;
    }
}
