using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class STile : MonoBehaviour {
	// ----------------------- public variables ------------------------
    public bool isRoad = false;	// on road, cannot be placed upon
    public bool isEmpty = true;	// is empty, has no tower on it
    public bool isDefault = false;  // is default, can not be lost or overtaken by other player
	public bool neutralExpansionTile = false;

	// ---------------------- nonpublic variables ----------------------
	[SerializeField] private string ownerID = "";	// player ID of owner
	private bool isContested;   // if tile is within area of both players
	private int towerRangeCount1, towerRangeCount2; // number of towers whose areas include the tile
    [SerializeField] private List<int> squarePosOnGrid;
    [SerializeField] private int gridPosIndex;
    private TowerSpawner p1spawner, p2spawner;

	private bool player1Preview, player2Preview;
    private MeshRenderer myRend; 

	private void Awake() {
		myRend = GetComponent<MeshRenderer>();
		if (myRend) {
            myRend.material.color = Color.clear;
        }
	}

    private void Start() {
        p1spawner = GameManager.Instance.player1Controller.gameObject.GetComponentInChildren<TowerSpawner>();
        p2spawner = GameManager.Instance.player2Controller.gameObject.GetComponentInChildren<TowerSpawner>();
    }

    private void Update() {
        if (myRend.material.color.a > 0) {
            if (myRend.material.color.r == 1f && myRend.material.color.g == 1f && myRend.material.color.b == 0f && p1spawner.isHoldingTower()) {
                myRend.material.color = new Color(myRend.material.color.r, myRend.material.color.g, myRend.material.color.b, p1spawner.GetPulseValue());
            }
            else if (myRend.material.color.r == 0f && myRend.material.color.g == 0f && myRend.material.color.b == 1f && p2spawner.isHoldingTower()) {
                myRend.material.color = new Color(myRend.material.color.r, myRend.material.color.g, myRend.material.color.b, p2spawner.GetPulseValue());
            }
            else {
                if ((myRend.material.color.r == 1f && myRend.material.color.g == 1f && myRend.material.color.b == 0f)
                    || (myRend.material.color.r == 1f && myRend.material.color.g == 1f && myRend.material.color.b == 1f)) {
                    myRend.material.color = new Color(myRend.material.color.r, myRend.material.color.g, myRend.material.color.b, 0.15f);
                }
                else {
                    myRend.material.color = new Color(myRend.material.color.r, myRend.material.color.g, myRend.material.color.b, 0.3f);
                }
            }
        }
    }

    //Easy method to check all bool flags at once
    public bool isSpawnable(string pID) {
        return ownerID == pID && !isRoad && isEmpty && !neutralExpansionTile;
    }

    public void colorCheck() {
        if ((ownerID == "" || InfluenceTileDictionary.GetAreTilesColored(ownerID)) && gameObject.activeSelf) {  // only change tile mat if the player's tiles are on
			try {
				if (ownerID == PlayerIDs.player1 && isEmpty) {
					myRend.material.color = new Color(1f, 1f, 0, .3f);
				}
				else if (ownerID == PlayerIDs.player2 && isEmpty) {
					myRend.material.color = new Color(0, 0, 1f, .3f);
				}
				else if (ownerID == "" || !isEmpty || isRoad) {
					myRend.material.color = new Color(0f, 0f, 0f, 0f); // slammed alpha so its not visible if it's not colored
				}
			}
			catch {
				myRend = GetComponent<MeshRenderer>();
			}
		}
    }

	public void ColorForPreview() {
		if (ownerID == "" && !isRoad && isEmpty) {
			if (player1Preview && player2Preview) {
                myRend.material.color = new Color(1f, 0, 1f, .25f);
			}
			else if (player1Preview) {
                myRend.material.color = new Color(1f, 0, 0, .25f);
			}
			else if (player2Preview) {
                myRend.material.color = new Color(1f, 1f, 1f, .25f);
			}
			else {
                myRend.material.color = new Color(0f, 0f, 0f, 0f);
			}
		}
	}

	private void CheckIfIsContested() {
		if (towerRangeCount1 > 0 && towerRangeCount2 > 0) {
			isContested = true;
		}
		else {
			isContested = false;
		}
	}

	private void CheckOwnership() {
		if (towerRangeCount1 > 0 || towerRangeCount2 > 0) {	// there is an owner, someone has towers in range
			if (towerRangeCount1 == 0) {	// player one does not, but someone does, so it must be player 2
				SetOwner(PlayerIDs.player2);
				InfluenceTileDictionary.AddTileToDict(gameObject);
			}
			else if (towerRangeCount2 == 0) {   // player two does not, but someone does, so it must be player 1
				SetOwner(PlayerIDs.player1);
				InfluenceTileDictionary.AddTileToDict(gameObject);
			}
		}
		else {  // there is no owner
			InfluenceTileDictionary.RemoveTileFromDict(gameObject, ownerID);
			SetOwner("");
		}

		colorCheck();
	}

	#region Getters and Setters
	// Square Pos getter
	public List<int> Pos {
		get { return squarePosOnGrid; }
	}

	public int PosIndex {
		get { return gridPosIndex; }
		set { gridPosIndex = value; }
	}

	public void SetOwner(string pID) {
		ownerID = pID;
	}

	public string GetOwnerID() {
		return ownerID;
	}

	public void AddTower(string pID) {
		if (pID == PlayerIDs.player1) {
			towerRangeCount1++;
		}
		else {
			towerRangeCount2++;
		}

		CheckIfIsContested();
		CheckOwnership();
	}

	public void RemoveTower(string pID) {
		if (pID == PlayerIDs.player1) {
			towerRangeCount1 = Mathf.Max(towerRangeCount1 - 1, 0);
		}
		else {
			towerRangeCount2 = Mathf.Max(towerRangeCount2 - 1, 0);
		}

		CheckIfIsContested();
		CheckOwnership();
	}

	public int GetTowerCount(string pID) {
		if (pID == PlayerIDs.player1) {
			return towerRangeCount1;
		}
		
		return towerRangeCount2;
	}

	public bool IsContested() {
		return isContested;
	}

	public void SetPreviewFlag(string pID) {
		if (pID == PlayerIDs.player1) {
			player1Preview = true;
		}
		else {
			player2Preview = true;
		}
	}

	public void UnsetPreviewFlag(string pID) {
		if (pID == PlayerIDs.player1) {
			player1Preview = false;
		}
		else {
			player2Preview = false;
		}
	}
	#endregion
}
