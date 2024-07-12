using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Database;
using TMPro;
using System;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    private DatabaseReference dbReference;
    public bool IsFirebaseInitialized { get; private set; } = false;

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
            return;
        }
    }

    public void InitializeFirebase(FirebaseApp app)
    {
        dbReference = FirebaseDatabase.GetInstance(app).RootReference;
        if (dbReference == null) // bu kontrollerin hepsi bu hatalari aldigimdan dolayi var her turlu ise yariyor kontroller 
        {
            Debug.LogError("Failed to get the database reference. dbReference is null.");
        }
        else
        {
            Debug.Log("Firebase database reference is initialized successfully.");
        }
        IsFirebaseInitialized = true;
        Debug.Log("Firebase initialized successfully in DatabaseManager.");
    }

    public void CreateUser(string userId, string username)
    {
        if (!IsFirebaseInitialized)
        {
            Debug.LogWarning("Firebase is not initialized yet. Try again later.");
            return;
        }

        User newUser = new User(username, 0); // 0 baþlangýç skoruyla yeni kullanýcý oluþturuluyor
        string json = JsonUtility.ToJson(newUser);

        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User created successfully.");
            }
            else
            {
                Debug.LogError("Failed to create user: " + task.Exception);
            }
        });
    }


    public IEnumerator GetUsername(string userId, Action<string> onCallback)
    {
        var userNameData = dbReference.Child("users").Child(userId).Child("username").GetValueAsync();
        yield return new WaitUntil(() => userNameData.IsCompleted);

        if (userNameData.Exception != null)
        {
            Debug.LogError(userNameData.Exception);
        }
        else if (userNameData.Result.Value != null)
        {
            DataSnapshot snapshot = userNameData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }
        else
        {
            onCallback.Invoke(null);
        }
    }

    public IEnumerator GetScore(string userId, Action<int> onCallback)
    {
        var userScoreData = dbReference.Child("users").Child(userId).Child("score").GetValueAsync();
        yield return new WaitUntil(() => userScoreData.IsCompleted);

        if (userScoreData.Exception != null)
        {
            Debug.LogError(userScoreData.Exception);
        }
        else if (userScoreData.Result.Value != null)
        {
            DataSnapshot snapshot = userScoreData.Result;
            onCallback.Invoke(int.Parse(snapshot.Value.ToString()));
        }
        else
        {
            onCallback.Invoke(0);
        }
    }

    public void UpdateUsername(string userId, string newUsername)
    {
        dbReference.Child("users").Child(userId).Child("username").SetValueAsync(newUsername);
    }

    public void UpdateScore(string userId, int newScore)
    {
        dbReference.Child("users").Child(userId).Child("score").SetValueAsync(newScore);
    }
}
