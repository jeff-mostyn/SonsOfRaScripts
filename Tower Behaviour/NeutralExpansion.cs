using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NeutralExpansion : MonoBehaviour
{
	#region Variable Declarations
	// ---------------------- public variables --------------------------
	public Constants.expansionType type;

	// --------------------- nonpublic variables ------------------------
	// references
	LayerMask tileLayer;
	[SerializeField] private Canvas ownershipGraphCanvas;
	[SerializeField] private List<STile> tilesInRange;
    [SerializeField] private Material expMat;
    [SerializeField] private Color[] nuetralColors;
    private LoadoutManager l;
    private PlayerController p1, p2;
	[SerializeField] private List<Image> ownershipPoints;
	[SerializeField] private Sprite p1Dot, p2Dot;
	[SerializeField] private GameObject ownershipChangeParticles;
	private ParticleSystem fx;

	// gameplay values
	int p1Tiles = 0, p2Tiles = 0;
	int ownershipThreshold;
	string ownerID = "";
	private bool isActive;
	private float inactiveTime;
	[SerializeField] private float inactiveCountdown, startupTime;
	[SerializeField] private Image countdown;
	[SerializeField] private Canvas countdownCanvas;

	#endregion
	// Start is called before the first frame update
	void Start() {
		inactiveTime = inactiveCountdown + startupTime;
		ownershipThreshold = Mathf.CeilToInt(tilesInRange.Count / 2);

        // set initial material
        l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
        expMat = Instantiate(expMat);
        expMat.SetColor("_PalCol1", nuetralColors[0]);
        expMat.SetColor("_PalCol2", nuetralColors[1]);
        GetComponentsInChildren<MeshRenderer>()[0].material = expMat;

		// get players
		p1 = GameManager.Instance.player1Controller;
		p2 = GameManager.Instance.player2Controller;

		GameObject tempFx = Instantiate(ownershipChangeParticles, transform);
		tempFx.transform.localPosition = Vector3.zero;

		fx = tempFx.GetComponent<ParticleSystem>();
	}

    // Update is called once per frame
    void Update() {
		if (isActive) {
			ownershipGraphCanvas.transform.rotation = Camera.main.transform.rotation;

			int tempP1Tiles = 0, tempP2Tiles = 0;
			for (int i = 0; i < tilesInRange.Count; i++) {
				tilesInRange[i].neutralExpansionTile = true;
				if (tilesInRange[i].GetOwnerID() == PlayerIDs.player1) {
					tempP1Tiles++;
				}
				else if (tilesInRange[i].GetOwnerID() == PlayerIDs.player2) {
					tempP2Tiles++;
				}
			}

			if (tempP1Tiles != p1Tiles && tempP1Tiles != 0) {
				p1Tiles = tempP1Tiles;
				UpdateOwnership();
			}

			if (tempP2Tiles != p2Tiles && tempP2Tiles != 0) {
				p2Tiles = tempP2Tiles;
				UpdateOwnership();
			}

			DisplayOwnershipGraph();
		}
		else {
			countdownCanvas.transform.rotation = Camera.main.transform.rotation;

			if (inactiveTime > 0) {
				inactiveTime -= Time.deltaTime;
				countdown.fillAmount = inactiveTime / inactiveCountdown;
			}
			else {
				isActive = true;
				countdownCanvas.gameObject.SetActive(false);
				ownershipGraphCanvas.gameObject.SetActive(true);
			}
		}
    }

	private void UpdateOwnership() {
		if (p1Tiles > ownershipThreshold && ownerID != PlayerIDs.player1) {
			if (ownerID == PlayerIDs.player2) {
				RemoveFromPlayer(p2);
			}

			ownerID = PlayerIDs.player1;
            //change colors based on owner ID
            expMat.SetColor("_PalCol1", l.getPaletteColor(0, ownerID));
            expMat.SetColor("_PalCol2", l.getPaletteColor(1, ownerID));
            GetComponentsInChildren<MeshRenderer>()[0].material = expMat;
			fx.Play();

			AddToPlayer(p1);
		}
		else if (p2Tiles > ownershipThreshold && ownerID != PlayerIDs.player2) {
			if (ownerID == PlayerIDs.player1) {
				RemoveFromPlayer(p1);
			}

			ownerID = PlayerIDs.player2;
            //change colors based on owner ID
            expMat.SetColor("_PalCol1", l.getPaletteColor(0, ownerID));
            expMat.SetColor("_PalCol2", l.getPaletteColor(1, ownerID));
            GetComponentsInChildren<MeshRenderer>()[0].material = expMat;
            fx.Play();

			AddToPlayer(p2);
		}
		else if (p1Tiles <= ownershipThreshold && p2Tiles <= ownershipThreshold && ownerID != "") {
			if (ownerID == PlayerIDs.player1) {
				RemoveFromPlayer(p1);
			}
			else {
				RemoveFromPlayer(p2);
			}

			ownerID = "";
            expMat.SetColor("_PalCol1", nuetralColors[0]);
            expMat.SetColor("_PalCol2", nuetralColors[1]);
            GetComponentsInChildren<MeshRenderer>()[0].material = expMat;
            fx.Play();
		}

		DisplayOwnershipGraph();
	}

	private void AddToPlayer(PlayerController p) {
		if (type == Constants.expansionType.mine) {
			p.AddMine();
		}
		else if (type == Constants.expansionType.temple) {
			p.AddTemple();
		}
	}

	private void RemoveFromPlayer(PlayerController p) {
		if (type == Constants.expansionType.mine) {
			p.RemoveMine();
		}
		else if (type == Constants.expansionType.temple) {
			p.RemoveTemple();
		}
	}

	private void DisplayOwnershipGraph() {
		for (int i = 0; i<tilesInRange.Count; i++) {
			if (tilesInRange[i].GetOwnerID() == "") {
				ownershipPoints[i].gameObject.SetActive(false);
			}
			else {
				ownershipPoints[i].gameObject.SetActive(true);
				if (tilesInRange[i].GetOwnerID() == PlayerIDs.player1) {
					ownershipPoints[i].sprite = p1Dot;
				}
				else if (tilesInRange[i].GetOwnerID() == PlayerIDs.player2) {
					ownershipPoints[i].sprite = p2Dot;
				}
			}
		}
	}
}
