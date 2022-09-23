using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Earthquake : GlobalBlessing_I {

	[Header("References")]
	[SerializeField] private Stunned stunEffect;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;
	
	// Update is called once per frame
	public override void Fire() {
		// SCREENSHAKE
		Camera.main.GetComponent<cameraShake>().ShakeTheCamera(0.2f, 1.2f);
		GameManager.Instance.player1Controller.ControllerVibration(1, 0.5f, 1.2f);
		GameManager.Instance.player2Controller.ControllerVibration(1, 0.5f, 1.2f);

		// play sound fx
		sound_cast();

		foreach (GameObject unit in LivingUnitDictionary.dict[PlayerIDs.player1]) {
			Stunned stunInstance = Instantiate(stunEffect, unit.transform);
			stunInstance.ApplyEffect(playerID, unit.GetComponent<UnitAI>(), duration);
		}

		foreach (GameObject unit in LivingUnitDictionary.dict[PlayerIDs.player2]) {
			Stunned stunInstance = Instantiate(stunEffect, unit.transform);
			stunInstance.ApplyEffect(playerID, unit.GetComponent<UnitAI>(), duration);
		}

		goOnCooldown();
	}

    protected override void SendPacket() {
        PO_Earthquake packet = new PO_Earthquake(bID, playerID);
        OnlineManager.Instance.SendPacket(packet);
    }

	private void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
