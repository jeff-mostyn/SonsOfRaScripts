using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PatronPassive : MonoBehaviour
{
	[Header("Patron Passive Refs")]
	public PlayerController pController;
	public string rewiredPlayerKey;

	public void Initialize(PlayerController p) {
		pController = p;
		rewiredPlayerKey = pController.rewiredPlayerKey;
	}
}
