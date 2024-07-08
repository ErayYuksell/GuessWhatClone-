using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject QuizPanel;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void TapPlayButton()
    {
        MainPanel.SetActive(false);
        QuizPanel.SetActive(true);
    }
}
