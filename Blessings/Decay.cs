using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decay : ActiveBlessing_I {

	// --------- public variables -----------
	// references
	public GameObject area;
	public GameObject PrimaryParticles;

	// gameplay values
	public float tickFrequency;

	// ----------- private variables ------------
	// references
	private GameObject primary;
	private List<Collider> unitsList;

	// gameplay values
	private Vector3 _location;
	private string _pID;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;
	FMOD.Studio.EventInstance fxState;

	public void Start() {
		unitLayer = 1 << LayerMask.NameToLayer("Unit");

		initializeCooldown();
		SetUpCutaway();
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

	public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true) {
		if (!isOnCd && playerFavor >= cost) {
			_location = location;
			_pID = pID;

            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { _location.x, _location.y, _location.z };
                PO_Decay packet = new PO_Decay(bID, _pID, loc);
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
		// cutawayCamera.CreateStationaryCutaway(_ground, _ground + Vector3.up, 8f, 1.5f);

		GameObject go = Instantiate(area, _location, Quaternion.identity) as GameObject;
		go.GetComponent<DecayEffect>().DeployBlessing(_pID, radius, duration);

		// play sound fx
		StartCoroutine(sound_cast());

		primary = Instantiate(PrimaryParticles, _location + new Vector3(0, 0.1f*radius, 0), PrimaryParticles.transform.rotation);
		primary.transform.localScale = new Vector3(radius, radius, radius);

		StartCoroutine(Particles());

		goOnCooldown();
	}

	IEnumerator Particles() {
		yield return new WaitForSeconds(duration);

		//Destroy(primary);
	}

	private IEnumerator sound_cast() {
		fxState = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		fxState.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		fxState.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		fxState.start();

		// need to have this pan
		yield return new WaitForSeconds(duration);

		fxState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		fxState.release();
	}
}
