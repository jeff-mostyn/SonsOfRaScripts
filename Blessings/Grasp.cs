using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rewired;

public class Grasp : ActiveBlessing_I
{
    private Player rewiredPlayer;

    [SerializeField] private GameObject chainFXarch;
    [SerializeField] private GameObject chainFXObl;
    [SerializeField] private GameObject chainFXstas;
    [SerializeField] private GameObject chainFXsun;

    private Vector3 _ground;

	private List<Collider> towersList;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	public Grasp()
    {
        cost = 0;
        radius = 30;
        cooldown = 0;
        power = 5; //duration of stun
    }

    public void Start()
    {
        towerLayer = 1 << LayerMask.NameToLayer("Tower");
		towersList = new List<Collider>();

		duration = power;

		initializeCooldown();
	}

    public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true)
    {
		Collider[] towers = Physics.OverlapSphere(location, radius, towerLayer);

        towersList = towers.ToList();
        towersList.RemoveAll(x => x.gameObject.GetComponent<TowerState>().rewiredPlayerKey == pID);
        towersList.RemoveAll(x => x.gameObject.GetComponent<TowerState>().state != TowerState.tStates.placed);

        if (towersList.Count != 0 && !isOnCd && playerFavor >= cost) {
            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { location.x, location.y, location.z };
                PO_Grasp packet = new PO_Grasp(bID, pID, loc);
                OnlineManager.Instance.SendPacket(packet);
            }

            Fire();
			return true;
		}
		else
			return false;
    }

    public override void Fire()
    {
		// play sound fx
		sound_cast();

		foreach (Collider tower in towersList)
        {
            helper(power, tower);
        }

		goOnCooldown();
	}

    private void helper(float stunDuration, Collider tower)
    {
        StartCoroutine(StunWait(stunDuration, tower));
    }

    IEnumerator StunWait(float duration, Collider tower)
    {
		GameObject tempFX = null;

		TowerState tState = tower.GetComponent<TowerState>();
        /*
         * if (tState.type == Constants.towerType.sunTower) {
            tower.GetComponentInChildren<SunTowerAttacker>().isStunned = true;
			tempFX = Instantiate(particles, tower.transform.position + new Vector3(0, 3, 0), particles.transform.rotation);
			tempFX.transform.SetParent(tower.transform);
		}
        else if (tState.type == Constants.towerType.archerTower || tState.type == Constants.towerType.obelisk) {
            tower.GetComponentInChildren<TowerAttacker>().isStunned = true;
			tempFX = Instantiate(particles, tower.transform.position + new Vector3(0, 3, 0), particles.transform.rotation);
			tempFX.transform.SetParent(tower.transform);
		}*/
        if (tState.type == Constants.towerType.sunTower)
        {
            tower.GetComponentInChildren<SunTowerAttacker>().isStunned = true;
            tempFX = Instantiate(chainFXsun, tower.transform.position, tower.transform.rotation);
            tempFX.transform.SetParent(tower.transform);
        }
        else if (tState.type == Constants.towerType.archerTower)
        {
            tower.GetComponentInChildren<TowerAttacker>().isStunned = true;
            tempFX = Instantiate(chainFXarch, tower.transform.position, tower.transform.rotation);
            tempFX.transform.SetParent(tower.transform);
        }
        else if (tState.type == Constants.towerType.obelisk)
        {
            tower.GetComponentInChildren<TowerAttacker>().isStunned = true;
            tempFX = Instantiate(chainFXObl, tower.transform.position, tower.transform.rotation);
            tempFX.transform.SetParent(tower.transform);
        }
        else if (tState.type == Constants.towerType.stasisTower)
        {
            tower.GetComponentInChildren<StasisTowerAttacker>().StunStasisTower();
            tempFX = Instantiate(chainFXstas, tower.transform.position, tower.transform.rotation);
            tempFX.transform.SetParent(tower.transform);
        }

        tempFX.GetComponent<TowerLockFXScript>().UpdateDur(duration);

        yield return new WaitForSecondsRealtime(duration);

        //in FX code, already destroys self
		//if (tempFX)
		//	Destroy(tempFX);

		if (tower && tState.type == Constants.towerType.sunTower) {
            tower.GetComponentInChildren<SunTowerAttacker>().isStunned = false;
        }
        else if (tower && tState.type == Constants.towerType.archerTower || tState.type == Constants.towerType.obelisk) {
            tower.GetComponentInChildren<TowerAttacker>().isStunned = false;	
		}
        else if (tState.type == Constants.towerType.stasisTower)
        {
            tower.GetComponentInChildren<StasisTowerAttacker>().UnstunStasisTower();
        }
    }

	protected void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
