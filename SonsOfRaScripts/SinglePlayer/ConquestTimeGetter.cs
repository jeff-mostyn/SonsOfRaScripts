using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConquestTimeGetter : MonoBehaviour
{
	const float SECONDS_IN_HOUR = 3600;
	const float SECONDS_IN_MINUTE = 60;
	bool timeSet;

	private void Start() {
		timeSet = false;
	}

	// Start is called before the first frame update
	void Update()
    {
		if (!timeSet) {
			float seconds = ConquestManager.Instance.timer;
			//int flatseconds = 0;
			int hours = 0;
			int minutes = 0;

			if (seconds > SECONDS_IN_HOUR) {
				hours = (int)Mathf.Floor(seconds / SECONDS_IN_HOUR);
				seconds -= hours * SECONDS_IN_HOUR;
			}
			if (seconds > SECONDS_IN_MINUTE) {
				minutes = (int)Mathf.Floor(seconds / SECONDS_IN_MINUTE);
				seconds -= minutes * SECONDS_IN_MINUTE;
			}


			GetComponent<Text>().text = hours.ToString("0") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00.00");
			timeSet = true;
		}
    }
}
