using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireLane : MonoBehaviour
{
    public List<GameObject> lanes;
    public float switchTimer;
    [SerializeField] private float timeLeft;
    public GameObject currentLane;
    //[SerializeField] private DamageOverTime fireLaneDot;

    private void Start()
    {
        timeLeft = switchTimer;
        currentLane = lanes[Random.Range(0, lanes.Count)];

		SonsOfRa.Events.GameEvents.UnitSpawn += AddUnit;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            CleanFireDot();
            SwitchLane();
            timeLeft = switchTimer;
        }
    }

	private void OnDestroy() {
		SonsOfRa.Events.GameEvents.UnitSpawn -= AddUnit;
	}

	private void AddUnit(UnitAI unit) {
		if (gameObject.activeSelf) {
			if (unit.gameObject.GetComponent<UnitMovement>().lane == currentLane.name) {
				unit.gameObject.AddComponent<GeneralDot>();
			}
		}
	}

    public void SwitchLane()
    {
        //currentLane.GetComponent<ParticleSystem>().Stop();
        currentLane = lanes[Random.Range(0, lanes.Count)];
        //currentLane.GetComponent<ParticleSystem>().Play();

        foreach (GameObject u in LivingUnitDictionary.dict[PlayerIDs.player1])
        {
            UnitMovement move = u.GetComponent<UnitMovement>();
            if (move.lane == currentLane.name)
            {
                u.AddComponent<GeneralDot>();
            }
        }

        foreach (GameObject u in LivingUnitDictionary.dict[PlayerIDs.player2])
        {
            UnitMovement move = u.GetComponent<UnitMovement>();
            if (move.lane == currentLane.name)
            {
                u.AddComponent<GeneralDot>();
            }
        }
    }

    public void CleanFireDot()
    {
        foreach (GameObject u in LivingUnitDictionary.dict[PlayerIDs.player1])
        {
            if (u.GetComponent<GeneralDot>() != null)
            {
                //Debug.Log("GETTING RID OF FIRE");
                GeneralDot gd = u.GetComponent<GeneralDot>();
                Destroy(gd);
            }
        }

        foreach (GameObject u in LivingUnitDictionary.dict[PlayerIDs.player2])
        {
            if (u.GetComponent<GeneralDot>() != null)
            {
                GeneralDot gd = u.GetComponent<GeneralDot>();
                Destroy(gd);
            }
        }
    }
}
