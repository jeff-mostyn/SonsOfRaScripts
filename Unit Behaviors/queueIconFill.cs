using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class queueIconFill : MonoBehaviour {

	public UnityEngine.UI.Image queueIcon;
	public UnitSpawner uSpawner; //This lets us call the unit's health each frame
	public Constants.radialCodes lane;

	float currentTime;
	float maxTime = -1;

	int unitCode;


	// Update is called once per frame
	void Update() {
		// prepare to set max health if countdown is low
		if (queueIcon.fillAmount < 0.25 || maxTime == -1) {
			unitCode = uSpawner.peekLane(lane);
			if (unitCode != -1) {
				maxTime = uSpawner.unitBuildTime[unitCode];
			}
			else {
				maxTime = 1;
			}
		}

		queueIcon.fillAmount = uSpawner.remainingTime(lane) / maxTime; //Update image fill
	}
}
