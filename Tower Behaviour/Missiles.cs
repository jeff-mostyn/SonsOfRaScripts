using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Missiles : MonoBehaviour {

	public enum MissileSource { archerTower, obelisk, catapult };

    public GameObject target;
	public UnitAI_Catapult catapultAI;
	public TowerHealth tHealth;
    public float velocity;
    public float damage;
	public bool hasPhysics;

	// Used for projectile motion calculation
	[Header("Projectile Motion")]
	[SerializeField]private float timeOfFlight;
	private float initialDistanceToTarget, distanceTraveled;
	private float timeRemaining;
	private float timePassed;
	private const float LAUNCH_ANGLE = 0.2984513f;	// in radians
	private const float g = 9.81f;
	private float newY, deltaY, initialY;
	private float cosLaunch, sinLaunch;
	[SerializeField] private float startAngle;
	[SerializeField] private Transform childObject;

	[Header("Splash Damage")]
	[SerializeField] private bool useSplashDamage;
	[SerializeField] private float splashDamageMultiplier, splashDamageRadius;
	public LayerMask towerLayer;

	public MissileSource src;

	[Header("Audio")]
	[FMODUnity.EventRef][SerializeField] private string impactEvent;

	void FixedUpdate() {
        if (target) {
			if (hasPhysics) {
				projectileMotionUpdate();
			}
			else {
				defaultUpdate();
			}
        }
        else {
			if (src == MissileSource.archerTower)
				PoolManager.Instance.returnArcherTowerProjectileToPool(gameObject);
            else if (src == MissileSource.catapult) {
                PoolManager.Instance.returnCatapultProjectileToPool(gameObject);
            }
		}
    }

    void OnTriggerEnter(Collider co) {
        if ((co.gameObject.tag == "P1Unit" || co.gameObject.tag == "P2Unit") && co.gameObject == target) {
            if (src == MissileSource.archerTower) {
                target.GetComponent<UnitAI>().takeDamage(damage, Constants.damageSource.tower);
				sound_impact();
                PoolManager.Instance.returnArcherTowerProjectileToPool(gameObject);
            }
            else {
                catapultAI.attackUnit_Helper();
				if (useSplashDamage) {
					SplashDamage(co);
				}
				sound_impact();
				PoolManager.Instance.returnCatapultProjectileToPool(gameObject);
            }
        }
        else if (co.gameObject.CompareTag("Tower") && co.gameObject == target
			&& src == MissileSource.catapult) {
			catapultAI.attackTower_Helper();
			if (useSplashDamage) {
				SplashDamage(co);
			}
			sound_impact();
			PoolManager.Instance.returnCatapultProjectileToPool(gameObject);
        }
    }

	private void defaultUpdate() {
		transform.LookAt(target.transform);
		Vector3 dir = new Vector3(target.transform.position.x, target.transform.position.y + 0.5f, target.transform.position.z) - transform.position;
		GetComponent<Rigidbody>().velocity = dir.normalized * velocity;

		if (!target) {
			if (src == MissileSource.catapult) {
				PoolManager.Instance.returnCatapultProjectileToPool(gameObject);
			}
			else if (src == MissileSource.archerTower) {
				PoolManager.Instance.returnArcherTowerProjectileToPool(gameObject);
			}
		}
	}

	#region Projectile Motion Functions
	// function to set up projectile motion
	public void setupProjectileMotion(float initialDist) {
		initialDistanceToTarget = initialDist;
		initialY = transform.position.y;

		childObject.localRotation = Quaternion.Euler(startAngle, 0f, 0f);
		velocity = (1 / Mathf.Cos(LAUNCH_ANGLE)) * Mathf.Sqrt((.5f * g * initialDistanceToTarget * initialDistanceToTarget) / ((initialDistanceToTarget * Mathf.Tan(LAUNCH_ANGLE)) + transform.position.y));	// this equation is a bitch, thx stackoverflow
		timeRemaining = timeOfFlight;
		timePassed = 0f;
	}

	private void projectileMotionUpdate() {
		timeRemaining -= Time.deltaTime;
		timePassed += Time.deltaTime;

		float distanceTraveledInFrame = velocity * Time.deltaTime * Mathf.Cos(LAUNCH_ANGLE);
		newY = initialY + (velocity * Mathf.Sin(LAUNCH_ANGLE) * timePassed) - (0.5f * g * timePassed * timePassed); // y = v0y*t - 0.5gt^2
		deltaY = newY - transform.position.y;

		float newRot = Mathf.Lerp(startAngle, startAngle * -1, Mathf.SmoothStep(0, 1, timePassed / timeOfFlight));

		transform.Translate(new Vector3(0f, deltaY, distanceTraveledInFrame), Space.Self);
		childObject.localRotation = Quaternion.Euler(newRot, 0f, 0f);

		if (timeRemaining < -timeOfFlight/4) {	// little bit of leeway 
			OnTriggerEnter(target.GetComponent<Collider>());
		}
	}
	#endregion

	private void SplashDamage(Collider damageTarget) {
		Debug.Log("splash damage");
		// find things to hit
		List<Collider> nearbyTowers = Physics.OverlapSphere(target.transform.position, splashDamageRadius, towerLayer).ToList();
		nearbyTowers.RemoveAll(x => x.gameObject.GetComponent<TowerState>().rewiredPlayerKey == catapultAI.GetTeamPlayerKey());

		// play FX
		//explosionFXInstance = Instantiate(explosionFX.gameObject, target.transform);
		//explosionFXInstance.transform.localPosition = Vector3.zero;
		//explosionFXInstance.transform.localRotation = Quaternion.Euler(-90f, 0, 0);
		//explosionFXInstance.transform.localScale *= burstDamageRadius;

		// deal damage
		foreach (Collider c in nearbyTowers) {
			if (c != damageTarget) {
				c.gameObject.GetComponentInChildren<TowerHealth>().TakeDamage(src == MissileSource.catapult ? catapultAI.damage * splashDamageMultiplier : damage * splashDamageMultiplier);
			}
		}
	}

	private void sound_impact() {
		FMOD.Studio.EventInstance attack = FMODUnity.RuntimeManager.CreateInstance(impactEvent);
		attack.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		attack.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		attack.start();
		attack.release();
	}
}
