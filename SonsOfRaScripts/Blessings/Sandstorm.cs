using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandstorm : LaneBlessing_I
{
	public string enemyPlayerID;

	[Header("Blessing Effect Stats")]
	public float tickTime;
	public float tickDamage;
	private float timeRemaining;

	[Header("Blessing Effect References")]
	[SerializeField] private Hastened hasteBuff;
	[SerializeField] private DamageOverTime sandstormDot;

    //VFX variables
    [Header("Effects")]
    [SerializeField] private float fxSpawnrate = 0.5f; //fx spawns per second
    [SerializeField] private GameObject fxObject;

    private float fxTimer = 0f;
    private float fxSpawnTimer = 0f;

    private List<GameObject> myFXs = new List<GameObject>();

    private Coroutine myFXCor;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string sandstormFXEvent;
	FMOD.Studio.EventInstance fxState;

	private void Start() {
		initializeCooldown();
	}

	protected override void Update() {
		if (timeRemaining > 0) {
			timeRemaining -= Time.deltaTime;
		}
		base.Update();
	}

	public override void Fire() {
		timeRemaining = duration;

		enemyPlayerID = playerID == PlayerIDs.player1 ? PlayerIDs.player2 : PlayerIDs.player1;

		foreach (GameObject u in LivingUnitDictionary.dict[playerID]) {
			UnitMovement move = u.GetComponent<UnitMovement>();
			if (move.lane == lane) {
				UnitAI ai = u.GetComponent<UnitAI>();
				ApplyEffectToUnit(ai);
			}
		}

		foreach (GameObject u in LivingUnitDictionary.dict[enemyPlayerID]) {
			UnitMovement move = u.GetComponent<UnitMovement>();
			if (move.lane == lane) {
				UnitAI ai = u.GetComponent<UnitAI>();
				ApplyEffectToUnit(ai);
			}
		}

		//spawn VFX
		if (myFXCor != null) {
            StopCoroutine(myFXCor);
        }
        myFXCor = StartCoroutine(SpawnSandstormFx());
		StartCoroutine(SubscribeToUnitSpawns());

		// play sound fx
		StartCoroutine(sound_sandstorm());

		goOnCooldown();
		laneSet = false;
	}

	IEnumerator SubscribeToUnitSpawns() {
		SonsOfRa.Events.GameEvents.UnitSpawn += ApplyEffectToUnit;
		SonsOfRa.Events.GameEvents.UnitTeamSwitch += ApplyEffectToUnit;
		yield return new WaitForSeconds(duration);
		SonsOfRa.Events.GameEvents.UnitSpawn -= ApplyEffectToUnit;
		SonsOfRa.Events.GameEvents.UnitTeamSwitch -= ApplyEffectToUnit;
	}

	IEnumerator SpawnSandstormFx() {
        fxTimer = 0f;
        fxSpawnTimer = 1 / fxSpawnrate;

        while (fxTimer < duration) {
            fxTimer += Time.deltaTime;
            fxSpawnTimer += Time.deltaTime;
            if (fxSpawnTimer >= 1 / fxSpawnrate) {
                GameObject thisFX = Instantiate(fxObject);
                thisFX.GetComponent<SandstormFXFollow>().SetupSpawn(lane, playerID);
                myFXs.Add(thisFX);

                fxSpawnTimer = 0f;
            }

            yield return null;
        }


        if (myFXs.Count > 0)
        {
            for (int i = myFXs.Count - 1; i >= 0; i--)
            {
                myFXs[i].GetComponent<SandstormFXFollow>().DestroySandFx();
                myFXs.Remove(myFXs[i]);
                yield return null;
            }
        }
    }

	public void ApplyEffectToUnit(UnitAI unit) {
		UnitMovement move = unit.gameObject.GetComponent<UnitMovement>();
		if (move.lane == lane) {
			UnitAI ai = unit.gameObject.GetComponent<UnitAI>();
			if (unit.GetTeamPlayerKey() == playerID) {
				// Add buff to unit
				Hastened buffInstance = Instantiate(hasteBuff, ai.gameObject.transform);
				buffInstance.ApplyEffect(playerID, ai, timeRemaining);
			}
			else {
				// add debuff to unit
				DamageOverTime dotInstance = Instantiate(sandstormDot, ai.gameObject.transform);
				dotInstance.ApplyEffect(playerID, ai, timeRemaining);
			}
		}
	}

    protected override void SendPacket() {
        PO_Sandstorm packet = new PO_Sandstorm(bID, playerID, lane);
        OnlineManager.Instance.SendPacket(packet);
    }

	private void OnDestroy() {
		SonsOfRa.Events.GameEvents.UnitSpawn -= ApplyEffectToUnit;
		SonsOfRa.Events.GameEvents.UnitTeamSwitch -= ApplyEffectToUnit;

		try {
			fxState.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			fxState.release();
		}
		catch {
			Debug.Log("sound tried to stop after already ending");
		}
	}

	private IEnumerator sound_sandstorm() {
		fxState = FMODUnity.RuntimeManager.CreateInstance(sandstormFXEvent);
		fxState.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		fxState.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		fxState.start();

		// need to have this pan
		yield return new WaitForSeconds(duration);

		fxState.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		fxState.release();
	}
}
