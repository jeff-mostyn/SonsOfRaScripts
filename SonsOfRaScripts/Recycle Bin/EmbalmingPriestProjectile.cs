using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class EmbalmingPriestProjectile : MonoBehaviour
{
	/*          for Slerp
    public float smoothFactor = 2;
    public GameObject end;
    */
	// ---------- public variables ------------
	public float movementSpeed;
	public float damage;
	public float duration;
	public string rewiredPlayerKey = "Player0";

	// ---------- private variables -------------
	private Player rewiredPlayer;
	[SerializeField] private BuffDebuff embalmDebuff;
	private GameObject embalmDebuffObj;
    private UnitAI casterAI;
	private GameObject caster;
    private GameObject target;

	// Use this for initialization
	public void startProjectile(string pID, GameObject _caster) {
		rewiredPlayerKey = pID;
		GetComponent<SphereCollider>().enabled = true;
		embalmDebuffObj = embalmDebuff.gameObject;
		caster = _caster;
        casterAI = caster.GetComponent<UnitAI>();

		StartCoroutine(fadeAndDestroy());
	}

	void Update() {
		transform.position += transform.forward * Time.deltaTime * movementSpeed;
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject == target) {
			UnitAI enemyAI = other.GetComponent<UnitAI>();
			if (!enemyAI.is_debuffEmbalm) {
				BuffDebuff debuff = Instantiate(embalmDebuff);
				debuff.ApplyEffect(rewiredPlayerKey, enemyAI);
			}
			else {
				for (int i = 0; i < enemyAI.activeEffects.Count; i++) {
					if (!enemyAI.activeEffects[i]) {
						enemyAI.activeEffects.RemoveAt(i);
						i--;
					}
					else if (enemyAI.activeEffects[i].type == BuffDebuff.BuffsAndDebuffs.embalm) {
						enemyAI.activeEffects[i].GetComponent<Embalm>().RefreshDuration();
					}
				}
			}

			if (enemyAI.takeDamage(damage * casterAI.damageModifier, caster.GetComponent<UnitAI>(), Constants.damageSource.unit)) {
				SonsOfRa.Events.GameEvents.InvokeUnitDealDamage(caster.GetComponent<UnitAI>(), enemyAI, damage * casterAI.damageModifier);
			}

			Destroy(gameObject);
		}
	}

	private IEnumerator fadeAndDestroy() {
		yield return new WaitForSeconds(duration);

		Destroy(gameObject);
	}

    public void SetTarget(GameObject _target) {
        target = _target;
    }
}
