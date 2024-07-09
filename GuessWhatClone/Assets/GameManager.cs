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
    public bool isConnected = false;
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
    }

    public void TapMultiPlayButton()
    {
        MainPanel.SetActive(false);
        isConnected = true;
        OpenLoadingPanel();
        PhotonManager.Instance.ConnectToServer();
    }

    public void OpenQuizPanel()
    {
        QuizPanel.SetActive(true);
        CreateAndJoinPanel.SetActive(false);
        WaitingPanel.SetActive(false);
    }

    public void OpenWaitingPanel()
    {
        WaitingPanel.SetActive(true);
        tick[0].gameObject.SetActive(true);
        CreateAndJoinPanel.SetActive(false);
    }

    public void OpenLoadingPanel()
    {
        LoadingPanel.SetActive(true);
    }

    public void OpenCreateJoinPanel()
    {
        CreateAndJoinPanel.SetActive(true);
        LoadingPanel.SetActive(false);
    }

    public void OpenTickPlayer2()
    {
        tick[1].gameObject.SetActive(true);
    }
}


