using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using UnityEngine.SceneManagement;

using Facebook.Unity;

// *** MAIN CLIENT CLASS FOR MANAGING CLIENT CONNECTIONS AND MESSAGES ***

public class Client : MonoBehaviour
{

    // Local player
    public NetworkClient networkClient;

    public string myPlayerId;
    public Dictionary<string, object> armyIds;

    private PlayerSessionObject playerSessionObj=new PlayerSessionObject();

    private bool connectionSuccess = false;

    //We get events back from the NetworkServer through this static list
    public static List<SimpleMessage> messagesToProcess = new List<SimpleMessage>();

    private float updateCounter = 0.0f;

    //Cognito credentials for sending signed requests to the API
    //public static Amazon.Runtime.ImmutableCredentials cognitoCredentials = null;

    public bool loading = false;

    public GameManagerScript gameManagerScript;

    public float playerWaitTime = 0.0f;
    public bool gameStarted = false;
    public bool playerJoined = false;

    private GameClientCallbacks gameClientCallbacks;

    public delegate void gameStartCallBack(bool x);
    public delegate void playerJoinedCallBack(float x,string playerId,Dictionary<string,object> armyIds);
    public delegate void connectionCallBack(bool x);

    public static Client clientInstance = null;

    void Awake()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        DontDestroyOnLoad(this.gameObject);
        if (clientInstance == null)
        {
            clientInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Called by Unity when the Gameobject is created
    void Start()
    {
        // Set up Mobile SDK
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(scene.buildIndex);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void FetchGameAndPlayerSession()
    {
        
        //StartCoroutine(ConnectToServer());
        AWSConfigs.AWSRegion = "ap-south-1"; // Your region here
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        // paste this in from the Amazon Cognito Identity Pool console
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            "ap-south-1:633d62b0-bdef-4b2b-8b33-1e091db82f24", // Identity pool ID
            RegionEndpoint.APSouth1 // Region
        );

        AmazonLambdaClient client = new AmazonLambdaClient(credentials, RegionEndpoint.APSouth1);
        InvokeRequest request = new InvokeRequest
        {
            FunctionName = "ConnectClientToServer",
            InvocationType = InvocationType.RequestResponse
        };

        loading = true;
        client.InvokeAsync(request,
            (response) =>
            {
                if (response.Exception == null)
                {
                    if (response.Response.StatusCode == 200)
                    {
                        var payload = Encoding.ASCII.GetString(response.Response.Payload.ToArray()) + "\n";
                        playerSessionObj = JsonUtility.FromJson<PlayerSessionObject>(payload);
                        Debug.Log(playerSessionObj.PlayerSessionId);
                        Debug.Log(playerSessionObj.IpAddress);
                        Debug.Log(playerSessionObj.Port);
                        Debug.Log(playerSessionObj);

                        if (playerSessionObj.PlayerSessionId == null)
                        {
                            Debug.Log($"Error in Lambda: {payload}");
                            loading=false;
                        }
                        else
                        {
                            StartCoroutine(ConnectToServer());
                            //QForMainThread(ActionConnectToServer, playerSessionObj.IpAddress, Int32.Parse(playerSessionObj.Port), playerSessionObj.PlayerSessionId);
                        }
                    }
                    else
                    {
                        loading = false;
                    }
                }
                else
                {
                    loading = false;
                    Debug.LogError(response.Exception);
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

    public void gameStaredCB(bool x)
    {
        gameStarted = true;
        SceneManager.LoadScene("Game");
    }

    public void playerJoinedCB(float x, string playerId,Dictionary<string,object> Ids)
    {
        armyIds = Ids;
        playerJoined = true;
        playerWaitTime = x;
        loading = false;
        this.myPlayerId = playerId;
        Debug.Log("playerId:" + playerId);
        SceneManager.LoadScene("MatchMaking");
    }

    public void connectionCB(bool x)
    {
        if (!x)
        {
            connectionSuccess = false;
        }
    }

    public void ConnectWithPlayerId(string playerIdx)
    {
        playerSessionObj = new PlayerSessionObject();
        playerSessionObj.IpAddress = "127.0.0.1";
        playerSessionObj.Port = 1935;
        playerSessionObj.GameSessionId = "gsess-abc";
        playerSessionObj.PlayerSessionId = playerIdx;
        StartCoroutine(ConnectToServer());
        //SceneManager.LoadScene("Game");
    }


    // Update is called once per frame
    void Update()
    {
        //this.ProcessMessages();
        if (connectionSuccess)
        {
            // Only send updates 5 times per second to avoid flooding server with messages
            this.updateCounter += Time.deltaTime;
            if (updateCounter < 0.2f)
            {
                return;
            }
            this.updateCounter = 0.0f;
            this.networkClient.Update();
            if (gameStarted)
            {
                ProcessMessages();
            }
        }

    }

    IEnumerator ConnectToServer()
    {

        yield return null;

        gameStartCallBack gscb = gameStaredCB;
        playerJoinedCallBack pjcb = playerJoinedCB;
        connectionCallBack ccb = connectionCB;
        this.networkClient = new NetworkClient(gscb,pjcb,ccb);

        yield return StartCoroutine(this.networkClient.DoMatchMakingAndConnect(playerSessionObj));

        if (this.networkClient.ConnectionSucceeded())
        {
            this.connectionSuccess = true;
            //SimpleMessage message = new SimpleMessage(MessageType.Success, "Player Successfully connected");
            //this.networkClient.SendMessage(message);
        }

        yield return null;
    }

    void GameStarted(int time)
    {
        playerWaitTime = time;
    }

    // Process messages received from server
   void ProcessMessages()
    {
        // Go through any messages to process
        foreach (SimpleMessage msg in messagesToProcess)
        {
            if(msg.messageType == MessageType.GameReady)
            {
                HandleGameReady(msg);
            }
            else if (msg.messageType == MessageType.TurnChange)
            {
                HandleTurnChanged(msg);
            }
            else if (msg.messageType == MessageType.BattleResult)
            {
                HandleBattleResult(msg);
            }
        }
        messagesToProcess.Clear();
    }

    void HandleGameReady(SimpleMessage msg)
    {
        Debug.Log("GameReady");
        gameManagerScript.LoadOppStrategy(msg.listdictdata);
        gameManagerScript.LoadBoardStatePos(msg.listData);
    }

    void HandleTurnChanged(SimpleMessage msg)
    {
        Debug.Log("TurnChanged");
        gameManagerScript.changeTurn(msg.turnId);
    }

    void HandleBattleResult(SimpleMessage msg)
    {
        Debug.Log("Battle Result");
        gameManagerScript.HandleBattleResult(msg);
    }
}