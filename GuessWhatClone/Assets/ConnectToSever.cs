using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectToSever : MonoBehaviourPunCallbacks // bazi callback fonksyionlarini kullanabilmek icin buna cevirdik 
{
   
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Photon sucunusuna baglanmami saglar 
    }

    public override void OnConnectedToMaster() // bellir bir olay gerceklestiginde photon tarafindan otamatik olarak cagrilan bir fonksiyon sunucuya basarili bir sekilde baglandiginda icerisinde kodlari calistirir 
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
