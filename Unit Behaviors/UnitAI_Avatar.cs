//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.AI;

//public class UnitAI_Avatar : UnitAI
//{
//    // enemy variables
//    protected UnitAI enemyAI;
//    protected List<UnitAI> enemiesInRange;

//    // advantage
//    protected bool hasAdvantage;

//    // Use this for initialization
//    void Start()
//    {
//        self = gameObject;
//        base.start();

//        hasAdvantage = false;

//        enemiesInRange = new List<UnitAI>();

//        //ANIM accessing animator in model gameobject
//        unitAni = gameObject.GetComponentsInChildren<Animator>()[0];
//    }

//    void Update()
//    {
//        base.update();
//    }

//    #region Combat Functions
//    // set state to attacking, check for advantage, and transition to attacking animation
//    protected override void EnterCombat()
//    {
//        toAttacking();

//        //ANIM - walk anim
//        unitAni.SetBool("walking", false);
//        unitAni.SetBool("attackState", true);
//    }


//    // Deal damage to enemy unit, check if it's health is at or below 0, adjust engagement status accordingly
//    public override void Attack()
//    {
//        enemyAI.takeDamage(damage, this, Constants.damageSource.unit);
//        AOEDamage(enemy.transform.position, 1);

//        if (enemyAI.getHealth() <= 0 || enemy == null)
//        {
//            if (enemy == null)
//            {
//                for (int i = 0; i < enemiesInRange.Count; i++)
//                {
//                    if (enemiesInRange[i] == null)
//                        enemiesInRange.RemoveAt(i);
//                }
//            }
//            else
//                enemiesInRange.Remove(enemy.GetComponent<UnitAI>());

//            toWalking();

//            attackCountDown = 1 / (attackSpeed * attackSpeedModifier);
//            enemy = null;
//            enemyAI = null;
//        }

//        //ANIM - attackAnimation
//        unitAni.SetTrigger("attack");

//        // Play Attack Sound
//        sound_attack();
//    }

//    void AOEDamage(Vector3 center, float radius)
//    {
//        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
//        int i = 0;
//        while (i < hitColliders.Length)
//        {
//            if(hitColliders[i].gameObject.tag == enemyUnitTag)
//            {
//                GameObject hit = hitColliders[i].gameObject;
//                hit.GetComponent<UnitAI>().takeDamage(damage * 0.75f, this, Constants.damageSource.unit);
//            }
//            i++;
//        }
//    }

//    // transition unit to walking and unset combat booleans
//    protected override void RemoveFromCombat()
//    {
//        toWalking();
//        if (hasAdvantage)
//        {
//            hasAdvantage = false;
//        }
//        if (agent.enabled)
//        {
//            agent.SetDestination(gameObject.GetComponent<UnitMovement>().target.position); //seems a bit contrived.
//            agent.isStopped = false;
//        }

//        unitAni.SetBool("walking", true);
//        unitAni.SetBool("attackState", false);
//    }

//    #endregion

//    #region Detection Functions

//    // if enemy unit enters range of unit and unit is not engaged with it, act upon that input
//    protected override void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.tag == enemyUnitTag)
//        {
//            UnitAI ai = other.gameObject.GetComponent<UnitAI>();

//            // detect shieldbearer and attack it if it has aggro space
//            if (ai.getType() == Constants.unitType.shieldbearer && !aggroed)
//            {
//                UnitAI_Shieldbearer shield = other.gameObject.GetComponent<UnitAI_Shieldbearer>();
//                if (shield.canAggro())
//                {
//                    enemiesInRange.Insert(0, ai);
//                    enemyAI = ai;
//                    enemy = ai.self;
//                    shield.drawAggro(this);
//                    aggroTarget = shield;

//                    EnterCombat();
//                }
//                else
//                {
//                    enemiesInRange.Add(ai);
//                }
//            }
//            else
//            {
//                enemiesInRange.Add(ai);
//            }
//        }
//    }

//    // If the "enemy" unit leaves unit's range, do not remain engaged with it
//    protected override void OnTriggerExit(Collider other)
//    {
//        enemiesInRange.Remove(other.gameObject.GetComponent<UnitAI>());
//        if (other.gameObject == enemy)
//        {
//            toWalking();
//            if (hasAdvantage)
//            {
//                hasAdvantage = false;
//            }
//            enemy = null;
//            enemyAI = null;

