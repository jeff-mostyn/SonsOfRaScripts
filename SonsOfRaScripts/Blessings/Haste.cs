using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haste : BuffBlessing_I
{
	[Header("References")]
	[SerializeField] private BuffDebuff hasteBuff;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	//iterate through the player's unit list and start a coroutine to temporarily speed them up
	public override void Fire() {
		sound_cast();

		//Call the speed buff function in each unit
		foreach (GameObject unit in LivingUnitDictionary.dict[playerID]) {
			ApplyBuff(unit.GetComponent<UnitAI>());
        }

        goOnCooldown();
    }

	public void ApplyBuff(UnitAI u) {
		BuffDebuff buff = Instantiate(hasteBuff, u.gameObject.transform);
		buff.ApplyEffect(playerID, u, duration);
	}

    protected override void SendPacket() {
        PO_Haste packet = new PO_Haste(bID, playerID);
        OnlineManager.Instance.SendPacket(packet);
    }

	protected void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
