using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleRage: ActiveBlessing_I {

    private List<Collider> unitList;
    private Vector3 _location;
    private string _pID;

	[Header("References")]
    [SerializeField] private BuffDebuff battleRageBuff;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	public void Start() {
        unitLayer = 1 << LayerMask.NameToLayer("Unit");

        unitList = new List<Collider>();

		initializeCooldown();
	}

	#region Activation Functions
	public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true) {
		Collider[] units = Physics.OverlapSphere(location, radius, unitLayer);

		//Remove all units that don't have a unitAI component for safety failure
		unitList = units.ToList();
        unitList.RemoveAll(u => u.GetComponent<UnitAI>().GetTeamPlayerKey() != pID);


        if (unitList.Count != 0 && !isOnCd && playerFavor >= cost) {
			_location = location;
            _pID = pID;

            // this is for online, worry about it later
            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { _location.x, _location.y, _location.z };
                int[] _units = unitList.Select(x => x.gameObject.GetComponent<EntityIdentifier>().ID).ToArray();

			    PO_BattleRage packet = new PO_BattleRage(bID, pID, loc, _units);
                OnlineManager.Instance.SendPacket(packet);
            }

            Fire();
			return true;
		}
		else
			return false;
	}

    public override void Fire() {
		// sound
		sound_cast();

        foreach(Collider u in unitList) {
			UnitAI uAI = u.gameObject.GetComponent<UnitAI>();
			if (uAI.type != Constants.unitType.catapult) {
				BuffDebuff buff = Instantiate(battleRageBuff, u.transform);
				buff.ApplyEffect(_pID, uAI, duration);
			}
        }

        goOnCooldown();
    }

	public bool forceCanFire(string pID, Vector3 location, float playerFavor, int[] units) {
		if (units.Length != 0 && !isOnCd && playerFavor >= cost) {
			_pID = pID;
			_location = location;
			ForceFire(units);
			return true;
		}
		else
			return false;
	}

	public void ForceFire(int[] units) {
		// sound
		sound_cast();

		unitList = new List<Collider>();

		for (int i = 0; i < units.Length; i++) {
			GameObject unit = LivingUnitDictionary.dict[_pID == PlayerIDs.player1 ? PlayerIDs.player1 : PlayerIDs.player2].Find(u => u.GetComponent<EntityIdentifier>().ID == units[i]);
			if (unit != null) {
				unitList.Add(unit.GetComponent<Collider>());
			}
		}

		// apply the effect
		foreach (Collider u in unitList) {
			UnitAI uAI = u.gameObject.GetComponent<UnitAI>();
			if (uAI.type != Constants.unitType.catapult) {
				BuffDebuff buff = Instantiate(battleRageBuff, u.transform);
				buff.ApplyEffect(_pID, uAI, duration);
			}
		}

		goOnCooldown();
	}
	#endregion

	private void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
