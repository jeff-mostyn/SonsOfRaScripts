using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_PlayerUpgrade : PacketObject
{
    public Constants.expansionType expansion;
    public string rewiredPlayerKey;

    public PO_PlayerUpgrade(string _rewiredPlayerKey, Constants.expansionType _expansion) {
        type = packetType.playerUpgrade;

        rewiredPlayerKey = _rewiredPlayerKey;
        expansion = _expansion;
    }
}
