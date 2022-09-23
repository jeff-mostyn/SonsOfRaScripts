using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetrayMissile : MonoBehaviour
{
    public Transform myTarget;

    [SerializeField] private float maxMissileSpeed = 2f;
    private Vector3 workingSpeed = new Vector3 (0f, 1f, 0f);
    [SerializeField] private float missileAccel = 3f;
    private float honeRange = 8f;

    //movement locks
    private Vector2 xMinMax = new Vector2(-2f, 2f);
    private Vector2 yMinMax = new Vector2(1f, 3f);
    private Vector2 zMinMax = new Vector2(-2f, 2f);

    [SerializeField] private float chaseTime = 1.3f;
    [SerializeField] private float chaseMod = 1.5f;
    [SerializeField] private float timeBeforeSpeedUp = 0.3f; //how many seconds prior to chase should particle speed up

    private IEnumerator myCor;

    [SerializeField] private Gradient startGrad;
    [SerializeField] private Gradient endGrad;
    [SerializeField] private Gradient redGrad;
    [SerializeField] private Gradient blueGrad;
    private TrailRenderer trailRend;

    [SerializeField] private ParticleSystem fireParts;
    [SerializeField] private ParticleSystem.MainModule fireMain;
    //private Gradient fireStartGrad;
    [SerializeField] private ParticleSystem smokeParts;
    [SerializeField] private ParticleSystem.MainModule smokeMain;
    //private Gradient smokeStartGrad;

    [SerializeField] private GameObject spawnPart;
    [SerializeField] private GameObject explosionPart;
    //[SerializeField] private GameObject auraParticle;
    public BuffDebuff traitorDebuff;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string impactEvent;

    // Start is called before the first frame update
    void Start()
    {
		if (myTarget != null) {
			//Determine what side unit is switching to
			if (myTarget.GetComponent<UnitAI>().GetTeamPlayerKey() == PlayerIDs.player1) {
				endGrad = blueGrad;
			}
			else {
				endGrad = redGrad;
			}

			//color
			trailRend = GetComponentInChildren<TrailRenderer>();
			trailRend.colorGradient = startGrad;

			fireMain = fireParts.main;
			smokeMain = smokeParts.main;

			fireMain.startColor = startGrad;
			smokeMain.startColor = startGrad;

			Instantiate(spawnPart, transform.position, Quaternion.identity);

			//movement coroutine
			if (myCor != null) StopCoroutine(myCor);

			myCor = MoveMissile();
			StartCoroutine(myCor);
		}
    }

    //Random Missile Movement
    IEnumerator MoveMissile()
    {
        Vector3 pathTarget = FindRandPathTarg();
        float distToTarg = Vector3.Distance(transform.position, pathTarget);

        while (chaseTime > 0f)
        {
            chaseTime -= Time.deltaTime;
            distToTarg = Vector3.Distance(transform.position, pathTarget);


            //extra speed calculation (speed up near end)
            float tempAccel = missileAccel;
            float tempMaxSpeed = maxMissileSpeed;

            if (chaseTime < timeBeforeSpeedUp)
            {
                tempAccel = missileAccel * Mathf.Lerp(1f, chaseMod, 1 - (timeBeforeSpeedUp/chaseTime)); //lerp modifier until it reaches chase time
                tempMaxSpeed = maxMissileSpeed * Mathf.Lerp(1f, chaseMod, 1 - (timeBeforeSpeedUp / chaseTime));
            }
            

            //move
            workingSpeed += (pathTarget - transform.position).normalized * tempAccel * Time.deltaTime;
            if(workingSpeed.magnitude > tempMaxSpeed)
            {
                workingSpeed = workingSpeed.normalized * maxMissileSpeed;
            }
            Vector3 newPos = transform.position + (workingSpeed * Time.deltaTime);
            transform.position = newPos;
            //Rotation
            Quaternion missileRot = Quaternion.LookRotation(workingSpeed.normalized, Vector3.up);
            transform.rotation = missileRot;

            //if target reached, find new target
            if (Vector3.Distance(transform.position, pathTarget) <= 1f)
            {
                pathTarget = FindRandPathTarg();
            }

            yield return null;
        }

        //Go to your main target
        pathTarget = myTarget.position;
        distToTarg = Vector3.Distance(transform.position, pathTarget);
        float totalDist = distToTarg;

        while (distToTarg > 0.1f)
        {
            //Debug.Log("attacking target");
            pathTarget = myTarget.position;
            distToTarg = Vector3.Distance(transform.position, pathTarget);

            //move
            workingSpeed += (pathTarget - transform.position).normalized * missileAccel * chaseMod * Time.deltaTime; //modify speed based on acceleration
            //Disable max speed for going to target
            /*if (workingSpeed.magnitude > maxMissileSpeed)
            {
                workingSpeed = workingSpeed.normalized * maxMissileSpeed;
            }*/

            //within certain range, hone in on target and make sure it reaches
            if (distToTarg < honeRange)
            {
                Vector3 honeSpeed = (pathTarget - transform.position).normalized * workingSpeed.magnitude;
                workingSpeed = Vector3.Lerp(workingSpeed, honeSpeed, (honeRange - distToTarg) / honeRange);
            }

            Vector3 newPos = transform.position + (workingSpeed * Time.deltaTime);
            transform.position = newPos;

            //Rotation
            Quaternion missileRot = Quaternion.LookRotation(workingSpeed.normalized, Vector3.up);
            transform.rotation = missileRot;

            //Color
            Gradient tempGrad = LerpTwoGradients(startGrad, endGrad, (totalDist - distToTarg) / (totalDist));

            trailRend.colorGradient = tempGrad;

            fireMain.startColor = tempGrad;
            smokeMain.startColor = tempGrad;

            yield return null;
        }

        //reached target
        GameObject myExplo = Instantiate(explosionPart, myTarget.position, Quaternion.identity, myTarget);
        ChangeParticleAndChildrenColor(myExplo, endGrad);

        //give traitor debuff
        BuffDebuff traitor = Instantiate(traitorDebuff, myTarget);
        UnitAI targUnitAI = myTarget.GetComponent<UnitAI>();
        traitor.ApplyEffect(targUnitAI.GetTeamPlayerKey(), targUnitAI);

		// play sound
		sound_impact();

        //wait for trail to complete
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }


    Vector3 FindRandPathTarg()
    {
        //find new target for flight path
        Vector3 newTarget = transform.position + new Vector3(Random.Range(xMinMax.x, xMinMax.y), Random.Range(yMinMax.x, yMinMax.y), Random.Range(zMinMax.x, zMinMax.y));
        //Debug.Log(newTarget);
        //Instantiate(marker, newTarget, transform.rotation);
        return newTarget;
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

    void ChangeParticleAndChildrenColor(GameObject myPart, Gradient myGrad)
    {
        List<ParticleSystem> allParts = new List<ParticleSystem>();

        //particle system on parent
        ParticleSystem myTopPart = myPart.GetComponent<ParticleSystem>();
        if (myTopPart != null)
            allParts.Add(myTopPart);
        //Child particles
        allParts.AddRange(myPart.GetComponentsInChildren<ParticleSystem>());

        //loop to get main module and change start color
        for(int i = 0; i < allParts.Count; i++)
        {
            ParticleSystem.MainModule mainMod = allParts[i].main;
            mainMod.startColor = myGrad;
        }
    }

	private void sound_impact() {
		FMOD.Studio.EventInstance attack = FMODUnity.RuntimeManager.CreateInstance(impactEvent);
		attack.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		attack.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		attack.start();
		attack.release();
	}
}
