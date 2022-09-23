using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
	public enum difficulty { Easy, Medium, Hard }
	public enum nodeDirection { left, right, up, bottom, none };
	public float baseValue = 1f;
    // Whether node is unlocked or not
    public bool canBattle, canMove;
	public bool blocked;
	public bool fortified;
	public bool hunkeredDown;
	public ModifierManager.modifiers fortification;
	public string owner = "";
	private float delayToEnemyPath = 2.6f;

	[Header("References")]
    public GameObject victoryParticle, availabilityParticle, activePulseEffect;
	public GameObject fortificationMarkerPrefab;
	private GameObject fortificationMarker;

    // Up to four connecting nodes
    [Header ("Connecting Nodes")]
    public GameObject upNode;
    public GameObject rightNode;
    public GameObject bottomNode;
    public GameObject leftNode;

	[Header("Connecting Paths")]
	public GameObject upPath;
	public GameObject rightPath;
	public GameObject bottomPath;
	public GameObject leftPath;

	[Header ("Them node deets")]
    public NodeDetails nodeDetails;

    // Info to display when selecting node
    [System.Serializable]
    public struct NodeDetails
    {
        public string name;
        public Constants.gameScenes map;
        public difficulty mode;
		public ConquestManager.enemyQuipSources quipType;
        public List<ModifierManager.modifiers> modifiers;
		public bool bonusNode;
		public ModifierManager.modifiers bonusMod;
        [TextArea(5,10)]
        public string description;
    }

    private void Awake()
    {
        ConquestManager.Instance.nodesList.Add(this.gameObject);
    }

    public void UnlockNodes() {
		canBattle = false;

		if (owner == ConquestManager.Instance.playerName) {
			if (upNode != null) {
				if (upNode.GetComponent<Node>().owner != owner && !upNode.GetComponent<Node>().blocked) {
					upNode.GetComponent<Node>().canBattle = true;
				}
				else {
					upNode.GetComponent<Node>().canBattle = false;
				}
			}

			if (rightNode != null) {
				if (rightNode.GetComponent<Node>().owner != owner && !rightNode.GetComponent<Node>().blocked) {
					rightNode.GetComponent<Node>().canBattle = true;
				}
				else {
					rightNode.GetComponent<Node>().canBattle = false;
				}
			}

			if (bottomNode != null) {
				if (bottomNode.GetComponent<Node>().owner != owner && !bottomNode.GetComponent<Node>().blocked) {
					bottomNode.GetComponent<Node>().canBattle = true;
				}
				else {
					bottomNode.GetComponent<Node>().canBattle = false;
				}
			}

			if (leftNode != null) {
				if (leftNode.GetComponent<Node>().owner != owner && !leftNode.GetComponent<Node>().blocked) {
					leftNode.GetComponent<Node>().canBattle = true;
				}
				else {
					leftNode.GetComponent<Node>().canBattle = false;
				}
			}
		}
		else {
			if ((upNode != null && upNode.GetComponent<Node>().owner == ConquestManager.Instance.playerName
				|| rightNode != null && rightNode.GetComponent<Node>().owner == ConquestManager.Instance.playerName
				|| bottomNode != null && bottomNode.GetComponent<Node>().owner == ConquestManager.Instance.playerName
				|| leftNode != null && leftNode.GetComponent<Node>().owner == ConquestManager.Instance.playerName)
				 && !blocked) {
					canBattle = true;
			}
		}
    }

	public void MarkUsable(bool attack) {
		Color pulseColor;

		canMove = true;
		if (attack) {
			canBattle = true;
			pulseColor = Color.red;
		}
		else {
			canBattle = false;
			pulseColor = Color.white;
		}

		ParticleSystem.MainModule main = availabilityParticle.GetComponent<ParticleSystem>().main;
		main.startColor = pulseColor;
		foreach (ParticleSystem p in availabilityParticle.GetComponentsInChildren<ParticleSystem>()) {
			main = p.main;
			main.startColor = pulseColor;
		}

		if (activePulseEffect) Destroy(activePulseEffect);
		activePulseEffect = Instantiate(availabilityParticle, transform);
		activePulseEffect.transform.localPosition = Vector3.zero;
		activePulseEffect.GetComponent<ParticleSystem>().Play();
		foreach (ParticleSystem p in activePulseEffect.GetComponentsInChildren<ParticleSystem>()) {
			p.Play();
		}
	}

	public void RemoveUsablePulseEffect() {
		if (activePulseEffect) Destroy(activePulseEffect);
	}

	public float getValue() {

		float ownerValueModifier = owner == "" ? 0.25f : GameObject.Find(owner).GetComponent<ConquestPlayer>().combatEfficiency;

		return baseValue + ownerValueModifier + nodeDetails.modifiers.Count * ConquestManager.Instance.modifierValue;
	}

	public bool isAdjacent(GameObject node) {
		if ((leftNode != null && leftNode == node)
			|| (rightNode != null && rightNode == node)
			|| (upNode != null && upNode == node)
			|| (bottomNode != null && bottomNode == node)) {
			return true;
		}
		return false;
	}

	public List<Node> FindConnectedNodes() {
		List<Node> connections = new List<Node>();
		
		if (rightNode) {
			connections.Add(rightNode.GetComponent<Node>());
		}
		if (leftNode) {
			connections.Add(leftNode.GetComponent<Node>());
		}
		if (upNode) {
			connections.Add(upNode.GetComponent<Node>());
		}
		if (bottomNode) {
			connections.Add(bottomNode.GetComponent<Node>());
		}

		return connections;
	}

	public void FortifyNode(ModifierManager.modifiers type, bool drop) {
		if (!fortified) {
			fortified = true;

			fortification = type;

			fortificationMarker = Instantiate(fortificationMarkerPrefab, transform);
			fortificationMarker.transform.localPosition = Vector3.zero;
			fortificationMarker.GetComponent<MeshRenderer>().material.SetColor("_Color", GameObject.Find(owner).GetComponent<ConquestPlayer>().playerColor);
			if (drop) {
				//fortificationMarker.GetComponent<ModelDrop>().ObjectDrop();
			}
		}
	}

	public void UnfortifyNode() {
		fortified = false;

		// get rid of fortification fx
		if (fortificationMarker) {
			Destroy(fortificationMarker);
			// do some kind of fx or something
		}
	}

	#region FX
	public void SpawnNodeParticle(ConquestPlayer player) //spawn victory particle effect
    {
		ParticleSystem.MainModule main = victoryParticle.GetComponent<ParticleSystem>().main;
		main.startColor = player.playerColor;
		foreach (ParticleSystem p in victoryParticle.GetComponentsInChildren<ParticleSystem>()) {
			main = p.main;
			main.startColor = player.playerColor;
		}
        StartCoroutine(WaitBeforeSpawn(victoryParticle, transform.position, player, ConquestManager.Instance.fillTime * 2.1f)); //2.1 for correct timing
    }

	public void SpawnNodeParticle() //spawn victory particle effect
	{
		ParticleSystem.MainModule main = victoryParticle.GetComponent<ParticleSystem>().main;
		main.startColor = Color.white;
		foreach (ParticleSystem p in victoryParticle.GetComponentsInChildren<ParticleSystem>()) {
			main = p.main;
			main.startColor = Color.white;
		}
		Instantiate(victoryParticle, transform.position, victoryParticle.transform.rotation);
	}

	IEnumerator WaitBeforeSpawn(GameObject myObj, Vector3 position, ConquestPlayer player, float waitTime = 0) //spawn object (mostly just the particle effect)
    {
        yield return new WaitForSeconds(waitTime);

        Instantiate(myObj, position, myObj.transform.rotation);
		player.sound_nodeCapture();
	}
	#endregion

	#region Path Filling
	public void ColorPath(Color playerColor, string playerName, nodeDirection sourceDir) {
		GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
		GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);

		if (leftNode && leftPath && leftNode.GetComponent<Node>().owner == playerName && sourceDir != nodeDirection.left) {
			leftPath.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
			leftPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			if(leftNode.GetComponent<MeshRenderer>().material.GetColor("_ForeColor") != playerColor)
			{
				leftNode.GetComponent<Node>().ColorPath(playerColor, playerName, nodeDirection.right);
			}			
		}
		if (rightNode && rightPath && rightNode.GetComponent<Node>().owner == playerName && sourceDir != nodeDirection.right) {
			rightPath.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
			rightPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			if (rightNode.GetComponent<MeshRenderer>().material.GetColor("_ForeColor") != playerColor)
			{
				rightNode.GetComponent<Node>().ColorPath(playerColor, playerName, nodeDirection.left);
			}
		}
		if (upNode && upPath && upNode.GetComponent<Node>().owner == playerName && sourceDir != nodeDirection.up) {
			upPath.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
			upPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			if (upNode.GetComponent<MeshRenderer>().material.GetColor("_ForeColor") != playerColor)
			{
				upNode.GetComponent<Node>().ColorPath(playerColor, playerName, nodeDirection.bottom);
			}
		}
		if (bottomNode && bottomPath && bottomNode.GetComponent<Node>().owner == playerName && sourceDir != nodeDirection.bottom) {
			bottomPath.GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 1f);
			bottomPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			if (bottomNode.GetComponent<MeshRenderer>().material.GetColor("_ForeColor") != playerColor)
			{
				bottomNode.GetComponent<Node>().ColorPath(playerColor, playerName, nodeDirection.up);
			}
		}
	}

	public void FillPaths(Color playerColor, string playerName) {
		if (leftNode && leftPath && leftNode.GetComponent<Node>().owner == playerName) { // fill path to conquered node
			leftPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 0f);
			leftPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			StartCoroutine(fillPath(leftPath.GetComponent<MeshRenderer>().material, false));
		}
		else if (leftNode && leftPath && leftNode.GetComponent<Node>().owner != playerName) { // unfill path to not conquered node
			leftPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 0f);
			leftPath.GetComponent<MeshRenderer>().material.SetColor("_BackColor", Color.white);
			StartCoroutine(fillPath(leftPath.GetComponent<MeshRenderer>().material, true, ConquestManager.Instance.fillTime * delayToEnemyPath));
		}

		if (rightNode && rightPath && rightNode.GetComponent<Node>().owner == playerName) { // fill path to conquered node
			rightPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 1f);
			rightPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			StartCoroutine(fillPath(rightPath.GetComponent<MeshRenderer>().material, false));
		}
		else if (rightNode && rightPath && rightNode.GetComponent<Node>().owner != playerName) { // unfill path to not conquered node
			rightPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 1f);
			rightPath.GetComponent<MeshRenderer>().material.SetColor("_BackColor", Color.white);
			StartCoroutine(fillPath(rightPath.GetComponent<MeshRenderer>().material, true, ConquestManager.Instance.fillTime * delayToEnemyPath));
		}

		if (upNode && upPath && upNode.GetComponent<Node>().owner == playerName) { // fill path to conquered node
			upPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 1f);
			upPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			StartCoroutine(fillPath(upPath.GetComponent<MeshRenderer>().material, false));
		}
		else if (upNode && upPath && upNode.GetComponent<Node>().owner != playerName) { // unfill path to not conquered node
			upPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 1f);
			upPath.GetComponent<MeshRenderer>().material.SetColor("_BackColor", Color.white);
			StartCoroutine(fillPath(upPath.GetComponent<MeshRenderer>().material, true, ConquestManager.Instance.fillTime * delayToEnemyPath));
		}

		if (bottomNode && bottomPath && bottomNode.GetComponent<Node>().owner == playerName) { // fill path to conquered node
			bottomPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 0f);
			bottomPath.GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			StartCoroutine(fillPath(bottomPath.GetComponent<MeshRenderer>().material, false));
		}
		else if (bottomNode && bottomPath && bottomNode.GetComponent<Node>().owner != playerName) { // unfill path to not conquered node
			bottomPath.GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 0f);
			bottomPath.GetComponent<MeshRenderer>().material.SetColor("_BackColor", Color.white);
			StartCoroutine(fillPath(bottomPath.GetComponent<MeshRenderer>().material, true, ConquestManager.Instance.fillTime * delayToEnemyPath));
		}

        //node (itself) fill code
		if (GetComponent<MeshRenderer>().material.GetColor("_ForeColor") == Color.white) {
			GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 1f);
			GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			StartCoroutine(fillPath(GetComponent<MeshRenderer>().material, false, ConquestManager.Instance.fillTime));
		}
		else {
			GetComponent<MeshRenderer>().material.SetFloat("_FlipYCut", 1f);
			GetComponent<MeshRenderer>().material.SetColor("_BackColor", GetComponent<MeshRenderer>().material.GetColor("_ForeColor"));
			GetComponent<MeshRenderer>().material.SetFloat("_CutoffY", 0f);
			GetComponent<MeshRenderer>().material.SetColor("_ForeColor", playerColor);
			StartCoroutine(fillPath(GetComponent<MeshRenderer>().material, false, ConquestManager.Instance.fillTime));
		}
	}

	IEnumerator fillPath(Material pathMat, bool unfill, float waitTime = 0) {
		float elapsedTime = 0f;

		yield return new WaitForSeconds(waitTime);

		while (elapsedTime < ConquestManager.Instance.fillTime) {
			if (!unfill) {
				pathMat.SetFloat("_CutoffY", Mathf.Lerp(0, 1, elapsedTime / ConquestManager.Instance.fillTime));
			}
			else {
				pathMat.SetFloat("_CutoffY", Mathf.Lerp(1, 0, elapsedTime / ConquestManager.Instance.fillTime));
			}
			yield return null;
			elapsedTime += Time.deltaTime;
		}

		pathMat.SetFloat("_CutoffY", unfill ? 0f : 1f);
		//GetComponent<MeshRenderer>().material.SetColor("_BackColor", Color.white);
	}
	#endregion
}
