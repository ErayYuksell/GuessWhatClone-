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
    float timeRemaining = 10f;
    bool isQuestionActive = false;
    bool isMultiplayer = false;

    private PhotonView photonView;
    private bool[] playersAnswered = new bool[2];
    private int correctAnswerIndex;
    private int[] playerSelections = new int[2];

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            photonView = GetComponent<PhotonView>();
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
        ResetOptionColors();
        timeRemaining = 10f;
        isQuestionActive = true;

        int randomIndex = Random.Range(0, remainingQuestions.Count);
        currentQuestion = remainingQuestions[randomIndex];
        remainingQuestions.RemoveAt(randomIndex);

        correctAnswerIndex = currentQuestion.correctAnswerIndex;

        if (currentQuestion == null)
        {
            Debug.LogError("Current question is null.");
            return;
        }

        photonView.RPC("RPC_UpdateQuestionUI", RpcTarget.All, currentQuestion.questionText, currentQuestion.answers, correctAnswerIndex);
    }

    [PunRPC]
    void RPC_UpdateQuestionUI(string questionText, string[] answers, int correctIndex)
    {
        UpdateQuestionUI(questionText, answers, correctIndex);
    }

    void UpdateQuestionUI(string questionText, string[] answers, int correctIndex)
    {
        Question.text = questionText;
        correctAnswerIndex = correctIndex;

        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
            int index = i;
            options[i].onClick.RemoveAllListeners();
            options[i].onClick.AddListener(() => OnAnswerSelected(index));
            // Gizle player iconlarýný baþlangýçta
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
        SetOptionsInteractable(false);  // Secenek secince butonlari kapa 
        photonView.RPC("RPC_PlayerAnswered", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, index);
    }

    [PunRPC]
    void RPC_PlayerAnswered(int playerID, int index)
    {
        playersAnswered[playerID - 1] = true;
        playerSelections[playerID - 1] = index;

        string playerIconName = (playerID == 1) ? "Player1Icon" : "Player2Icon";
        options[index].transform.Find(playerIconName).gameObject.SetActive(true);

        if (playersAnswered[0] && playersAnswered[1])
        {
            CheckAnswers();
        }
    }

    void CheckAnswers()
    {
        // Correct Answer Color for both players
        options[correctAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);

        // Player 1's Answer
        if (playerSelections[0] != correctAnswerIndex)
        {
            options[playerSelections[0]].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
        }

        // Player 2's Answer
        if (playerSelections[1] != correctAnswerIndex)
        {
            options[playerSelections[1]].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
        }

        StartCoroutine(WaitAndShowNextQuestion(2));
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
            scoreText.text = "Score: " + score.ToString();
        }
        else
        {
            Debug.LogError("Score TextMeshProUGUI is not assigned.");
        }
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
            ShowQuestion();
        }
    }
}
