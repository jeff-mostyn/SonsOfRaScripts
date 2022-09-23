using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainMenuAnim : MonoBehaviour {

    Animator unitAni;
    private bool freezeAnimState = false;
    private float animClock = 0;
    private int animIndex = 0;

    [SerializeField] private GameObject ScrollMesh; //for disabling and re-enabling scroll
    private enum animStates {Start, Tutorial, Settings, Quit}
    private animStates currAnim;

    // Use this for initialization
    void Start () {
        unitAni = gameObject.GetComponent<Animator>();
        currAnim = animStates.Quit;
        StartAnim();
    }
	
	// Update is called once per frame
	void Update () {
		if (currAnim == animStates.Start) {
            if (animClock <= 0f) {
                if (animIndex == 0) {
                    Break1Anim();
                    animIndex++;
                } else if (animIndex == 1) {
                    Break2Anim();
                    animIndex++;
                } else {
                    Break3Anim();
                    animIndex = 0;
                }
                animClock = SelectRandomTime();
            } else {
                animClock -= Time.deltaTime;
            }
        }
	}

    float SelectRandomTime() {
        return Random.Range(10f, 20f);
    }

    //functions for activating animation triggers
    public void StartAnim() {
        if (!freezeAnimState)
        {
            //Check if already in start state
            if (!(currAnim == animStates.Start))
            {
                unitAni.SetTrigger("enterStart");
            }

            ScrollMesh.SetActive(false);
            animClock = SelectRandomTime();
            currAnim = animStates.Start;
        }
    }

    public void DirectToIdle() {
        unitAni.SetTrigger("direct2Idle");

        ScrollMesh.SetActive(false);
        animClock = SelectRandomTime();
        currAnim = animStates.Start;
    }

    public void TutorialAnim() {
        if (!freezeAnimState)
        {
            if (!(currAnim == animStates.Tutorial))
            {
                unitAni.SetTrigger("enterTut");
            }

            ScrollMesh.SetActive(false);
            currAnim = animStates.Tutorial;
        }
    }

    public void SettingsAnim() {
        if (!freezeAnimState)
        {
            if (!(currAnim == animStates.Settings))
            {
                //Check that you aren't going from SettingAnim to SettingAnim (if you are, don't reset anim)
                unitAni.SetTrigger("enterSet");
            }
            //ScrollMesh.SetActive(false);
            currAnim = animStates.Settings;
        }
    }

     public void SettingsStill() {
        unitAni.SetBool("stillSet", true);

        //ScrollMesh.SetActive(true);
        currAnim = animStates.Settings;

        FreezeAnim();
    }

    public void SettingsEndStill() {
        unitAni.SetBool("stillSet", false);
        currAnim = animStates.Settings;

        UnfreezeAnim();
    }

    public void QuitAnim() {
        if (!freezeAnimState)
        {
            if (!(currAnim == animStates.Quit))
            {
                unitAni.SetTrigger("enterQuit");
            }

            ScrollMesh.SetActive(false);
            currAnim = animStates.Quit;
        }
    }

    //Stopping Animations from changing (in overlay screens)
    public void FreezeAnim() {
        freezeAnimState = true;
    }

    public void UnfreezeAnim() {
        freezeAnimState = false;
    }

    //anim breaks in idle sequence
    public void Break1Anim() {
        unitAni.SetTrigger("idleBreak1");
    }

    public void Break2Anim() {
        unitAni.SetTrigger("idleBreak2");
    }

    public void Break3Anim() {
        unitAni.SetTrigger("idleBreak3");
    }

    //Scroll Event
    public void ScrollEvent()
    {
        ScrollMesh.SetActive(true);
    }
}
