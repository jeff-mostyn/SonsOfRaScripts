using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//discord deselect stuff
using Rewired;
using UnityEngine.EventSystems;

public class MenuScripts : MonoBehaviour {

    public Transform canvas;
    public Transform gameUI;

    public GameObject player1;
    public GameObject player2;
    private PlayerController pc1, pc2;

    private SceneChangeManager sc;

	[SerializeField] private PauseMenuController pauseControl;

	void Start() {
		sc = SceneChangeManager.Instance;
        if(player1 != null) {
            pc1 = player1.GetComponent<PlayerController>();
        }
        if (player2 != null) {
            pc2 = player2.GetComponent<PlayerController>();
        }
    }

	#region Gamemode Select
	public void PlayQuickplay () {
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.quickplay);
		Play(false);
	}

    //difficult specific quickplays
    public void PlayQuickplayEasy () {
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.quickplay);
        SettingsManager.Instance.SetDifficulty(0);
        Play(false);
	}

    public void PlayQuickplayNormal () {
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.quickplay);
        SettingsManager.Instance.SetDifficulty(1);
        Play(false);
	}

    public void PlayQuickplayHard () {
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.quickplay);
        SettingsManager.Instance.SetDifficulty(2);
        Play(false);
	}

	public void PlayConquest(bool restart) {
		Constants.gameScenes nextScene;
		if (restart)
		{
			nextScene = Constants.gameScenes.patronSelectConquest;
			SaveSystem.ClearData();
		}
		else
		{
			nextScene = Constants.gameScenes.conquest1;
		}
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.conquest);
		//SaveSystem.ClearData();

		sc.setNextSceneName(Constants.sceneNames[nextScene]);
		sc.LoadNextScene();
	}

	public void PlayArcade()
	{
		if (ArcadeManager.Instance) {
			Destroy(ArcadeManager.Instance.gameObject);
		}
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.arcade);

		SoundManager.Instance.sound_click();

		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.patronSelectConquest]);
		sc.LoadNextScene();
	}

	public void PlayLocalMulti() {
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.localMulti);
		Play(false);
	}

	public void PlayTutorial()
	{
		SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.tutorial);
		SettingsManager.Instance.SetUseTimer(false);
		Constants.gameScenes nextScene;
		nextScene = Constants.gameScenes.mapSenet;

		SoundManager.Instance.sound_hover();

		sc.setNextSceneName(Constants.sceneNames[nextScene]);
		sc.LoadNextScene();
	}

	private void Play(bool singleGodSelect) {
		Constants.gameScenes nextScene;
		if (singleGodSelect) {
			nextScene = Constants.gameScenes.patronSelectConquest;
		}
		else {
			nextScene = Constants.gameScenes.patronSelect;
		}

		SoundManager.Instance.sound_click();

		sc.setNextSceneName(Constants.sceneNames[nextScene]);
		sc.LoadNextScene();
    }

	public void GodSelect() {
		if (GameObject.Find("MatchManager") != null) {
			Destroy(GameObject.Find("MatchManager"));
		}
		if (GameObject.Find("StatCollector") != null) {
			Destroy(GameObject.Find("StatCollector"));
		}

		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.patronSelect]);
		sc.LoadNextScene();
	}

    public void GoToLobbyMenu() {
		if (SettingsManager.Instance.PlayOnline) {
			SettingsManager.Instance.SetGameMode(SettingsManager.gamemodes.online);

			SoundManager.Instance.sound_click();

			sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.lobby]);
			sc.LoadNextScene();
		}
    }
    #endregion

    public void Rematch() {
		sc.setNextSceneName(LoadoutManager.Instance.GetMap());
		sc.LoadNextScene();
	}

    public void DiscordLink() {
		if (SettingsManager.Instance.language == Lang.language.SChinese) {  // for chinese go to QQ
			return;
		}
		else {
			Application.OpenURL("https://discord.gg/zV8R2Aw");
		}
	}

	public void SurveyLink() {
		Application.OpenURL("https://forms.gle/MoqFaSnQY2B56YPz8");
	}

	public void UnselectDiscordButton() {
		if (ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasMouse) {
            EventSystem.current.SetSelectedGameObject(null);
		}
	}

	// Called from pause canvas
	public void RestartGame() //this one is for when players restart from the pause menu.
    {
		// clean up match manager
		if (GameObject.Find("MatchManager") != null) {
			Destroy(GameObject.Find("MatchManager"));
		}
		if (GameObject.Find("StatCollector") != null) {
			Destroy(GameObject.Find("StatCollector"));
		}

		// unfilter music
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("paused", 0);
		MusicManager.Instance.ForceMusicReload();

		Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

		SceneChangeManager.Instance.setNextSceneName(sceneName);
		SceneChangeManager.Instance.LoadNextScene();

		pauseControl.LeaveMenu();
        
        Time.timeScale = 1;
	}

    public void StatScreen()    //this is for when players finish game and want to see their stats
    {
		if (GameObject.Find("MatchManager") != null) {
			Destroy(GameObject.Find("MatchManager"));
		}

		// clean up match manager
		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.postGame]);
        sc.LoadNextScene();
    }

    public void ResumeGame() {
        pc1.isStunned = false;
        pc2.isStunned = false;
        canvas.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);

		pauseControl.LeaveMenu();

		Time.timeScale = 1;
	}

	public void ResumeConquest() {
		transform.parent.gameObject.SetActive(false);
	}

	public void QuitMatch() {
		// unfilter music
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("paused", 0);

		if (SettingsManager.Instance.GetIsOnline() && !GameManager.Instance.gameOver) {
            canvas.gameObject.SetActive(false);
            gameUI.gameObject.SetActive(true);

            pauseControl.LeaveMenu();

            if (OnlineManager.Instance.GetIsHost()) {
                GameManager.Instance.p1.GetComponentInChildren<KeepManager>().takeDamage(int.MaxValue);
                OnlineManager.Instance.SendPacket(new PO_KeepDamage(PlayerIDs.player1, int.MaxValue));
            }
            else {
                GameManager.Instance.p2.GetComponentInChildren<KeepManager>().takeDamage(int.MaxValue);
                OnlineManager.Instance.SendPacket(new PO_KeepDamage(PlayerIDs.player2, int.MaxValue));
            }
        }
		else if (!SettingsManager.Instance.GetIsArcade()) {
 			MainMenu();
		}
		else {
			pc1.isStunned = false;
			pc2.isStunned = false;
			canvas.gameObject.SetActive(false);
			gameUI.gameObject.SetActive(true);

			pauseControl.LeaveMenu();

			Time.timeScale = 1;

			GameManager.Instance.p1.GetComponentInChildren<KeepManager>().takeDamage(int.MaxValue);
		}
	}

    public void MainMenu() {
		// clean up match manager
		if (GameObject.Find("MatchManager") != null) {
			Destroy(GameObject.Find("MatchManager"));
		}
		if (GameObject.Find("StatCollector") != null) {
			Destroy(GameObject.Find("StatCollector"));
		}

        if (SettingsManager.Instance.GetIsOnline()) {
            OnlineManager.Instance.currentLobby.Leave();
			LobbyManager.Instance.inLobby = false;

			Destroy(LobbyManager.Instance.gameObject);
            Destroy(OnlineManager.Instance.gameObject, 1f);

			SoundManager.Instance.sound_back_menu();
        }

		MusicManager.Instance.MusicFade(true);

		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.mainMenu]);
		sc.LoadNextScene();
		Time.timeScale = 1;

        LoadoutManager old = GameObject.FindObjectOfType<LoadoutManager>();
        if(old != null) {
            Destroy(old.gameObject);
        }
	}

	public void SaveQuitToMainMenu()
	{
		GameObject.Find("Conquest Manager").GetComponent<ConquestManager>().SavePlayers();
		if (GameObject.Find("MatchManager") != null)
		{
			Destroy(GameObject.Find("MatchManager"));
		}
		if (GameObject.Find("StatCollector") != null) {
			Destroy(GameObject.Find("StatCollector"));
		}

		MusicManager.Instance.MusicFade(true);

		sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.mainMenu]);
		sc.LoadNextScene();
		Time.timeScale = 1;

		LoadoutManager old = GameObject.FindObjectOfType<LoadoutManager>();
		if (old != null)
		{
			Destroy(old.gameObject);
		}
	}

    public void QuitGame()
    {
		if (!SettingsManager.Instance.getIsDemoMode() 
			|| SceneManager.GetActiveScene().name == Constants.sceneNames[Constants.gameScenes.discordSplash]) {
			Application.Quit();
		}
        else {
			sc.setNextSceneName(Constants.sceneNames[Constants.gameScenes.discordSplash]);
			sc.LoadNextScene();
		}
    }
}
