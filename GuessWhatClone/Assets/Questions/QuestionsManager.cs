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
    [SerializeField] List<Question> remainingQuestions;

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
    bool isQuestionActive = false; // zamani kontrol etmek icin olan bir bool 
    bool isMultiplayer = false; // multiplayer ve single arasi gecis icin gereken bool 

    private PhotonView photonView;
    private bool[] playersAnswered = new bool[2];
    private int correctAnswerIndex;
    private int[] playerSelections = new int[2];

    int player1_score = 0;
    int player2_score = 0;

    [SerializeField] RectTransform lineTransform;
    [SerializeField] RectTransform player1_Image;
    [SerializeField] RectTransform player2_Image;

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
            Debug.LogError("No remaining questions.");
            return;
        }

        if (isMultiplayer) // bazen sorular 2 cihazda farkli oluyordu bunu engellemek icin masterClient soruyu seciyor daha sonra tum cihazlara gosretiyor 
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
        SetOptionsInteractable(false);  // Seçenek seçilince butonlarý kapat 

        if (isMultiplayer)
        {
            ShowSelectedAnswer(PhotonNetwork.LocalPlayer.ActorNumber, index);  // Kendi seçimini göster
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

    void ShowSelectedAnswer(int playerID, int index) // oyuncu kendi cihazinda yaptigi secimi direkt olarak gorecek 
    {
        string playerIconName = (playerID == 1) ? "Player1Icon" : "Player2Icon"; // player id ye gore image secimi yapiyor daha sonra tiklanan secenekteki image i aciyor 
        options[index].transform.Find(playerIconName).gameObject.SetActive(true);
    }

    void ShowAllSelectedAnswers()  // oyuncularin birbirinden kopya cekmemesi icin iki oyuncuda secenekleri sectikten sonra secilen secenekler gozukecek 
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
        // Correct Answer Color for both players
        options[correctAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);

        // Tek oyunculu modda skor artýrýmý
        if (!isMultiplayer)
        {
            if (playerSelections[0] == correctAnswerIndex)
            {
                score += 10;
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
            // Multiplayer modunda oyuncu 1'in cevabý
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                if (playerSelections[0] == correctAnswerIndex)
                {
                    score += 10;
                    UpdateScoreText();
                    MovePlayersImage(1);
                }
                else
                {
                    options[playerSelections[0]].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
                }
            }

            // Multiplayer modunda oyuncu 2'nin cevabý
            if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                if (playerSelections[1] == correctAnswerIndex)
                {
                    score += 10;
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
                photonView.RPC("RPC_MoveTheImage", RpcTarget.All, playerID, new Vector2(50, 0)); // RPC fonksiyonu paremetresi olarak direkt olarak rectTransform veremedim hata verdi sanirim direkt olarak image cart curt da veremem 
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
