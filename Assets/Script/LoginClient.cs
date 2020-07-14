using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoSync;
using Amazon.CognitoSync.SyncManager;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using UnityEngine.SocialPlatforms;

public class LoginClient : MonoBehaviour
{
    private Dataset playerInfo;
    private CognitoSyncManager syncManager;
    public CognitoAWSCredentials awsCredentials;
    private CognitoAWSCredentials dbUsersCredentials;
    private DynamoDBContext dbContext;
    private AmazonDynamoDBClient dbClient;

    private bool sync = false;
    private bool loginPage = true;

    public bool loading = true;

    private int fbandcsInitiated = 0;

    private static LoginClient loginClientInstance = null;

    private StrategyManagerScript.LoadStrategyCallBack lscb;
    private StrategyManagerScript.SaveStrategyCallBack sscb;

    private int currentTempStrategy = -1;
    private Dictionary<string, int> currentStrategy;

    public delegate void UserDataCB(Users userData);
    public UserDataCB udcb;

    public delegate void PictureCallBack(Texture2D texture);
    public PictureCallBack pcb;

    public Texture2D fbTexture;
    public Texture2D googleTexture;

    public bool fbauth = false;
    public bool gauth = false;

    public Users curUserData;

    public bool firstTime = false;

    void Awake()
    {
        loading = true;
        sync = false;
        loginPage = true;
        fbandcsInitiated = 0;
        UnityInitializer.AttachToGameObject(this.gameObject);
        DontDestroyOnLoad(this.gameObject);

        if (loginClientInstance == null)
        {
            loginClientInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (!FB.IsInitialized)
        {
            FB.Init(FbInitCallBack);
        }
        else
        {
            fbandcsInitiated = fbandcsInitiated + 1;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Login Client Started");
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        awsCredentials = new CognitoAWSCredentials(
             "ap-south-1:0edf52b9-63a5-4fbe-9299-aa9c9413aa2d", // Identity pool ID
              RegionEndpoint.APSouth1 // Region
         );
        syncManager = new CognitoSyncManager(awsCredentials, RegionEndpoint.APSouth1);
        dbClient = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.APSouth1);
        dbContext = new DynamoDBContext(dbClient);
        playerInfo = syncManager.OpenOrCreateDataset("playerInfo");
        playerInfo.OnSyncSuccess += SyncSuccessCallBack;
        playerInfo.OnSyncFailure += HandleSyncFailure;
        fbandcsInitiated = fbandcsInitiated + 1;
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestEmail().RequestIdToken().RequestServerAuthCode(false).Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        //Social.localUser.Authenticate(GoogleLoginCallback);
    }

    void FbInitCallBack()
    {
        FB.ActivateApp();
        Debug.Log("FB has been initialized");
        fbandcsInitiated = fbandcsInitiated + 1;
    }

    public void FaceBookLogin()
    {
        if (!FB.IsLoggedIn)
        {
            FB.LogInWithReadPermissions(new List<string> { "public_profile", "email" }, FbLoginCallBack);
        }
    }

    void FbLoginCallBack(ILoginResult result)
    {
        if (result.Error == null)
        {
            Debug.Log("Facebook login successfull");
            fbauth = true;
            gauth = false;
            string uid = AccessToken.CurrentAccessToken.UserId;
            Debug.Log("uid is null");
            if (playerInfo != null && !string.IsNullOrEmpty(playerInfo.Get("uid")) && !uid.Equals(playerInfo.Get("uid")))
            {
                awsCredentials.Clear();
                playerInfo.Delete();
            }
            Debug.Log("aws credentials is null");
            awsCredentials.AddLogin("graph.facebook.com", AccessToken.CurrentAccessToken.TokenString);
            Debug.Log("playerInfo is null");
            playerInfo.SynchronizeOnConnectivity();
            loading = true;
        }
        else
        {
            Debug.Log("Facebook Login unsuccessfully:" + result.Error);
        }
    }

    public void GoogleLogin()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                Debug.Log("Google login successfull");
                ((GooglePlayGames.PlayGamesPlatform)Social.Active).SetGravityForPopups(Gravity.BOTTOM);
                fbauth = false;
                gauth = true;
                string uid = Social.localUser.id;
                Debug.Log("uid is null");
                Debug.Log("mPlatform.GetIdToken " + ((PlayGamesLocalUser)Social.localUser).mPlatform.GetIdToken());
                Debug.Log("Social.localUser...GetIdToken " + ((PlayGamesLocalUser)Social.localUser).GetIdToken());
                Debug.Log("PlayGamesPlatform...GetServerAuthCode " + PlayGamesPlatform.Instance.GetServerAuthCode());
                Debug.Log("PlayGamesPlatform...GetIdToken " + PlayGamesPlatform.Instance.GetIdToken());
                if (playerInfo != null && !string.IsNullOrEmpty(playerInfo.Get("uid")) && !uid.Equals(playerInfo.Get("uid")))
                {
                    awsCredentials.Clear();
                    playerInfo.Delete();
                }
                Debug.Log("aws credentials is null");
                string token = PlayGamesPlatform.Instance.GetIdToken();
                awsCredentials.AddLogin("accounts.google.com", token);
                Debug.Log(token);
                Debug.Log("playerInfo is null");
                playerInfo.SynchronizeOnConnectivity();
                loading = true;
            }
            else
            {
                Debug.Log("Google Login unsuccessfully");
            }
        });
    }

    private void HandleSyncFailure(object sender, SyncFailureEventArgs e)
    {
        Dataset dataset = sender as Dataset;
        if (dataset.Metadata != null)
        {
            Debug.Log("Sync failed for dataset : " + dataset.Metadata.DatasetName);
        }
        else
        {
            Debug.Log("Sync failed");
        }
        // Handle the error
        Debug.Log(e.Exception);
        Debug.Log(e.Exception.Message);
        Debug.Log(e.Exception.InnerException);
    }

    void SyncSuccessCallBack(object sender, SyncSuccessEventArgs e)
    {
        Debug.Log("Synchronize Successfull");
        List<Record> newRecords = e.UpdatedRecords;
        for (int k = 0; k < newRecords.Count; k++)
        {
            Debug.Log(newRecords[k].Key + " was updated: " + newRecords[k].Value);
        }
        if (string.IsNullOrEmpty(playerInfo.Get("uid")))
        {
            firstTime = true;
            Debug.Log("Player Data Not Updated");
            if (fbauth)
            {
                playerInfo.Put("uid", AccessToken.CurrentAccessToken.UserId);
                fetchFBName();
            }
            else if (gauth)
            {
                playerInfo.Put("uid", Social.localUser.id);
                playerInfo.Put("name", Social.localUser.userName);
                UploadUserData("G");
            }
        }
        else
        {
            UserData.name = playerInfo.Get("name");
            UserData.uid = playerInfo.Get("uid");
            if (fbauth)
                UserData.provider = "FB";
            else if (gauth)
                UserData.provider = "G";
            Debug.Log("Player Data Synchronized");
            sync = true;
            loginPage = true;
            loading = false;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.buildIndex);
    }

    public void Logout()
    {
        if (fbauth)
        {
            FB.LogOut();
        }
        else if (gauth)
        {
            PlayGamesPlatform.Instance.SignOut();
        }
        awsCredentials.ClearCredentials();
        UserData.name = null;
        UserData.provider = null;
        UserData.uid = null;
        SceneManager.LoadScene("Login");
    }

    public void fetchFBName()
    {
        FB.API("me?fields=first_name", HttpMethod.GET, NameCallBack);
    }

    public void getFBProfileImage(PictureCallBack pcb)
    {
        this.pcb = pcb;
        if (fbTexture != null)
        {
            this.pcb(fbTexture);
        }
        else
        {
            FB.API("me/picture?width=100&height=100", HttpMethod.GET, FBPictureCallBack);
        }    
    }

    void NameCallBack(IGraphResult result)
    {
        Debug.Log("Retrieved name from fb");
        IDictionary<string, object> profil = result.ResultDictionary;
        playerInfo.Put("name", profil["first_name"].ToString());
        UploadUserData("FB");
        //playerInfo.SynchronizeOnConnectivity();
    }

    void FBPictureCallBack(IGraphResult result)
    {
        Debug.Log("FB picture cb");
        fbTexture = result.Texture;
        Debug.Log("Fb texture loaded");
        this.pcb(result.Texture);
    }

    public void getGoogleProfileImage(PictureCallBack pcb)
    {
        Debug.Log("Goole picture request");
        this.pcb = pcb;
        if (googleTexture != null)
        {
            this.pcb(googleTexture);
        }
        else
        {
            //Participant p = PlayGamesPlatform.Instance.RealTime.GetSelf();
            //Debug.Log(p.Player.AvatarURL);
            //Debug.Log(Social.localUser.image);
            //Debug.Log(((PlayGamesLocalUser)Social.localUser).mPlatform.image);
            //Debug.Log(((PlayGamesLocalUser)Social.localUser).image);
            
            //StartCoroutine(LoadImage(Social.localUser.image));
        }
    }

    IEnumerator LoadGoogleImage()
    {
        while (Social.localUser.image == null)
        {
            Debug.Log("IMAGE NOT FOUND");
            yield return null;
        }
        Debug.Log("Image Found");
        googleTexture = Social.localUser.image;
        this.pcb(Social.localUser.image);
        /*using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(googleTexture);
            //ImgMine.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0f, 0f));
            this.pcb(googleTexture);
        }*/
    }


    void UpdateUI()
    {
        sync = false;
        loginPage = false;
        Debug.Log(playerInfo.Get("name"));
        Debug.Log(playerInfo.Get("uid"));
        if (!firstTime) {
            SceneManager.LoadScene("Home");
        }
        else
        {
            firstTime = false;
            SceneManager.LoadScene("Instructions");
        }

    }

    private void UploadUserData(string provider)
    {
        Debug.Log("Uploading user data to DynamoDB");
        Users myUser = new Users
        {
            uid = awsCredentials.GetIdentityId(),
            name = playerInfo.Get("name"),
            providerId = playerInfo.Get("uid"),
            provider = provider,
            GameSessionIds = new List<string> { },
            GameSessions = new List<Dictionary<string, string>> { },
            Strategies = new List<Dictionary<string, int>> { },
            gamesPlayed = 0,
            gamesWon = 0,
            gamesLost = 0,
            level = "NOVICE",
            levelPoints = 0,
            tickets = 2,
            imageUrl = null
        };
        myUser.Strategies.Add(StrategyManagerScript.randomStrategyGenerator());
        dbContext.SaveAsync(myUser, (result) => {
            if (result.Exception == null)
            {
                Debug.Log("Data uploaded to DynamoDB");
                curUserData = myUser;
                playerInfo.SynchronizeOnConnectivity();
            }
            else
            {
                Debug.Log(result.Exception.Message);
                Debug.Log(result.Exception.InnerException);
                Debug.Log(result.Exception);
            }
        });
    }

