using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutawayCamController : MonoBehaviour
{
	const float MAX_Z_TRANSLATION = 13.5f;
	const float MAX_X_TRANSLATION = 12.5f;
	const float CUTAWAY_CAMERA_HEIGHT = 7.5f;
	//CutAway Parent has camera as child, use parent as a pivot to rotate camera while pointing at target
	//There is CutAwayCanvas which has render texture of camera, mask for camera view, crack overlay, and animator to control mask and overlay

	private GameObject cutawayCamera;
	public GameObject cutawayCanvas;
    private Camera mainCamera;
    [SerializeField] private GameObject myDisplayObj;
    private Animator cutawayAnim;
    [SerializeField] private float exitAnimTime = 0.5f;
    private bool cutawayOpen = false; //is a cutaway cam already open
	[SerializeField] Vector3 originPosition;

    // Start is called before the first frame update
    void Start() {
        cutawayCamera = transform.GetChild(0).gameObject;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        cutawayAnim = myDisplayObj.GetComponent<Animator>();
    }

    public bool CutawayReady() { //way to check if you can create cutaway right now
        return !cutawayOpen;
    }

    public void CreateStationaryCutaway(Vector3 subjectPosition, Vector3 subjectLookAtPos, float distanceFromSubject, float cutawayDuration) {
		if (CutawayReady()) {
			// Make sure camera is positioned correctly
			resetCamera();

			// Mark start of cutaway
			cutawayCamera.SetActive(true);
			cutawayOpen = true;

			// find location on canvas to put display there
			myDisplayObj.transform.position = mainCamera.WorldToScreenPoint(subjectPosition);

			// orient and position camera
			setCameraPosition(subjectPosition, distanceFromSubject);
			pointCamera(subjectLookAtPos);

			//play opening cutaway anim
			cutawayAnim.SetTrigger("Open");

			//NEXT STEPS: COROUTINE TO CLOSE, DISABLE CAM (AND ENABLE AT START)
			StartCoroutine(CloseCutaway(cutawayDuration));
		}
    }

	public void CloseCutawayGameEnd() {
		StopAllCoroutines();
		StartCoroutine(CloseCutaway(0));
		//cutawayCanvas.SetActive(false);
	}

    IEnumerator CloseCutaway(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        //animate it closing
        cutawayAnim.SetTrigger("Close");

        yield return new WaitForSeconds(exitAnimTime); //wait for overlay to finish animating before new cutaway
        cutawayOpen = false;
		//turn off camera after conclusion (may be unnecessary if not contributing to rendering
        cutawayCamera.SetActive(false);
    }

	// ====== Progressive Enhancements (In Mark's mind) ======
	// - A better camera angle system, allowing more control of X and Y (for more dynamic shots) with randomness (for more variation between shots)
	// - More Robust CutawayReady check, with cooldown to prevent spam uses (unless maybe ultimates), maybe even checks to determine when's an exciting moment to use it
	// - Varying sizes (small cutway for small events, medium cutaway for most blessings, large cutaway for ultimates)
	// - Change RenderTexture resolution, so more powerful machines can have a higher res cutaway cam (tying it into settings)
	// - Scaling cutaway cam based on main camera's distance to board and how small assets appear (unsure about this one)
	// - New overlay art

	#region Helpers
	// find direction from origin to location of subject of cutaway, and move the camera along that vector to
	// just short of the location, then move the camera up
	private void setCameraPosition(Vector3 _subjectPosition, float _distanceFromSubject) {
		Vector3 direction = _subjectPosition - transform.position;
		float distance = direction.magnitude;
		Vector3 unitVector = direction.normalized;

		transform.position = originPosition + (unitVector * (distance - _distanceFromSubject));

		transform.position = transform.position + new Vector3(0f, CUTAWAY_CAMERA_HEIGHT, 0f);
	}

	// point the camera at the subject
	private void pointCamera(Vector3 _subjectPosition) {
		cutawayCamera.transform.LookAt(_subjectPosition);
	}

	// return camera to starting position and rotation
	private void resetCamera() {
		transform.position = originPosition;
		cutawayCamera.transform.rotation = Quaternion.identity;
	}
	#endregion
}
