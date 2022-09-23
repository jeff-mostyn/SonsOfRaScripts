using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LivingUnitDictionary{
    public static Dictionary<string, List<GameObject>> dict = new Dictionary<string, List<GameObject>>()
    {
        { PlayerIDs.player1, new List<GameObject>() },
        { PlayerIDs.player2, new List<GameObject>() }
    };
}
