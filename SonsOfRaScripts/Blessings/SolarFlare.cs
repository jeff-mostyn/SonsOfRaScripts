using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SolarFlare : ActiveBlessing_MultiTarget_I {
	// -------------- public variables ----------------
	// references
	[Header("Particle Beam References")]
	public ParticleBeamObj deployable;
	private GameObject beam;
	//private LineRenderer beamLine;
    private GameObject beamObj;
    private bool beamGrown = false; //has beam finished appearing
    [SerializeField] private LineRenderer line;

	// gameplay values
	[Header("Particle Beam Stats")]
	public float tickDamage;
	public float tickTime;

	[Header("Particle Beam FX Values")]
	[SerializeField] private float beamPulseMagnitude;
	[SerializeField] private float beamPulseFrequency;

	// -------------- private variables ---------------
	private List<Collider> towersList, unitsList;
	private string _pID;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string solarFlareFXEvent;
	FMOD.Studio.EventInstance fxState;

	public void Start() {
		unitLayer = 1 << LayerMask.NameToLayer("Unit");
		towerLayer = 1 << LayerMask.NameToLayer("Tower");

		targets = new List<Vector3>();
		targeters = new List<GameObject>();

        beamGrown = false;

		duration = timeBetweenTargets;

        initializeCooldown();
		SetUpCutaway();
	}

	protected override void Update() {
		if (targets.Count == 1) {
			line.gameObject.SetActive(true);
			line.positionCount = 2;

			line.SetPosition(0, targets[0] + new Vector3(0f, .2f, 0f));
			line.SetPosition(1, reticule.transform.position + new Vector3(0f, .2f, 0f));
		}
		else {
			line.gameObject.SetActive(false);
			line.positionCount = 0;
		}

		base.Update();
	}

	private void OnDestroy() {
		try {
			fxState.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			fxState.release();
		}
		catch {
			Debug.Log("sound tried to stop after already ending");
		}
	}

	public override bool canFire(string pID, Vector3 height, float playerFavor, bool sendPacket = true) {
		_pID = pID;

		if (!isOnCd && targets.Count == targetCount && playerFavor >= cost) {
            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc1 = { targets[0].x, targets[0].y, targets[0].z };
                float[] loc2 = { targets[1].x, targets[1].y, targets[1].z };
                PO_SolarFlare packet = new PO_SolarFlare(bID, _pID, loc1, loc2);
                OnlineManager.Instance.SendPacket(packet);
            }

            Fire();
			return true;
		}
		else {
			return false;
		}
	}

	public override void Fire() {
		StartCutawayAndFire();
	}

	private void StartCutawayAndFire() {
		// play sound fx
		StartCoroutine(sound_solarFlare());

		// particle fx
		beam = Instantiate(deployable.gameObject, targets[0], Quaternion.identity);
		beam.GetComponent<ParticleBeamObj>().DeployBlessing(_pID, radius, timeBetweenTargets, tickTime, tickDamage);
        //beamLine = beam.GetComponentInChildren<LineRenderer>();
        beamObj = beam.transform.Find("ParticleBeamFX").Find("BeamObj").gameObject;
        beamGrown = false;
        beamObj.transform.localScale = new Vector3(0f, 1f, 0f);

        StartCoroutine(StartLaserLine());
		StartCoroutine(MoveBeam());

		goOnCooldown();

		// clean up data used for this fire
		foreach (GameObject obj in targeters) {
			Destroy(obj);
		}
		targeters.Clear();
	}

	IEnumerator StartLaserLine() {
        //Grow Beam
        float timer = 0f;
        float growTime = 0.1f;
        while (timer < growTime)
        {
            timer += Time.deltaTime;
            beamObj.transform.localScale = new Vector3(Mathf.Lerp(0f, 1f, timer / growTime), 1f, Mathf.Lerp(0f, 1f, timer / growTime));
            yield return null;
        }

        beamGrown = true;

        // screen shake and vibration
        Camera.main.GetComponent<cameraShake>().ShakeTheCamera(0.85f, 0.4f);
		GameManager.Instance.player1Controller.ControllerVibration(1, 0.65f, 0.5f);
		GameManager.Instance.player2Controller.ControllerVibration(1, 0.65f, 0.5f);
		yield return new WaitForSeconds(0.4f);
		Camera.main.GetComponent<cameraShake>().ShakeTheCamera(0.15f, timeBetweenTargets-0.5f);
		GameManager.Instance.player1Controller.ControllerVibration(1, 0.4f, timeBetweenTargets-0.5f);
		GameManager.Instance.player2Controller.ControllerVibration(1, 0.4f, timeBetweenTargets-0.5f);
	}

	IEnumerator MoveBeam() {
		Vector3 direction = targets[1] - targets[0];
		Vector3 velocity = direction / timeBetweenTargets;

		float timeElapsed = 0f;
		float timeChange = 0f;

        while (timeElapsed < timeBetweenTargets)
        {
            timeChange = Time.deltaTime;
            timeElapsed += timeChange;

            // move line
            beam.transform.position += (velocity * timeChange);

            // pulse width of line
            if (beamGrown) { //if beam has finished appearing, have it fluxuate
                float newBeamSize = 1f + beamPulseMagnitude * Mathf.Sin(timeElapsed * beamPulseFrequency);
                beamObj.transform.localScale = new Vector3(newBeamSize, 1f, newBeamSize);
            }

            yield return null;
		}

		targets.Clear();

		beam.GetComponent<ParticleBeamObj>().StopCoroutine("ApplyDOT");
		foreach (ParticleSystem p in beam.GetComponentsInChildren<ParticleSystem>()) {
			p.Stop();
		}
		StartCoroutine(StopLaserLine());
	}

	IEnumerator StopLaserLine() {
        float timer = 0f;
        float growTime = 0.5f;
        while (timer < growTime)
        {
            timer += Time.deltaTime;
            beamObj.transform.localScale = new Vector3(Mathf.Lerp(1f, 0f, timer / growTime), 1f, Mathf.Lerp(1f, 0f, timer / growTime));
            yield return null;
        }
        beamObj.transform.localScale = new Vector3(0f, 1f, 0f);
    }

	private IEnumerator sound_solarFlare() {
		fxState = FMODUnity.RuntimeManager.CreateInstance(solarFlareFXEvent);
		fxState.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		fxState.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		fxState.start();

		// need to have this pan
		yield return new WaitForSeconds(duration);

		fxState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		fxState.release();
	}
}
