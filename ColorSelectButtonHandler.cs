using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelectButtonHandler : MonoBehaviour
{
    public void AssignColors(string playerId) {
		LoadoutManager l = LoadoutManager.Instance;

		//if (playerId == PlayerIDs.player1) {
		//	l.p1Colors = GetComponentInChildren<PlayerColorPalette>().GetColorPalette();
		//}
		//else {
		//	l.p2Colors = GetComponentInChildren<PlayerColorPalette>().GetColorPalette();
		//}
	}
}
