using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadoutManager : MonoBehaviour {

    public static LoadoutManager Instance;

	public List<GameObject> towers;
	public List<BaseExpansion> expansions;

	public Patron p1Patron, p2Patron;

	[Header("Player Cosmetic Customizations")]
    //color Palettes
    // 0= palette 1, 1= palette 2, 2= unit armor
    public Color[] p1Colors;
    public Color[] p2Colors;
	public Sprite p1Portrait, p2Portrait;
	public GameObject p1Cosmetic, p2Cosmetic;

    public GameObject[] p1Blessings; // IN ORDER: 0 - top; 1 - left; 2 - bottom; 3 - right
    public GameObject[] p2Blessings;

    public GameObject[] p1Towers;
    public GameObject[] p2Towers;

    public Text countdownText;

    public bool p1AllTowers =false;
    public bool p2AllTowers = false;

    public bool p1AllBlessings = false;
    public bool p2AllBlessings = false;

    public bool p1ShowReady = false;
    public bool p2ShowReady = false;

    public bool p1Ready = false;
    public bool p2Ready = false;

    public const int mainSceneIndex = 11;
    
    public bool isLoaded = false;

	private SceneChangeManager sc;
    private float afterSelectTimer = 3; //delay for proceeding after slecting both gods
    private bool changedScene = false;
    private bool countingDown = false;
    [SerializeField]
    private float countdown = 3;
    private Coroutine co;
    private Scene s;
	private string map;

    protected AudioSource src;

    void Awake() {
		// Have playlist persist across scenes.
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start () {
        src = GetComponent<AudioSource>();
        sc = SceneChangeManager.Instance;
	}

    private void Update()
    {
        s = SceneManager.GetActiveScene();
        if (s.name == Constants.sceneNames[Constants.gameScenes.patronSelect] 
			|| s.name == Constants.sceneNames[Constants.gameScenes.patronSelectConquest])
        {
            if(countdownText == null)
            {
                countdownText = GameObject.Find("Countdown").GetComponent<Text>();
            }
            if (playersReady())
            {
                countdownText.gameObject.SetActive(true);
                if (countingDown == false)
                {
                    co = StartCoroutine(CountDown());
                }
                afterSelectTimer -= Time.deltaTime;
            }
            else
            {
                if (co != null)
                {
                    StopCoroutine(co);

                }
                countdownText.gameObject.transform.localScale = Vector3.one;
                countdownText.text = null;
                countdownText.gameObject.SetActive(false);
                countdown = 3;
                afterSelectTimer = 3;
                countingDown = false;
                changedScene = false;
            }
            if (afterSelectTimer <= 0 && changedScene == false)
            {
                StopCoroutine(co);
                changedScene = true;
                confirm();
            }
        }
		else if(s.name == Constants.sceneNames[Constants.gameScenes.mainMenu]) {
            Destroy(gameObject);
        }
    }

    private bool playersReady() {
		if (p1Ready && (p2Ready || SettingsManager.Instance.GetIsArcade())) {
			return true;
		}
		else {
			return false;
		}
	}	

	private void confirm() {
		p1Ready = false;
		p2Ready = false;

		// clean up match manager if it exists
		if (GameObject.Find("MatchManager") != null) {
			Destroy(GameObject.Find("MatchManager"));
		}

		if (!SettingsManager.Instance.GetIsArcade()) {
			setBlessingsByPatron();
			SetColorPalettes(CustomizationManager.Instance.p1Palette, CustomizationManager.Instance.p2Palette);

			sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.mapSelect]);
			sc.LoadNextScene();
		}
		else {
			sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.arcade]);
			sc.LoadNextScene();
		}
	}

    // -------------- Getters / Setters --------------
    public void setBlessingsByPatron()
    {
        for (int i = 0; i < p1Blessings.Length; i++)
        {
            p1Blessings[i] = p1Patron.loadout[i].gameObject;
        }
        for (int i = 0; i < p2Blessings.Length; i++)
        {
            p2Blessings[i] = p2Patron.loadout[i].gameObject;
        }
    }

    public void setReady(string pID, Patron p) {
		if (pID == PlayerIDs.player1) {
			p1Patron = p;
			p1Ready = true;
		}
		else {
			p2Patron = p;
			p2Ready = true;
		}
	}

	public void unready(string pID) {
		if (pID == PlayerIDs.player1) {
			p1Patron = null;
			p1Ready = false;
		}
		else {
			p2Patron = null;
			p2Ready = false;
		}
	}

	public bool getReady(string pID) {
		if (pID == PlayerIDs.player1)
			return p1Ready;
		else
			return p2Ready;
	}

	public Patron getPatronAssignment(string playerKey) {
		if (playerKey == PlayerIDs.player1) {
			return p1Patron;
		}
		else {
			return p2Patron;
		}
	}

	public PatronPassive getPassiveAssignment(string playerKey) {
		if (playerKey == PlayerIDs.player1) {
			return p1Patron.passive;
		}
		else {
			return p2Patron.passive;
		}
	}

	public GameObject getBlessingAssignment(int i, string playerKey) {
		if (playerKey == "Player0") {
			return p1Blessings[i];
		}
		else if (playerKey == "Player1") {
			return p2Blessings[i];
		}
		else {
			return null;
		}
	}

	public GameObject getTowerAssignment(int i, string playerKey) {
		if (playerKey == "Player0") {
			return p1Towers[i];
		}
		else if (playerKey == "Player1") {
			return p2Towers[i];
		}
		else {
			return null;
		}
	}

    public Color getPaletteColor(int i, string playerKey) {
        if (playerKey == "Player0")
        {
            return p1Colors[i];
        }
        else if (playerKey == "Player1")
        {
            return p2Colors[i];
        }
        else
        {
            return new Color (0,0,0,0);
        }
    }

	public void SetColorPalettes(Color[] p1, Color[] p2) {
		p1Colors = p1;
		p2Colors = p2;
	}

	public void SetPortraits(Sprite p1, Sprite p2) {
		p1Portrait = p1;
		p2Portrait = p2;
	}

	public void SetCosmetics(GameObject p1, GameObject p2) {
		p1Cosmetic = p1;
		p2Cosmetic = p2;
	}

	public void SetMap(string _map) {
		map = _map;
	}

	public string GetMap() {
		return map;
	}

    private IEnumerator CountDown()
    {
        countingDown = true;

        countdownText.text = countdown.ToString();
		SoundManager.Instance.sound_countDown();

		float startTime = Time.time;

        while (Time.time - startTime < 1)
        {
            float amount = (Time.time - startTime) / 1;
            countdownText.gameObject.transform.localScale = Vector3.Lerp(Vector3.one * 1.0f, Vector3.one * 1.5f, amount);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, Mathf.Lerp(1,0,amount));
            yield return null;
        }

        countdown -= 1;
        countdownText.text = countdown.ToString();
        SoundManager.Instance.sound_countDown();

        startTime = Time.time;

        while (Time.time - startTime < 1)
        {
            float amount = (Time.time - startTime) / 1;
            countdownText.gameObject.transform.localScale = Vector3.Lerp(Vector3.one * 1.0f, Vector3.one * 1.5f, amount);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, Mathf.Lerp(1, 0, amount));
            yield return null;
        }

        countdown -= 1;
        countdownText.text = countdown.ToString();
		SoundManager.Instance.sound_countDown();

		startTime = Time.time;

        while (Time.time - startTime < 1)
        {
            float amount = (Time.time - startTime) / 1;
            countdownText.gameObject.transform.localScale = Vector3.Lerp(Vector3.one * 1.0f, Vector3.one * 1.5f, amount);
            countdownText.color = new Color(countdownText.color.r, countdownText.color.g, countdownText.color.b, Mathf.Lerp(1, 0, amount));
            yield return null;
        }
    }
}