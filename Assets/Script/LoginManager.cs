using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{

    public Button facebookLoginButton;
    public Button googleLoginButton;

    public CanvasGroup loadingPanel;
    public CanvasGroup loginPanel;

    public LoginClient loginClient;

    public Button goHome;

    // Start is called before the first frame update
    void Start()
    {
        facebookLoginButton.onClick.AddListener(FacebookLogin);
        googleLoginButton.onClick.AddListener(GoogleLogin);
        goHome.onClick.AddListener(GoHome);
        loginClient = GameObject.Find("LoginClient").GetComponent<LoginClient>();
    }

    public void FacebookLogin()
    {
        loginClient.FaceBookLogin();
    }

    public void GoogleLogin()
    {
        loginClient.GoogleLogin();
    }

    public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }

    // Update is called once per frame
    void Update()
    {
        if (loginClient.loading)
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
        loginPanel.alpha = 0f;
        loginPanel.blocksRaycasts = false;
        loadingPanel.alpha = 1f;
        loadingPanel.blocksRaycasts = true;
    }

    public void hideLoading()
    {
        loginPanel.alpha = 1f;
        loginPanel.blocksRaycasts = true;
        loadingPanel.alpha = 0f;
        loadingPanel.blocksRaycasts = false;
    }
}
