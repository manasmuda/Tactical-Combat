using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MatchMakingManager : MonoBehaviour
{

    public CanvasGroup player1DataPanel;
    public CanvasGroup player2DataPanel;
    public CanvasGroup player2WaitingPanel;
    public Text player1Name;
    public Text player2Name;
    public Text waitingTimeText;
    public Image player1Image;
    public Image player2Image;

    public bool startTimer = false;
    public float waitingTime = 0.0f;

    private Client client;

    // Start is called before the first frame update
    void Start()
    {
        client = GameObject.Find("Client").GetComponent<Client>();
        showPanel(player1DataPanel);
        showPanel(player2WaitingPanel);
        hidePanel(player2DataPanel);
        if (client.playerJoined)
        {
            startTimer = true;
            waitingTime = client.playerWaitTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer)
        {
            if (waitingTime <= 0.0f)
            {
                waitingTime = 0.0f;
                startTimer = false;
                if (client.playerJoined)
                {
                    client.playerJoined = false;
                    Debug.Log("No other players available");
                    SceneManager.LoadScene("Home");
                }
            }
            else
            {
                waitingTime = waitingTime - Time.deltaTime;
            }
            waitingTimeText.text = "Estimated Waiting Time: " + (Convert.ToInt32(waitingTime)).ToString();
        }
    }

    void hidePanel(CanvasGroup x)
    {
        x.alpha = 0f;
        x.blocksRaycasts = false;
    }

    void showPanel(CanvasGroup x)
    {
        x.alpha = 1f;
        x.blocksRaycasts = true;
    }
}
