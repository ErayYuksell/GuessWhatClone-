using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject mainPanel; // Main panel

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI
    public void LoginScreen() // Back button
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
        mainPanel.SetActive(false);
    }

    public void RegisterScreen() // Register button
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void MainScreen() // After login
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void PassButton()
    {
        registerUI.SetActive(false);
        loginUI.SetActive(true);
    }
}
