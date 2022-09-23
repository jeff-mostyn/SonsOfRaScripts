using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {
    #region Variables and Declarations
    public static PoolManager Instance;

	public GameObject archerTowerProjectilePrefab;
	public GameObject obeliskProjectilePrefab;
    [SerializeField] private GameObject catapultProjectivePrefab;

	public int archerTowerProjectilePoolCount;
	public int obeliskProjectilePoolCount;
    [SerializeField] private int catapultProjectilePoolCount;

	private Queue<GameObject> archerTowerProjectiles;
	private Queue<GameObject> obeliskProjectiles;
    private Queue<GameObject> catapultProjectiles;
	private GameObject projectileToAdd;
    #endregion

    #region Unity Overrides
    private void Awake() {
		// There can only be one
		if (Instance == null) {
			Instance = this;
		}
		else {
			Instance = this;
			GameObject.Destroy(this);
		}

		if (GameObject.FindObjectsOfType<PoolManager>().Length > 1) {
			Destroy(this.gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		archerTowerProjectiles = new Queue<GameObject>();
		//obeliskProjectiles = new Queue<GameObject>();
        catapultProjectiles = new Queue<GameObject>();

		for (int i=0; i<archerTowerProjectilePoolCount; i++) {
			projectileToAdd = Instantiate(archerTowerProjectilePrefab);
			projectileToAdd.SetActive(false);
			archerTowerProjectiles.Enqueue(projectileToAdd);
		}

        for (int i = 0; i < catapultProjectilePoolCount; i++) {
            projectileToAdd = Instantiate(catapultProjectivePrefab, Vector3.zero, Quaternion.identity);
            projectileToAdd.SetActive(false);
            catapultProjectiles.Enqueue(projectileToAdd);
        }
	}
    #endregion

    // -------------------------- Pool Functions -----------------------------------
    #region Archer Tower
    // Archer Tower
    public GameObject getArcherTowerProjectile(Vector3 position, Quaternion rotation) {
		GameObject obj = archerTowerProjectiles.Dequeue();
		obj.SetActive(true);
		obj.transform.position = position;
		obj.transform.rotation = rotation;
		return obj;
	}

	public void returnArcherTowerProjectileToPool(GameObject obj) {
		obj.SetActive(false);
		archerTowerProjectiles.Enqueue(obj);
	}
    #endregion

    // Catapault
    public GameObject getCatapultProjectileFromPool(Vector3 position, Quaternion rotation) {
        GameObject obj = catapultProjectiles.Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }
    
    public void returnCatapultProjectileToPool(GameObject obj)
    {
        obj.SetActive(false);
        catapultProjectiles.Enqueue(obj);
    }
}
