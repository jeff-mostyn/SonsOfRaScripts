using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleRageBuff : BuffDebuff
{
    #region Declarations
    [Header("Effect")]
    [SerializeField] private float moveSpeedBoost;
    [SerializeField] private float attackSpeedBoost;
    [SerializeField] private float burstDamagePerStack;
    [SerializeField] private float burstDamageRadius;

	[Header("Visuals")]
	[SerializeField] private GameObject mask;
    private GameObject maskInstance;
	[SerializeField] private ParticleSystem particleFX, explosionFX;
    private GameObject FXInstance, explosionFXInstance;
	[SerializeField] private float fadeTime;

    [Header("Layers")]
    public LayerMask unitLayer;
    public LayerMask towerLayer;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string BattleRageBurstEvent;

	BattleHardened bhBuff;
	private bool buffOver = false;
    #endregion

    private void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent && !buffOver) {
				if (bhBuff.GetStacks() > 0) {
					BurstDamage();
				}

                Cleanse();
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;

		if (!target.activeEffects.Find(effect => effect.type == BuffsAndDebuffs.battleRage)
            && target.activeEffects.Find(effect => effect.type == BuffsAndDebuffs.battleHardened)) {
            bhBuff = (BattleHardened)target.activeEffects.Find(effect => effect.type == BuffsAndDebuffs.battleHardened);

			duration = _duration;

			target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());

            // instantiate particles
			// mask
			maskInstance = Instantiate(mask, ((UnitAI_Infantry)t).headLocator);
			maskInstance.transform.localPosition = Vector3.zero;
			maskInstance.transform.localRotation = Quaternion.identity;
			StartCoroutine(FadeMask(true));

			// fx
			FXInstance = Instantiate(particleFX.gameObject, t.baseLocator);
			FXInstance.transform.localPosition = new Vector3(0f, 0.1f, 0f);
			FXInstance.transform.localRotation = Quaternion.identity;

            target.adjustMoveSpeedModifier(moveSpeedBoost);
			target.adjustAttackSpeedModifier(attackSpeedBoost);
        }
	}

	public override void Cleanse() {
		buffOver = true;

        target.adjustMoveSpeedModifier(-moveSpeedBoost);
		target.adjustAttackSpeedModifier(-attackSpeedBoost);

		StartCoroutine(FadeMask(false));

        Destroy(maskInstance, fadeTime);
        Destroy(FXInstance);
        Destroy(gameObject, fadeTime);
    }

    private void BurstDamage() {
		// find things to hit
		List<Collider> nearbyUnits = Physics.OverlapSphere(target.transform.position, burstDamageRadius, unitLayer).ToList();
        List<Collider> nearbyTowers = Physics.OverlapSphere(target.transform.position, burstDamageRadius, towerLayer).ToList();
        nearbyUnits.RemoveAll(x => x.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey() == target.GetTeamPlayerKey());
        nearbyTowers.RemoveAll(x => x.gameObject.GetComponent<TowerState>().rewiredPlayerKey == target.GetTeamPlayerKey());

		sound_burst();

		// play FX
		explosionFXInstance = Instantiate(explosionFX.gameObject, target.transform);
		explosionFXInstance.transform.localPosition = Vector3.zero;
		explosionFXInstance.transform.localRotation = Quaternion.Euler(-90f, 0, 0);
		explosionFXInstance.transform.localScale *= burstDamageRadius;

		Debug.Log("popping fx");

		// deal damage
		foreach (Collider c in nearbyUnits) {
            c.gameObject.GetComponent<UnitAI>().takeDamage(burstDamagePerStack * bhBuff.GetStacks(), Constants.damageSource.blessing);
        }
        foreach (Collider c in nearbyTowers) {
            c.gameObject.GetComponentInChildren<TowerHealth>().TakeDamage(burstDamagePerStack * bhBuff.GetStacks());
        }

		bhBuff.ClearStacks();
	}

	private IEnumerator FadeMask(bool fadeIn) {
		float target = fadeIn ? 1f : 0f;
		float start = fadeIn ? 0f : 1f;

		MeshRenderer renderer = maskInstance.GetComponentInChildren<MeshRenderer>();
		Material mat0 = renderer.materials[0];
		Material mat1 = renderer.materials[1];


		mat0.color = new Color(mat0.color.r, mat0.color.g, mat0.color.b, start);
		mat1.color = new Color(mat1.color.r, mat1.color.g, mat1.color.b, start);

		float timer = 0f;
		Color tmpCol;
		while (timer < fadeTime) {
			float alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, timer / fadeTime));

			tmpCol = mat0.color;
			tmpCol.a = alpha;
			mat0.color = tmpCol;

			tmpCol = mat1.color;
			tmpCol.a = alpha;
			mat1.color = tmpCol;

			yield return null;
			timer += Time.deltaTime;
		}
	}

	public void sound_burst() {
		FMOD.Studio.EventInstance burst = FMODUnity.RuntimeManager.CreateInstance(BattleRageBurstEvent);
		burst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		burst.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		burst.start();
		burst.release();
	}
}
