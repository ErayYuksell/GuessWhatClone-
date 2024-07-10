using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject QuizPanel;
    [SerializeField] GameObject CreateAndJoinPanel;
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] GameObject WaitingPanel;
    [SerializeField] GameObject MultiplayerModeChoisePanel;
    [SerializeField] Image[] tick = new Image[2];

    private void Awake()
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

    public void TapPlayButton()
    {
        MainPanel.SetActive(false);
        QuizPanel.SetActive(true);
        QuestionsManager.Instance.SetMultiplayer(false);  // Tek kiþilik mod için QuestionsManager'da multiplayer'ý false yap
        QuestionsManager.Instance.ShowQuestion();  // Sorularý baþlat
    }

    public void TapMultiPlayButton() // calisma mantigi olarak multiplayer butonuna tikladigimda loading panel acilir ve servere baglanmaya calisir photon managerdaki 
                                     // OnConnectMaster baglaninca otamatik calisir bu calisinca lobiye katiliriz lobby katilinca otamatik OnJoinedLobby calisir ve CreateAndJoinPanel acilir 
    {
        MainPanel.SetActive(false);
        MultiplayerModeChoisePanel.SetActive(true);
        //LoadingPanel.SetActive(true);
        //PhotonManager.Instance.ConnectToServer();
    }

    public void OpenQuizPanel()
    {
        QuizPanel.SetActive(true);
        CreateAndJoinPanel.SetActive(false);
        WaitingPanel.SetActive(false);
        QuestionsManager.Instance.SetMultiplayer(true);  // Çok oyunculu mod için multiplayer'ý true yap
    }

    public void OpenWaitingPanel()
    {
        WaitingPanel.SetActive(true);
        tick[0].gameObject.SetActive(true);
        CreateAndJoinPanel.SetActive(false);
    }

    public void WaitingBackButton()
    {
        WaitingPanel.SetActive(false);
        CreateAndJoinPanel.SetActive(true);
    }

    public void OpenCreateJoinPanel()
    {
        CreateAndJoinPanel.SetActive(true);
        LoadingPanel.SetActive(false);
    }

    public void LobbyBackButton()
    {
        CreateAndJoinPanel.SetActive(false);
        MainPanel.SetActive(true);
    }

    public void OpenTickPlayer2()
    {
        tick[1].gameObject.SetActive(true);
    }

    // MultiplayerModeChoisePanel

    public void PlayPrivateRoom()
    {
        MultiplayerModeChoisePanel.SetActive(false);
        LoadingPanel.SetActive(true);
        PhotonManager.Instance.ConnectToServer();
    }

    public void PlayAnotherPlayer()
    {
       
    }
}
