using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Empower : ActiveBlessing_I {

    private List<Collider> unitsList;
    private Vector3 _location;
    private string _pID;

    public GameObject area;

	public ParticleSystem PrimaryParticles, AuxParticles, unitBuffParticles;
	private GameObject primary, aux, unitBuff;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;
	FMOD.Studio.EventInstance fxState;

	public Empower()
    {
        cost = 40;
        radius = 6f;
        cooldown = 0f;
    }

    public void Start()
    {
        unitLayer = 1 << LayerMask.NameToLayer("Unit");

		initializeCooldown();
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

	public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true)
    {
		if (!isOnCd && playerFavor >= cost) {
			_location = location;
			_pID = pID;

            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { _location.x, _location.y, _location.z };
                PO_Empower packet = new PO_Empower(bID, _pID, loc);
                OnlineManager.Instance.SendPacket(packet);
            }

            StartCoroutine(StartCutawayAndFire());
			return true;
		}
		else {
			return false;
		}
    }

    public override void Fire()
    {
        GameObject go = Instantiate(area, _location, Quaternion.identity) as GameObject;
        go.GetComponent<MontuBuff>().DeployBlessing(_pID, radius, duration);

		// play sound fx
		StartCoroutine(sound_cast());

		// play particle fx
		primary = Instantiate(PrimaryParticles.gameObject, _location, PrimaryParticles.gameObject.transform.rotation);
		primary.transform.localScale = new Vector3(primary.transform.localScale.x * radius, primary.transform.localScale.y, primary.transform.localScale.z * radius);
		aux = Instantiate(AuxParticles.gameObject, _location, AuxParticles.gameObject.transform.rotation);
		aux.transform.position = new Vector3(aux.transform.position.x, aux.transform.position.y + 3, aux.transform.position.z);

		StartCoroutine(Particles());

		goOnCooldown();
	}

	private IEnumerator StartCutawayAndFire() {
		float distFromSubject = 8f;
		float cutawayDuraiton = 1.5f;

		//cutawayCamera.CreateStationaryCutaway(_location + Vector3.up, _location + Vector3.up * 1.5f, distFromSubject, cutawayDuraiton);

		yield return new WaitForSeconds(0.4f);

		Fire();
	}

	IEnumerator Particles() {
		yield return new WaitForSecondsRealtime(duration);

		Destroy(primary);
		Destroy(aux);
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
