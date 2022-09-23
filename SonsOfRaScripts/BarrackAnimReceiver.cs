using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrackAnimReceiver : MonoBehaviour
{
    [SerializeField] private Animator TargetAnim;
    private Animator myAnim;

    [MinMax(1,5)] //Gives it cool bar display in editor (thanks to @febucci : https://www.febucci.com/)
    [SerializeField] private Vector2 AttackTimeRange;
    private float CurrAttackTime;
    private float MyTimer;


    // Start is called before the first frame update
    void Start()
    {
        myAnim = gameObject.GetComponent<Animator>();
        FindRandAttackTime();
    }

    private void Update()
    {
        MyTimer += Time.deltaTime;
        if(MyTimer >= CurrAttackTime)
        {
            myAnim.SetTrigger("Attack"); // set unit animator to play attack anim
            MyTimer = 0f;
            FindRandAttackTime();
        }
    }

    public void Attack()
    {
        TargetAnim.SetTrigger("Hit"); //tell target they have been hit
    }

    private void FindRandAttackTime()
    {
        CurrAttackTime = Random.Range(AttackTimeRange.x, AttackTimeRange.y);
    }
}
