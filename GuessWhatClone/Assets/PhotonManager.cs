using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;
    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings(); // Photon sunucusuna baðlanmamý saðlar 
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        GameManager.Instance.OpenMultiplayerChoiseModePanel();
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text); // Private Room kurar
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinRoom(joinInput.text); // Private Rooma katýlýr 
    }

    public void JoinRandomRoom() // Rastgele bir odaya katýlmayý saðlar
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join Random Room Failed");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        GameManager.Instance.OpenWaitingPanel();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New Player Joined Room");
        GameManager.Instance.OpenTickPlayer2();
        CheckPlayersInRoom();
    }

    private void CheckPlayersInRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            photonView.RPC("RPC_StartCountdown", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_StartCountdown()
    {
        GameManager.Instance.StartCountdown();
    }
}
