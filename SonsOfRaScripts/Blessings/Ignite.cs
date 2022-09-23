using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rewired;

public class Ignite : ActiveBlessing_I
{

	// -------------- public variables ----------------
	// references
    public GameObject particleSys;
	public GameObject scorch;

	// gameplay values
	//public float splashDamage;
	public float scorchDuration;
	public float scorchFadePctPerSecond; // from 0 - 1
	public float scorchMaxScale; // decimal percent
	public float scorchMinScale; // decimal percent

	// -------------- private variables ---------------
	private List<Collider> towersList, unitsList;
	private Vector3 _location, _ground;
	private string _pID;
	private Collider closest;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	//Default constructor to set up parameters of abilities
	public Ignite()
    {
        cost = 40;
        radius = 10f;
        power = 9000000f; //LoL school of "Kill This Thing"
		cooldown = 0f;
    }

	public void Start() {
		unitLayer = 1 << LayerMask.NameToLayer("Unit");
		towerLayer = 1 << LayerMask.NameToLayer("Tower");

		initializeCooldown();
		//SetUpCutaway();
	}

	public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true) {
		_location = location;
		_pID = pID;

		Collider[] towers = Physics.OverlapSphere(location, radius, towerLayer);

		//Remove all towers belonging to the player and any that don't have a towerstate component for safety failure
		towersList = towers.ToList();
		towersList.RemoveAll(x => x.gameObject.GetComponent<TowerState>().rewiredPlayerKey == _pID);

		if (towersList.Count != 0 && !isOnCd && playerFavor >= cost) {
            // if online, sync up
            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { _location.x, _location.y, _location.z };
                PO_Ignite packet = new PO_Ignite(bID, _pID, loc);
                OnlineManager.Instance.SendPacket(packet);
            }

			StartCoroutine(StartCutawayAndFire());
			return true;
		}
		else
			return false;
	}

	public override void Fire()
    {
		if (closest) {
			//Damage the tower
			closest.GetComponent<TowerHealth>().TakeDamage(power);

			// Screen Shake
			Camera.main.GetComponent<cameraShake>().ShakeTheCamera(0.15f, .35f);

			// Controller vibration
			GameManager.Instance.player1Controller.ControllerVibration(1, 0.4f, 0.35f);
			GameManager.Instance.player2Controller.ControllerVibration(1, 0.4f, 0.35f);

			// Spawn particles
			GameObject ps = Instantiate(particleSys, closest.transform.position, particleSys.transform.rotation);//Spawn the particle effect at the tower location

			float scorchScale = Random.Range(scorchMinScale, scorchMaxScale);
			float scorchRotate = Random.Range(0f, 360f);
			GameObject scorchObj = Instantiate(scorch, new Vector3(closest.transform.position.x, 0.3f, closest.transform.position.z), scorch.transform.rotation);
			scorchObj.transform.Rotate(new Vector3(0, 0, 1), scorchRotate); // not sure why this has to be done on z axis but it works
			scorchObj.transform.localScale *= scorchScale;

			sound_cast();

			// Scorch mark and cooldown
			StartCoroutine(solarFlareParticles(ps));
			StartCoroutine(scorchMark(scorchObj));

			goOnCooldown();
		}
	}

	private IEnumerator StartCutawayAndFire() {
		//Go through the list to find the closest tower and destroy it
		//Finding the closest tile to the center of the tower(i.e. cursor position)
		if (towersList.Count != 0) {
			closest = towersList[0];
			float smallestDist = Vector3.Distance(_location, closest.transform.position);

			foreach (Collider tower in towersList) {
				float lastDist = Vector3.Distance(_location, tower.transform.position);
				if (lastDist < smallestDist) {
					closest = tower;
					smallestDist = lastDist;
				}
			}

			// Cutaway Camera
			//cutawayCamera.CreateStationaryCutaway(closest.transform.position + new Vector3(0f, closest.bounds.size.y / 2, 0f),
                //closest.transform.position + new Vector3(0f, closest.bounds.size.y / 2, 0f), 6f, 1.5f);

            yield return new WaitForSeconds(0.4f);

			Fire();
		}
	}

	private IEnumerator solarFlareParticles(GameObject particles) {
		yield return new WaitUntil(delegate { return !particles.GetComponent<ParticleSystem>().IsAlive(); });

		Destroy(particles);
	}

	private IEnumerator scorchMark(GameObject scorchMarkObject) {
		yield return new WaitForSeconds(scorchDuration);

		SpriteRenderer r = scorchMarkObject.GetComponent<SpriteRenderer>();
		Color tmp;

		while (r.color.a > 0) {
			tmp = r.color;
			tmp.a = tmp.a - (scorchFadePctPerSecond * Time.deltaTime);	// maybe do this by storing start time and using diff b/w that and current time div. by total time
			r.color = tmp;
			yield return null;
		}

		Destroy(scorchMarkObject);
	}

    public void ForceFire(string pID, Vector3 location, float playerFavor) {
        _location = location;
        _pID = pID;

        Collider[] towers = Physics.OverlapSphere(location, radius, towerLayer);

        //Remove all towers belonging to the player and any that don't have a towerstate component for safety failure
        towersList = towers.ToList();
        towersList.RemoveAll(x => x.gameObject.GetComponent<TowerState>().rewiredPlayerKey == _pID);

        //StartCoroutine(StartCutawayAndFire());
    }

	protected void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
