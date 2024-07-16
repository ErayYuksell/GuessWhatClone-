using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;
    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    PhotonView photonView;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                photonView = gameObject.AddComponent<PhotonView>();
                photonView.ViewID = 1; // Doðru viewID'yi ayarlayýn
            }
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
    //burdaki calisma mantigi su sekilde ana menude multiplayer a tikladigimizda loading panel acilir ve servera baglanir baglanma gerceklesince otamatik olarak OnConnectToMaster calisir onun icindeki fonksiyonlada MultiplayerChoisePanel acilir
    //Burada playerAnotherPlayera tikladik diyelim JoinRandomRoom calisir ve odaya katilinca otamatik olarak OnJoinedRoom calisir ve WaitingPanel acilir odaya biri katilincada otamatik olarak OnPlayerEnteredRoom calisir ve Kisi sayisini kontrol
    //ettikten sonra QuizPanel acilir

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
