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
    public CanvasGroup roomPanel;
    public CanvasGroup joinRoomPanel;
    public CanvasGroup homeMainPanel;
    public CanvasGroup ticketRewardPanel;

    public Button ShowAdButton;
    public Button LoadAdButton;

    public Button createRoomButton;
    public Button joinRoomButton;
    public Button backRoomButton;

    public Button joinButton;
    public Button backJoinButton;
    public InputField roomIdEdit;

    public Button ticketAdButton;
    public Button ticketCancelButton;


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

    public AdManager adManager;

    public Button adTestLoader;

    // Start is called before the first frame update
    void Start()
    {
        loginClient = GameObject.Find("LoginClient").GetComponent<LoginClient>();
        udcb = UpdateUserData;
        loginClient.GetUserData(udcb);
        logoutButton.onClick.AddListener(Logout);
        randomPlayButton.onClick.AddListener(RandomPlay);
        playWithFriendButton.onClick.AddListener(PlayWithFriend);
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        joinButton.onClick.AddListener(JoinWithId);
        backJoinButton.onClick.AddListener(BackJoin);
        backRoomButton.onClick.AddListener(BackRoom);
        ticketAdButton.onClick.AddListener(GetTicketsWithAd);
        ticketCancelButton.onClick.AddListener(GetTicketsCancel);
        ShowAdButton.onClick.AddListener(ShowAd);
        LoadAdButton.onClick.AddListener(LoadAd);
        TestGameButton.onClick.AddListener(TestGame);
        MyStrategiesButton.onClick.AddListener(MyStrategies);
        client = GameObject.Find("Client").GetComponent<Client>();
        adManager = GameObject.Find("AdManager").GetComponent<AdManager>();
        adManager.LoadRewardAd();
        hidePanel(ticketRewardPanel);
        adTestLoader.onClick.AddListener(adTestLoaderAction);
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
        //loginClient.getGoogleProfileImage(profileImageCallBack);
        if (userData.provider == "G")
        {
            Debug.Log("Provider is Google");
            loginClient.getGoogleProfileImage(profileImageCallBack);
        }
        else if(userData.provider == "FB")
        {
            Debug.Log("Provider is FB");
            loginClient.getFBProfileImage(profileImageCallBack);
            
        }
    }

    public void profileImageCallBack(Texture2D texture)
    {
        Debug.Log("Image CallBack");
        profileImage.sprite=Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0f, 0f));
    }

    public void adTestLoaderAction()
    {
        for (int i = 0; i < 10; i++) { adManager.LoadAd(); adManager.LoadRewardAd(); }
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
        Dictionary<string, string> payLoad = new Dictionary<string, string> { };
        payLoad.Add("GameType", "1");
        client.FetchGameAndPlayerSession(payLoad);
    }

    void TestGame()
    {
        client.ConnectWithPlayerId(playerIdInput.text.ToString());
    }

    void PlayWithFriend()
    {
        if (loginClient.curUserData.tickets > 0)
        {
            hidePanel(homePanel);
            hidePanel(joinRoomPanel);
            showPanel(roomPanel);
        }
        else
        {
            showPanel(homePanel);
            hidePanel(joinRoomPanel);
            hidePanel(roomPanel);
            inactivePanel(homeMainPanel);
            inactivePanel(homePanel);
            showPanel(ticketRewardPanel);
        }
    }

    void CreateRoom()
    {
        Dictionary<string, string> payLoad = new Dictionary<string, string> { };
        payLoad.Add("GameType", "2");
        payLoad.Add("PlayerType", "1");
        client.FetchGameAndPlayerSession(payLoad);
    }

    void JoinRoom()
    {
        hidePanel(roomPanel);
        hidePanel(homePanel);
        showPanel(joinRoomPanel);
    }

    void JoinWithId()
    {
        Dictionary<string, string> payLoad = new Dictionary<string, string> { };
        payLoad.Add("GameType", "2");
        payLoad.Add("PlayerType", "2");
        payLoad.Add("RoomId", roomIdEdit.text);
        roomIdEdit.text = "";
        client.FetchGameAndPlayerSession(payLoad);
    }

    void BackRoom()
    {
        hidePanel(roomPanel);
        hidePanel(joinRoomPanel);
        showPanel(homePanel);
    }

    void BackJoin()
    {
        hidePanel(joinRoomPanel);
        hidePanel(homePanel);
        showPanel(roomPanel);
    }

    void GetTicketsWithAd()
    {
        adManager.DisplayRewardAd(RewardTicketCallBack);
    }

    void RewardTicketCallBack(bool x)
    {
        showPanel(homePanel);
        hidePanel(joinRoomPanel);
        hidePanel(roomPanel);
        hidePanel(ticketRewardPanel);
        adManager.LoadRewardAd();
        if (x)
        {
            loginClient.curUserData.tickets = loginClient.curUserData.tickets + 1;
            loginClient.UpdateUserData();
            hidePanel(homePanel);
            showPanel(roomPanel);
            
        }
    }

    void GetTicketsCancel()
    {
        showPanel(homeMainPanel);
        showPanel(homePanel);
        hidePanel(roomPanel);
        hidePanel(joinRoomPanel);
        hidePanel(ticketRewardPanel);
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
        showPanel(loadingPanel);
        hidePanel(homeMainPanel);
    }

    public void hideLoading()
    {
        hidePanel(loadingPanel);
        showPanel(homeMainPanel);
    }

    public void hidePanel(CanvasGroup x)
    {
        x.alpha = 0f;
        x.blocksRaycasts = false;
    }

    public void showPanel(CanvasGroup x)
    {
        x.alpha = 1f;
        x.blocksRaycasts = true;
    }

    public void inactivePanel(CanvasGroup x)
    {
        x.alpha = 0.5f;
        x.blocksRaycasts = false;
    }
}
