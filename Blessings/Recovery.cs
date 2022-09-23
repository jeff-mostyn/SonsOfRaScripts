using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recovery : BuffBlessing_I  {
	public HealOverTime healEffect;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	public override void Fire() {
		sound_cast();

        foreach (GameObject unit in LivingUnitDictionary.dict[playerID])
        {
			HealOverTime healInstance = Instantiate(healEffect, unit.transform);
			healInstance.ApplyEffect(playerID, unit.GetComponent<UnitAI>(), duration);
        }

		goOnCooldown();
	}

    protected override void SendPacket() {
        PO_Recovery packet = new PO_Recovery(bID, playerID);
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
