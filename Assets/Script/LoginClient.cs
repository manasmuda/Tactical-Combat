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

public class LoginClient : MonoBehaviour
{

    private Dataset playerInfo;
    private CognitoSyncManager syncManager;
    private CognitoAWSCredentials awsCredentials;
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
        fbandcsInitiated = fbandcsInitiated + 1;

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
            Debug.Log("Player Data Not Updated");
            playerInfo.Put("uid", AccessToken.CurrentAccessToken.UserId);
            fetchFBName();
        }
        else
        {
            UserData.name = playerInfo.Get("name");
            UserData.provider = "FB";
            UserData.uid = playerInfo.Get("uid");
            Debug.Log("Player Data Synchronized");
            loading = false;
            sync = true;
            loginPage = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.buildIndex);
    }

    public void Logout()
    {
        FB.LogOut();
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

    void NameCallBack(IGraphResult result)
    {
        Debug.Log("Retrieved name from fb");
        IDictionary<string, object> profil = result.ResultDictionary;
        playerInfo.Put("name", profil["first_name"].ToString());
        UploadUserData();
        //playerInfo.SynchronizeOnConnectivity();
    }

    void UpdateUI()
    {
        sync = false;
        loginPage = false;
        Debug.Log(playerInfo.Get("name"));
        Debug.Log(playerInfo.Get("uid"));
        SceneManager.LoadScene("Home");
    }

    private void UploadUserData()
    {
        Debug.Log("Uploading user data to DynamoDB");
        Users myUser = new Users
        {
            uid = playerInfo.Get("uid"),
            name = playerInfo.Get("name"),
            identityId = awsCredentials.GetIdentityId(),
            provider = "FB",
            GameSessionIds = new List<string> { },
            GameSessions = new List<Dictionary<string, string>> { },
            Strategies = new List<Dictionary<string, int>> { }
        };
        myUser.Strategies.Add(StrategyManagerScript.randomStrategyGenerator());
        dbContext.SaveAsync(myUser, (result) => {
            if (result.Exception == null)
            {
                Debug.Log("Data uploaded to DynamoDB");
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
                loading = true;
                awsCredentials.AddLogin("graph.facebook.com", AccessToken.CurrentAccessToken.TokenString);
                Debug.Log("Already LoggedIn");
                playerInfo.SynchronizeOnConnectivity();
            }
            else
            {
                loading = false;
            }
        }
        if (FB.IsLoggedIn && sync && loginPage)
        {
            UpdateUI();
        }

    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void getUserStrategies(StrategyManagerScript.LoadStrategyCallBack x)
    {
        lscb = x;
        Debug.Log("Started Fetching User Data");
        dbContext.LoadAsync<Users>("2689486394707778", (result) =>{
            Debug.Log("Load Result achieved");
            if (result.Exception != null)
            {
                Debug.Log(result.Exception);
                lscb(null);
                return;
            }
            Debug.Log("Fetch Data Successful:");
            Users retrievedUserData = result.Result as Users;
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
        dbContext.LoadAsync<Users>(playerInfo.Get("uid"), (result) =>
        {
            if (result.Exception == null)
            {
                Users userRetrieved = result.Result as Users;
                if (userRetrieved != null)
                {
                    if (userRetrieved.Strategies.Count > currentTempStrategy)
                    {
                        foreach(KeyValuePair<string,int> tempItem in currentStrategy)
                        {
                            Debug.Log(tempItem.Key+":"+tempItem.Value);
                        }
                        Debug.Log("Strategy dict updated");
                        userRetrieved.Strategies[currentTempStrategy] = currentStrategy;
                    }
                    else {
                        userRetrieved.Strategies.Add(currentStrategy);
                    }
                }
                else {
                    userRetrieved.Strategies = new List<Dictionary<string, int>> {currentStrategy};
                }
                dbContext.SaveAsync<Users>(userRetrieved, (res) =>
                {
                    if (res.Exception == null)
                    {
                        sscb(true);
                    }
                    else
                    {
                        Debug.Log("Save: Save Failed:" + result.Exception);
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

}


[DynamoDBTable("Users")]
public class Users 
{
    [DynamoDBHashKey]   // Hash key.
    public string uid { get; set; }
    [DynamoDBProperty]
    public string identityId { get; set; }
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
}