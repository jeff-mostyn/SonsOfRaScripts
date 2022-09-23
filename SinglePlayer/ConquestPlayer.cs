using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class ConquestPlayer : MonoBehaviour
{
	public GameObject startingNodeObject;
    public GameObject currentNodeObject;
	public GameObject keep, headLocation;
	protected GameObject keepHead;
	public GameObject cursor;
    public Color playerColor;
	public PlayerColorPalette palette;
    public bool firstLoad;

	public Patron patron;
	public string patronName;
	public int influence;

	[Header("Camera")]
	public Camera gameCamera;
	public Transform mapCamera;
	public bool lookingAtMap = true;
	protected bool cameraMoving = false;

	[Header("Movement")]
	public float moveTime;
    public float pickupTime;
    public float pickupHeight;
	public float speed = 0.5f;
    public bool moving = false;
    [SerializeField] protected float moveSensitivity = 0.8f;
    protected float vertMove = 0f;
    protected float horzMove = 0f;
	protected PlayerDirectionalInput pInput;
	protected LayerMask nodeLayerMask;

	[Header("General Gameplay")]
	public bool isTurn;
	public float baseValue = 0.5f;
	public float combatEfficiency = 0.5f;
	public int actionPoints;

	[Header("General UI")]
	public Image godFaceImage;
	public GameObject playerImageObj;
	public GameObject turnHighlightRing;
	public TextMeshProUGUI influenceDisplay;

    protected Node currentNode;
    protected Transform nextNode;
    protected LoadoutManager lm;
    protected SceneChangeManager scm;

	protected AudioSource src;


	protected void start() {
		src = GetComponent<AudioSource>();
		scm = SceneChangeManager.Instance;
		lm = LoadoutManager.Instance;

		if (influenceDisplay.text.Contains("#")) {
			influenceDisplay.SetText(influence.ToString());
		}
	}

	public void SavePlayer()
    {
        SaveSystem.SaveConquestPlayer(this);
		ConquestManager.Instance.SaveNodes();
    }

    public void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadConquestPlayer(this);

        if(data != null)
        {
            Vector3 position;
            position.x = data.position[0];
            position.y = data.position[1];
            position.z = data.position[2];
			combatEfficiency = data.combatEfficiency;
            transform.position = position;
			patronName = data.patronName;

			influence = data.influence;

            currentNodeObject = ClosestNode();
            currentNode = currentNodeObject.GetComponent<Node>();

            firstLoad = data.firstLoad;
        }
		else {
			influence = 0;
			combatEfficiency = 0.5f;
		}

		influenceDisplay.SetText(influence.ToString());
	}

    public GameObject ClosestNode() // really need to figure out a way to serialize a reference to a GO
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (GameObject potentialTarget in ConquestManager.Instance.nodesList)
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget;
    }

    // General function for moving player around and setting new variables
    protected virtual void UpdateNode(GameObject newNode, GameObject oldNode)
    {
		// have opponent slide back if need be
		if (currentNode.owner != "" && currentNode.owner != gameObject.name
			&& GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().currentNodeObject == currentNodeObject) {
			GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().MoveToCenterOfNode(currentNode.transform.position);
		}

		// reset node marking
		if (newNode.GetComponent<Node>().isAdjacent(oldNode)) {
			actionPoints--;
		}
		else {
			actionPoints = 0;
		}

		// Setting new nodes to player
		currentNodeObject = newNode;
        currentNode = currentNodeObject.GetComponent<Node>();

		RemarkNodes();

        // Coroutine to move the gameObject to new position
        StartCoroutine(TestMove(newNode.transform));
        moving = true;
    }

	public void SetModifiers() {
		ModifierManager.Instance.ResetMods();
		ModifierManager.Instance.SetMods(currentNode.nodeDetails.modifiers);
		if (currentNode.fortified) {
			ModifierManager.Instance.SetFortification(currentNode.fortification);
		}
		if (currentNode.hunkeredDown) {
			ModifierManager.Instance.SetHunkerDown();
		}
	}

	protected void MarkUsableNodes(Node n, int pointsRemaining, Node parent = null) {
		if (pointsRemaining == 0) {
			return;
		}

		Node examinedNode = null;
		List<GameObject> connectedNodes = new List<GameObject>();
		if (n.upNode) connectedNodes.Add(n.upNode);
		if (n.bottomNode) connectedNodes.Add(n.bottomNode);
		if (n.leftNode) connectedNodes.Add(n.leftNode);
		if (n.rightNode) connectedNodes.Add(n.rightNode);

		for (int i = 0; i < connectedNodes.Count; i++) {
			if (connectedNodes[i] && connectedNodes[i].GetComponent<Node>() != parent) {
				examinedNode = connectedNodes[i].GetComponent<Node>();
				if (examinedNode.owner == gameObject.name) {
					examinedNode.MarkUsable(false);
					MarkUsableNodes(examinedNode, pointsRemaining - 1, n);
				}
				else if (pointsRemaining == 2) {
					examinedNode.MarkUsable(true);
				}
			}
		}
	}

	public float getValue() {
		return baseValue + combatEfficiency;
	}

	#region Movement Coroutines
	public void MoveToSideOnOccupiedNode(bool movingToNode) {
		StartCoroutine(SlideToSide(movingToNode ? -1 : 1));
	}

	public void MoveToCenterOfNode(Vector3 nodePosition) {
		StartCoroutine(SlideBackToCenter(nodePosition));
	}

	protected void BounceBack() {
		(float[] shortestDistances, int[] parents) = returnDijkstra(ConquestManager.Instance.nodesList.IndexOf(currentNode.gameObject));
		GameObject nodeToMoveTo = ConquestManager.Instance.nodesList[findBounceBackNode(parents, ConquestManager.Instance.nodesList.IndexOf(startingNodeObject), ConquestManager.Instance.nodesList.IndexOf(currentNode.gameObject))];
		StartCoroutine(TestMove(nodeToMoveTo.transform));
		currentNode = nodeToMoveTo.GetComponent<Node>();
		currentNodeObject = nodeToMoveTo;
	}

	protected IEnumerator TestMove(Transform newNode)
    {
        if (moving) yield break;

        moving = true;
        var startPos = transform.position;
        var timer = 0.0f;
		Node newNodeScript = newNode.gameObject.GetComponent<Node>();

		if (newNodeScript.owner != "" && newNodeScript.owner != gameObject.name
			&& GameObject.Find(newNodeScript.owner).GetComponent<ConquestPlayer>().currentNodeObject == newNode.gameObject) {
			GameObject.Find(newNodeScript.owner).GetComponent<ConquestPlayer>().MoveToSideOnOccupiedNode(false);
		}

		while (timer <= 1.0f)
        {
            var height = Mathf.Sin(Mathf.PI * timer) * pickupHeight;
            transform.position = Vector3.Lerp(startPos, new Vector3(newNode.position.x,
                                                                    newNode.position.y + 1,
                                                                    newNode.position.z),
                                                                    Mathf.SmoothStep(0.0f, 1.0f, timer)) + Vector3.up * height;

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(newNode.position.x,
                                         gameObject.transform.position.y,
                                         newNode.position.z);

        moving = false;

		//if (actionPoints == 0 && gameObject.name == "ConquestPlayer") {
		//	yield return new WaitForSeconds(1f);
		//	EndTurn();
		//}
	}

	protected IEnumerator SlideToSide(int direction) {
		moving = true;

		var startPos = transform.position;
		var timer = 0.0f;

		while (timer <= 0.35f) {
			transform.position = Vector3.Lerp(startPos, new Vector3(startPos.x + direction * 1.5f,
																	startPos.y,
																	startPos.z + direction * 1.5f),
																	Mathf.SmoothStep(0.0f, 1.0f, timer/0.35f));

			timer += Time.deltaTime;
			yield return null;
		}
		moving = false;
	}

	protected IEnumerator SlideBackToCenter(Vector3 nodePosition) {
		moving = true;

		var startPos = transform.position;
		var timer = 0.0f;

		while (timer <= 0.35f) {
			transform.position = Vector3.Lerp(startPos, new Vector3(nodePosition.x, startPos.y, nodePosition.z), Mathf.SmoothStep(0.0f, 1.0f, timer / 0.35f));

			timer += Time.deltaTime;
			yield return null;
		}
		moving = false;
	}
	#endregion

	#region FX
	public void sound_nodeCapture() {
		SoundManager.Instance.conquestNodeCapture(src);
	}

	IEnumerator TickUpInfluence(int influenceToAdd) {
		influenceDisplay.SetText(influence.ToString());
		int startingInfluence = influence;
		influence += influenceToAdd;
		
		float animationTime = 0.5f;

		int diff = influence - startingInfluence;
		int TickCount = diff / 10;

		float timePerTick = animationTime / TickCount;

		for (int i = 0; i<TickCount; i++) {
			startingInfluence += 10;
			influenceDisplay.SetText(startingInfluence.ToString());

			yield return new WaitForSeconds(timePerTick);
		}

		influenceDisplay.SetText(influence.ToString());
	}
	#endregion

	public abstract void SetKeepAesthetics();

	protected void SetHeadPosition() {
		keepHead = Instantiate(patron.keepHead, headLocation.transform);
		keepHead.transform.localPosition = Vector3.zero;
		keepHead.transform.localRotation = Quaternion.identity;
		GetComponentInChildren<MeshRenderer>().material.color = playerColor;
	}

	protected void SetCursor() {
		cursor.GetComponent<MeshFilter>().mesh = patron.conquestPiece.GetComponent<MeshFilter>().sharedMesh;
		cursor.GetComponent<MeshRenderer>().material = Instantiate(patron.conquestPiece.GetComponent<MeshRenderer>().sharedMaterial);

		cursor.transform.rotation = keepHead.transform.rotation;
	}

	protected bool CanFortify() {
		return !currentNode.fortified && currentNode.owner == gameObject.name;
	}

	protected void RemarkNodes() {
		foreach (GameObject g in ConquestManager.Instance.nodesList) {
			if (g != currentNodeObject) {
				Node n = g.GetComponent<Node>();
				n.canBattle = false;
				n.canMove = false;
			}
		}
		MarkUsableNodes(currentNode, actionPoints);
		foreach (GameObject node in ConquestManager.Instance.nodesList) {
			if (!node.GetComponent<Node>().canMove || actionPoints == 0) {
				Destroy(node.GetComponent<Node>().activePulseEffect);
			}
		}
	}

	#region Turns
	/// <summary>
	/// Core turn-management function
	/// </summary>
	public virtual void StartTurn() {
		actionPoints = 2;

		int influenceToAdd = 0;
		foreach (GameObject g in ConquestManager.Instance.nodesList) {
			if (g.GetComponent<Node>().owner == gameObject.name) {
				if (g.GetComponent<Node>().nodeDetails.bonusNode) {
					influenceToAdd += 30;
				}
				else {
					influenceToAdd += 10;
				}
			}
		}

		StartCoroutine(TickUpInfluence(influenceToAdd));

		if (currentNodeObject && !currentNode) {
			currentNode = currentNodeObject.GetComponent<Node>();
		}

		if (currentNode.hunkeredDown) {
			currentNode.hunkeredDown = false;
		}
	}

	/// <summary>
	/// Core turn-management function
	/// </summary>
	public abstract void TakeTurn();

	/// <summary>
	/// Grab player's current node from conquest manager
	/// </summary>
	protected void ResetAfterCombat() {
		if (ConquestManager.Instance.combatNodeName != "") {
			currentNodeObject = GameObject.Find(ConquestManager.Instance.combatNodeName);
		}
		currentNode = currentNodeObject.GetComponent<Node>();
	}

	/// <summary>
	/// Handle the post-conflict aspects of turn if turn involved attacking a player
	/// </summary>
	public abstract void FinishTurn();

	/// <summary>
	/// Tie up any loose ends, mark turn as done, play post-turn FX, etc.
	/// </summary>
	public virtual void EndTurn() {
		if (actionPoints > 0) {
			currentNode.hunkeredDown = true;
		}

		isTurn = false;
		turnHighlightRing.SetActive(false);
		actionPoints = 0;

		foreach (GameObject g in ConquestManager.Instance.nodesList) {
			g.GetComponent<Node>().RemoveUsablePulseEffect();
		}

		ConquestManager.Instance.NextTurn();
	}
	#endregion

	#region Battle
	public abstract void Victorious();

	public void Defeat() {
		combatEfficiency -= 0.05f;

		// have opponent slide back if need be
		if (currentNode.owner != "" && currentNode.owner != gameObject.name
			&& GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().currentNodeObject == currentNodeObject) {
			GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().MoveToCenterOfNode(currentNode.transform.position);
		}

		if (currentNode.owner != gameObject.name) {
			if (currentNode.owner != "") {
				currentNode.SpawnNodeParticle(GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>());
			}
			else {
				currentNode.SpawnNodeParticle();
			}

			BounceBack();
		}
	}
	#endregion

	#region Logic
	protected (float[], int[]) returnDijkstra(int startVertex) {
		int nVertices = ConquestManager.Instance.nodesList.Count;

		// shortestDistances[i] will hold the shortest distance from src to i  
		float[] shortestDistances = new float[nVertices];

		// added[i] will true if vertex i is included / in shortest path tree  
		// or shortest distance from src to i is finalized  
		bool[] added = new bool[nVertices];

		// Initialize all distances as INFINITE and added[] as false  
		for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++) {
			shortestDistances[vertexIndex] = float.MaxValue;
			added[vertexIndex] = false;
		}

		// Distance of source vertex from itself is always 0  
		shortestDistances[startVertex] = 0;

		// Parent array to store shortest path tree  
		int[] parents = new int[nVertices];

		// The starting vertex does not have a parent  
		parents[startVertex] = -1;

		// Find shortest path for all vertices  
		for (int i = 0; i < nVertices - 1; i++) {

			// Pick the minimum distance vertex from the set of vertices not yet  
			// processed. nearestVertex is always equal to startNode in first iteration.  
			int nearestVertex = -1;
			float shortestDistance = float.MaxValue;
			for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++) {
				if (!added[vertexIndex] && shortestDistances[vertexIndex] < shortestDistance) {
					nearestVertex = vertexIndex;
					shortestDistance = shortestDistances[vertexIndex];
				}
			}

			// Mark the picked vertex as processed  
			added[nearestVertex] = true;

			// Update dist value of the adjacent vertices of the picked vertex.  
			for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++) {
				// only evaulate if node at vertexIndex is adjacent to the current nearestNode
				if (ConquestManager.Instance.nodesList[nearestVertex].GetComponent<Node>().isAdjacent(ConquestManager.Instance.nodesList[vertexIndex])) {
					Node currentNode = ConquestManager.Instance.nodesList[nearestVertex].GetComponent<Node>();
					float edgeDistance = currentNode.owner == gameObject.name ? 0f : 50f;

					if ((shortestDistance + edgeDistance) < shortestDistances[vertexIndex]) {
						parents[vertexIndex] = nearestVertex;
						shortestDistances[vertexIndex] = shortestDistance + edgeDistance;
					}
				}
			}
		}

		return (shortestDistances, parents);
	}

	protected int findBounceBackNode(int[] parents, int keepNodeIndex, int currentNodeIndex) {
		int dist = 0;
		int index = keepNodeIndex;

		// get number of nodes from curren node to keep
		while (index != currentNodeIndex) {
			index = parents[index];
			dist += 1;
		}

		Debug.Log("Distance from home: " + dist);

		// if we are within 2 nodes of own keep, return there
		if (dist <= 2) {
			return keepNodeIndex;
		}

		// check if you own the node two from your current node
		// if yes, go there
		// if no, check if you own the node one from your current node
		// if yes, go there
		// if no, go in reverse along path until reaching a node you own
		int twoAway = findNthNodeInPath(parents, keepNodeIndex, dist - 2);
		if (ConquestManager.Instance.nodesList[twoAway].GetComponent<Node>().owner == gameObject.name) {
			return twoAway;
		}
		else {
			int oneAway = findNthNodeInPath(parents, keepNodeIndex, dist - 1);
			if (ConquestManager.Instance.nodesList[oneAway].GetComponent<Node>().owner == gameObject.name) {
				return oneAway;
			}
			else {
				int nodeInPath;
				for (int i = 3; i <= dist; i++) {
					nodeInPath = findNthNodeInPath(parents, keepNodeIndex, dist - i);
					if (ConquestManager.Instance.nodesList[nodeInPath].GetComponent<Node>().owner == gameObject.name) {
						return nodeInPath;
					}
				}
			}
		}

		return keepNodeIndex;
	}

	private int findNthNodeInPath(int[] parents, int startingNodeIndex, int n) {
		int index = startingNodeIndex;

		Debug.Log("Starting index: " + startingNodeIndex);

		for (int i = 0; i<n; i++) {
			index = parents[index];
		}

		Debug.Log("index: " + index);

		return index;
	}
	#endregion
}
