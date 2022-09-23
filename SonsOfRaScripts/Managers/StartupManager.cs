using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartupManager : MonoBehaviour {

    public GameObject P1, P2;
    public GameObject cameraGroup;
    public ParticleSystem dust;
    public GameObject fade;
    public TextMeshProUGUI timer;
    public Image[] neutralExpansionGUI;

    private ModelDrop md1, md2;
    private CameraMoveScript cms;
    private PlayerController pc1, pc2;
	private SoundManager s;
    private Image[] p1gui, p2gui;
    private TextMeshProUGUI[] p1txt, p2txt;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string leftDropEvent;
	[FMODUnity.EventRef] [SerializeField] private string rightDropEvent;

	bool isSinglePlayer;

    // Use this for initialization

    void Start () {
		fade.SetActive(true);
        cms = cameraGroup.GetComponent<CameraMoveScript>();
        md1 = P1.GetComponent<ModelDrop>();
        md2 = P2.GetComponent<ModelDrop>();

        pc1 = P1.GetComponent<PlayerController>();
		
        pc2 = P2.GetComponent<PlayerController>();

        p1gui = P1.GetComponentsInChildren<Image>();
        p2gui = P2.GetComponentsInChildren<Image>();
        p1txt = P1.GetComponentsInChildren<TextMeshProUGUI>();
        p2txt = P2.GetComponentsInChildren<TextMeshProUGUI>();

        fadeFunctionImage(p1gui, 0, 0);
        fadeFunctionImage(p2gui, 0, 0);
        fadeFunctionText(timer, 0, 0);
        fadeFunctionText(p1txt, 0, 0);
        fadeFunctionText(p2txt, 0, 0);

        if(neutralExpansionGUI.Length > 0)
        {
            fadeFunctionImage(neutralExpansionGUI, 0, 0);
        }
        

        pc1.isStunned = true;
        pc2.isStunned = true;

		s = SoundManager.Instance;

        StartCoroutine(MatchBegin());
	}

    private IEnumerator MatchBegin()
    {
		if (SettingsManager.Instance.GetMatchPlay()) {
			MatchManager.Instance.Fade(0.5f, true, true, false);
			MatchManager.Instance.SetupRoundCircles();
		}
		else {
			MatchManager.Instance.gameObject.SetActive(false);
		}

        GameObject.Find("PauseController").GetComponent<PauseGame>().DisablePause = true;
        //Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        md1.ObjectDrop(); //drop keep1 - 140/13
        md2.ObjectDrop(); //drop keep2 - 200/23.5

		yield return new WaitForSeconds(0.6f);

		cms.moveFromStartToTarget(cms.MainView, cms.p1s, 1f); //move to keep1

		yield return new WaitForSeconds(0.7f);

		if (SettingsManager.Instance.GetMatchPlay()) {
			MatchManager.Instance.Fade(1f, false, true, false);
		}

		yield return new WaitForSeconds(0.175f);
		sound_drop(leftDropEvent, GameManager.Instance.player1Controller);
		yield return new WaitForSeconds(.425f);

		Transform spawn = P1.transform.Find("dustLocation");
        Instantiate(dust, spawn.position, dust.transform.rotation);

		yield return new WaitForSeconds(.4f);

        cms.moveFromStartToTarget(cms.p1s, cms.p2s, 0.8f); //move to keep2

        yield return new WaitForSeconds(0.655f);
		sound_drop(rightDropEvent, GameManager.Instance.player2Controller);
		yield return new WaitForSeconds(0.445f);

        spawn = P2.transform.Find("dustLocation");
        Instantiate(dust, spawn.position, dust.transform.rotation);

		yield return new WaitForSeconds(.4f);

        cms.moveFromStartToTarget(cms.p2s, cms.MainView, 1f); //move to main view

        yield return new WaitForSeconds(1f);
		
		//Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("UI");
		fadeFunctionImage(p1gui, 1, 1);
        fadeFunctionImage(p2gui, 1, 1);
        fadeFunctionText(timer, 1, 1);
		if (SettingsManager.Instance.GetMatchPlay()) {
			MatchManager.Instance.Fade(1f, true, false, true);
		}
		fadeFunctionText(p1txt, 1, 1);
        fadeFunctionText(p2txt, 1, 1);

        if (neutralExpansionGUI.Length > 0)
        {
            fadeFunctionImage(neutralExpansionGUI, 1, 1);
		}

        pc1.isStunned = false;
        pc2.isStunned = false;

		LivingTowerDictionary.NukeDictionary();

        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.gameStarted = true;
        
        if (SettingsManager.Instance.GetIsOnline()) {
            if (OnlineManager.Instance.GetIsHost()) {
                GameManager.Instance.StartRegularSync();
            }
        }

        if (SettingsManager.Instance.GetIsConquest()) {
			QuipsManager.Instance.PlayGameBeginningQuip(PlayerIDs.player2);
		}

		GameObject.Find("PauseController").GetComponent<PauseGame>().DisablePause = false;
        Destroy(fade);
        yield return null;
    }

    private void fadeFunctionText(TextMeshProUGUI[] l, float direction, float duration) //direction 0 for transparency, 1 for opaque
    {
        foreach (TextMeshProUGUI txt in l)
        {
            txt.CrossFadeAlpha(direction, duration, false);
        }
    }
    private void fadeFunctionText(TextMeshProUGUI t, float direction, float duration) //direction 0 for transparency, 1 for opaque
    {
            t.CrossFadeAlpha(direction, duration, false);
    }
    private void fadeFunctionImage(Image[] l, float direction, float duration) //direction 0 for transparency, 1 for opaque
    {
        foreach (Image im in l)
        {
            im.CrossFadeAlpha(direction, duration, false);
        }
    }

    private IEnumerator MatchBegin2()
    {
        md1.ObjectDrop(); //drop keep1 - 105/9
        md2.ObjectDrop(); //drop keep2 - 105/16.5
        yield return new WaitForSecondsRealtime(0.6f);
        cms.moveToKeep(1, 1f); //move to keep1
        yield return new WaitForSecondsRealtime(.7f);
        P1.GetComponent<PlayerController>().healthBar.SetActive(true); //hpbar and dust 1
        Transform spawn = P1.transform.Find("dustLocation");
        Instantiate(dust, spawn.position, dust.transform.rotation);
        yield return new WaitForSecondsRealtime(.4f);
        cms.moveToKeep(3, 0.8f); //move to keep2
        yield return new WaitForSecondsRealtime(.7f);
        P2.GetComponent<PlayerController>().healthBar.SetActive(true); //hpbar and dust 2
        spawn = P2.transform.Find("dustLocation");
        Instantiate(dust, spawn.position, dust.transform.rotation);
        yield return new WaitForSecondsRealtime(.4f);

        cms.moveToKeep(0, 1f); //move to main view

        yield return new WaitForSecondsRealtime(1f);
        pc1.isStunned = false;
        pc2.isStunned = false;

        yield return null;
    }

    private IEnumerator MatchBegin3() //best so far
    {
        md1.ObjectDrop(); //drop keep1
        md2.ObjectDrop(); //drop keep2
        yield return new WaitForSecondsRealtime(0.6f);
        cms.moveToKeep(1, 1f); //move to keep1
        yield return new WaitForSecondsRealtime(1.3f);
        P1.GetComponent<PlayerController>().healthBar.SetActive(true); //hpbar and dust 1
        Transform spawn = P1.transform.Find("dustLocation");
        Instantiate(dust, spawn.position, dust.transform.rotation);
        yield return new WaitForSecondsRealtime(.4f);
        cms.moveToKeep(3, 0.8f); //move to keep2
        yield return new WaitForSecondsRealtime(1.1f);
        P2.GetComponent<PlayerController>().healthBar.SetActive(true); //hpbar and dust 2
        spawn = P2.transform.Find("dustLocation");
        Instantiate(dust, spawn.position, dust.transform.rotation);
        yield return new WaitForSecondsRealtime(.4f);

        cms.moveToKeep(0, 1f); //move to main view

        yield return new WaitForSecondsRealtime(1f);
        pc1.isStunned = false;
        pc2.isStunned = false;

        yield return null;
    }

	private void sound_drop(string eventName, PlayerController player) {
		FMOD.Studio.EventInstance drop = FMODUnity.RuntimeManager.CreateInstance(eventName);
		drop.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(player.transform.Find("Keep")));
		drop.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		drop.start();
		drop.release();
	}
}
