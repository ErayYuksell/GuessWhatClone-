using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class QuestionsManager : MonoBehaviourPunCallbacks
{
    public static QuestionsManager Instance;

    [Header("Question and options")]
    [SerializeField] TextMeshProUGUI Question;
    [SerializeField] Button[] options;
    [SerializeField] List<Question> questions = new List<Question>();
    List<Question> remainingQuestions;

    [Header("Valid question and selected answer")]
    Question currentQuestion;
    int selectedAnswerIndex = -1;

    [Header("Colors")]
    [SerializeField] Color correctColor = Color.green;
    [SerializeField] Color wrongColor = Color.red;
    [SerializeField] Color defaultColor = Color.white;

    [Header("Counter and Points")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timerText;
    int score = 0;
    int score1 = 0;
    int score2 = 0;
    float timeRemaining = 10f;
    bool isQuestionActive = false;
    bool isMultiplayer = false;

    private PhotonView photonView;
    private bool[] playersAnswered = new bool[2];
    private int correctAnswerIndex;
    private int[] playerSelections = new int[2];

    [SerializeField] RectTransform lineTransform;
    [SerializeField] RectTransform player1_Image;
    [SerializeField] RectTransform player2_Image;

    [SerializeField] TextMeshProUGUI winningPlayerText;
    [SerializeField] TextMeshProUGUI winningScoreText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            photonView = GetComponent<PhotonView>();
            if (photonView == null) 
            {
                Debug.LogError("PhotonView NULL");
                photonView = gameObject.AddComponent<PhotonView>();
                photonView.ViewID = 2; // Doðru viewID'yi ayarlayýn
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        if (questions != null && questions.Count > 0)
        {
            remainingQuestions = new List<Question>(questions);
            UpdateScoreText();
            if (remainingQuestions.Count > 0)
            {
                if (!isMultiplayer)
                {
                    player2_Image.gameObject.SetActive(false);
                    ShowQuestion();
                }
            }
            else
            {
                Debug.LogError("No questions available in the list.");
            }
        }
        else
        {
            Debug.LogError("Questions list is null or empty.");
        }
    }

    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        if (isQuestionActive)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = Mathf.Clamp(timeRemaining, 0, 10).ToString("F0");

            if (timeRemaining <= 0)
            {
                isQuestionActive = false;
                CheckAnswerTimedOut();
            }
        }
    }

    public void ShowQuestion()
    {
        if (remainingQuestions == null || remainingQuestions.Count == 0)
        {
            //ShowWinningPanel();
            return;
        }

        if (isMultiplayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int randomIndex = Random.Range(0, remainingQuestions.Count);
                currentQuestion = remainingQuestions[randomIndex];
                remainingQuestions.RemoveAt(randomIndex);

                if (currentQuestion == null)
                {
                    Debug.LogError("Current question is null.");
                    return;
                }

                correctAnswerIndex = currentQuestion.correctAnswerIndex;
                photonView.RPC("RPC_UpdateQuestionUI", RpcTarget.All, currentQuestion.questionText, currentQuestion.answers, correctAnswerIndex);
            }
        }
        else
        {
            int randomIndex = Random.Range(0, remainingQuestions.Count);
            currentQuestion = remainingQuestions[randomIndex];
            remainingQuestions.RemoveAt(randomIndex);

            if (currentQuestion == null)
            {
                Debug.LogError("Current question is null.");
                return;
            }

            correctAnswerIndex = currentQuestion.correctAnswerIndex;
            UpdateQuestionUI(currentQuestion.questionText, currentQuestion.answers, correctAnswerIndex);
        }
    }

    [PunRPC]
    void RPC_UpdateQuestionUI(string questionText, string[] answers, int correctIndex)
    {
        UpdateQuestionUI(questionText, answers, correctIndex);
    }

    void UpdateQuestionUI(string questionText, string[] answers, int correctIndex)
    {
        ResetOptionColors();
        timeRemaining = 10f;
        isQuestionActive = true;

        Question.text = questionText;
        correctAnswerIndex = correctIndex;

        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
            int index = i;
            options[i].onClick.RemoveAllListeners();
            options[i].onClick.AddListener(() => OnAnswerSelected(index));
            options[i].transform.Find("Player1Icon").gameObject.SetActive(false);
            options[i].transform.Find("Player2Icon").gameObject.SetActive(false);
            options[i].interactable = true;
        }

        playersAnswered[0] = false;
        playersAnswered[1] = false;
    }

    public void OnAnswerSelected(int index)
    {
        selectedAnswerIndex = index;
        isQuestionActive = false;
        SetOptionsInteractable(false);

        if (isMultiplayer)
        {
            ShowSelectedAnswer(PhotonNetwork.LocalPlayer.ActorNumber, index);
            photonView.RPC("RPC_PlayerAnswered", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, index);
        }
        else
        {
            playersAnswered[0] = true;
            playerSelections[0] = index;
            CheckAnswers();
        }
    }

    [PunRPC]
    void RPC_PlayerAnswered(int playerID, int index)
    {
        playersAnswered[playerID - 1] = true;
        playerSelections[playerID - 1] = index;

        if (playersAnswered[0] && playersAnswered[1])
        {
            ShowAllSelectedAnswers();
            CheckAnswers();
        }
    }

    void ShowSelectedAnswer(int playerID, int index)
    {
        string playerIconName = (playerID == 1) ? "Player1Icon" : "Player2Icon";
        options[index].transform.Find(playerIconName).gameObject.SetActive(true);
    }

    void ShowAllSelectedAnswers()
    {
        for (int i = 0; i < playerSelections.Length; i++)
        {
            int selectedIndex = playerSelections[i];
            if (selectedIndex >= 0 && selectedIndex < options.Length)
            {
                string playerIconName = (i == 0) ? "Player1Icon" : "Player2Icon";
                options[selectedIndex].transform.Find(playerIconName).gameObject.SetActive(true);
            }
        }
    }

    void CheckAnswers()
    {
        options[correctAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);

        if (!isMultiplayer)
        {
            if (playerSelections[0] == correctAnswerIndex)
            {
                score1 += 10;
                UpdateScoreText();
                MovePlayersImage(1);
            }
            else
            {
                options[playerSelections[0]].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
            }
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                if (playerSelections[0] == correctAnswerIndex)
                {
                    score += 10;
                    score1 += 10;
                    photonView.RPC("RPC_UpdateScore", RpcTarget.All, score1, score2);
                    UpdateScoreText();
                    MovePlayersImage(1);
                }
                else
                {
                    options[playerSelections[0]].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
                }
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                if (playerSelections[1] == correctAnswerIndex)
                {
                    score += 10;
                    score2 += 10;
                    photonView.RPC("RPC_UpdateScore", RpcTarget.All, score1, score2);
                    UpdateScoreText();
                    MovePlayersImage(2);
                }
                else
                {
                    options[playerSelections[1]].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
                }
            }
        }

        StartCoroutine(WaitAndShowNextQuestion(2));
    }


    public void MovePlayersImage(int playerID)
    {
        if (isMultiplayer)
        {
            if (playerID == 1)
            {
                photonView.RPC("RPC_MoveTheImage", RpcTarget.All, playerID, new Vector2(50, 0));
            }
            else if (playerID == 2)
            {
                photonView.RPC("RPC_MoveTheImage", RpcTarget.All, playerID, new Vector2(50, 0));
            }
        }
        else
        {
            Vector2 currentPosition = player1_Image.anchoredPosition;
            Vector2 targetPosition = currentPosition + new Vector2(50, 0);

            player1_Image.DOAnchorPos(targetPosition, 1f);
        }
    }

    [PunRPC]
    public void RPC_MoveTheImage(int playerID, Vector2 offset)
    {
        RectTransform playerImage = null;

        if (playerID == 1)
        {
            playerImage = player1_Image;
        }
        else if (playerID == 2)
        {
            playerImage = player2_Image;
        }

        if (playerImage != null)
        {
            Vector2 currentPosition = playerImage.anchoredPosition;
            Vector2 targetPosition = currentPosition + offset;

            playerImage.DOAnchorPos(targetPosition, 1f);
        }
    }

    public void CheckAnswerTimedOut()
    {
        Debug.Log("Time's up! Wrong Answer");
        options[correctAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);

        StartCoroutine(WaitAndShowNextQuestion(2));
    }

    public void UpdateScoreText()
    {
        if (scoreText != null)
        {
            if (isMultiplayer)
            {
                scoreText.text = "Score:  " + score.ToString(); // score degiskeni multiplayer da neden var cunku farkli cihazlarda sadece sokru kendilerine gosteriyorum diger cihazlara gostermedigim icin player 1 sa score1 dememe gerek yok 
            }
            else
            {
                scoreText.text = "Score: " + score1.ToString();
            }
        }
        else
        {
            Debug.LogError("Score TextMeshProUGUI is not assigned.");
        }
    }

    [PunRPC]
    void RPC_UpdateScore(int updatedScore1, int updatedScore2)
    {
        score1 = updatedScore1;
        score2 = updatedScore2;
    }


    private void ResetOptionColors()
    {
        foreach (var option in options)
        {
            if (option != null)
            {
                option.GetComponent<Image>().color = defaultColor;

                option.transform.Find("Player1Icon").gameObject.SetActive(false);
                option.transform.Find("Player2Icon").gameObject.SetActive(false);
            }
        }
    }

    private void SetOptionsInteractable(bool interactable)
    {
        foreach (var option in options)
        {
            option.interactable = interactable;
        }
    }

    private IEnumerator WaitAndShowNextQuestion(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (remainingQuestions.Count > 0)
        {
            ShowQuestion();
        }
        else
        {
            photonView.RPC("RPC_ShowWinningPanel", RpcTarget.All);
        }
    }

    public void SetMultiplayer(bool isMultiplayer)
    {
        this.isMultiplayer = isMultiplayer;
        if (isMultiplayer)
        {
            photonView.RPC("RPC_RequestStartQuiz", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RPC_RequestStartQuiz()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (photonView == null)
            {
                photonView = gameObject.AddComponent<PhotonView>();
                photonView.ViewID = 2; // Doðru viewID'yi ayarlayýn
            }
            ShowQuestion();
        }
    }

    [PunRPC]
    public void RPC_ShowWinningPanel() // Winning Panel
    {
        GameManager.Instance.OpenWinningPanel();
        if (isMultiplayer)
        {
            if (score1 > score2)
            {
                winningPlayerText.text = "Player 1";
                winningScoreText.text = score1.ToString();
            }
            else if (score2 > score1)
            {
                winningPlayerText.text = "Player 2";
                winningScoreText.text = score2.ToString();
            }
            else
            {
                winningPlayerText.text = "Draw";
                winningScoreText.text = score1.ToString() + " - " + score2.ToString();
            }
        }
        else
        {
            winningPlayerText.text = "Username: "; // ekleme yapilacak
            winningScoreText.text = score1.ToString();
        }

    }
}
