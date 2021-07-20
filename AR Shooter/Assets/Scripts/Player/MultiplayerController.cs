using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections;

public class MultiplayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI textPlayersCount;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject hubCam;
    [SerializeField] private GameObject hubUI;
    [SerializeField] private GameObject aliveStateUI;

    internal static Transform ownerPlayer;
    private NetworkedPlayerStatus ownerPlayerScript;
    private int CountOfPlayers { get { return PhotonNetwork.PlayerList.Length; } }

    public static bool isReady = false;
    private static Vector3 startHostPos = Vector3.zero;
    private static int readyPlayers = 0;

    private void Start()
    {
        textPlayersCount.text = "players: " + CountOfPlayers;

        if (CountOfPlayers > 1)
        {
            readyButton.interactable = true;
        }

        if (PhotonNetwork.IsMasterClient) StartCoroutine(StartHub());
    }

    private IEnumerator StartHub()
    {
        yield return new WaitUntil(AllPlayersReady);

        photonView.RPC("StartRoom", RpcTarget.All, hubCam.transform.position);
    }

    private bool AllPlayersReady()
    {
        return readyPlayers == CountOfPlayers;
    }

    public void Leave()
    {
        PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        if (isReady) photonView.RPC("NewPlayer", RpcTarget.All, -1);

        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        textPlayersCount.text = "players: " + CountOfPlayers;

        if (CountOfPlayers > 1)
        {
            readyButton.interactable = true;
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        textPlayersCount.text = "players: " + CountOfPlayers;

        if (CountOfPlayers < 2)
        {
            readyButton.interactable = false;
        }
    }

    [PunRPC]
    private IEnumerator StartRoom(Vector3 pos)
    {
        hubUI.SetActive(false);
        aliveStateUI.SetActive(true);
        Vector3 currentPos = hubCam.transform.position;

        Destroy(hubCam.transform.parent.gameObject);

        yield return new WaitUntil(HubARDeleted);

        PhotonNetwork.CurrentRoom.IsOpen = false;

        ownerPlayer = PhotonNetwork.Instantiate(player.name, pos + currentPos - startHostPos, Quaternion.identity).transform.Find("Cam Pos");
        if (ownerPlayer.parent.gameObject.TryGetComponent(out NetworkedPlayerStatus ownerPlayerScript)) this.ownerPlayerScript = ownerPlayerScript;
    }

    private bool HubARDeleted()
    {
        return hubCam == null;
    }

    public void Ready(bool ready)
    {
        if (isReady != ready)
        {
            isReady = ready;

            photonView.RPC("NewPlayer", RpcTarget.All, ready? 1 : -1);
        }
    }

    [PunRPC]
    private void NewPlayer(int delta)
    {
        readyPlayers += delta;

        startHostPos = hubCam.transform.position;
    }

    public void Respawn()
    {
        if (ownerPlayerScript != null)
        {
            ownerPlayerScript.Respawn();
        }
        else if (ownerPlayer.parent.gameObject.TryGetComponent(out NetworkedPlayerStatus ownerPlayerScript)) this.ownerPlayerScript = ownerPlayerScript;
    }
}
