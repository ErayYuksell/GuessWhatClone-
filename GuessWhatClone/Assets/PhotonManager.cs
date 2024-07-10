using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks // bazi callback fonksyionlarini kullanabilmek icin buna cevirdik 
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
        PhotonNetwork.ConnectUsingSettings(); // Photon sucunusuna baglanmami saglar 
    }

    public override void OnConnectedToMaster() // bellir bir olay gerceklestiginde photon tarafindan otamatik olarak cagrilan bir fonksiyon sunucuya basarili bir sekilde baglandiginda icerisinde kodlari calistirir 
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        GameManager.Instance.OpenCreateJoinPanel();
    }


    //public void CreateRoom()
    //{
    //    PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    //}

    //public void JoinRandomRoom() // bu sekilde var olan random bir odaya da girebilirim 
    //{
    //    PhotonNetwork.JoinRandomRoom();
    //}

    // ust taraftaki oda olusturma ve odaya girme daha genel dusun rastgele odalara girebilir ya da olusturabilirsin 
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text);
    }
    public void JoinLobby()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }
    public override void OnJoinedRoom() // odaya katildigimizda otamatik olarak cagrilir 
    {
        Debug.Log("Joined Room");
        //PhotonNetwork.LoadLevel("GameScene"); // cok oyunculu bir sahne yuklemek istedigimde bunu kullanmam lazim 
        GameManager.Instance.OpenWaitingPanel();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) // yeni bir oyuncu odaya girdiginde otamatik calisir 
    {
        Debug.Log("New Player Joined Room");
        GameManager.Instance.OpenTickPlayer2();
        CheckPlayersInRoom();
    }

    private void CheckPlayersInRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            photonView.RPC("RPC_OpenQuizPanel", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_OpenQuizPanel()
    {
        GameManager.Instance.OpenQuizPanel();
    }

    //public override void OnJoinRandomFailed(short returnCode, string message)
    //{
    //    Debug.Log("Join Random Room Failed");
    //    CreateRoom();
    //}

}



