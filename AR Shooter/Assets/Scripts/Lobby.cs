using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loading;
    [SerializeField] private Button[] multiplayerButtons;

    private void Start()
    {
        PhotonNetwork.NickName = "Player " + Random.Range(1, 999);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        FindRoom(PhotonNetwork.CountOfRooms > 0);
    }

    private void FindRoom(bool canFind)
    {
        multiplayerButtons[1].interactable = canFind;

        if (canFind) loading.SetActive(false);
        else loading.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        multiplayerButtons[0].interactable = true;
    }

    public static void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5 });
    }

    public static void JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(2);
    }
}
