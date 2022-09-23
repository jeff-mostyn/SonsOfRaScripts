using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisAnimDial : MonoBehaviour
{
    [SerializeField] private int TexCount;
    [SerializeField] private Texture2D[] MyShadTextures;
    [SerializeField] private Texture2D[] MyEmisTextures;
    private int RGBplace = 0;
    private int TexIndex = 0;

    [SerializeField] private float TexChangeTime;
    private float myTimer;

    [SerializeField] private TowerState MyTowerState;
    //private bool matSetUp = false;

    Material myStasisMat;

    // Start is called before the first frame update
    void Start()
    {
        myStasisMat = MyTowerState.mainMat;
        StartTexture();
        

        /*if (MyTowerState.rewiredPlayerKey == PlayerIDs.player1){
            myPartColor = p1PartColor;
        } else {
            myPartColor = p2PartColor;
        }*/
    }
    // Update is called once per frame
    void Update()
    { 
        myTimer += Time.deltaTime;

        if(myTimer >= TexChangeTime)
        {
            NextTexture();
            myTimer = 0f;
        }
        

    }

    void StartTexture()
    {
        myStasisMat.SetFloat("_RGBInd", 0f);
        myStasisMat.SetTexture("_ShadMap", MyShadTextures[0]);
        myStasisMat.SetTexture("_EmisMap", MyEmisTextures[0]);
    }

    void NextTexture()
    {
        RGBplace++;

        if (RGBplace > 2){
            RGBplace = 0;
            TexIndex++;
        }

        if(TexIndex * 3 + RGBplace > TexCount - 1)
        {
            RGBplace = 0;
            TexIndex = 0;
        }

        myStasisMat.SetFloat("_RGBInd", RGBplace);
        //Debug.Log(RGBplace);
        myStasisMat.SetTexture("_ShadMap", MyShadTextures[TexIndex]);
        myStasisMat.SetTexture("_EmisMap", MyEmisTextures[TexIndex]);


        /* Particle spawning for sundial change
        
        if (MyTowerState.state == TowerState.tStates.placed)
        {
            ParticleSystem fxInstance = Instantiate(MySingleParticle, new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), transform.rotation);
            fxInstance.transform.SetParent(transform);

            //give particle correct color
            ParticleSystem.MainModule mainMod = fxInstance.main;
            mainMod.startColor = myPartColor;
        }*/
    }
}
