using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class lobbyListing : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameField, pingCount, pingWarning;
	[SerializeField] private Image pingIcon;
	[SerializeField] private List<Sprite> pingIconSprites;
	private string location;
    public Button JoinLobbyButton;
    public CanvasGroup cg;
	public LobbyMenu menu;
	public Image MapImage;


    public Steamworks.Data.Lobby lobby;

	private void Start() {
		lobbyNameField.SetText("");
		location = "";

		if (string.IsNullOrWhiteSpace(lobbyNameField.GetParsedText())) {
			JoinLobbyButton.interactable = false;
			cg.alpha = 0;
		}
		else {
			JoinLobbyButton.interactable = true;
			cg.alpha = 1;
		}

		pingWarning.SetText(Lang.OnlineText[Lang.onlineText.connectionWarning][SettingsManager.Instance.language]);
	}

	public void Refresh() {
		try {
			lobby.Refresh();

			if (string.IsNullOrWhiteSpace(lobbyNameField.GetParsedText()) || lobbyNameField.text == SteamWorksManager.Instance.GetUsername()) {
				JoinLobbyButton.interactable = false;
				cg.alpha = 0;
			}
			else {
				JoinLobbyButton.interactable = true;
				cg.alpha = 1;

				MapImage.sprite = menu.maps.Find(x => x.name == lobby.GetData("mapName")).mapImage;

				if (location != "") {
					int ping = SteamNetworkingUtils.EstimatePingTo((Steamworks.Data.NetPingLocation)Steamworks.Data.NetPingLocation.TryParseFromString(location));

					if (ping < 0 || ping > 300) {
						pingIcon.sprite = pingIconSprites[3];
					}
					else if (ping > 100) {
						pingIcon.sprite = pingIconSprites[2];
					}
					else if (ping > 50) {
						pingIcon.sprite = pingIconSprites[1];
					}
					else {
						pingIcon.sprite = pingIconSprites[0];
					}
					
					if (ping > 0) {
						pingCount.SetText("~" + ping.ToString() + " ms");
					}
					else {
						pingCount.SetText("N/A");
					}
				}
				else {
					pingIcon.sprite = pingIconSprites[3];
					pingCount.SetText("N/A");
				}
			}
		}
		catch (System.Exception e) {
			Debug.Log("refresh failed");
			Debug.Log(e.ToString());
			JoinLobbyButton.interactable = false;
			cg.alpha = 0;
		}
    }

    private async void JoinLobby_Handler() {
		lobby.Refresh();

		try {
			bool canJoin = await LobbyManager.Instance.CanJoinLobby(lobby);
			if (canJoin) {
				LobbyManager.Instance.JoinLobby(lobby.Id);
			}
			else {  // the lobby is full or empty, remove it from the list
				LobbyManager.Instance.DisplayNotification(Lang.OnlineNotifications[Lang.onlineNotifications.invalidLobby][SettingsManager.Instance.language]);
				menu.UpdateLobbiesList();
			}
		}
		catch {
			LobbyManager.Instance.DisplayNotification(Lang.OnlineNotifications[Lang.onlineNotifications.invalidLobby][SettingsManager.Instance.language]);
			menu.UpdateLobbiesList();
		}
    }

	public void JoinLobby() {
		JoinLobby_Handler();
	}

	public void SetTextFieldValues(string name, string _location) {
		try {
			lobbyNameField.SetText(name);

			location = _location;

			if (string.IsNullOrWhiteSpace(lobbyNameField.GetParsedText())) {
				JoinLobbyButton.interactable = false;
				cg.alpha = 0;
			}
			else {
				JoinLobbyButton.interactable = true;
				cg.alpha = 1;
			}
		}
		catch (System.Exception e) {
			Debug.Log("Failed in the set text field values");
			Debug.Log(e);
		}
	}

	public void FadeInWarning() {
		if (pingIcon.sprite == pingIconSprites[2] || pingIcon.sprite == pingIconSprites[3]) {
			GetComponentInChildren<ConnectionWarning>().FadeIn();
		}
	}

	public void FadeOutWarning() {
		GetComponentInChildren<ConnectionWarning>().FadeOut();
	}
}
