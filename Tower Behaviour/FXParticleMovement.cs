using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXParticleMovement : MonoBehaviour {

	public float travelTime;

	private float timeLeft;
	private Vector3 distance;
	private Vector3 distancePerSecond;

	// Use this for initialization
	void Start () {
		timeLeft = travelTime;
	}
	
	public IEnumerator moveToHelper() {
		while (timeLeft >= 0) {
			transform.Translate(distancePerSecond.x * Time.deltaTime, distancePerSecond.y * Time.deltaTime, distancePerSecond.z * Time.deltaTime);
			yield return null;
			timeLeft -= Time.deltaTime;
		}
		Destroy(gameObject);
	}

	public void moveTo(Vector3 targetPosition) {
		distance = transform.position - targetPosition;
		distancePerSecond = distance / travelTime;
	}
}
