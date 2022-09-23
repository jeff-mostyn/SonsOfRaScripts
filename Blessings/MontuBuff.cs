using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MontuBuff : MonoBehaviour {

	[SerializeField] DamageBuff empowerBuff;
    private string id;
    private float duration;
    List<GameObject> targetsList;

    private void Start() {
        targetsList = new List<GameObject>();
    }

    //Multiply the unit's attack variable directly
    private void OnTriggerEnter(Collider other)
    {
        if ((id == PlayerIDs.player1 && other.gameObject.tag == "P1Unit")
                || (id == PlayerIDs.player2 && other.gameObject.tag == "P2Unit"))
        {
			targetsList.Add(other.gameObject);
			UnitAI ai = other.gameObject.GetComponent<UnitAI>();
			DamageBuff buff = Instantiate(empowerBuff, ai.gameObject.transform);
			buff.ApplyEffect(id, ai, duration);
		}
    }

    //remove buff by dividing the unit's attack variable
    //if more multiplier are added later down the line then moving the multiplier to a variable in UnitAI might be a better idea.
    private void OnTriggerExit(Collider other)
    {
        if ((id == PlayerIDs.player1 && other.gameObject.tag == "P1Unit")
                || (id == PlayerIDs.player2 && other.gameObject.tag == "P2Unit"))
        {
			UnitAI ai = other.gameObject.GetComponent<UnitAI>();
			BuffDebuff damageBoost = ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.empowerBuff);
			if (damageBoost) {
				damageBoost.Cleanse();
			}
			targetsList.Remove(other.gameObject);
		}
	}

    public void DeployBlessing(string playerID, float radius, float dur)
    {
        id = playerID;
        duration = dur;
        SetDiameter(radius*2);
        Destroy(gameObject, duration);
    }

    //Sets range for both the collider and the particle or effect it is using, keep it at scale of 1
    private void SetDiameter(float radius)
    {
        gameObject.transform.localScale = new Vector3(radius, 0.1f, radius);
    }

    //remove the buff from any units that remain inside the area when it reaches full duration
    private void OnDestroy()
    {
		if (targetsList.Count != 0)
        {
            foreach (GameObject g in targetsList)
            {
				if (g != null) {
					UnitAI ai = g.GetComponent<UnitAI>();
					BuffDebuff damageBoost = ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.empowerBuff);
					if (damageBoost) {
						damageBoost.Cleanse();
					}
				}
			}
        }
    }
}
