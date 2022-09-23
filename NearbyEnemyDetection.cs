using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NearbyEnemyDetection : MonoBehaviour
{
	public string rewiredPlayerKey;

	private ContextualCameraShift camShift;
	private string enemyUnitTag;

	public List<GameObject> enemyUnits;
	private float listFlushCooldown;

    // Start is called before the first frame update
    void Start()
    {
		camShift = GameManager.Instance.CameraParent.GetComponent<ContextualCameraShift>();

		enemyUnitTag = rewiredPlayerKey == PlayerIDs.player1 ? "P2Unit" : "P1Unit";

		enemyUnits = new List<GameObject>();

		listFlushCooldown = 1f;
	}

	private void Update() {
		listFlushCooldown -= Time.deltaTime;

		if (listFlushCooldown <= 0) {
			enemyUnits.RemoveAll(u => u == null);
			camShift.SetUnitCount(enemyUnits.Count, rewiredPlayerKey);
			listFlushCooldown = 1f;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == enemyUnitTag) {
			enemyUnits.Add(other.gameObject);

			enemyUnits.RemoveAll(u => u == null);

			camShift.SetUnitCount(enemyUnits.Count, rewiredPlayerKey);
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == enemyUnitTag) {
			enemyUnits.Remove(other.gameObject);

			enemyUnits.RemoveAll(u => u == null);

			camShift.SetUnitCount(enemyUnits.Count, rewiredPlayerKey);
		}
	}
}
