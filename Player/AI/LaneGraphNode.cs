using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneGraphNode
{
	public Constants.radialCodes lane;
	public LaneGraphNode next { get; set; }
	public LaneGraphNode previous { get; }
	public GameObject waypoint { get; }

	public LaneGraphNode (GameObject _waypoint, LaneGraphNode _previous, Constants.radialCodes _lane) {
		lane = _lane;
		waypoint = _waypoint;
		previous = _previous;
		next = null;
	}

	public float GetDistanceToNext() {
		if (previous != null) {
			return Mathf.Abs(Vector3.Distance(waypoint.transform.position, next.waypoint.transform.position));
		}
		else {
			return -1;
		}
	}

	public float GetDistanceToPrevious() {
		if (previous != null) {
			return Mathf.Abs(Vector3.Distance(waypoint.transform.position, previous.waypoint.transform.position));
		}
		else {
			return -1;
		}
	}

	public float GetDistanceToObject(GameObject unit) {
		return Mathf.Abs(Vector3.Distance(waypoint.transform.position, unit.transform.position));
	}
}
