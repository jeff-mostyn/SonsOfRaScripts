using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rewired;

public class Cyclone : ActiveBlessing_I {
    private Player rewiredPlayer;

    public GameObject particles;
	private Vector3 particlesPosition;

	private List<Collider> unitList;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	public void Start()
    {
        unitLayer = 1 << LayerMask.NameToLayer("Unit");
        towerLayer = 1 << LayerMask.NameToLayer("Tower");

		initializeCooldown();
		//SetUpCutaway();
	}

	public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true) {
		particlesPosition = location;

		Collider[] units = Physics.OverlapSphere(location, radius, unitLayer);

		//Remove all units belonging to the player and any that don't have a unitAI component for safety failure
		unitList = units.ToList();
		unitList.RemoveAll(x => x.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey() == pID);

		if (unitList.Count != 0 && !isOnCd && playerFavor >= cost) {
            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { location.x, location.y, location.z };
                PO_Cyclone packet = new PO_Cyclone(bID, pID, loc);
                OnlineManager.Instance.SendPacket(packet);
            }

            StartCoroutine(StartCutawayAndFire(particlesPosition));
			return true;
		}
		else
			return false;
	}

	public override void Fire() {
		// Spawn tornado
		GameObject ps = Instantiate(particles, particlesPosition, particles.transform.rotation);

        //scale children of ring particle
        List<Transform> cycChildren = new List<Transform>();
        cycChildren.AddRange(ps.GetComponentsInChildren<Transform>());
        for (int i = 0; i < cycChildren.Count; i++) {
            cycChildren[i].localScale *= radius * 2f;
        }

		// play sound
		sound_cast();

		StartCoroutine(destroyParticles(ps));

		//Go through each unit and call the damage function of the unit
		unitList.ForEach(x => x.gameObject.GetComponent<UnitAI>().takeDamage(power, Constants.damageSource.blessing));

		goOnCooldown();
	}

	private IEnumerator StartCutawayAndFire(Vector3 position) {
		float distFromSubject = 5.5f;
		float cutawayDuraiton = 1.5f;

		//cutawayCamera.CreateStationaryCutaway(position + Vector3.up, position + Vector3.up, distFromSubject, cutawayDuraiton);

		yield return new WaitForSeconds(0.1f); //was 0.4

		Fire();
	}

	private IEnumerator destroyParticles(GameObject particles) {
		yield return new WaitUntil(delegate { return !particles.GetComponent<ParticleSystem>().IsAlive(); });

		if (particles)
			Destroy(particles);
	}

	private void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
