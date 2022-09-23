using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rewired;

public class MysticalImmunity : BuffBlessing_I {

    private Player rewiredPlayer;
    private GameObject k;

	[Header("References")]
    public GameObject shockwave;
	//[SerializeField] private Immune immuneEffect;
	[SerializeField] private Immune immuneEffect;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	//default constructor
	public MysticalImmunity()
    {
        cost = 0;
        cooldown = 0;
    }

	public override void Fire() {
		foreach (GameObject unit in LivingUnitDictionary.dict[playerID]) {
			Immune immuneInstance = Instantiate(immuneEffect, unit.transform);
			immuneInstance.ApplyEffect(playerID, unit.GetComponent<UnitAI>(), duration);
		}

		if (playerID == "Player0") {
            k = GameManager.Instance.p1.transform.Find("Keep").gameObject;
        }
        else {
            k = GameManager.Instance.p2.transform.Find("Keep").gameObject;
        }

		// Shake and vibrate
        Camera.main.GetComponent<cameraShake>().ShakeTheCamera(0.1f, .5f);
		GameManager.Instance.player1Controller.ControllerVibration(1, 0.3f, 0.5f);
		GameManager.Instance.player2Controller.ControllerVibration(1, 0.3f, 0.5f);

		// play sound fx
		sound_cast();

		// particle fx
		GameObject s = Instantiate(shockwave, new Vector3(k.transform.position.x, k.transform.position.y + 1.5f, k.transform.position.z),
            Quaternion.Euler(new Vector3(90,0,0)));
        Destroy(s, 3);
		goOnCooldown();
	}

    protected override void SendPacket() {
        PO_Immunity packet = new PO_Immunity(bID, playerID);
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
