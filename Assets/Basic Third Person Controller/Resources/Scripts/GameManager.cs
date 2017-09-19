using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine.UI;


public class GameManager : PunBehaviour {

	public GameObject position1;
	GameObject localPlayer = null;
	ExitGames.Client.Photon.Hashtable playerCustomProperties;
	// Use this for initialization
	void Start () {


		if (PhotonNetwork.isMasterClient) {
			photonView.RPC ("StartGame", PhotonTargets.All);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	[PunRPC]
	void StartGame(){
		InstantiatePlayer ();	
	}

	void InstantiatePlayer()
	{

		localPlayer = PhotonNetwork.Instantiate("Soldier", position1.transform.position, Quaternion.identity, 0);

		localPlayer.GetComponent<PlayerBehaviour>().enabled = true;
		localPlayer.GetComponent<CameraBehaviour>().enabled = true;
		localPlayer.transform.Find ("Camera").gameObject.SetActive (true);
	}
}
