using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject QuizPanel;
    [SerializeField] GameObject CreateAndJoinPanel;
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] GameObject WaitingPanel;
    [SerializeField] GameObject MultiplayerModeChoisePanel;
    [SerializeField] TextMeshProUGUI CountdownText;
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

    public void TapMultiPlayButton()
    {
        MainPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        PhotonManager.Instance.ConnectToServer();
    }

    public void OpenMultiplayerChoiseModePanel()
    {
        LoadingPanel.SetActive(false);
        MultiplayerModeChoisePanel.SetActive(true);
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
        LoadingPanel.SetActive(false);
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
        WaitingPanel.SetActive(false);
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
        CreateAndJoinPanel.SetActive(true);
    }

    public void PlayAnotherPlayer()
    {
        MultiplayerModeChoisePanel.SetActive(false);
        LoadingPanel.SetActive(true);
        PhotonManager.Instance.JoinRandomRoom();
    }

    [PunRPC]
    public void StartCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        CountdownText.gameObject.SetActive(true);
        int countdown = 5;
        while (countdown > 0)
        {
            CountdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }
        OpenQuizPanel();
    }
}
