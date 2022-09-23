using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticUnitPassive : PatronPassive
{
	public abstract void TriggerUnitDeathEffect(UnitAI unit, string unitPlayerId, Constants.damageSource damageSource);
}