#if UNITY_ANDROID
	public void UsedOnlyForAOTCodeGeneration() {
		//Bug reported on github https://github.com/aws/aws-sdk-net/issues/477
		//IL2CPP restrictions: https://docs.unity3d.com/Manual/ScriptingRestrictions.html
		//Inspired workaround: https://docs.unity3d.com/ScriptReference/AndroidJavaObject.Get.html

		AndroidJavaObject jo = new AndroidJavaObject("android.os.Message");
		int valueString = jo.Get<int>("what");
        string stringValue = jo.Get<string>("what");
	}
#endif

    void Update()
    {
        if (fbandcsInitiated == 2)
        {
            Debug.Log("FB and AWS initiated");
            fbandcsInitiated = 0;
            loading = false;
            if (FB.IsLoggedIn)
            {
                fbauth = true;
                gauth = false;
                loading = true;
                awsCredentials.AddLogin("graph.facebook.com", AccessToken.CurrentAccessToken.TokenString);
                Debug.Log("Already LoggedIn FB");
                playerInfo.SynchronizeOnConnectivity();
                fbandcsInitiated = 0;
            }
            else// if (PlayGamesPlatform.Instance.IsAuthenticated() || Social.localUser.authenticated)
            {
                PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.NoPrompt, (result) => {
                    // handle results
                    Debug.Log(result);
                    if (result==GooglePlayGames.BasicApi.SignInStatus.Success)
                    {
                        gauth = true;
                        fbauth = false;
                        loading = true;
                        awsCredentials.AddLogin("accounts.google.com", PlayGamesPlatform.Instance.GetIdToken());
                        Debug.Log("Already LoggedIn G");
                        playerInfo.SynchronizeOnConnectivity();
                    }
                    else
                    {
                        fbauth = false;
                        gauth = false;
                        loading = false;
                    }
                });
            }
            /*else
            {
                loading = false;
            }*/
        }
        if ((FB.IsLoggedIn || Social.localUser.authenticated) && sync && loginPage)
        {
            UpdateUI();
        }

    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GetUserData(UserDataCB udcb)
    {
        this.udcb = udcb;
        Debug.Log("Started Fetching User Data");
        dbContext.LoadAsync<Users>(awsCredentials.GetIdentityId(), (result) => {
            Debug.Log("Load Result achieved");
            if (result.Exception != null)
            {
                Debug.Log(result.Exception);
                if (udcb != null)
                {
                    udcb(null);
                }
                return;
            }
            Debug.Log("Fetch Data Successful:");
            curUserData = result.Result as Users;
            if (udcb != null)
            {
                udcb(curUserData);
            }
        });
    }

    public void getUserStrategies(StrategyManagerScript.LoadStrategyCallBack x)
    {
        lscb = x;
        Debug.Log("Started Fetching User Data");
        dbContext.LoadAsync<Users>(awsCredentials.GetIdentityId(), (result) =>{
            Debug.Log("Load Result achieved");
            if (result.Exception != null)
            {
                Debug.Log(result.Exception);
                lscb(null);
                return;
            }
            Debug.Log("Fetch Data Successful:");
            Users retrievedUserData = result.Result as Users;
            curUserData = retrievedUserData;
            lscb(retrievedUserData.Strategies);
            Debug.Log("Fetch Data Successful:"+ retrievedUserData.Strategies.Count);
        });
    }

    public void SaveStrategy(Dictionary<string,int> dictx,int x,StrategyManagerScript.SaveStrategyCallBack xcb)
    {
        bool saved = false;
        sscb = xcb;
        currentTempStrategy = x;
        currentStrategy = dictx;
        dbContext.LoadAsync<Users>(awsCredentials.GetIdentityId(), (result) =>
        {
            if (result.Exception == null)
            {
                Users userRetrieved = result.Result as Users;
                if (userRetrieved != null)
                {
                    curUserData = userRetrieved;
                    if (curUserData.Strategies.Count > currentTempStrategy)
                    {
                        foreach(KeyValuePair<string,int> tempItem in currentStrategy)
                        {
                            Debug.Log(tempItem.Key+":"+tempItem.Value);
                        }
                        Debug.Log("Strategy dict updated");
                        curUserData.Strategies[currentTempStrategy] = currentStrategy;
                    }
                    else {
                        curUserData.Strategies.Add(currentStrategy);
                    }
                }
                else {
                    curUserData.Strategies = new List<Dictionary<string, int>> {currentStrategy};
                }
                dbContext.SaveAsync<Users>(curUserData, (res) =>
                {
                    if (res.Exception == null)
                    {
                        sscb(true);
                    }
                    else
                    {
                        Debug.Log("Save: Save Failed:" + res.Exception);
                        sscb(false);
                    }   
                });
            }
            else
            {
                Debug.Log("Save: Load Failed:" + result.Exception);
                sscb(false);
            }
        });
        
    }

    public void UpdateUserData()
    {
        dbContext.SaveAsync<Users>(curUserData, (res) =>
        {
            if (res.Exception == null)
            {
                
            }
            else
            {
                Debug.Log("Save: Save Failed:" + res.Exception);
            }
        });
    }

}


[DynamoDBTable("Users")]
public class Users 
{
    [DynamoDBHashKey]   // Hash key.
    public string uid { get; set; }
    [DynamoDBProperty]
    public string providerId { get; set; }
    [DynamoDBProperty]
    public string name { get; set; }
    [DynamoDBProperty]
    public string provider { get; set; }
    [DynamoDBProperty] 
    public List<string> GameSessionIds { get; set; }
    [DynamoDBProperty]
    public List<Dictionary<string,int>> Strategies { get; set; }
    [DynamoDBProperty]
    public List<Dictionary<string, string>> GameSessions { get; set; }
    [DynamoDBProperty]
    public int gamesPlayed { get; set; }
    [DynamoDBProperty]
    public int gamesWon { get; set; }
    [DynamoDBProperty]
    public int gamesLost { get; set; }
    [DynamoDBProperty]
    public int levelPoints { get; set; }
    [DynamoDBProperty]
    public string level { get; set; }
    [DynamoDBProperty]
    public int tickets { get; set; }
    [DynamoDBProperty]
    public string imageUrl { get; set; }
}