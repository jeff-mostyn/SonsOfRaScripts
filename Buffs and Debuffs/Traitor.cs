using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traitor : BuffDebuff
{
	#region Declarations
	// ---------------- Nonpublic Variables -------------------
	// gameplay values
	private string originalPlayerID;
	private Material originalMaterial;

	// Visuals
	[Header("Visuals")]
	public List<Material> p1UnitMat;
	public List<Material> p2UnitMat;
	public Material p1CatapultMat, p2CatapultMat;
	[SerializeField] private GameObject particleFxRed, particleFxBlue;
    //[SerializeField] private GameObject particleFxLarge, particleFxSmall;
    private GameObject activeFx;

	[Header("Values")]
	[SerializeField] private new float duration;
	#endregion

	private void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent) {
				ReturnToOwner();
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;

		// remove all non-permanent, non-traitor effects on unit
		for (int i = 0; i < target.activeEffects.Count; i++) {
			if (!target.activeEffects[i].isPermanent && target.activeEffects[i].type != BuffsAndDebuffs.traitor) {
				target.activeEffects[i].Cleanse();
			}
		}

		if (!target.activeEffects.Find(effect => effect.type == BuffsAndDebuffs.traitor)) {
			// swap unit's team
			target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());
			originalPlayerID = target.GetTeamPlayerKey();

			if (originalPlayerID == PlayerIDs.player1) {
				target.SetTeamPlayerKey(PlayerIDs.player2);
				//target.gameObject.GetComponent<UnitMovement>().setRewiredPlayerKey(PlayerIDs.player2);
				// We use the opponent's barracks count because if they are upgraded once, they should still look upgraded
				AssignMat(target, p2UnitMat[GameManager.Instance.player1Controller.GetTrainingGroundCount()], p2CatapultMat);
			}
			else {
				target.SetTeamPlayerKey(PlayerIDs.player1);
				//target.gameObject.GetComponent<UnitMovement>().setRewiredPlayerKey(PlayerIDs.player1);
				AssignMat(target, p1UnitMat[GameManager.Instance.player2Controller.GetTrainingGroundCount()], p1CatapultMat);
			}

			//instantiate particles based on side
			GameObject myFx;
			if (originalPlayerID == PlayerIDs.player2) {
				myFx = particleFxRed;
			}
			else {
				myFx = particleFxBlue;
			}

			ParticleSystem.MainModule tempMain = myFx.GetComponent<ParticleSystem>().main;
			tempMain.duration = duration - 6f; //offset for lifetime of effect

			activeFx = Instantiate(myFx, target.gameObject.transform.GetChild(0));

			//scale effect based on unit type
			if (target.getType() == Constants.unitType.shieldbearer || target.getType() == Constants.unitType.archer) {
				activeFx.transform.localScale *= 1.5f;

			}
			else if (target.getType() == Constants.unitType.catapult) {
				activeFx.transform.localScale = new Vector3(activeFx.transform.localScale.x * 3f, activeFx.transform.localScale.y * 1.5f, activeFx.transform.localScale.z * 3f);
			}

			activeFx.transform.localPosition = Vector3.zero;

			SonsOfRa.Events.GameEvents.InvokeUnitTeamSwitch(target);
			target.RefreshTargets();
		}
		else { // Cancel existing betrayal effect
			for (int i = 0; i < target.activeEffects.Count; i++) {
				if (target.activeEffects[i].type == BuffsAndDebuffs.traitor) {
					target.activeEffects[i].Cleanse();
				}
			}
		}
	}

	private void AssignMat(UnitAI targetAI, Material p1UnitMat, Material catapultMat) {
		GameObject obj = targetAI.gameObject;
		originalMaterial = obj.GetComponentsInChildren<MeshRenderer>()[0].material;

		// swap materials
		if (targetAI.getType() == Constants.unitType.catapult) {
			obj.GetComponentsInChildren<MeshRenderer>()[0].material = catapultMat; //Replace with for loop
			obj.GetComponentsInChildren<MeshRenderer>()[1].material = catapultMat;
			obj.GetComponentsInChildren<MeshRenderer>()[2].material = catapultMat;
		}
		else {
			obj.GetComponentsInChildren<MeshRenderer>()[0].material = p1UnitMat;
			if (targetAI.getType() != Constants.unitType.mummy) { // mummies only have one mesh renderer
				obj.GetComponentsInChildren<MeshRenderer>()[1].material = p1UnitMat;
			}
			obj.GetComponentsInChildren<SkinnedMeshRenderer>()[0].material = p1UnitMat;
		}
	}

	public override void Cleanse() {
		ReturnToOwner();
	}

	private void ReturnToOwner() {
		target.SetTeamPlayerKey(originalPlayerID);

        AssignMat(target, originalMaterial, originalMaterial);

		target.RefreshTargets();

		SonsOfRa.Events.GameEvents.InvokeUnitTeamSwitch(target);

		Destroy(activeFx);
		Destroy(gameObject);
	}
}
