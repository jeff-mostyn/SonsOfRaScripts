using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CursorMovement : MonoBehaviour {
	#region variables and declarations
	const float INPUT_THRESHOLD = 0.85f;
	const float DEAD_ZONE_LEEWAY_TIME = 0.2f;

	// -------------- public variables ---------------
	public string rewiredPlayerKey = "Player0";

	// ----------- nonpublic variables ---------------
	[Header("Movement Settings")]
	[SerializeField] private int movesBeforeFastMove;
	[SerializeField] private float slowMoveInterval;
	[SerializeField] private float fastMoveInterval;
	private int slowMoveCount = 0;
	private float cursorMoveTimeCounter = float.MaxValue;
	private float deadZoneCounter = 0;


	[Header("Grid")]
    [SerializeField] private Squares squaresObj;
	[SerializeField] private List<int> initialPosOnGrid;
    public GameObject current;

	private TowerSpawner tSpawner;

	private float horzMove = 0f;
	private float vertMove = 0f;
	private Vector3 moveDir = Vector3.zero;
	private Player rewiredPlayer;
	private PlayerController pController;
	private PlayerDirectionalInput pInput;

	private int gridXPos, gridYPos;

	// Mouse movement variables
#if !UNITY_XBOXONE
	private Camera mainCam;
	LayerMask gridLayerMask;
#endif

	// constants
	private const int ROW_SQUARE_COUNT = 36;
    #endregion

    #region Custom Methods
    private void CenterOnSpace() {
        GameObject placeToMove = squaresObj.GetSpace(gridXPos, gridYPos);
        transform.position = new Vector3(placeToMove.transform.position.x, transform.position.y, placeToMove.transform.position.z);
    }

    private bool canCursorMove() {
		cursorMoveTimeCounter += Time.deltaTime;
		if ((slowMoveCount < movesBeforeFastMove && cursorMoveTimeCounter > slowMoveInterval) ||
			slowMoveCount >= movesBeforeFastMove && cursorMoveTimeCounter > fastMoveInterval) {

			return true;
		}
		else {
			return false;
		}
    }

	private void updateMovementValuesOnMove() {
		if (slowMoveCount < movesBeforeFastMove) {
			slowMoveCount++;
		}
		cursorMoveTimeCounter = 0;
	}

    #region HELPER FUNCTIONS
    // Check if unauthorized square is ABOVE the current position
    private bool CanMoveUp() {
        if (gridYPos - 1 < 0) {
            return false;
        }
        else {
            return true;
        }
    }

    // Check if unauthorized square is BELOW the current position
    private bool CanMoveDown() {
        if (gridYPos + 1 > 23) {
            return false;
        }
        else {
            return true;
        }
    }

    // Check if unauthorized square is RIGHT of the current position
    private bool CanMoveRight() {
        if (gridXPos + 1 > 35) {
            return false;
        }
        else {
            return true;
        }
    }

    // Check if unauthorized square is LEFT of the current position
    private bool CanMoveLeft() {
        if (gridXPos - 1 < 0) {
            return false;
        }
        else {
            return true;
        }
    }

	public void MoveTo(int x, int y) {
		GameObject placeToMove = squaresObj.GetSpace(x, y);
		transform.position = new Vector3(placeToMove.transform.position.x, gameObject.transform.position.y, placeToMove.transform.position.z);
		gridXPos = placeToMove.GetComponent<STile>().Pos[1];    // Tiles store row/y in [0] and col/x in [1]
		gridYPos = placeToMove.GetComponent<STile>().Pos[0];

		tSpawner.PreviewInfluence();
        current = placeToMove;
    }

    public void MoveToTemp(GameObject tile) {
        transform.position = new Vector3(tile.transform.position.x, gameObject.transform.position.y, tile.transform.position.z);
        tSpawner.PreviewInfluence();
    }
	#endregion

	#region Utility Methods
	// Unlike the helper methods, these methods are using specific points on the map

	// based on the row and column of the cursor, find the index of the grid square in the flat array
	private int getGridIndex(int row, int col) {
		return ((row * ROW_SQUARE_COUNT) + col);
	}
    #endregion
    #endregion

    private void Awake() {
        CenterOnSpace();

		gridXPos = initialPosOnGrid[0];
		gridYPos = initialPosOnGrid[1];

        current = squaresObj.GetSpace(gridXPos, gridYPos);

		// handle online player assignment
		if (!SettingsManager.Instance.GetIsOnline()) {
			rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerKey);
		}
		else {
			if (rewiredPlayerKey == OnlineManager.Instance.fakePlayerKey) {
				rewiredPlayer = ReInput.players.GetPlayer(PlayerIDs.player1);
			}
			else {
				rewiredPlayer = null;
			}
		}

#if !UNITY_XBOXONE
		mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
		gridLayerMask = 1 << LayerMask.NameToLayer("Tile");
#endif
	}

	private void Start() {
		tSpawner = GetComponent<TowerSpawner>();
		pController = tSpawner.GetPlayerController();
		pInput = tSpawner.playerObject.GetComponent<PlayerDirectionalInput>();

		gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update () {
		horzMove = pInput.GetHorizNonRadialInput(rewiredPlayer);
		vertMove = pInput.GetVertNonRadialInput(rewiredPlayer);

		// let you flick stick immediately again to move
		// only reset to slow movement if the stick is in dead zone for more than a certain amt of time
		if (Mathf.Abs(horzMove) < INPUT_THRESHOLD && Mathf.Abs(vertMove) < INPUT_THRESHOLD) {
			cursorMoveTimeCounter = float.MaxValue;

			deadZoneCounter += Time.deltaTime;
			if (deadZoneCounter > DEAD_ZONE_LEEWAY_TIME) {
				deadZoneCounter = 0;
				slowMoveCount = 0;
			}
		}

		if (canCursorMove() && pController.GetCanMoveReticule()) {
            // RIGHT
            if (horzMove > INPUT_THRESHOLD || rewiredPlayer.GetButtonDown(RewiredConsts.Action.DPadRight)) {
                if (CanMoveRight()) {
                    GameObject placeToMove = squaresObj.GetSpace(gridXPos + 1, gridYPos);
                    if (placeToMove.CompareTag("KeepTile") || placeToMove.activeSelf) {
                        transform.position = new Vector3(placeToMove.transform.position.x, gameObject.transform.position.y, placeToMove.transform.position.z);
                        gridXPos = placeToMove.GetComponent<STile>().Pos[1];    // Tiles store row/y in [0] and col/x in [1]
						gridYPos = placeToMove.GetComponent<STile>().Pos[0];

                        current = squaresObj.GetSpace(gridXPos, gridYPos);

                        tSpawner.PreviewInfluence();
						updateMovementValuesOnMove();
					}
				}
            }
			// LEFT
            else if (horzMove < -INPUT_THRESHOLD || rewiredPlayer.GetButtonDown(RewiredConsts.Action.DPadLeft)) {
                if (CanMoveLeft()) {
					GameObject placeToMove = squaresObj.GetSpace(gridXPos - 1, gridYPos);
                    if (placeToMove.CompareTag("KeepTile") || placeToMove.activeSelf) {
                        transform.position = new Vector3(placeToMove.transform.position.x, gameObject.transform.position.y, placeToMove.transform.position.z);
						gridXPos = placeToMove.GetComponent<STile>().Pos[1];    // Tiles store row/y in [0] and col/x in [1]
						gridYPos = placeToMove.GetComponent<STile>().Pos[0];

                        current = squaresObj.GetSpace(gridXPos, gridYPos);

                        tSpawner.PreviewInfluence();
						updateMovementValuesOnMove();
					}
				}
            }
            // DOWN
            else if (vertMove > INPUT_THRESHOLD || rewiredPlayer.GetButtonDown(RewiredConsts.Action.DPadDown)) {
                if (CanMoveDown()) {
                    GameObject placeToMove = squaresObj.GetSpace(gridXPos, gridYPos + 1);
                    if (placeToMove.CompareTag("KeepTile") || placeToMove.activeSelf) {
                        transform.position = new Vector3(placeToMove.transform.position.x, gameObject.transform.position.y, placeToMove.transform.position.z);
						gridXPos = placeToMove.GetComponent<STile>().Pos[1];    // Tiles store row/y in [0] and col/x in [1]
						gridYPos = placeToMove.GetComponent<STile>().Pos[0];

                        current = squaresObj.GetSpace(gridXPos, gridYPos);

                        tSpawner.PreviewInfluence();
						updateMovementValuesOnMove();
					}
				}
            }       
			// UP
            else if (vertMove < -INPUT_THRESHOLD || rewiredPlayer.GetButtonDown(RewiredConsts.Action.DPadUp)) {
                if (CanMoveUp()) {
                    GameObject placeToMove = squaresObj.GetSpace(gridXPos, gridYPos - 1);
                    if (placeToMove.CompareTag("KeepTile") || placeToMove.activeSelf) {
                        transform.position = new Vector3(placeToMove.transform.position.x, gameObject.transform.position.y, placeToMove.transform.position.z);
						gridXPos = placeToMove.GetComponent<STile>().Pos[1];	// Tiles store row/y in [0] and col/x in [1]
						gridYPos = placeToMove.GetComponent<STile>().Pos[0];

                        current = squaresObj.GetSpace(gridXPos, gridYPos);

						tSpawner.PreviewInfluence();
						updateMovementValuesOnMove();
					}
				}
            }
        }
    }

	private void OnEnable() {
		gridXPos = initialPosOnGrid[0];
		gridYPos = initialPosOnGrid[1];

		CenterOnSpace();
	}

    public Squares GetSquare() {
        return squaresObj;
    }

    public int GetX() {
        return gridXPos;
    }

    public int GetY() {
        return gridYPos;
    }


#if !UNITY_XBOXONE
	public void MoveCursorWithMouse(bool placingTower) {
		Ray cameraRay = mainCam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
		RaycastHit[] collisions = Physics.RaycastAll(cameraRay, Mathf.Infinity, gridLayerMask);

		if (collisions.Length == 1) {
			transform.position = new Vector3(collisions[0].collider.gameObject.transform.position.x,
				collisions[0].collider.gameObject.transform.position.y,
				collisions[0].collider.gameObject.transform.position.z);

            current = collisions[0].collider.gameObject;

            if (placingTower) {
				tSpawner.PreviewInfluence();
			}
		}
		else if (collisions.Length > 1) {
			Transform closest = collisions[0].collider.transform;

			float smallestDist = Vector3.Distance(mainCam.transform.position, closest.transform.position);

			foreach (RaycastHit hit in collisions) {
				float lastDist = Vector3.Distance(mainCam.transform.position, hit.collider.transform.position);
				if (lastDist < smallestDist) {
					closest = hit.collider.transform;
					smallestDist = lastDist;
				}
			}

            current = closest.gameObject;

			transform.position = new Vector3(closest.position.x, closest.position.y, closest.position.z);
			if (placingTower) {
				tSpawner.PreviewInfluence();
			}
		}
	}
#endif
}
