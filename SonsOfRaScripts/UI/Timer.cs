using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Timer : MonoBehaviour
{

    public TextMeshProUGUI timerText;
    //public float startTime;
    public GameObject K1, K2;

    private bool done = false;
    private KeepManager p1, p2;
    [SerializeField]
    private float timeLeft;
    private GameManager gm;
    private cameraShake cs;

	// Start is called before the first frame update
	private void Awake() {
		timerText.SetText("");
	}

	void Start()
    {
        gm = GameManager.Instance;
        cs = Camera.main.GetComponent<cameraShake>();
        p1 = K1.GetComponent<KeepManager>();
        p2 = K2.GetComponent<KeepManager>();

        timeLeft = SettingsManager.Instance.GetRoundTimeMinutes() * 60;	// get minutes, multiply to seconds

        string minutes = ((int)timeLeft / 60).ToString();
        string seconds = ((int)timeLeft % 60).ToString("d2");

        timerText.SetText(minutes + ":" + seconds);
    }

    // Update is called once per frame
    void Update()
    {
        if(timeLeft > 10)
        {
            if(GameManager.Instance.gameStarted == true)
            {
                timeLeft -= Time.deltaTime;

                string minutes = ((int)timeLeft / 60).ToString();
                string seconds = ((int)timeLeft % 60).ToString("d2");

                timerText.SetText("<mspace=0.58em>" + minutes + "</mspace>:<mspace=0.58em>" + seconds + "</mspace>"); 
            }
        }
        else if(timeLeft > 0)
        {
            timerText.fontStyle = FontStyles.Bold;
            timerText.color = Color.red;
            timerText.color = new Color(timerText.color.r, timerText.color.g, timerText.color.b, Mathf.Sin(Time.time * 6f));

            timeLeft -= Time.deltaTime;

            string minutes = ((int)timeLeft / 60).ToString();
            string seconds = ((int)timeLeft % 60).ToString("d2");

            timerText.SetText("<mspace=0.58em>" + minutes + "</mspace>:<mspace=0.58em>" + seconds + "</mspace>");
        }
        else
        {
            if(done == false)
                TimeOver();
        }

        //if (Input.GetKeyDown("t")) //dev tool, to be removed
        //{
        //    timerText.color = Color.white;
        //    timeLeft += 60;
        //}
    }

    private void TimeOver() {
        done = true;
        if(p1.getHealth() > p2.getHealth()) {
            p2.KillKeep();
        }
        else if(p2.getHealth() > p1.getHealth()) {
            p1.KillKeep();
        }
        else {
            p1.takeDamage(p1.getHealth() - 10);
            p2.takeDamage(p2.getHealth() - 10);
            cs.ShakeTheCamera(0.3f, 0.5f);
            timerText.color = Color.red;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            timerText.fontStyle = FontStyles.Bold;
            timerText.SetText(Lang.MenuText[Lang.menuText.suddenDeath][SettingsManager.Instance.language]);
			MusicManager.Instance.PlaySuddenDeath();
        }
    }

    public float GetTimeRemaining() {
        return timeLeft;
    }

    public void SetTimeRemaining(float _t) {
        timeLeft = _t;
    }

	public bool IsSuddenDeath() {
		return done;
	}
}
