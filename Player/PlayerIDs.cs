using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerIDs
{
    public static string player1 = "Player0";
    public static string player2 = "Player1";


	public static string GetOpponentPID(string id) {
		if (id == player1) {
			return player2;
		}
		else {
			return player1;
		}
	}
}
