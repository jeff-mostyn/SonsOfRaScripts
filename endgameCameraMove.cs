using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endgameCameraMove : MonoBehaviour
{
	[Header("References")]
	public Transform p1StartLocation;
	public Transform p2StartLocation;
	public Transform p1KeepLocation, p2KeepLocation;
	public Camera cam;
	public Transform trolley;

	[Header("Feel Adjustments")]
	[Tooltip("Speed in degrees per second")]
	public float rotationSpeed;

	private bool doRotation = false;
	private Vector3 targetPos;

    // Update is called once per frame
    void Update()
    {
		if (doRotation) {
			transform.RotateAround(targetPos, Vector3.up, rotationSpeed * Time.deltaTime);
			transform.LookAt(targetPos);
		}
    }

	public void StartRotation(string loserId) {
		if (loserId == PlayerIDs.player1) {
			// move to p1Start
			transform.position = p1StartLocation.position;
			transform.rotation = Quaternion.Euler(p1StartLocation.rotation.eulerAngles);
			targetPos = p1KeepLocation.position + new Vector3(0f, 1.25f, 0f);
		}
		else {
			// move to p2Start
			transform.position = p2StartLocation.position;
			transform.rotation = Quaternion.Euler(p2StartLocation.rotation.eulerAngles);
			rotationSpeed *= -1;
			targetPos = p2KeepLocation.position + new Vector3(0f, 1.25f, 0f);
		}
		trolley.localPosition = Vector3.zero;
		trolley.localRotation = Quaternion.Euler(Vector3.zero);
		doRotation = true;
	}
}
