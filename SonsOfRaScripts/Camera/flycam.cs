using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flycam : MonoBehaviour {

	//flyspeed is multiplied by the current Axis value, ranging from 0 to 1, thus guaranteeing a smooth transition
	///////between being stationary and moving.

	//
	//NB!!! This script does not include mouse input. To be able to look around with your mouse, add the Mouse Look script from Unity's default camera control scripts!
	//

	//The E and Q buttons can be used to move up and down with more ease.
	///////The movement is done relative to your current rotation.

	//This script is built so that it would be very easy to mess around with and improve or change - have a go!
	//For using Debug.Log, a function has already been created for you. This should keep things more tidied up.
	//Note: Debug text is not shown outside Unity's editor, thus not appearing in a standalone build

	//You can toggle between defaultCam and Fly Cam with the default key, F12. The switching is done in the switchCamera function.

	/*
	////Feel free to use this code for whatever project you might need it for.
	////Crediting me is not required.
	////Have fun and good luck with your games!
	*/

	public float flySpeed = 2;
	public bool isEnabled;
 
	bool shift;
	bool ctrl;
	float accelerationAmount = 30;
	float accelerationRatio = 3;
	float slowDownRatio = 0.2f;

	public float speedH = 2.0f;
	public float speedV = 2.0f;

	private float yaw = 0.0f;
	private float pitch = 0.0f;

	private bool flyCamActive = true;
	public Camera defaultCam, flyCam;

	private void Update() {
		//use shift to speed up flight
		/*if (Input.GetKeyDown(KeyCode.Alpha0)) {
			if (!flyCamActive) {
				flyCam.depth = 0;
				flyCamActive = !flyCamActive;
			}
			else {
				flyCam.depth = -2;
				flyCamActive = !flyCamActive;
			}

		}*/

		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
			shift = true;
			flySpeed *= accelerationRatio;
		}

		if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) {
			shift = false;
			flySpeed /= accelerationRatio;
		}

		//use ctrl to slow up flight
		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) {
			ctrl = true;
			flySpeed *= slowDownRatio;
		}

		if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) {
			ctrl = false;
			flySpeed /= slowDownRatio;
		}

		if (Input.GetKey(KeyCode.W)) {
			transform.Translate(Vector3.forward * flySpeed);
		}
		else if (Input.GetKey(KeyCode.S)) {
			transform.Translate(Vector3.back * flySpeed);
		}

		if (Input.GetKey(KeyCode.A)) {
			transform.Translate(Vector3.left * flySpeed);
		}
		else if (Input.GetKey(KeyCode.D)) {
			transform.Translate(Vector3.right * flySpeed);
		}


		if (Input.GetKey(KeyCode.E)) {
			transform.Translate(Vector3.up * flySpeed);
		}
		else if (Input.GetKey(KeyCode.Q)) {
			transform.Translate(Vector3.down * flySpeed);
		}


		yaw += speedH * Input.GetAxis("Mouse X");
		pitch -= speedV * Input.GetAxis("Mouse Y");

		transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
	}
}
