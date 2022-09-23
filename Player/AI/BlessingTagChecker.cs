using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlessingTagChecker
{
	private Dictionary<AI_BlessingHandler.blessingKeywords, bool> blessingConditions;

    public BlessingTagChecker(List<AI_BlessingHandler.blessingKeywords> tags) {
		blessingConditions = new Dictionary<AI_BlessingHandler.blessingKeywords, bool>();
		for (int i=0; i<tags.Count; i++) {
			blessingConditions.Add(tags[i], false);
		}
	}

	public bool CheckConditions() {
		foreach (AI_BlessingHandler.blessingKeywords key in blessingConditions.Keys.ToList()) {
			if (blessingConditions[key] == false) {
				return false;
			}
		}

		return true;
	}

	public void MarkCondition(AI_BlessingHandler.blessingKeywords tag) {
		if (blessingConditions.ContainsKey(tag)) {
			blessingConditions[tag] = true;
		}
	}

	public void ResetDict() {
		foreach (AI_BlessingHandler.blessingKeywords key in blessingConditions.Keys.ToList()) {
			blessingConditions[key] = false;
		}
	}
}
