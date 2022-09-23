using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Betrayal : ActiveBlessing_I {

    private List<Collider> unitList;
    private Vector3 _location;
    private string _pID;

    private List<Transform> traitorsList;

	[Header("References")]
    [SerializeField] private GameObject particles;
    [SerializeField] private GameObject betMissile;
	[SerializeField] private Texture lvl2EmisMap;

	[Header("Betrayal Stats")]
	[SerializeField] private float guaranteedPercent;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string castEvent;

	public void Start() {
        unitLayer = 1 << LayerMask.NameToLayer("Unit");
		SetUpMaterialsForDebuff();

        traitorsList = new List<Transform>();

		initializeCooldown();
	}

	#region Activation Functions
	public override bool canFire(string pID, Vector3 location, float playerFavor, bool sendPacket = true) {
		Collider[] units = Physics.OverlapSphere(location, radius, unitLayer);

		//Remove all units that don't have a unitAI component for safety failure
		unitList = units.ToList();

		if (unitList.Count != 0 && !isOnCd && playerFavor >= cost) {
			_location = location;

            Random rand = new Random();
            int guaranteedNumber = Mathf.CeilToInt(unitList.Count * guaranteedPercent);

            // iterate through list of units, 50/50 shot to apply Traitor to them
            for (int i = 0; i < unitList.Count; i++) {
                if (i < guaranteedNumber || Random.Range(0.0f, 1.0f) > 0.5f) {
                    traitorsList.Add(unitList[i].transform);
                }
            }

            if (SettingsManager.Instance.GetIsOnline() && sendPacket) {
                float[] loc = { _location.x, _location.y, _location.z };
                int[] _units = traitorsList.Select(x => x.gameObject.GetComponent<EntityIdentifier>().ID).ToArray();

                PO_Betrayal packet = new PO_Betrayal(bID, pID, loc, _units);
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

        List<Transform> unitTraitors = new List<Transform>(traitorsList);
        traitorsList.Clear();

        GameObject ringPart = Instantiate(particles, new Vector3(_location.x, _location.y + 0.1f, _location.z), Quaternion.identity);

        //scale children of ring particle
        List<Transform> ringChildren = new List<Transform>();
        ringChildren.AddRange(ringPart.GetComponentsInChildren<Transform>());
        for (int i = 0; i < ringChildren.Count; i++) {
            ringChildren[i].localScale *= radius * 2f;
        }

        StartCoroutine(SpawnSouls(unitTraitors));

        goOnCooldown();
    }

    public bool forceCanFire(string pID, Vector3 location, float playerFavor, int[] traitors) {
        if (traitors.Length != 0 && !isOnCd && playerFavor >= cost) {
            _pID = pID;
            _location = location;
            ForceFire(traitors);
            return true;
        }
        else
            return false;
    }

    public void ForceFire(int[] traitors) {
		// sound
		sound_cast();

        List<Transform> unitTraitors = new List<Transform>();

        for (int i=0; i<traitors.Length; i++) {
            GameObject unit = LivingUnitDictionary.dict[traitors[i] % 2 == 0 ? PlayerIDs.player1 : PlayerIDs.player2].Find(u => u.GetComponent<EntityIdentifier>().ID == traitors[i]);
            if (unit != null) {
                unitTraitors.Add(unit.transform);
            }
        }

        GameObject ringPart = Instantiate(particles, new Vector3(_location.x, _location.y + 0.1f, _location.z), Quaternion.identity);

        //scale children of ring particle
        List<Transform> ringChildren = new List<Transform>();
        ringChildren.AddRange(ringPart.GetComponentsInChildren<Transform>());
        for (int i = 0; i < ringChildren.Count; i++) {
            ringChildren[i].localScale *= radius * 2f;
        }

        StartCoroutine(SpawnSouls(unitTraitors));

        goOnCooldown();
    }
	#endregion

	Vector3 FindCirclePos(Vector3 centerPos, float radius, int myNum, int spawnNum) {
        float angDiff = 360 / spawnNum;

        Vector3 pos;
        //Use math to find position around transform, seperated based on angDiff
        pos.x = centerPos.x + radius * Mathf.Sin((myNum * angDiff) * Mathf.Deg2Rad);
        pos.z = centerPos.z + radius * Mathf.Cos((myNum * angDiff) * Mathf.Deg2Rad);
        pos.y = centerPos.y;

        return pos;
    }

    IEnumerator SpawnSouls(List<Transform> targetList)
    {
        yield return new WaitForSeconds(0.12f);

        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject thisSoul = Instantiate(betMissile, FindCirclePos(new Vector3(_location.x, _location.y + 0.1f, _location.z), radius, i, targetList.Count), Quaternion.identity);
            thisSoul.GetComponent<BetrayMissile>().myTarget = targetList[i];

            yield return null;
        }
    }

	private void SetUpMaterialsForDebuff() {
		BetrayMissile missileScript = betMissile.GetComponent<BetrayMissile>();
		LoadoutManager l = LoadoutManager.Instance;
		//UnitSpawner uSpawner1 = GameObject.Find("Player1").GetComponentInChildren<UnitSpawner>();
		//UnitSpawner uSpawner2 = GameObject.Find("Player2").GetComponentInChildren<UnitSpawner>();

		UnitSpawner uSpawner1 = GameManager.Instance.p1.GetComponentInChildren<UnitSpawner>();
		UnitSpawner uSpawner2 = GameManager.Instance.p2.GetComponentInChildren<UnitSpawner>();

		// Catapult Mats
		((Traitor)missileScript.traitorDebuff).p1CatapultMat = Instantiate(uSpawner1.catapultMat[0]);
		((Traitor)missileScript.traitorDebuff).p1CatapultMat.SetColor("_PalCol1", l.getPaletteColor(0, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p1CatapultMat.SetColor("_PalCol2", l.getPaletteColor(1, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p2CatapultMat = Instantiate(uSpawner2.catapultMat[0]);
		((Traitor)missileScript.traitorDebuff).p2CatapultMat.SetColor("_PalCol1", l.getPaletteColor(0, PlayerIDs.player2));
		((Traitor)missileScript.traitorDebuff).p2CatapultMat.SetColor("_PalCol2", l.getPaletteColor(1, PlayerIDs.player2));

		// Level 0 Unit Mat
		((Traitor)missileScript.traitorDebuff).p1UnitMat[0] = Instantiate(uSpawner1.unitMat[0]);
		((Traitor)missileScript.traitorDebuff).p1UnitMat[0].SetColor("_PalCol1", l.getPaletteColor(0, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p1UnitMat[0].SetColor("_PalCol2", l.getPaletteColor(1, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[0] = Instantiate(uSpawner2.unitMat[0]);
		((Traitor)missileScript.traitorDebuff).p2UnitMat[0].SetColor("_PalCol1", l.getPaletteColor(0, PlayerIDs.player2));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[0].SetColor("_PalCol2", l.getPaletteColor(1, PlayerIDs.player2));

		//level 1 unit mat
		((Traitor)missileScript.traitorDebuff).p1UnitMat[1] = Instantiate(uSpawner1.unitMat[1]);
		((Traitor)missileScript.traitorDebuff).p1UnitMat[1].SetColor("_PalCol1", l.getPaletteColor(2, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p1UnitMat[1].SetColor("_PalCol2", l.getPaletteColor(0, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[1] = Instantiate(uSpawner2.unitMat[1]);
		((Traitor)missileScript.traitorDebuff).p2UnitMat[1].SetColor("_PalCol1", l.getPaletteColor(2, PlayerIDs.player2));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[1].SetColor("_PalCol2", l.getPaletteColor(0, PlayerIDs.player2));

		//level 2 unit mat
		((Traitor)missileScript.traitorDebuff).p1UnitMat[2] = Instantiate(uSpawner1.unitMat[2]);
		((Traitor)missileScript.traitorDebuff).p1UnitMat[2].SetColor("_PalCol1", l.getPaletteColor(2, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p1UnitMat[2].SetColor("_PalCol2", l.getPaletteColor(1, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[2] = Instantiate(uSpawner2.unitMat[2]);
		((Traitor)missileScript.traitorDebuff).p2UnitMat[2].SetColor("_PalCol1", l.getPaletteColor(2, PlayerIDs.player2));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[2].SetColor("_PalCol2", l.getPaletteColor(1, PlayerIDs.player2));
		//level 2 emissive element
		((Traitor)missileScript.traitorDebuff).p1UnitMat[2].SetColor("_EmisColor", l.getPaletteColor(1, PlayerIDs.player1));
		((Traitor)missileScript.traitorDebuff).p1UnitMat[2].SetTexture("_EmisMap", lvl2EmisMap);
		((Traitor)missileScript.traitorDebuff).p2UnitMat[2].SetColor("_EmisColor", l.getPaletteColor(1, PlayerIDs.player2));
		((Traitor)missileScript.traitorDebuff).p2UnitMat[2].SetTexture("_EmisMap", lvl2EmisMap);
	}

	private void sound_cast() {
		FMOD.Studio.EventInstance cast = FMODUnity.RuntimeManager.CreateInstance(castEvent);
		cast.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		cast.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		cast.start();
		cast.release();
	}
}
