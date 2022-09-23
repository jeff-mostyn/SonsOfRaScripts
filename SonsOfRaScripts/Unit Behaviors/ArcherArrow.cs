using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherArrow : MonoBehaviour {

	// ---------- public variables ------------
	public float hitCount;
    public float movementSpeed;
    public float damage;
    public float duration;
    public string rewiredPlayerKey = "Player0";
	public float fadePctPerSecond;

	// ---------- private variables -------------
    private Player rewiredPlayer;
	private UnitAI archer;
    [SerializeField] private Gradient endGrad;

    // Use this for initialization
    public void startArrow(string pID, float damage, UnitAI _archer)
    {
        rewiredPlayerKey = pID;
        GetComponent<BoxCollider>().enabled = true;
		StartCoroutine(fadeAndDestroy());

		archer = _archer;
    }

    void Update () {
        transform.position += transform.forward * Time.deltaTime * movementSpeed;
    }

    private void OnTriggerEnter(Collider other) {
        if ((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
                || (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit"))
        {
            UnitAI target = other.GetComponent<UnitAI>();
            if (target.getType() == Constants.unitType.shieldbearer)
            {
                if (target.takeDamage(damage, archer, Constants.damageSource.unit)) {
					SonsOfRa.Events.GameEvents.InvokeUnitDealDamage(archer, target, damage);
                }
                ((UnitAI_Shieldbearer)target).sound_blockArrow();
				Destroy(gameObject);
            }
            else
            {
                if (target.takeDamage(damage, archer, Constants.damageSource.unit)) {
					SonsOfRa.Events.GameEvents.InvokeUnitDealDamage(archer, target, damage);
				}
                //Debug.Log("Archer dmg: " + damage);
                PenetratingDamage();
            }
        }
    }

    private void PenetratingDamage() //reduce damage after every hit, down to minimum of 25
    {
		hitCount -= 1;

		if (hitCount == 0) {
			//Destroy(gameObject);
			GetComponent<BoxCollider>().enabled = false;
		}
		else {
			damage /= 2;
		}
    }

	private IEnumerator fadeAndDestroy() {
		yield return new WaitForSeconds(duration);

		ParticleSystem p = GetComponent<ParticleSystem>();

		p.Play();

		Material m = GetComponentInChildren<MeshRenderer>().material;
		TrailRenderer t = GetComponent<TrailRenderer>();

		Gradient tmpGrad, origGrad;
        Color tmpCol;

        origGrad = t.colorGradient;
        float lerpAmt = 0f;

        while (lerpAmt < 1 && m.color.a > 0){
            //trail gradient
            lerpAmt += fadePctPerSecond * Time.deltaTime;
            tmpGrad = LerpTwoGradients(origGrad, endGrad, lerpAmt);

            t.colorGradient = tmpGrad;

            //arrow mat
            tmpCol = m.color;
            tmpCol.a = tmpCol.a - (fadePctPerSecond * Time.deltaTime);
            m.color = tmpCol;

            yield return null;
        }

		Destroy(gameObject);
	}

    
    Gradient LerpTwoGradients(Gradient grad1, Gradient grad2, float lerpVal)
    {
        List<float> keyTimes = new List<float>();
        //color keytimes
        for (int i = 0; i < grad1.colorKeys.Length; i++)
        {
            float k = grad1.colorKeys[i].time;
            if (!keyTimes.Contains(k))
                keyTimes.Add(k);
        }
        for (int i = 0; i < grad2.colorKeys.Length; i++)
        {
            float k = grad2.colorKeys[i].time;
            if (!keyTimes.Contains(k))
                keyTimes.Add(k);
        }
        //alpha keytimes
        for (int i = 0; i < grad1.alphaKeys.Length; i++)
        {
            float k = grad1.alphaKeys[i].time;
            if (!keyTimes.Contains(k))
                keyTimes.Add(k);
        }
        for (int i = 0; i < grad2.alphaKeys.Length; i++)
        {
            float k = grad2.alphaKeys[i].time;
            if (!keyTimes.Contains(k))
                keyTimes.Add(k);
        }

        GradientColorKey[] colors = new GradientColorKey[keyTimes.Count];
        GradientAlphaKey[] alphas = new GradientAlphaKey[keyTimes.Count];

        for (int i = 0; i < keyTimes.Count; i++)
        {
            float key = keyTimes[i];
            Color currColor = Color.Lerp(grad1.Evaluate(key), grad2.Evaluate(key), lerpVal);
            colors[i] = new GradientColorKey(currColor, key);
            alphas[i] = new GradientAlphaKey(currColor.a, key);
        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(colors, alphas);
        return gradient;
    }
}
