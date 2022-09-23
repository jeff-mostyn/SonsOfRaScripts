using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LivingTowerDictionary
{
    public static Dictionary<string, List<GameObject>> dict = new Dictionary<string, List<GameObject>>() {
        { PlayerIDs.player1, new List<GameObject>() },
        { PlayerIDs.player2, new List<GameObject>() }
    };

	public static void NukeDictionary() {
		dict = new Dictionary<string, List<GameObject>>() {
			{ PlayerIDs.player1, new List<GameObject>() },
			{ PlayerIDs.player2, new List<GameObject>() }
		};
	}
}
