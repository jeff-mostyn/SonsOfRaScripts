using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeData
{
	public string owner;
	public bool fortified;
	public bool hunkeredDown;
	public int fortification;
	public Node.NodeDetails details;

    public NodeData(Node node)
    {
		owner = node.owner;
		fortified = node.fortified;
		hunkeredDown = node.hunkeredDown;
		fortification = node.fortified ? (int)node.fortification : -1;
		details = node.nodeDetails;
    }
}
