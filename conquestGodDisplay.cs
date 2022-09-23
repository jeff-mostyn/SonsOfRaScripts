using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class conquestGodDisplay : MonoBehaviour
{
	[Header("God Images")]
	[SerializeField] private Image playerGod;
	[SerializeField] private Image enemy1God, enemy2God, enemy3God;

	// Start is called before the first frame update
	void Start()
    {
		playerGod.sprite = ConquestManager.Instance.playerPatrons["Conquest Player"].RedSprite;

		enemy1God.sprite = ConquestManager.Instance.playerPatrons["EnemyPlayer1"].ConquestSprite;
		if (ConquestManager.Instance.playerPatrons["EnemyPlayer1"].patronID == Constants.patrons.Ra) {
			enemy1God.gameObject.transform.localScale = new Vector3(enemy1God.gameObject.transform.localScale.x * -1, 
				enemy1God.gameObject.transform.localScale.y, 
				enemy1God.gameObject.transform.localScale.z);
		}

		enemy2God.sprite = ConquestManager.Instance.playerPatrons["EnemyPlayer2"].ConquestSprite;
		if (ConquestManager.Instance.playerPatrons["EnemyPlayer2"].patronID == Constants.patrons.Ra) {
			enemy2God.gameObject.transform.localScale = new Vector3(enemy2God.gameObject.transform.localScale.x * -1,
				enemy2God.gameObject.transform.localScale.y,
				enemy2God.gameObject.transform.localScale.z);
		}

		enemy3God.sprite = ConquestManager.Instance.playerPatrons["EnemyPlayer3"].ConquestSprite;
		if (ConquestManager.Instance.playerPatrons["EnemyPlayer3"].patronID == Constants.patrons.Ra) {
			enemy3God.gameObject.transform.localScale = new Vector3(enemy3God.gameObject.transform.localScale.x * -1,
				enemy3God.gameObject.transform.localScale.y,
				enemy3God.gameObject.transform.localScale.z);
		}
	}
}
