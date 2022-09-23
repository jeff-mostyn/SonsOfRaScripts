using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConquestPlayer_Enemy : ConquestPlayer
{
	#region Declarations
	public List<ConquestPlayer> Enemies;

	// knowledge
	int[] parents;
	Node target;
	int targetIndex;

	// decision-making
	// fortify
	private float minFortifyChance = .1f;
	private float maxFortifyChance = 0.25f;
	private int maxFortifyInfluence = 250;

	// fight buff
	private float minFightBuffChance = 0.1f;
	private float maxFightBuffChance = 0.33f;
	private int maxFightBuffInfluence = 150;
	#endregion

	void Start() {
		base.start();
	}

	#region Turns
	public override void StartTurn() {
		// play any kind of turn fx, change ui, etc.
		base.StartTurn();

		TakeTurn();
	}

	// might need to be a coroutine so everything happens in timed fashion
	public override void TakeTurn() {
		isTurn = true;
		turnHighlightRing.SetActive(true);

		StartCoroutine(PlayOutTurn());
	}

	IEnumerator PlayOutTurn() {
		// varied declarations
		float fightBuffChance = Mathf.Lerp(minFightBuffChance, maxFightBuffChance, influence / maxFightBuffInfluence);
		float fortifyChance = Mathf.Lerp(minFortifyChance, maxFortifyChance, influence / maxFortifyInfluence);

		// move camera to player's location, parent it up
		ConquestManager.Instance.CenterCameraOnPlayer(this);

		Debug.Log(gameObject.name + " turn");

		yield return new WaitUntil(delegate { return moving == false; });

		// figure out which node we can move to/attack
		foreach (GameObject g in ConquestManager.Instance.nodesList) {
			if (g != currentNode) {
				Node n = g.GetComponent<Node>();
				n.canBattle = false;
				n.canMove = false;
			}
		}
		MarkUsableNodes(currentNode, actionPoints);

		yield return new WaitForSeconds(3f);

		// choose to build a fortification
		float rand = Random.Range(0f, 1f);
		Debug.Log("fortification roll: " + rand + " fortification chance: " + fortifyChance);
		if (rand <= fortifyChance && !currentNode.fortified) {
			int fortificationIndex = SelectFortification();
			if (fortificationIndex != -1) {
				currentNode.FortifyNode(ConquestManager.Instance.fortifications[fortificationIndex], true);
				influence -= ConquestManager.Instance.fortificationCosts[fortificationIndex];
				influenceDisplay.text = influence.ToString();

				yield return new WaitForSeconds(2f);
			}
		}

		// TARGET SELECTION
		// determine best target node. options are: enemy home nodes (and bonus nodes later)
		GameObject targetNode = null;
		int targetNodeIndex = -1;
		if (BaseUnderThreat()) {	// The node outside of the AI's starting node has been taken by another player, so try to take it back
			targetNode = startingNodeObject.GetComponent<Node>().FindConnectedNodes()[0].gameObject;
			targetNodeIndex = ConquestManager.Instance.nodesList.IndexOf(targetNode);
		}
		else {  // BAU
			List<int> targets = dijkstra(ConquestManager.Instance.nodesList.IndexOf(startingNodeObject));
			if (targets.Count > 1 && targets[1] == ConquestManager.Instance.nodesList.IndexOf(Enemies[0].startingNodeObject)) {
				targetNodeIndex = targets[Random.Range(0, 2)];
			}
			else {
				targetNodeIndex = targets[0];
			}
		}

		targetNodeIndex = FindMovableNodeOnPath(targetNodeIndex);
		targetNode = ConquestManager.Instance.nodesList[FindMovableNodeOnPath(targetNodeIndex)];

		// have opponent slide back if need be
		if (currentNode.owner != "" && currentNode.owner != gameObject.name 
			&&  GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().currentNodeObject == currentNodeObject) {
			GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>().MoveToCenterOfNode(currentNode.transform.position);
		}

		// spend action points
		if (currentNode.isAdjacent(targetNode)) {
			actionPoints -= 1;
		}
		else {
			actionPoints -= 2;
		}

		// move to chosen node
		StartCoroutine(TestMove(targetNode.transform));

		yield return new WaitUntil(delegate { return moving == false; });

		currentNode = targetNode.GetComponent<Node>();
		currentNodeObject = targetNode;

		RemarkNodes();

		if (currentNode.canBattle) {
			Debug.Log("attempt battle");
			// engage in combat
			actionPoints -= 1;

			if (currentNode.fortified) {
				rand = Random.Range(0f, 1f);
			}

			if (currentNode.owner == ConquestManager.Instance.playerName) {
				ConquestManager.Instance.players[0].GetComponent<ConquestPlayer_Human>().currentNodeObject = targetNode;
				ConquestManager.Instance.players[0].GetComponent<ConquestPlayer_Human>().SetLoadouts(this);
				ConquestManager.Instance.SavePlayers();
				ConquestManager.Instance.combatNodeName = currentNode.name;
				ConquestManager.Instance.nodeOwned = currentNode.owner != "";
				ConquestManager.Instance.players[0].GetComponent<ConquestPlayer_Human>().SetEncounterQuipSource(patron);
				SetModifiers();
				if (rand <= fightBuffChance) {
					int fightBuffIndex = SelectFightBuff();
					if (fightBuffIndex != -1) {
						// add fight buff to list of modifiers
						ModifierManager.Instance.SetFightBuff(ConquestManager.Instance.fightBuffs[fightBuffIndex]);
						influence -= ConquestManager.Instance.fightBuffCosts[fightBuffIndex];
						influenceDisplay.text = influence.ToString();
					}
				}
				ModifierManager.Instance.playerOwnedNode = true;

				// Do we want to use a fight buff?

				ConquestManager.Instance.SavePlayers();

				scm.setNextSceneName(Constants.sceneNames[currentNode.nodeDetails.map]);
				scm.LoadNextScene(true);
			}
			else {
				yield return new WaitForSeconds(1f);

				if (rand <= fightBuffChance) {
					int fightBuffIndex = SelectFightBuff();
					if (fightBuffIndex != -1) {
						// add fight buff to list of modifiers
						ModifierManager.Instance.SetFightBuff(ConquestManager.Instance.fightBuffs[fightBuffIndex]);
						influence -= ConquestManager.Instance.fightBuffCosts[fightBuffIndex];
						influenceDisplay.text = influence.ToString();
					}
				}

				float enemyEfficiency = 0;
				if (currentNode.owner != "") {
					ConquestPlayer opponent = GameObject.Find(currentNode.owner).GetComponent<ConquestPlayer>();
					enemyEfficiency = opponent.combatEfficiency 
						+ (currentNode.fortified ? ConquestManager.Instance.fortifcationAutoResolveModifier : 0)
						+ (currentNode.hunkeredDown ? ConquestManager.Instance.hunkerDownAutoResolveModifier : 0);
				}
				else {
					enemyEfficiency = 0.0625f;
				}
				combatEfficiency = combatEfficiency + (ModifierManager.Instance.fightBuffApplied() ? ConquestManager.Instance.fightBuffAutoResolveModifier : 0);
				float efficiencySum = combatEfficiency + enemyEfficiency;
				float efficiencyPercent = combatEfficiency / efficiencySum;
				rand = Random.Range(0f, 1f);

				Debug.Log(rand + " : " + efficiencyPercent);

				if (rand < efficiencyPercent) {	// if win
					Victorious();
				}
				else { // if lose
					Defeat();
				}

				RemarkNodes();

				yield return new WaitForSeconds(3f);

				EndTurn();
			}
		}
		else {
			yield return new WaitForSeconds(1f);
			EndTurn();
		}
	}

	public override void FinishTurn() {
		ResetAfterCombat();

		StartCoroutine(PlayOutFinishTurn());
	}

	IEnumerator PlayOutFinishTurn() {
		ResetAfterCombat();

		if (!ConquestManager.Instance.playerVictory) {
			Victorious();
		}
		else {
			Defeat();
		}

		yield return new WaitForSeconds(3f);

		EndTurn();
	}

	public override void EndTurn() {
		base.EndTurn();
	}
	#endregion

	#region Battle
	public override void Victorious() {
		combatEfficiency += 0.05f;

		MoveToCenterOfNode(currentNode.transform.position);

		if (currentNode.owner != gameObject.name && currentNode.fortified) {
			currentNode.UnfortifyNode();
		}

		string enemyName = currentNode.owner;
		currentNode.owner = gameObject.name;
		currentNode.GetComponent<Node>().FillPaths(playerColor, gameObject.name);
		currentNode.GetComponent<Node>().SpawnNodeParticle(this);

		if (enemyName != "") {
			GameObject.Find(enemyName).GetComponent<ConquestPlayer>().Defeat();
		}

		ConquestManager.Instance.CheckDefeat(enemyName);

		if (currentNode.GetComponent<Node>().nodeDetails.quipType == ConquestManager.enemyQuipSources.penultimate && enemyName != "") {
			currentNode.GetComponent<Node>().nodeDetails.quipType = ConquestManager.enemyQuipSources.unspecified;
		}


		MoveToCenterOfNode(currentNode.transform.position);

		//currentNode.UnlockNodes();

		ConquestManager.Instance.CheckPlayerDefeat();
	}
	#endregion

	public override void SetKeepAesthetics() {
		SetHeadPosition();
		SetCursor();

		// set up keep colors
		Material keepMat = keep.GetComponent<MeshRenderer>().material;
		keepMat.SetColor("_PalCol1", palette.GetColorPalette()[0]);
		keepMat.SetColor("_PalCol2", palette.GetColorPalette()[1]);

		Material headMat = keepHead.GetComponent<MeshRenderer>().material;
		headMat.SetColor("_PalCol1", palette.GetColorPalette()[0]);
		headMat.SetColor("_PalCol2", palette.GetColorPalette()[1]);

		Material cursorMat = cursor.GetComponent<MeshRenderer>().material;
		cursorMat.SetColor("_PalCol1", palette.GetColorPalette()[0]);
		cursorMat.SetColor("_PalCol2", palette.GetColorPalette()[1]);
	}

	#region Logic
	// Bastardized Dijkstra's algorithm 
	private List<int> dijkstra(int startVertex) {
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
		parents = new int[nVertices];

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
					float edgeDistance = currentNode.owner == gameObject.name ? 0f : ConquestManager.Instance.nodesList[nearestVertex].GetComponent<Node>().getValue();

					if ((shortestDistance + edgeDistance) < shortestDistances[vertexIndex]) {
						parents[vertexIndex] = nearestVertex;
						shortestDistances[vertexIndex] = shortestDistance + edgeDistance;
					}
				}
			}
		}

		return findTargets(startVertex, shortestDistances, parents);
	}

	private List<int> findTargets(int startVertex, float[] distances, int[] parents) {
		List<int> targets = new List<int>();
		int nVertices = distances.Length;
		for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++) {
			GameObject currentNode = ConquestManager.Instance.nodesList[vertexIndex];
			if (vertexIndex != startVertex 
				&& ((currentNode == Enemies[0].startingNodeObject && !ConquestManager.Instance.defeated[Enemies[0].gameObject.name]) 
					|| (currentNode == Enemies[1].startingNodeObject && !ConquestManager.Instance.defeated[Enemies[1].gameObject.name])
					|| (currentNode == Enemies[2].startingNodeObject && !ConquestManager.Instance.defeated[Enemies[2].gameObject.name]))) {
				if (targets.Count == 0 || distances[targets[0]] < distances[vertexIndex]) {
					targets.Add(vertexIndex);
				}
				else {
					targets.Insert(0, vertexIndex);
				}
				//Debug.Log(startVertex + " -> " + vertexIndex + "  " + distances[vertexIndex] + " " + printPath(vertexIndex, parents));
			}
		}

		return targets;
	}

	private string printPath(int currentVertex, int[] parents) {

		// Base case : Source node has  
		// been processed  
		if (currentVertex == -1) {
			return "";
		}
		return printPath(parents[currentVertex], parents) + " " + currentVertex;
	}

	private int FindMovableNodeOnPath(int target) {
		int index = target;

		while (!ConquestManager.Instance.nodesList[index].GetComponent<Node>().canBattle && !ConquestManager.Instance.nodesList[index].GetComponent<Node>().canMove) {
			index = parents[index];
		}

		return index;
	}

	private bool BaseUnderThreat() {
		return startingNodeObject.GetComponent<Node>().FindConnectedNodes().FindAll(n => n.owner != gameObject.name && n.owner != "").Count != 0;
	}

	/// <summary>
	/// Choose a fortification to build
	/// </summary>
	/// <returns>The integer index in the Conquest Manager's list of fortifications of the chosen fortification</returns>
	private int SelectFortification() {
		List<ModifierManager.modifiers> purchaseableFortifications = new List<ModifierManager.modifiers>(ConquestManager.Instance.fortifications);

		for (int i=purchaseableFortifications.Count - 1; i >= 0; i--) {
			if (influence < ConquestManager.Instance.fortificationCosts[i]) {
				purchaseableFortifications.RemoveAt(i);
			}
		}

		return purchaseableFortifications.Count > 0 ? ConquestManager.Instance.fortifications.IndexOf(purchaseableFortifications[Random.Range(0, purchaseableFortifications.Count)]) : -1;
	}

	/// <summary>
	/// Choose a fight buff to use
	/// </summary>
	/// <returns>The integer index in the Conquest Manager's list of fight buffs of the chosen buff</returns>
	private int SelectFightBuff() {
		List<ModifierManager.modifiers> purchaseableFightBuffs = new List<ModifierManager.modifiers>(ConquestManager.Instance.fightBuffs);

		for (int i = purchaseableFightBuffs.Count - 1; i >= 0; i--) {
			if (influence < ConquestManager.Instance.fortificationCosts[i]) {
				purchaseableFightBuffs.RemoveAt(i);
			}
		}

		return purchaseableFightBuffs.Count > 0 ? ConquestManager.Instance.fortifications.IndexOf(purchaseableFightBuffs[Random.Range(0, purchaseableFightBuffs.Count)]) : -1;
	}
	#endregion
}
