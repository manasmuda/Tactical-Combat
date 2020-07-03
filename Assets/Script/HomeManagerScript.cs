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

    // Start is called before the first frame update
    void Start()
    {
        logoutButton.onClick.AddListener(Logout);
        randomPlayButton.onClick.AddListener(RandomPlay);
        playWithFriendButton.onClick.AddListener(PlayWithFriend);
        ShowAdButton.onClick.AddListener(ShowAd);
        LoadAdButton.onClick.AddListener(LoadAd);
        TestGameButton.onClick.AddListener(TestGame);
        MyStrategiesButton.onClick.AddListener(MyStrategies);
        client = GameObject.Find("Client").GetComponent<Client>();
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
        client.FetchGameAndPlayerSession();
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
