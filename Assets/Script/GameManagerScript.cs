using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{

    private Client client;
    private LoginClient loginClient;

    private bool Placing = true;
    private int pieceSelected = -1;

    public MyPlayerData myPlayerData;
    public OppPlayerData oppPlayerData;

    public List<string> kingArmyAlias = new List<string> { "king", "commander_king", "knight_king_7", "knight_king_6", "knight_king_5", "soldier_king_4", "soldier_king_3_1", "soldier_king_3_2", "soldier_king_2_1", "soldier_king_2_2", "soldier_king_1_1", "soldier_king_1_2" };
    public List<string> lord1ArmyAlias = new List<string> { "lord1", "commander_lord1", "knight_lord1_7", "knight_lord1_6", "knight_lord1_5", "soldier_lord1_4", "soldier_lord1_3", "soldier_lord1_2", "soldier_lord1_1" };
    public List<string> lord2ArmyAlias = new List<string> { "lord2", "commander_lord2", "knight_lord2_7", "knight_lord2_6", "knight_lord2_5", "soldier_lord2_4", "soldier_lord2_3", "soldier_lord2_2", "soldier_lord2_1" };

    public List<GameObject> kingArmyObjects = new List<GameObject> { };
    public List<GameObject> lord1Objects = new List<GameObject> { };
    public List<GameObject> lord2Objects = new List<GameObject> { };

    public Mesh emptyMesh;
    public Mesh kingMesh;
    public Mesh lordMesh;
    public Mesh commanderMesh;
    public List<Mesh> soldiersMesh;
    public List<Mesh> knightsMesh;

    public List<int> indexMatcher = new List<int> { };

    public List<int> boardPosStates = new List<int> { };

    public GameObject baseObject;
    public GameObject pieceObject;

    public GameObject diedPieceObject;

    public Material blueBaseMat;
    public Material redBaseMat;
    public Material bronzeMat;
    public Material silverMat;
    public Material goldMat;

    public GameObject redTeamCamera;
    public GameObject blueTeamCamera;

    private GameObject myCamera;

    public GameObject redLight;
    public GameObject blueLight;

    public Slider zoomSlider;

    private Material myMat;
    private Material oppMat;
    private Text myTimer;
    private Text oppTimer;

    public CanvasGroup LoadingPanel;
    public CanvasGroup PlayPanel;
    public CanvasGroup MessagePanel;
    public CanvasGroup StrategiesPanel;
    public Text MessageText;
    public Text Player1Text;
    public Text Player2Text;
    public Text player1Timer;
    public Text player2Timer;
    public Button FirstStrategyButton;
    public Button SecondStrategyButton;
    public Button ThirdStrategyButton;
    public Button RandomStrategyButton;
    public CanvasGroup FirstStrategyCG;
    public CanvasGroup SecondStrategyCG;
    public CanvasGroup ThirdStrategyCG;

    public bool myTurn=false;
    public bool oppTurn = false;

    private List<Dictionary<string, int>> strategies;

    private float timer = 0f;
    private bool timerMode = false;

    public int selectedPiecePos=-1;
    public GameObject selectedGameObject = null;
    public List<int> movePos = null;
    public List<GameObject> movePosObject = null;

    private Queue<Action> _mainThreadQueue = new Queue<Action>();

    // Start is called before the first frame update
    void Start()
    {
        showLoading();
        client = GameObject.Find("Client").GetComponent<Client>();
        client.gameManagerScript = this;
        loginClient = GameObject.Find("LoginClient").GetComponent<LoginClient>();
        if (client.gameStarted)
        {
            myPlayerData = new MyPlayerData(client.myPlayerId);
            oppPlayerData = new OppPlayerData();
            if (myPlayerData.playerId == "1")
            {
                myMat = blueBaseMat;
                oppMat = redBaseMat;
                myTimer = player1Timer;
                oppTimer = player2Timer;
                redLight.SetActive(false);
                blueLight.SetActive(true);
                blueTeamCamera.SetActive(true);
                redTeamCamera.SetActive(false);
                myPlayerData.posMultiplier = 1;
                myPlayerData.offset = 70;
                myPlayerData.fDir = -1;
                myPlayerData.angleYOffSet = -90.0f;
                oppPlayerData.posMultiplier = -1;
                oppPlayerData.offset = 29;
                oppPlayerData.fDir = 1;
                oppPlayerData.angleYOffSet = 90.0f;
                myCamera = blueTeamCamera;
            }
            else
            {
                myMat = redBaseMat;
                oppMat = blueBaseMat;
                myTimer = player2Timer;
                oppTimer = player1Timer;
                redLight.SetActive(true);
                blueLight.SetActive(false);
                blueTeamCamera.SetActive(false);
                redTeamCamera.SetActive(true);
                myPlayerData.posMultiplier = -1;
                myPlayerData.offset = 29;
                myPlayerData.fDir = 1;
                myPlayerData.angleYOffSet = 90.0f;
                oppPlayerData.posMultiplier = 1;
                oppPlayerData.offset = 70;
                oppPlayerData.fDir = -1;
                oppPlayerData.angleYOffSet = -90.0f;
                myCamera = redTeamCamera;
            }
            myTimer.text = "";
            oppTimer.text = "";
            StrategyManagerScript.LoadStrategyCallBack lscb = LoadStrategyCB;
            loginClient.getUserStrategies(lscb);
            zoomSlider.onValueChanged.AddListener(delegate { zoomCameraChange(); });
        }
    }

    void zoomCameraChange()
    {
        float zv=myPlayerData.fDir*zoomSlider.value;
        myCamera.transform.position = new Vector3(0f,17.5f,18.5f*zv);
    }

    void LoadStrategyCB(List<Dictionary<string,int>> x)
    {
        FirstStrategyButton.onClick.AddListener(delegate{ LoadStrategy(x[0]);});
        SecondStrategyButton.onClick.AddListener(delegate { LoadStrategy(x[1]);});
        ThirdStrategyButton.onClick.AddListener(delegate { LoadStrategy(x[2]);});
        RandomStrategyButton.onClick.AddListener(delegate { LoadStrategy(StrategyManagerScript.randomStrategyGenerator()); });
        if (x != null)
        {
            Debug.Log(x.Count + " strategies are loaded");
            if (x.Count == 1)
            {
                showPanel(FirstStrategyCG);
            }
            else if (x.Count == 2)
            {
                showPanel(FirstStrategyCG);
                showPanel(SecondStrategyCG);
            }
            else if (x.Count == 3)
            {
                showPanel(FirstStrategyCG);
                showPanel(SecondStrategyCG);
                showPanel(ThirdStrategyCG);
            }
        }
        else
        {
            Debug.Log("There are no strategies");
        }
        showStrategyPanel();
    }

    // Update is called once per frame
    void Update()
    {
        if (myTurn)
        {
            if (Input.touchCount > 0)
            {
                RaycastHit hit;
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit, 100.0f, LayerMask.GetMask("Board")))
                    {
                        Debug.Log(hit.point.x);
                        Debug.Log(hit.point.z);
                        int pi = Convert.ToInt32(Math.Floor((12.5 - hit.point.z) / 2.5f));
                        int pj = Convert.ToInt32(Math.Floor((12.5 + hit.point.x) / 2.5));
                        Debug.Log("i:" + pi + ",j:" + pj);
                        int sp = 10 * pi + pj;
                        Debug.Log("sp:" + sp);
                        float tempx = 0.0f;
                        float tempy = 0.0f;
                        if (boardPosStates[sp] == Convert.ToInt32(myPlayerData.playerId))
                        {
                            Debug.Log("Piece Selected");
                            selectedPiecePos = -1;
                            Destroy(selectedGameObject);
                            if (movePosObject != null)
                            {
                                for (int i = 0; i < movePosObject.Count; i++)
                                {
                                    Destroy(movePosObject[i]);
                                }
                            }
                            movePosObject = null;
                            selectedPiecePos = sp;
                            tempx = (pj - 5) * (2.5f) + (1.25f);
                            tempy = (5 - pi) * (2.5f) - (1.25f);
                            selectedGameObject = Instantiate(baseObject, new Vector3(tempx, 0.25f, tempy), Quaternion.Euler(0f,0f,0f)) as GameObject;
                            movePos = new List<int> { };
                            movePosObject = new List<GameObject> { };
                            if (pi > 0 && !(boardPosStates[10 * (pi - 1) + pj] == -1 || boardPosStates[10 * (pi - 1) + pj]==Convert.ToInt32(myPlayerData.playerId)))
                            { 

                                movePos.Add(10 * (pi - 1) + pj);
                                tempx = (pj - 5) * (2.5f) + (1.25f);
                                tempy = (5 - (pi - 1)) * (2.5f) - (1.25f);
                                GameObject tempObject = Instantiate(baseObject, new Vector3(tempx, 0.25f, tempy), Quaternion.Euler(0f, 0f, 0f)) as GameObject;
                                movePosObject.Add(tempObject);
                            }
                            if (pi < 9 && !(boardPosStates[10 * (pi + 1) + pj] == -1 || boardPosStates[10 * (pi + 1) + pj] == Convert.ToInt32(myPlayerData.playerId)))
                            {
                                movePos.Add(10 * (pi + 1) + pj);
                                tempx = (pj - 5) * (2.5f) + (1.25f);
                                tempy = (5 - (pi + 1)) * (2.5f) - (1.25f);
                                GameObject tempObject = Instantiate(baseObject, new Vector3(tempx, 0.25f, tempy), Quaternion.Euler(0f, 0f, 0f)) as GameObject;
                                movePosObject.Add(tempObject);
                            }
                            if (pj > 0 && !(boardPosStates[10 * (pi) + (pj-1)] == -1 || boardPosStates[10 * (pi) + (pj-1)] == Convert.ToInt32(myPlayerData.playerId)))
                            {
                                movePos.Add(10 * (pi) + (pj - 1));
                                tempx = ((pj - 1) - 5) * (2.5f) + (1.25f);
                                tempy = (5 - (pi)) * (2.5f) - (1.25f);
                                GameObject tempObject = Instantiate(baseObject, new Vector3(tempx, 0.25f, tempy), Quaternion.Euler(0f, 0f, 0f)) as GameObject;
                                movePosObject.Add(tempObject);
                            }
                            if (pj < 9 && !(boardPosStates[10 * (pi) + (pj+1)] == -1 || boardPosStates[10 * (pi) + (pj+1)] == Convert.ToInt32(myPlayerData.playerId)))
                            {
                                movePos.Add(10 * (pi) + (pj + 1));
                                tempx = ((pj + 1) - 5) * (2.5f) + (1.25f);
                                tempy = (5 - (pi)) * (2.5f) - (1.25f);
                                GameObject tempObject = Instantiate(baseObject, new Vector3(tempx, 0.25f, tempy), Quaternion.Euler(0f, 0f, 0f)) as GameObject;
                                movePosObject.Add(tempObject);
                            }
                        }
                        else if (selectedPiecePos != -1)
                        {
                            Debug.Log("Selected Move");
                            if (movePos != null && movePos.Contains(sp))
                            {
                                SimpleMessage msg = new SimpleMessage(MessageType.PlayerMove);
                                msg.playerId = myPlayerData.playerId;
                                msg.dictData.Add("id",myPlayerData.positionIdMatcher[selectedPiecePos]);
                                msg.dictData.Add("source", selectedPiecePos);
                                msg.dictData.Add("dest", sp);
                                client.networkClient.SendMessage(msg);
                                showPanel(MessagePanel);
                                MessageText.text = "Battle Started\nWaiting For Result";
                                selectedPiecePos = -1;
                                Destroy(selectedGameObject);
                                if (movePosObject != null)
                                {
                                    for (int i = 0; i < movePosObject.Count; i++)
                                    {
                                        Destroy(movePosObject[i]);
                                    }
                                }
                                movePosObject = null;
                                myTurn = false;
                            }
                        }
                    }
                }
            }
        }

        if (timerMode)
        {
            timer = timer - Time.deltaTime;
            if (myTurn)
            {
                myTimer.text = Convert.ToString(Convert.ToInt32(Math.Floor(timer)));
            }
            else if(oppTurn)
            {
                oppTimer.text = Convert.ToString(Convert.ToInt32(Math.Floor(timer)));
            }
            if (timer <= 0)
            {
                if (myTurn)
                {
                    myTurn = false;
                    myTimer.text = "";
                }
                else if(oppTurn)
                {
                    oppTurn = false;
                    oppTimer.text = "";
                }
                timerMode = false;
                timer = 0f;
                selectedPiecePos = -1;
                Destroy(selectedGameObject);
                if (movePosObject != null)
                {
                    for (int i = 0; i < movePosObject.Count; i++)
                    {
                        Destroy(movePosObject[i]);
                    }
                }
                movePosObject = null;
                //SimpleMessage msg = new SimpleMessage(MessageType.BattleResult);
                //client.networkClient.SendMessage(msg);
            }
        }
    }

    void checkBoardIndex(float x,float y)
    {
        int i = Convert.ToInt32((12.5-y)/2.5);
        int j = Convert.ToInt32((12.5-x)/2.5);

    }

    void LoadStrategy(Dictionary<string,int> loadingStrategy)
    {
        showLoading();
        List<string> keys = new List<string>(loadingStrategy.Keys);
        List<int> values = new List<int>(loadingStrategy.Values);
        Debug.Log("strategy is loaded properly");
        SimpleMessage message = new SimpleMessage(MessageType.PlayerStrategy);
        message.playerId = myPlayerData.playerId;
        Debug.Log("Message Initiated");
        for (int i = 0; i < keys.Count; i++)
        {
            message.dictData.Add(keys[i],values[i]);
            int tempBoardPos = values[i];
            tempBoardPos = myPlayerData.offset + myPlayerData.posMultiplier * tempBoardPos;
            int ip = (tempBoardPos) / 10;
            int jp = tempBoardPos % 10;
            float tempx = (jp - 5) * (2.5f) + (1.25f);
            float tempy = (5 - ip) * (2.5f) - (1.25f);
            GameObject tempObject = Instantiate(pieceObject, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, myPlayerData.angleYOffSet, 0f)) as GameObject;
            if (keys[i].Split('_')[0] == "king")
            {
                tempObject.GetComponent<MeshFilter>().mesh = kingMesh;
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = myMat;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                myPlayerData.InitializeKing(ip, jp, Convert.ToString(client.armyIds[keys[i]]));
                myPlayerData.kingObject = tempObject;
            }
            else if (keys[i].Split('_')[0] == "lord1")
            {
                tempObject.GetComponent<MeshFilter>().mesh = lordMesh;
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = myMat;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                myPlayerData.InitializeLord1(ip, jp, Convert.ToString(client.armyIds[keys[i]]));
                myPlayerData.lord1Object = tempObject;
            }
            else if (keys[i].Split('_')[0] == "lord2")
            {
                tempObject.GetComponent<MeshFilter>().mesh = lordMesh;
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = myMat;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                myPlayerData.InitializeLord2(ip, jp, Convert.ToString(client.armyIds[keys[i]]));
                myPlayerData.lord2Object = tempObject;
            }
            else if (keys[i].Split('_')[0] == "commander")
            {
                tempObject.GetComponent<MeshFilter>().mesh = commanderMesh;
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = myMat;
                tempMaterials[1] = silverMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                if (keys[i].Split('_')[1] == "king")
                {
                    myPlayerData.InitializeCommanderK(ip, jp, Convert.ToString(client.armyIds[keys[i]]));
                    myPlayerData.commanderkObject = tempObject;
                }
                else if (keys[i].Split('_')[1] == "lord1")
                {
                    myPlayerData.InitializeCommanderL1(ip, jp, Convert.ToString(client.armyIds[keys[i]]));
                    myPlayerData.commanderl1Object = tempObject;
                }
                else if (keys[i].Split('_')[1] == "lord2")
                {
                    myPlayerData.InitializeCommanderL2(ip, jp, Convert.ToString(client.armyIds[keys[i]]));
                    myPlayerData.commanderl2Object = tempObject;
                }
            }
            else if (keys[i].Split('_')[0] == "knight")
            {
                tempObject.GetComponent<MeshFilter>().mesh = knightsMesh[Convert.ToInt32(keys[i].Split('_')[2])-5];
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = myMat;
                tempMaterials[1] = silverMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                myPlayerData.addKnight(ip, jp, Convert.ToInt32(keys[i].Split('_')[2]), keys[i].Split('_')[1], Convert.ToString(client.armyIds[keys[i]]));
                myPlayerData.knightObjects.Add(Convert.ToString(client.armyIds[keys[i]]), tempObject);
            }
            else if (keys[i].Split('_')[0] == "soldier")
            {
                tempObject.GetComponent<MeshFilter>().mesh = soldiersMesh[Convert.ToInt32(keys[i].Split('_')[2]) - 1];
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = myMat;
                tempMaterials[1] = bronzeMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                myPlayerData.addSoldier(ip, jp, Convert.ToInt32(keys[i].Split('_')[2]), keys[i].Split('_')[1], Convert.ToString(client.armyIds[keys[i]]));
                myPlayerData.soldierObjects.Add(Convert.ToString(client.armyIds[keys[i]]), tempObject);
            }
        }
        client.networkClient.SendMessage(message);
        showPlayPanel();
        if (oppPlayerData.gold.Count == 0)
        {
            showPanel(MessagePanel);
            MessageText.text = "Waiting";
        }
    }

    public void LoadOppStrategy(List<Dictionary<string, object>> oppStrategyX)
    {
        hidePanel(MessagePanel);
        Debug.Log("Opponent strategy Loaing Started");
        for (int i = 0; i < oppStrategyX.Count; i++)
        {
            float tempx = (Convert.ToInt32(oppStrategyX[i]["posJ"]) - 5) * (2.5f) + (1.25f);
            float tempy = (5 - Convert.ToInt32(oppStrategyX[i]["posI"])) * (2.5f) - (1.25f);
            GameObject tempObject = Instantiate(pieceObject, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, oppPlayerData.angleYOffSet, 0f)) as GameObject;
            tempObject.GetComponent<MeshFilter>().mesh = emptyMesh;
            Debug.Log(oppStrategyX[i]["color"]);
            if (oppStrategyX[i]["color"].ToString() == "gold")
            {
                Debug.Log("Gold Opponent player loaded");
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = oppMat;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                oppPlayerData.gold.Add(Convert.ToString(oppStrategyX[i]["id"]),oppStrategyX[i]);
                oppPlayerData.goldObjects.Add(Convert.ToString(oppStrategyX[i]["id"]), tempObject);
            }
            else if (oppStrategyX[i]["color"].ToString() == "silver")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = oppMat;
                tempMaterials[1] = silverMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                oppPlayerData.silver.Add(Convert.ToString(oppStrategyX[i]["id"]),oppStrategyX[i]);
                oppPlayerData.silverObjects.Add(Convert.ToString(oppStrategyX[i]["id"]), tempObject);
            }
            else if (oppStrategyX[i]["color"].ToString() == "bronze")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[0] = oppMat;
                tempMaterials[1] = bronzeMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                oppPlayerData.bronze.Add(Convert.ToString(oppStrategyX[i]["id"]),oppStrategyX[i]);
                oppPlayerData.bronzeObjects.Add(Convert.ToString(oppStrategyX[i]["id"]), tempObject);
            }
        }
    }

    public void LoadBoardStatePos(List<object> bpx)
    {
        boardPosStates=bpx.ConvertAll<int>(k => Convert.ToInt32(k));
        for(int i = 0; i < boardPosStates.Count; i++)
        {
            Debug.Log(i+":"+boardPosStates[i]);
        }
    }

    public void changeTurn(string x)
    {
        if (x == myPlayerData.playerId)
        {
            Debug.Log("My Turn Started");
            myTurn = true;
            oppTurn = false;
            myTimer.text = "30";
            oppTimer.text = "";
            timer = 30f;
            timerMode = true;
        }
        else
        {
            Debug.Log("Opponent Turn Started");
            myTurn = false;
            oppTurn = true;
            oppTimer.text = "30";
            myTimer.text = "";
            timer = 30f;
            timerMode = true;
        }
    }

    public void HandleBattleResult(SimpleMessage msg)
    {
        hidePanel(MessagePanel);
        Debug.Log("Battle Result:" + msg.listdictdata.Count);
        for(int i = 0; i < msg.listdictdata.Count; i++)
        {
            if (Convert.ToBoolean(msg.listdictdata[i]["myTeam"]))
            {
                string tempId = Convert.ToString(msg.listdictdata[i]["id"]);
                if (Convert.ToString(msg.listdictdata[i]["action"]) == "move")
                {
                    Debug.Log("Action: Move");
                    boardPosStates = msg.listData.ConvertAll<int>(k => Convert.ToInt32(k));
                    myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(msg.listdictdata[i]["source"]));
                    myPlayerData.positionIdMatcher.Add(Convert.ToInt32(msg.listdictdata[i]["dest"]), tempId);
                    if (Convert.ToString(msg.listdictdata[i]["color"]) == "gold")
                    {
                        if (Convert.ToString(myPlayerData.king["id"]) == tempId)
                        {
                            Vector3 tempvector = myPlayerData.kingObject.transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.king["posI"] = pi;
                            myPlayerData.king["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.kingObject, tempvector, 2f));
                        }
                        else if (Convert.ToString(myPlayerData.lord1["id"]) == tempId)
                        {
                            Vector3 tempvector = myPlayerData.lord1Object.transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.lord1["posI"] = pi;
                            myPlayerData.lord1["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.lord1Object, tempvector, 2f));
                        }
                        else if (Convert.ToString(myPlayerData.lord2["id"]) == tempId)
                        {
                            Vector3 tempvector = myPlayerData.lord2Object.transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.lord2["posI"] = pi;
                            myPlayerData.lord2["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.lord2Object, tempvector, 2f));
                        }
                    }
                    else if (Convert.ToString(msg.listdictdata[i]["color"]) == "silver")
                    {
                        if (Convert.ToString(myPlayerData.commanderk["id"]) == tempId)
                        {
                            Vector3 tempvector = myPlayerData.commanderkObject.transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.commanderk["posI"] = pi;
                            myPlayerData.commanderk["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.commanderkObject, tempvector, 2f));
                        }
                        else if (Convert.ToString(myPlayerData.commanderl1["id"]) == tempId)
                        {
                            Vector3 tempvector = myPlayerData.commanderl1Object.transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.commanderl1["posI"] = pi;
                            myPlayerData.commanderl1["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.commanderl1Object, tempvector, 2f));
                        }
                        else if (Convert.ToString(myPlayerData.commanderl2["id"]) == tempId)
                        {
                            Vector3 tempvector = myPlayerData.commanderl2Object.transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.commanderl2["posI"] = pi;
                            myPlayerData.commanderl2["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.commanderl2Object, tempvector, 2f));
                        }
                        else if (myPlayerData.knights.ContainsKey(tempId))
                        {
                            Vector3 tempvector = myPlayerData.knightObjects[tempId].transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.knights[tempId]["posI"] = pi;
                            myPlayerData.knights[tempId]["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.knightObjects[tempId], tempvector, 2f));
                        }
                    }
                    else if (Convert.ToString(msg.listdictdata[i]["color"]) == "bronze")
                    {
                        if (myPlayerData.soldiers.ContainsKey(tempId))
                        {
                            Vector3 tempvector = myPlayerData.soldierObjects[tempId].transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            myPlayerData.soldiers[tempId]["posI"] = pi;
                            myPlayerData.soldiers[tempId]["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(myPlayerData.soldierObjects[tempId], tempvector, 2f));
                        }
                    }
                }
                else if(Convert.ToString(msg.listdictdata[i]["action"]) == "power")
                {

                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "add")
                {
                    Debug.Log("Action: add");
                    int tempPos = Convert.ToInt32(msg.listdictdata[i]["pos"]);
                    int tempPower = Convert.ToInt32(msg.listdictdata[i]["power"]);
                    if (Convert.ToString(msg.listdictdata[i]["type"]) == "knight"){
                        myPlayerData.addKnight(tempPos / 10, tempPos % 10, tempPower, "king",tempId);
                        float tempx = (Convert.ToInt32(tempPos % 10) - 5) * (2.5f) + (1.25f);
                        float tempy = (5 - Convert.ToInt32(tempPos / 10)) * (2.5f) - (1.25f);
                        GameObject tempObject = Instantiate(pieceObject, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, myPlayerData.angleYOffSet, 0f)) as GameObject;
                        tempObject.GetComponent<MeshFilter>().mesh = knightsMesh[tempPower - 5];
                        Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                        tempMaterials[0] = myMat;
                        tempMaterials[1] = silverMat;
                        tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                        myPlayerData.knightObjects.Add(tempId, tempObject);
                    }
                    else if (Convert.ToString(msg.listdictdata[i]["type"]) == "soldier")
                    {
                        myPlayerData.addSoldier(tempPos / 10, tempPos % 10, tempPower, "king", tempId);
                        float tempx = (Convert.ToInt32(tempPos % 10) - 5) * (2.5f) + (1.25f);
                        float tempy = (5 - Convert.ToInt32(tempPos / 10)) * (2.5f) - (1.25f);
                        GameObject tempObject = Instantiate(pieceObject, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, myPlayerData.angleYOffSet, 0f)) as GameObject;
                        tempObject.GetComponent<MeshFilter>().mesh =soldiersMesh[tempPower - 1];
                        Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                        tempMaterials[0] = myMat;
                        tempMaterials[1] = bronzeMat;
                        tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                        myPlayerData.soldierObjects.Add(tempId, tempObject);
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "dead")
                {
                    Debug.Log("Action: Dead");
                    string tempColor = Convert.ToString(msg.listdictdata[i]["color"]);
 
                    if (tempColor == "gold")
                    {
                        if (Convert.ToString(myPlayerData.lord1["id"]) == tempId)
                        {
                            myPlayerData.lord1["state"] = "dead";
                            Destroy(myPlayerData.lord1Object);
                            myPlayerData.lord1Object = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.lord1["posI"]) *10+Convert.ToInt32(myPlayerData.lord1["posJ"]));
                        }
                        else if (Convert.ToString(myPlayerData.lord2["id"]) == tempId)
                        {
                            myPlayerData.lord2["state"] = "dead";
                            Destroy(myPlayerData.lord1Object);
                            myPlayerData.lord2Object = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.lord2["posI"]) * 10 + Convert.ToInt32(myPlayerData.lord2["posJ"]));
                        }
                    }
                    else if (tempColor == "silver")
                    {
                        if (Convert.ToString(myPlayerData.commanderk["id"]) == tempId)
                        {
                            myPlayerData.commanderk["state"] = "dead";
                            Destroy(myPlayerData.commanderkObject);
                            myPlayerData.commanderkObject = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.commanderk["posI"]) * 10 + Convert.ToInt32(myPlayerData.commanderk["posJ"]));
                        }
                        else if (myPlayerData.knights.ContainsKey(tempId))
                        {
                            myPlayerData.knights[tempId]["state"] = "dead";
                            Destroy(myPlayerData.knightObjects[tempId]);
                            myPlayerData.knightObjects[tempId] = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.knights[tempId]["posI"]) * 10 + Convert.ToInt32(myPlayerData.knights[tempId]["posJ"]));
                        }
                    }
                    else if (tempColor == "bronze")
                    {
                        if (myPlayerData.soldiers.ContainsKey(tempId))
                        {
                            myPlayerData.soldiers[tempId]["state"] = "dead";
                            Destroy(myPlayerData.soldierObjects[tempId]);
                            myPlayerData.soldierObjects[tempId] = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.soldiers[tempId]["posI"]) * 10 + Convert.ToInt32(myPlayerData.soldiers[tempId]["posJ"]));
                        }
                    }
                }
                else if(Convert.ToString(msg.listdictdata[i]["action"]) == "injured")
                {
                    Debug.Log("Action: Injured");
                    string tempColor = Convert.ToString(msg.listdictdata[i]["color"]);

                    if (tempColor == "silver")
                    {
                        if (Convert.ToString(myPlayerData.commanderl1["id"]) == tempId)
                        {
                            myPlayerData.commanderl1["state"] = "injured";
                            Destroy(myPlayerData.commanderl1Object);
                            myPlayerData.commanderl1Object = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.commanderl1["posI"]) * 10 + Convert.ToInt32(myPlayerData.commanderl1["posJ"]));
                            myPlayerData.fort1.Add(tempId);
                        }
                        else if (Convert.ToString(myPlayerData.commanderl2["id"]) == tempId)
                        {
                            myPlayerData.commanderl2["state"] = "injured";
                            Destroy(myPlayerData.commanderl2Object);
                            myPlayerData.commanderl2Object = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.commanderl2["posI"]) * 10 + Convert.ToInt32(myPlayerData.commanderl2["posJ"]));
                            myPlayerData.fort2.Add(tempId);
                        }
                        else if (myPlayerData.knights.ContainsKey(tempId))
                        {
                            myPlayerData.knights[tempId]["state"] = "injured";
                            Destroy(myPlayerData.knightObjects[tempId]);
                            myPlayerData.knightObjects[tempId] = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.knights[tempId]["posI"]) * 10 + Convert.ToInt32(myPlayerData.knights[tempId]["posJ"]));
                            if (Convert.ToString(msg.listdictdata[i]["fort"]) == "1")
                            {
                                myPlayerData.fort1.Add(tempId);
                            }
                            else
                            {
                                myPlayerData.fort2.Add(tempId);
                            }
                        }
                    }
                    else if (tempColor == "bronze")
                    {
                        if (myPlayerData.soldiers.ContainsKey(tempId))
                        {
                            myPlayerData.soldiers[tempId]["state"] = "injured";
                            Destroy(myPlayerData.soldierObjects[tempId]);
                            myPlayerData.soldierObjects[tempId] = null;
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.soldiers[tempId]["posI"]) * 10 + Convert.ToInt32(myPlayerData.soldiers[tempId]["posJ"]));
                            if (Convert.ToString(msg.listdictdata[i]["fort"]) == "1")
                            {
                                myPlayerData.fort1.Add(tempId);
                            }
                            else
                            {
                                myPlayerData.fort2.Add(tempId);
                            }
                        }
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "changeserves")
                {
                    Debug.Log("Action: ChangeServes");
                    string tempColor = Convert.ToString(msg.listdictdata[i]["color"]);
                    if (tempColor == "silver")
                    {
                        if (Convert.ToString(myPlayerData.commanderl1["id"]) == tempId)
                        {
                            myPlayerData.commanderl1["state"] = "change";
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.commanderl1["posI"]) * 10 + Convert.ToInt32(myPlayerData.commanderl1["posJ"]));
                            myPlayerData.addKnight(Convert.ToInt32(myPlayerData.commanderl1["posI"]), Convert.ToInt32(myPlayerData.commanderl1["posJ"]), 5,"king", Convert.ToString(msg.listdictdata[i]["newId"]));
                            myPlayerData.commanderl1Object.GetComponent<MeshFilter>().mesh = knightsMesh[0];
                            myPlayerData.knightObjects.Add(Convert.ToString(msg.listdictdata[i]["newId"]),myPlayerData.commanderl1Object);
                            myPlayerData.commanderl1Object = null;
                        }
                        else if (Convert.ToString(myPlayerData.commanderl2["id"]) == tempId)
                        {
                            myPlayerData.commanderl2["state"] = "change";
                            myPlayerData.positionIdMatcher.Remove(Convert.ToInt32(myPlayerData.commanderl2["posI"]) * 10 + Convert.ToInt32(myPlayerData.commanderl2["posJ"]));
                            myPlayerData.addKnight(Convert.ToInt32(myPlayerData.commanderl2["posI"]), Convert.ToInt32(myPlayerData.commanderl1["pos2"]), 5, "king", Convert.ToString(msg.listdictdata[i]["newId"]));
                            myPlayerData.commanderl2Object.GetComponent<MeshFilter>().mesh = knightsMesh[0];
                            myPlayerData.knightObjects.Add(Convert.ToString(msg.listdictdata[i]["newId"]), myPlayerData.commanderl2Object);
                            myPlayerData.commanderl2Object = null;
                        }
                        else if (myPlayerData.knights.ContainsKey(tempId))
                        {
                            myPlayerData.knights[tempId]["power"] = 5;
                            myPlayerData.knights[tempId]["serves"] = "king";
                            myPlayerData.knightObjects[tempId].GetComponent<MeshFilter>().mesh = knightsMesh[0];
                        }
                    }
                    else if(tempColor=="bronze"){
                        if (myPlayerData.soldiers.ContainsKey(tempId))
                        {
                            myPlayerData.soldiers[tempId]["power"] = 1;
                            myPlayerData.soldiers[tempId]["serves"] = "king";
                            myPlayerData.soldierObjects[tempId].GetComponent<MeshFilter>().mesh =soldiersMesh[0];
                        }
                    }
                }
                else if(Convert.ToString(msg.listdictdata[i]["action"]) == "clearfort")
                {
                    Debug.Log("Action: Clear Fort");
                    if (Convert.ToString(msg.listdictdata[i]["fort"]) == "1")
                    {
                        for(int j = 0; j < myPlayerData.fort1Objects.Count; j++)
                        {
                            Destroy(myPlayerData.fort1Objects[j]);
                        }
                        myPlayerData.fort1Objects = null;
                        myPlayerData.fort1 = null;
                    }
                    else
                    {
                        for (int j = 0; j < myPlayerData.fort2Objects.Count; j++)
                        {
                            Destroy(myPlayerData.fort2Objects[j]);
                        }
                        myPlayerData.fort2Objects = null;
                        myPlayerData.fort2 = null;
                    }
                }
            }
            else
            {
                string tempId = Convert.ToString(msg.listdictdata[i]["id"]);
                if (Convert.ToString(msg.listdictdata[i]["action"]) == "move")
                {
                    boardPosStates = msg.listData.ConvertAll<int>(k => Convert.ToInt32(k));
                    //oppPlayerData.positionIdMatcher.Remove(Convert.ToInt32(msg.listdictdata[i]["source"]));
                    //oppPlayerData.positionIdMatcher.Add(Convert.ToInt32(msg.listdictdata[i]["dest"]), tempId);
                    if (Convert.ToString(msg.listdictdata[i]["color"]) == "gold")
                    {
                        if (oppPlayerData.gold.ContainsKey(tempId))
                        {
                            Vector3 tempvector = oppPlayerData.goldObjects[tempId].transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            oppPlayerData.gold[tempId]["posI"] = pi;
                            oppPlayerData.gold[tempId]["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(oppPlayerData.goldObjects[tempId], tempvector, 2f));
                        }
                    }
                    else if (Convert.ToString(msg.listdictdata[i]["color"]) == "silver")
                    {
                        if (oppPlayerData.silver.ContainsKey(tempId))
                        {
                            Vector3 tempvector = oppPlayerData.silverObjects[tempId].transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            oppPlayerData.silver[tempId]["posI"] = pi;
                            oppPlayerData.silver[tempId]["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(oppPlayerData.silverObjects[tempId], tempvector, 2f));
                        }
                    }
                    else if (Convert.ToString(msg.listdictdata[i]["color"]) == "bronze")
                    {
                        if (oppPlayerData.bronze.ContainsKey(tempId))
                        {
                            Vector3 tempvector = oppPlayerData.bronzeObjects[tempId].transform.position;
                            int pp = Convert.ToInt32(msg.listdictdata[i]["dest"]);
                            int pi = pp / 10;
                            int pj = pp % 10;
                            oppPlayerData.bronze[tempId]["posI"] = pi;
                            oppPlayerData.bronze[tempId]["posJ"] = pj;
                            float tempx = (pj - 5) * (2.5f) + (1.25f);
                            float tempy = (5 - pi) * (2.5f) - (1.25f);
                            tempvector.x = tempx;
                            tempvector.z = tempy;
                            StartCoroutine(MovePiece(oppPlayerData.bronzeObjects[tempId], tempvector, 2f));
                        }
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "power")
                {

                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "add")
                {
                    Debug.Log("Action: add");
                    int tempPos = Convert.ToInt32(msg.listdictdata[i]["pos"]);
                    if (Convert.ToString(msg.listdictdata[i]["color"]) == "silver")
                    {
                        oppPlayerData.AddSilver(tempPos / 10, tempPos % 10, tempId);
                        float tempx = (Convert.ToInt32(tempPos%10) - 5) * (2.5f) + (1.25f);
                        float tempy = (5 - Convert.ToInt32(tempPos/10)) * (2.5f) - (1.25f);
                        GameObject tempObject = Instantiate(pieceObject, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, oppPlayerData.angleYOffSet, 0f)) as GameObject;
                        tempObject.GetComponent<MeshFilter>().mesh = emptyMesh;
                        Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                        tempMaterials[0] = oppMat;
                        tempMaterials[1] = silverMat;
                        tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                        oppPlayerData.silverObjects.Add(tempId, tempObject);
                    }
                    else if (Convert.ToString(msg.listdictdata[i]["color"]) == "bronze")
                    {
                        oppPlayerData.AddBronze(tempPos / 10, tempPos % 10, tempId);
                        float tempx = (Convert.ToInt32(tempPos % 10) - 5) * (2.5f) + (1.25f);
                        float tempy = (5 - Convert.ToInt32(tempPos / 10)) * (2.5f) - (1.25f);
                        GameObject tempObject = Instantiate(pieceObject, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, oppPlayerData.angleYOffSet, 0f)) as GameObject;
                        tempObject.GetComponent<MeshFilter>().mesh = emptyMesh;
                        Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                        tempMaterials[0] = oppMat;
                        tempMaterials[1] = bronzeMat;
                        tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                        oppPlayerData.bronzeObjects.Add(tempId, tempObject);
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "dead")
                {
                    Debug.Log("Action: Dead");
                    string tempColor = Convert.ToString(msg.listdictdata[i]["color"]);

                    if (tempColor == "gold")
                    {
                            oppPlayerData.gold[tempId]["state"] = "dead";
                            Destroy(oppPlayerData.goldObjects[tempId]);
                            oppPlayerData.goldObjects[tempId] = null;    
                    }
                    else if (tempColor == "silver")
                    {
                        
                            oppPlayerData.silver[tempId]["state"] = "dead";
                            Destroy(oppPlayerData.silverObjects[tempId]);
                            oppPlayerData.silverObjects[tempId] = null;
                    }
                    else if (tempColor == "bronze")
                    {
                            oppPlayerData.bronze[tempId]["state"] = "dead";
                            Destroy(oppPlayerData.bronzeObjects[tempId]);
                        oppPlayerData.bronzeObjects[tempId] = null;
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "injured")
                {
                    Debug.Log("Action: Injured");
                    string tempColor = Convert.ToString(msg.listdictdata[i]["color"]);
                    if (tempColor == "silver")
                    {
                            oppPlayerData.silver[tempId]["state"] = "injured";
                            Destroy(oppPlayerData.silverObjects[tempId]);
                            oppPlayerData.silverObjects[tempId] = null;
                            if (Convert.ToString(msg.listdictdata[i]["fort"]) == "1")
                            {
                                oppPlayerData.fort1.Add(tempId);
                            }
                            else
                            {
                                oppPlayerData.fort2.Add(tempId);
                            }
                    }
                    else if (tempColor == "bronze")
                    {
                            oppPlayerData.bronze[tempId]["state"] = "injured";
                            Destroy(oppPlayerData.bronzeObjects[tempId]);
                            oppPlayerData.bronzeObjects[tempId] = null;
                            if (Convert.ToString(msg.listdictdata[i]["fort"]) == "1")
                            {
                                oppPlayerData.fort1.Add(tempId);
                            }
                            else
                            {
                                oppPlayerData.fort2.Add(tempId);
                            }
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "changeserves")
                {
                    Debug.Log("Action: ChangeServes");
                    string tempColor = Convert.ToString(msg.listdictdata[i]["color"]);
                    if (tempColor == "silver")
                    {
                        if (oppPlayerData.silver.ContainsKey(tempId))
                        {
                            oppPlayerData.AddSilver(Convert.ToInt32(oppPlayerData.silver[tempId]["posI"]), Convert.ToInt32(oppPlayerData.silver[tempId]["posJ"]), Convert.ToString(msg.listdictdata[i]["newId"]));
                            oppPlayerData.silver.Remove(tempId);
                        }
                    }
                }
                else if (Convert.ToString(msg.listdictdata[i]["action"]) == "clearfort")
                {
                    Debug.Log("Action: Clear Fort");
                    if (Convert.ToString(msg.listdictdata[i]["fort"]) == "1")
                    {
                        for (int j = 0; j < oppPlayerData.fort1Objects.Count; j++)
                        {
                            Destroy(oppPlayerData.fort1Objects[j]);
                        }
                        oppPlayerData.fort1Objects = null;
                        oppPlayerData.fort1 = null;
                    }
                    else
                    {
                        for (int j = 0; j < oppPlayerData.fort2Objects.Count; j++)
                        {
                            Destroy(oppPlayerData.fort2Objects[j]);
                        }
                        oppPlayerData.fort2Objects = null;
                        oppPlayerData.fort2 = null;
                    }
                }
            }
        }
    }

    public IEnumerator MovePiece(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;


    }

    void showLoading()
    {
        showPanel(LoadingPanel);
        hidePanel(PlayPanel);
        hidePanel(MessagePanel);
        hidePanel(StrategiesPanel);
    }

    void showStrategyPanel()
    {
        hidePanel(LoadingPanel);
        hidePanel(PlayPanel);
        hidePanel(MessagePanel);
        showPanel(StrategiesPanel);
    }

    void showPlayPanel()
    {
        hidePanel(LoadingPanel);
        showPanel(PlayPanel);
        hidePanel(MessagePanel);
        hidePanel(StrategiesPanel);
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

    private void QForMainThread(Action fn)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(); });
        }
    }

    private void QForMainThread<T1>(Action<T1> fn, T1 p1)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(p1); });
        }
    }

    private void QForMainThread<T1, T2>(Action<T1, T2> fn, T1 p1, T2 p2)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(p1, p2); });
        }
    }

    private void QForMainThread<T1, T2, T3>(Action<T1, T2, T3> fn, T1 p1, T2 p2, T3 p3)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(() => { fn(p1, p2, p3); });
        }
    }

    private void RunMainThreadQueueActions()
    {
        // as our server messages come in on their own thread
        // we need to queue them up and run them on the main thread
        // when the methods need to interact with Unity
        lock (_mainThreadQueue)
        {
            while (_mainThreadQueue.Count > 0)
            {
                _mainThreadQueue.Dequeue().Invoke();
            }
        }
    }
}
