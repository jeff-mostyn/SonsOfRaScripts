using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralDot : MonoBehaviour
{
	[Header("Values")]
	[SerializeField] private float tickDamage = 5;
	[SerializeField] private float tickTime = 1f;
	private float timeUntilTick = 0f;
	private UnitAI target;

	private void Start()
	{
		target = gameObject.GetComponent<UnitAI>();
	}

	void Update()
	{
		timeUntilTick -= Time.deltaTime;
		if (timeUntilTick <= 0)
		{
			timeUntilTick = tickTime;
			target.takeDamage(tickDamage, Constants.damageSource.blessing);
		}
	}
}
