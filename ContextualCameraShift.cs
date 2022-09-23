using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextualCameraShift : MonoBehaviour
{
	public float defaultFOV;
	public float zoomMaxFOV;
	public float ZPosDelta;
	public float movementDuration;
	public float movementRestDuration;

	public int relevantUnitCount;

	private Camera cam;

	private int p1Units, p2Units;
	private float movementCooldown;

	private enum position { neutral, focusNeutral, focusP1, focusP2 };
	private position currentPosition;
	public Coroutine cameraAdjustment;
	private Dictionary<position, float> camPositionToZPosition;
	private Dictionary<position, float> camPositionToFOV;

    // Start is called before the first frame update
    void Start()
    {
		cam = GetComponentInChildren<Camera>();

		p1Units = 0;
		p2Units = 0;

		movementCooldown = movementRestDuration + movementDuration;
		currentPosition = position.neutral;

		camPositionToZPosition = new Dictionary<position, float> {
			{ position.neutral, 0f },
			{ position.focusP1, -ZPosDelta },
			{ position.focusP2, ZPosDelta },
			{ position.focusNeutral, 0f },
		};

		camPositionToFOV = new Dictionary<position, float> {
			{ position.neutral, defaultFOV },
			{ position.focusP1, zoomMaxFOV },
			{ position.focusP2, zoomMaxFOV },
			{ position.focusNeutral, defaultFOV - ((defaultFOV - zoomMaxFOV) / 2f)}
		};
	}

    // Update is called once per frame
    void Update() {
		if (movementCooldown > 0) {
			movementCooldown -= Time.deltaTime;
		}
	}

	private position ReEvaulatePosition() {
		if (p1Units >= relevantUnitCount && p2Units >= relevantUnitCount) {
			return position.focusNeutral;
		}
		else if (p1Units >= relevantUnitCount) {
			return position.focusP1;
		}
		else if (p2Units >= relevantUnitCount) {
			return position.focusP2;
		}
		else {
			return position.neutral;
		}
	}

	IEnumerator MoveCamera() {
		float currentZPosition = transform.localPosition.z;
		float currentFOV = cam.fieldOfView;

		float targetZPos = camPositionToZPosition[currentPosition];
		float targetFOV = camPositionToFOV[currentPosition];

		float elapsedTime = 0f;

		while (elapsedTime < movementDuration) {
			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				Mathf.Lerp(currentZPosition, targetZPos, Mathf.SmoothStep(0f, 1f, elapsedTime/movementDuration)));
			cam.fieldOfView = Mathf.Lerp(currentFOV, targetFOV, Mathf.SmoothStep(0f, 1f, elapsedTime / movementDuration));

			yield return null;
			elapsedTime += Time.deltaTime;
		}

		transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				targetZPos);
		cam.fieldOfView = targetFOV;

		cameraAdjustment = null;
	}

	public void SetUnitCount(int count, string playerId) {
		if (playerId == PlayerIDs.player1) {
			p1Units = count;
		}
		else {
			p2Units = count;
		}
		//Debug.Log(p1Units + " : " + p2Units);

		position newPosition = ReEvaulatePosition();
		if (newPosition != currentPosition && (cameraAdjustment != null || movementCooldown <= 0) && !GameManager.Instance.gameOver) {
			currentPosition = newPosition;

			if (cameraAdjustment != null) {
				StopCoroutine(cameraAdjustment);
			}

			cameraAdjustment = StartCoroutine(MoveCamera());
			movementCooldown = movementRestDuration + movementDuration;
		}
	}
}