//            unitAni.SetBool("walking", true);
//            unitAni.SetBool("attackState", false);
//        }
//    }

//    protected override void CheckForTarget()
//    {
//        if (enemiesInRange.Count != 0)
//        {
//            if (enemy == null || object.Equals(enemy, null) || enemyAI.getHealth() <= 0)
//            {
//                enemiesInRange.Remove(enemyAI);

//                toWalking();

//                enemy = null;
//                enemyAI = null;
//            }
//            if (enemiesInRange.Count != 0)
//            {  // unit is not an archer
//                for (int i = 0; i < enemiesInRange.Count; i++)
//                {
//                    if (enemiesInRange[i] != null)
//                    {
//                        enemy = enemiesInRange[i].self;
//                        enemyAI = enemiesInRange[i];

//                        // next enemy is a shieldbearer
//                        if (enemyAI.type == Constants.unitType.shieldbearer)
//                        {
//                            UnitAI_Shieldbearer shield = enemy.GetComponent<UnitAI_Shieldbearer>();
//                            // this shieldbearer has room to draw aggro
//                            if (shield.canAggro())
//                            {
//                                shield.drawAggro(this);
//                                aggroTarget = shield;
//                                aggroed = true;
//                            }
//                        }

//                        EnterCombat();
//                    }
//                }
//            }
//        }
//    }

//	public override void RefreshTargets() {
//		enemiesInRange.Clear();

//		DetectTargets();
//	}

//	private void DetectTargets() {
//		int unitLayer = 1 << LayerMask.NameToLayer("Unit");
//		List<Collider> unitsList = new List<Collider>();
//		Collider[] units;
//		GameObject unit;

//		// find towers in range, get list of all enemy towers
//		units = Physics.OverlapSphere(transform.position, range/* / 10*/, unitLayer);
//		unitsList = units.ToList();
//		unitsList.RemoveAll(x => x.gameObject.GetComponent<UnitAI>().rewiredPlayerKey == rewiredPlayerKey);

//		// go through towers in list, make sure to add new ones to enemy towers in range
//		for (int i = 0; i < unitsList.Count; i++) {
//			unit = unitsList[i].gameObject;
//			UnitAI ai = unit.GetComponent<UnitAI>();
//			if (!enemiesInRange.Exists(x => x == unit)) {
//				enemiesInRange.Add(ai);
//			}
//		}
//	}
//	#endregion

//	#region Death Functions

//	// Unit death sequence: give favor to opposing player and destroy game object. Could also initiate animation here
//	protected override void Die(Constants.damageSource source)
//    {
//        if (hasAdvantage)
//        {
//            hasAdvantage = false;
//        }

//        baseMovementSpeed = 0;

//        //ANIM - death animation
//        float randomVal = Random.Range(0.0f, 3.0f);
//        if (randomVal < 1f)
//        {
//            unitAni.SetTrigger("death1");
//            unitAni.SetBool("walking", false);

//        }
//        else if (randomVal >= 1f && randomVal < 2)
//        {
//            unitAni.SetTrigger("death2");
//            unitAni.SetBool("walking", false);

//        }
//        else
//        {
//            unitAni.SetTrigger("death3");
//            unitAni.SetBool("walking", false);
//        }

//        // play favor burst effect
//        Instantiate(favorBurst, transform.position, Quaternion.identity);

//        // Play Death Sound
//        sound_death();

//        Destroy(gameObject, 1);
//    }

//    #endregion

//    #region Effect Functions
//    protected override void sound_death()
//    {
//        s.unitDeath(soundPlayer, transform.position.z);
//    }

//    protected override void sound_attack()
//    {
//        s.shieldAttack(soundPlayer, transform.position.z);
//    }

//    // win/lose animations at the end of the game
//    public override void MyTeamWon()
//    {
//        unitAni.SetTrigger("winGame"); //doesn't work on catapults but doesn't throw an error
//        state = unitStates.stunned;
//        agent.enabled = false;
//        gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
//        stunDuration = 5f;
//        gameEnded = true;
//    }

//    public override void MyTeamLost()
//    {
//        unitAni.SetTrigger("loseGame"); //doesn't work on catapults but doesn't throw an error
//        state = unitStates.stunned;
//        agent.enabled = false;
//        gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
//        stunDuration = 5f;
//        gameEnded = true;
//    }

//    #endregion
//}
