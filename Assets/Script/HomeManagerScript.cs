using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeManagerScript : MonoBehaviour
{
    public Button randomPlayButton;
    public Button logoutButton;
    public Button playWithFriendButton;

    public InputField playerIdInput;
    public Button TestGameButton;

    public CanvasGroup loadingPanel;
    public CanvasGroup homePanel;

    public Button ShowAdButton;
    public Button LoadAdButton;

    public Button MyStrategiesButton;

    private Client client;
    private LoginClient loginClient;

    private LoginClient.UserDataCB udcb;

    public Text profileNameText;
    public Image profileImage;

    public Text levelText;
    public Image levelImage;
    public Slider levelPoints;
    public Text ticketsText;

    public List<Sprite> LevelSprites;

    // Start is called before the first frame update
    void Start()
    {
        loginClient = GameObject.Find("LoginClient").GetComponent<LoginClient>();
        udcb = UpdateUserData;
        if (loginClient.curUserData == null)
        {
            loginClient.GetUserData(udcb);
        }
        else
        {
            UpdateUserData(loginClient.curUserData);
        }
        logoutButton.onClick.AddListener(Logout);
        randomPlayButton.onClick.AddListener(RandomPlay);
        playWithFriendButton.onClick.AddListener(PlayWithFriend);
        ShowAdButton.onClick.AddListener(ShowAd);
        LoadAdButton.onClick.AddListener(LoadAd);
        TestGameButton.onClick.AddListener(TestGame);
        MyStrategiesButton.onClick.AddListener(MyStrategies);
        client = GameObject.Find("Client").GetComponent<Client>();
    }

    public void UpdateUserData(Users userData)
    {
        profileNameText.text = userData.name;
        levelText.text = userData.level;
        ticketsText.text = "Tickets: "+Convert.ToString(userData.tickets);
        levelPoints.value = userData.levelPoints;
        if (userData.level == "NOVICE")
        {
            levelImage.sprite = LevelSprites[0];
        }
        else if (userData.level == "AMATEUR")
        {
            levelImage.sprite = LevelSprites[1];
        }
        else if (userData.level == "PRO")
        {
            levelImage.sprite = LevelSprites[2];
        }
    }

    void Logout()
    {
        GameObject.Find("LoginClient").GetComponent<LoginClient>().Logout();
    }

    void ShowAd()
    {
        GameObject.Find("AdManager").GetComponent<AdManager>().DisplayAd();
    }

    void LoadAd(){
        GameObject.Find("AdManager").GetComponent<AdManager>().LoadAd();
    }

    void RandomPlay()
    {
        client.FetchGameAndPlayerSession();
    }

    void TestGame()
    {
        client.ConnectWithPlayerId(playerIdInput.text.ToString());
    }

    void PlayWithFriend()
    {
        //client.FetchGameAndPlayerSession();
    }

    void MyStrategies() 
    {
        SceneManager.LoadScene("StrategyMaker");
    } 

    // Update is called once per frame
    void Update()
    {
        if (client.loading)
        {
            showLoading();
        }
        else
        {
            hideLoading();
        }
    }

    public void showLoading()
    {
        homePanel.alpha = 0f;
        homePanel.blocksRaycasts = false;
        loadingPanel.alpha = 1f;
        loadingPanel.blocksRaycasts = true;
    }

    public void hideLoading()
    {
        homePanel.alpha = 1f;
        homePanel.blocksRaycasts = true;
        loadingPanel.alpha = 0f;
        loadingPanel.blocksRaycasts = false;
    }
}
