using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_KeepDamage : PacketObject
{
    public string rewiredPlayerKey;
    public float damage;

    public PO_KeepDamage(string _rewiredPlayerKey, float _damage) {
        type = packetType.keepDamage;

        rewiredPlayerKey = _rewiredPlayerKey;
        damage = _damage;
    }
}
