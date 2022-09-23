using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// struct
[System.Serializable]
public struct CameraConfiguration {
	public string name;
	public Transform location;
	public float FOV;
}

public class PlayerCameraControl : MonoBehaviour
{
	public enum cameraModes { overhead, standard, zoom, left, right, close };

	[Header("Pan Parameters")]
	public float cameraPanUnitsPerSecond;
	public float panBorderThickness = 10f;
	public Vector2 maxPanAllowance;

	[Header("Zoom Parameters")]
	public float maxZoom;
	public float minZoom;
	public float minY;
	public float unitsPerScrollClick;
	public float minTimeForZoom;
	public float maxTimeForZoom;
	private float currentZoomPercent, newX, newZ;
	private Vector3 originalPosition;
	private float position;
	private Coroutine zoomCoroutine;

	[Header("References")]
	public List<CameraConfiguration> configs;
	public Camera cam;
	public Transform trolley;
	public Transform cameraLookat;

	private cameraModes currentMode;
	private Player p;

	private Vector2 moveVector;
	private Vector2 WASDMoveVector;
	private bool playerRadialOpen;

	private void Start() {
		currentMode = cameraModes.standard;
		p = ReInput.players.GetPlayer(0);
		currentZoomPercent = 0f;
		originalPosition = transform.position;
		playerRadialOpen = false;

		if (!(SettingsManager.Instance.GetIsSinglePlayer() || SettingsManager.Instance.GetIsOnline())) {
			enabled = false;
		}

		position = 0;
		zoomCoroutine = null;
	}

	private void Update() {
		if (GameManager.Instance.gameStarted && !GameManager.Instance.gameOver) {
			// get mouse pan stuff
			if (p.controllers.hasKeyboard) {
				moveVector.x = Input.mousePosition.x >= Screen.width - panBorderThickness ? 1f :
					Input.mousePosition.x <= panBorderThickness ? -1f : 0f;
				moveVector.y = Input.mousePosition.y >= Screen.height - panBorderThickness ? -1f :
					Input.mousePosition.y <= panBorderThickness ? 1f : 0f;
			}
			moveVector = moveVector + WASDMoveVector;

			// scroll to zoom
			float scroll = p.GetAxis(RewiredConsts.Action.Zoom_Camera);
			if (Mathf.Abs(scroll) > 0.2) {
				if (scroll > 0 && position * unitsPerScrollClick <= maxZoom) {
					position += 1;
				}
				else if (scroll < 0 && position * unitsPerScrollClick >= minZoom) {
					position -= 1;
				}

				if (zoomCoroutine != null) {
					StopCoroutine(zoomCoroutine);
					zoomCoroutine = null;
				}
				zoomCoroutine = StartCoroutine(Zoom());
			}

			// can't pan if radial is open
			if (playerRadialOpen) {
				moveVector = Vector2.zero;
			}

			// handle pan
			if (moveVector.magnitude >= 0) {
				Vector2 moveDist = moveVector.normalized * cameraPanUnitsPerSecond * Time.deltaTime;

				newX = transform.position.x + moveDist.y;
				newX = Mathf.Clamp(newX, originalPosition.x + (-maxPanAllowance.x * currentZoomPercent), originalPosition.x + (maxPanAllowance.x * currentZoomPercent));

				newZ = transform.position.z + moveDist.x;
				newZ = Mathf.Clamp(newZ, originalPosition.z + (-maxPanAllowance.y * currentZoomPercent), originalPosition.z + (maxPanAllowance.y * currentZoomPercent));

				transform.position = new Vector3(newX, transform.position.y, newZ);
			}
		}
	}

	public void MoveToPosition(cameraModes mode ) {
		transform.localPosition = configs[(int)mode].location.localPosition;
		transform.rotation = Quaternion.Euler(configs[(int)mode].location.rotation.eulerAngles);
		cam.fieldOfView = configs[(int)mode].FOV;
		currentMode = mode;
	}

	public void PanCamera(Vector2 dir) {
		WASDMoveVector = dir;
	}

	public void SetPlayerRadialOpen(bool isOpen) {
		playerRadialOpen = isOpen;
	}

	public cameraModes getCameraMode() {
		return currentMode;
	} 

	public float GetZoomPercent() {
		return currentZoomPercent;
	}

	public bool GetIsZooming() {
		return zoomCoroutine != null;
	}

	IEnumerator Zoom() {
		float target;
		float elapsedTime = 0;
		float startZ = trolley.localPosition.z;
		float timeDelta = maxTimeForZoom - minTimeForZoom;
		float maxZoomTimeDistance = maxZoom * .75f;
		float timeForZoom = minTimeForZoom;

		while (elapsedTime < timeForZoom) {
			target = position * unitsPerScrollClick;
			target = target > 0 ? Mathf.Min(maxZoom, target) : Mathf.Max(minZoom, target);

			timeForZoom = minTimeForZoom + timeDelta * Mathf.Abs(target - startZ) / maxZoomTimeDistance;

			float trolleyZPosition = Mathf.Lerp(startZ, target, Mathf.SmoothStep(0, 1, elapsedTime/ timeForZoom));
			trolleyZPosition = Mathf.Clamp(trolleyZPosition, minZoom, maxZoom);
			currentZoomPercent = trolleyZPosition <= 0f ? 0 : Mathf.Max(0f, trolleyZPosition) / maxZoom;
			trolley.localPosition = new Vector3(0f, minY * currentZoomPercent, trolleyZPosition);

			trolley.transform.LookAt(cameraLookat);

			elapsedTime += Time.deltaTime;
			yield return null;
		}
		zoomCoroutine = null;
	}
}
