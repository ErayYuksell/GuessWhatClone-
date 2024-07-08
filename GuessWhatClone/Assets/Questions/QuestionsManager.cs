using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QuestionsManager : MonoBehaviour
{
    // Soru ve seçenekler ile ilgili alanlar
    [Header(" Question and options")]
    [SerializeField] TextMeshProUGUI Question;
    [SerializeField] Button[] options;
    [SerializeField] List<Question> questions = new List<Question>();
    List<Question> remainingQuestions;

    // Geçerli soru ve seçilen cevap
    [Header("Valid question and selected answer")]
    Question currentQuestion;
    int selectedAnswerIndex = -1;

    // Renkler ile ilgili alanlar
    [Header("Colors")]
    [SerializeField] Color correctColor = Color.green;
    [SerializeField] Color wrongColor = Color.red;
    [SerializeField] Color defaultColor = Color.white;

    // Sayaç ve puan ile ilgili alanlar
    [Header("Counter and Points")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timerText;
    int score = 0;
    float timeRemaining = 10f;
    bool isQuestionActive = false;


    void Start()
    {
        remainingQuestions = new List<Question>(questions);
        UpdateScoreText();
        ShowQuestion();
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
                // Zaman dolduðunda, yanlýþ cevap verilmiþ gibi iþlem yap
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

        if (remainingQuestions.Count == 0)
        {
            Debug.Log("Tüm sorular cevaplandý.");
            return;
        }

        int randomIndex = Random.Range(0, remainingQuestions.Count);
        currentQuestion = remainingQuestions[randomIndex];
        remainingQuestions.RemoveAt(randomIndex);

        Question.text = currentQuestion.questionText;

        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[i];
            int index = i; // Closure sorunu nedeniyle index'in yerel bir kopyasýný alýn
            options[i].onClick.RemoveAllListeners();
            options[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    public void OnAnswerSelected(int index)
    {
        selectedAnswerIndex = index;
        isQuestionActive = false;
        CheckAnswer();
    }

    public void CheckAnswer()
    {
        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct Answer");
            score += 10;
            UpdateScoreText();
            options[selectedAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);
        }
        else
        {
            Debug.Log("Wrong Answer");
            options[selectedAnswerIndex].GetComponent<Image>().DOColor(wrongColor, 0.5f).SetEase(Ease.InOutSine);
            options[currentQuestion.correctAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);
        }

        StartCoroutine(WaitAndShowNextQuestion(2));
    }

    public void CheckAnswerTimedOut()
    {
        Debug.Log("Time's up! Wrong Answer");
        options[currentQuestion.correctAnswerIndex].GetComponent<Image>().DOColor(correctColor, 0.5f).SetEase(Ease.InOutSine);

        StartCoroutine(WaitAndShowNextQuestion(2));
    }

    public void UpdateScoreText()
    {
        scoreText.text ="Score: " + score.ToString();
    }

    private void ResetOptionColors()
    {
        foreach (var option in options)
        {
            option.GetComponent<Image>().color = defaultColor;
        }
    }

    private IEnumerator WaitAndShowNextQuestion(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ShowQuestion();
    }
}
