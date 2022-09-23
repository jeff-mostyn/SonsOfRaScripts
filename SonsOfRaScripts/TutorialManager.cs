using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using UnityEngine.Video;
using System;

public class TutorialManager : MonoBehaviour
{
    [Header("Canvas/UI")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI ttDescription;
    public VideoPlayer vp1;
    public Canvas canvas;
    public CanvasGroup cg;
    public CanvasGroup ttcg;
    public GameObject confirmButton;

    [Header("Player")]
    public GameObject P1;
    public GameObject P2;
    public Meter playerMeter, enemyMeter;
    public UnitSpawner enemySpawner, playerSpawner;
    public bool canControl = false;
    private Player rewiredPlayer1;
    //private Player rewiredPlayer2;
    private Human_PlayerController player1;
    private AI_PlayerController player2;

    [Header("Settings")]
    public GameObject tutorialLoadout;
    public float fadeTime;
    public List<DetailsStruct> canvasDetails = new List<DetailsStruct>();
    public int spriteNumber;
    public int spriteNumber2;

    // Info to display when selecting node
    [System.Serializable]
    public struct DetailsStruct
    {
        public VideoClip keyboardClip;
        public VideoClip controllerClip;
    }

    public enum tutorialState { intro, unit, tower, blessing, meter, end };
    public tutorialState state;

    void Start()
    {
        confirmButton.SetActive(false);
        cg.alpha = 0;
        P1 = GameObject.Find("Player1");
        P2 = GameObject.Find("Player2");
        rewiredPlayer1 = ReInput.players.GetPlayer("Player0");
        //rewiredPlayer2 = ReInput.players.GetPlayer("Player1");
        player1 = P1.GetComponent<Human_PlayerController>();
        player2 = P2.GetComponent<AI_PlayerController>();

        playerSpawner = P1.GetComponent<UnitSpawner>();
        enemySpawner = P2.GetComponent<UnitSpawner>();

        playerMeter = P1.GetComponentInChildren<Meter>();
        enemyMeter = P2.GetComponentInChildren<Meter>();
        playerMeter.canGain = false;
        enemyMeter.canGain = false;

        P1.GetComponentInChildren<KeepManager>().infiniteHealth = true;
        P2.GetComponentInChildren<KeepManager>().infiniteHealth = true;

        state = tutorialState.unit;
        SwitchPart();
        StartCoroutine("TutorialBegin");
        Stun(true);
    }

    void Update()
    {
        if ((rewiredPlayer1.GetButtonDown(RewiredConsts.Action.Select) || Input.GetMouseButtonDown(0)) && canControl == false)
        {
            canControl = true;
            player1.isStunned = false;
            SwitchPart();
        }
    }

    public void SwitchPart()
    {
        confirmButton.SetActive(false);
        switch (state)
        {
            case tutorialState.unit:
                StartCoroutine("UnitBuild");
                break;
            case tutorialState.tower:
                StartCoroutine("TowerBuild");
                break;
            case tutorialState.blessing:
                StartCoroutine("BlessingUse");
                break;
            case tutorialState.meter:
                StartCoroutine("MeterUse");
                break;
            case tutorialState.end:
                StartCoroutine("ttFadeCanvasOut", true);
                P1.GetComponentInChildren<KeepManager>().infiniteHealth = false;
                P2.GetComponentInChildren<KeepManager>().infiniteHealth = false;
                Stun(false);
                break;
        }
    }

    IEnumerator TutorialBegin()
    {
        canControl = true;
        vp1.clip = null;
        title.text = Lang.TutorialText[Lang.tutorialText.welcome][SettingsManager.Instance.language];
        description.text = Lang.TutorialText[Lang.tutorialText.tutorialIntro][SettingsManager.Instance.language];

        yield return new WaitForSecondsRealtime(5f);
        StartCoroutine("FadeCanvasOut", false);
        yield return new WaitForSecondsRealtime(2f);

        Stun(true);
        state = tutorialState.unit;
        confirmButton.SetActive(true);
        canControl = false;
        LockRadial(1, false);
    }

    IEnumerator UnitBuild()
    {
        title.text = Lang.MenuText[Lang.menuText.units][SettingsManager.Instance.language];

        spriteNumber = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 24 : 20;
        spriteNumber2 = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 23 : 8;
        description.text = string.Format(Lang.TutorialText[Lang.tutorialText.spearmanInstructions][SettingsManager.Instance.language], spriteNumber, spriteNumber2);


        vp1.clip = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? canvasDetails[0].keyboardClip : canvasDetails[0].controllerClip;

        while (LivingUnitDictionary.dict["Player0"].Count < 3)
        {
            yield return null;
        }
        enemySpawner.SpawnUnit(0, "top", GameManager.Instance.GetNextEntityID());

        
        StartCoroutine("FadeCanvasOut", true);

        ttDescription.text = Lang.TutorialText[Lang.tutorialText.unitCombat][SettingsManager.Instance.language];

        StartCoroutine("ttFadeCanvasOut", false);
        while (LivingUnitDictionary.dict["Player1"].Count > 0)
        {
            yield return null;
        }

        description.text = Lang.TutorialText[Lang.tutorialText.unitDifferences][SettingsManager.Instance.language];

        StartCoroutine("ttFadeCanvasOut", true);
        StartCoroutine("FadeCanvasOut", false);
        state = tutorialState.tower;
        confirmButton.SetActive(true);
        canControl = false;
        yield return null;
    }

    IEnumerator TowerBuild()
    {
        LockRadial(12, false);
        player1.addGold(1500);
        title.text = Lang.MenuText[Lang.menuText.towers][SettingsManager.Instance.language];

        spriteNumber = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 24 : 20;
        spriteNumber2 = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 23 : 8;
        description.text = string.Format(Lang.TutorialText[Lang.tutorialText.archerTowerInstructions][SettingsManager.Instance.language], spriteNumber, spriteNumber2);
        vp1.clip = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? canvasDetails[1].keyboardClip : canvasDetails[1].controllerClip;

        InfluenceTileDictionary.ColorTiles("Player0");
        while (LivingTowerDictionary.dict["Player0"].Count < 3)
        {
            InfluenceTileDictionary.ColorTiles("Player0");
            yield return null;
        }
        InfluenceTileDictionary.UncolorTiles("Player0");

        description.text = Lang.TutorialText[Lang.tutorialText.towerDifferences][SettingsManager.Instance.language];
        state = tutorialState.blessing;
        confirmButton.SetActive(true);
        canControl = false;
        yield return null;
    }

    IEnumerator BlessingUse()
    {
        LockRadial(7, false);
        player1.addFavor(50);
        title.text = Lang.MenuText[Lang.menuText.blessings][SettingsManager.Instance.language];

        description.text = Lang.TutorialText[Lang.tutorialText.favorGain][SettingsManager.Instance.language];
        vp1.clip = canvasDetails[2].keyboardClip;

        //add click/pause before fade out
        yield return new WaitForSecondsRealtime(4f);
        StartCoroutine("FadeCanvasOut", true);

        spriteNumber = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 23 : 8;
        ttDescription.text = string.Format(Lang.TutorialText[Lang.tutorialText.igniteInstructions][SettingsManager.Instance.language], spriteNumber);// for some reason, this breaks? Can fix?

        StartCoroutine("ttFadeCanvasOut", false);
        player2.TerritoryExpansionPlaceTower(Constants.radialCodes.top);
        while (StatCollector.Instance.GetRecording(0).p1BlessingsUsed < 1) //change to just check number of enemy towers? meh
        {            
            yield return null;
        }
        yield return new WaitForSecondsRealtime(3f);

        StartCoroutine("ttFadeCanvasOut", true);
        StartCoroutine("FadeCanvasOut", false);

        description.text = Lang.TutorialText[Lang.tutorialText.blessingDifferences][SettingsManager.Instance.language];
        state = tutorialState.meter;
        confirmButton.SetActive(true);
        canControl = false;
        yield return null;
    }

    IEnumerator MeterUse()
    {
        playerMeter.FillMeter(0.34f);
        vp1.clip = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? canvasDetails[3].keyboardClip : canvasDetails[3].controllerClip;
        title.text = Lang.TutorialText[Lang.tutorialText.meterTitle][SettingsManager.Instance.language];
        spriteNumber = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 25 : 10;
        description.text = string.Format(Lang.TutorialText[Lang.tutorialText.meterUnlock][SettingsManager.Instance.language], spriteNumber);
        while (playerMeter.GetCurrentMeter() > 0.3f)
        {
            yield return null;
        }
        playerMeter.unlockBlocked = true;
        yield return new WaitForSecondsRealtime(2f);

        spriteNumber = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? 26 : 11;
        description.text = string.Format(Lang.TutorialText[Lang.tutorialText.meterExpansion][SettingsManager.Instance.language], spriteNumber);

        vp1.clip = ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasKeyboard ? canvasDetails[4].keyboardClip : canvasDetails[4].controllerClip;
        playerMeter.FillMeter(1f);
        while (StatCollector.Instance.GetRecording(0).p1Expansions.Count < 1)
        {
            yield return null;
        }
        playerMeter.unlockBlocked = false;
        state = tutorialState.end;
        LockRadial(1, true);
        confirmButton.SetActive(true);
        canControl = false;

        title.text = "";
        StartCoroutine("FadeCanvasOut", true);
        ttDescription.text = Lang.TutorialText[Lang.tutorialText.ready][SettingsManager.Instance.language]; //can we make this bold?
        StartCoroutine("ttFadeCanvasOut", false);

        //reset gold to default
        player1.setGold(1000);
        player2.setGold(1000);

        //able to gain meter now
        playerMeter.canGain = true;
        enemyMeter.canGain = true;
        playerMeter.ResetMeter();
        enemyMeter.ResetMeter();
        yield return null;
    }

    //StartCoroutine("FadeCanvasOut", false);
    IEnumerator FadeCanvasOut(bool fadeOut)
    {
        //true for fading out, false for fading in
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {            
            if (fadeOut)
            {
                cg.alpha = Mathf.Lerp(1, 0, (elapsedTime / fadeTime));
            }
            else
            {
                cg.alpha = Mathf.Lerp(0, 1, (elapsedTime / fadeTime));
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //making sure it hits 0 or 1
        if (fadeOut)
        {
            cg.alpha = 0;
        }
        else
        {
            cg.alpha = 1;
        }
        yield return null;
    }

    IEnumerator ttFadeCanvasOut(bool fadeOut) //tooltip canvas group fading
    {
        //true for fading out, false for fading in
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            if (fadeOut)
            {
                ttcg.alpha = Mathf.Lerp(1, 0, (elapsedTime / fadeTime));
            }
            else
            {
                ttcg.alpha = Mathf.Lerp(0, 1, (elapsedTime / fadeTime));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //making sure it hits 0 or 1
        if (fadeOut)
        {
            ttcg.alpha = 0;
        }
        else
        {
            ttcg.alpha = 1;
        }
        yield return null;
    }

    public void Stun(bool q)
    {
        if (q)
        {
            player1.isStunned = true;
            player2.isStunned = true;
        }
        else
        {
            player1.isStunned = false;
            player2.isStunned = false;
        }
    }

    public void LockRadial(int zoneToUnlock, bool defaultRadial)
    {
        if (defaultRadial) //resets back to default radial if true
        {
            for (int i = 1; i < 13; i++)
            {
                playerMeter.Lock(i);
            }
            playerMeter.Unlock(1);
            playerMeter.Unlock(4);
            playerMeter.Unlock(5);
            playerMeter.Unlock(6);
            playerMeter.Unlock(7);
            playerMeter.Unlock(8);
            playerMeter.Unlock(12);
            player1.UpdateLocks();
        }
        else //lock everything but 1 zone (1-spearman, 12-archertower, 5-top blessing)
        {
            for (int i = 1; i < 13; i++)
            {
                playerMeter.Lock(i);
            }
            playerMeter.Unlock(zoneToUnlock);
            player1.UpdateLocks();
        }
    }

    public void OnDestroy()
    {
        SettingsManager.Instance.SetUseTimer(true);
    }
}
