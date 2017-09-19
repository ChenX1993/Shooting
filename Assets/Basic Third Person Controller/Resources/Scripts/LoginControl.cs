using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;


public class LoginControl : PunBehaviour {
	public Text connectionState;
	public Text play1;
	public Text play2;
	public Text username;

	PhotonView pView;
	ExitGames.Client.Photon.Hashtable costomProperties;
	// Use this for initialization
	void Start () {
		pView = GetComponent<PhotonView> ();
		connectionState.text = "";
	}
	
	// Update is called once per frame
	void Update () {
		connectionState.text = PhotonNetwork.connectionStateDetailed.ToString ();
	}


	public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps){
		foreach (PhotonPlayer p in PhotonNetwork.playerList) {
			if (p.IsLocal) {
				continue;
			}
			play2.text = p.NickName;
		}
	}

	public void ClickLogInButton(){							
		if (!PhotonNetwork.connected)
			PhotonNetwork.ConnectUsingSettings ("1.0");		
		if (username.text == "") {
			username.text = "tourist";
		}
		//Debug.Log (username.text);
		PhotonNetwork.player.name = username.text;
		//PlayerPrefs.SetString ("Username", username.text);	
	}

	public void ClickRoomButton(){
		RoomInfo[] roomInfos = PhotonNetwork.GetRoomList();	
		bool isRoomNameRepeat = false;

		foreach (RoomInfo info in roomInfos) {
			if ("room1" == info.Name) {
				isRoomNameRepeat = true;
				break;
			}
		}

		if (isRoomNameRepeat) {
			PhotonNetwork.JoinRoom ("room1");
			foreach (PhotonPlayer p in PhotonNetwork.playerList) {
				if (p.IsLocal) {
					play2.text = p.NickName;
				} else {
					play1.text = p.NickName;
				}
			}
		} else {
			PhotonNetwork.CreateRoom ("room1");
			play1.text = PhotonNetwork.player.NickName;
		}
	}

	public void ClickStartGameButton(){
		PhotonNetwork.room.open = false;
		if (PhotonNetwork.isMasterClient) {
			pView.RPC ("LoadGameScene", PhotonTargets.All, "Demo");
		}
	}

	public void Clicktest() {
		username.text = "12345";
	}

	[PunRPC]
	public void LoadGameScene(string sceneName){	
		PhotonNetwork.LoadLevel (sceneName);
	}
}
