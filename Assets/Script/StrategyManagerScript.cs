using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StrategyManagerScript : MonoBehaviour
{
    private static readonly System.Random random = new System.Random();

    public Button saveButton;
    public Button cancelButton;
    public Dropdown armyType;
    public Button confirmButton;

    public Button backButton;

    public Button AddNewButton;

    public CanvasGroup firstStrategyCG;
    public CanvasGroup secondStrategyCG;
    public CanvasGroup thirdStrategyCG;
    public CanvasGroup addNewStrategyCG;

    public Button firstStrategyButton;
    public Button secondStrategyButton;
    public Button thirdStrategyButton;

    public Button TestLoadingButton;

    public CanvasGroup StrategyListPanel;
    public CanvasGroup EditStrategyPanel;
    public CanvasGroup LoadingPanel;

    public CanvasGroup kingArmyCG;
    public CanvasGroup lord1ArmyCG;
    public CanvasGroup lord2ArmyCG;

    public List<Mesh> kingArmyMesh = new List<Mesh>{ };
    public List<Button> kingB = new List<Button> { };
    public List<CanvasGroup> kinCG = new List<CanvasGroup> { };
    public List<int> kingP = new List<int> { };
    public List<GameObject> kingArmyObjects = new List<GameObject> { };
    public List<Mesh> lord1ArmyMesh = new List<Mesh>{ };
    public List<Button> lord1B = new List<Button> { };
    public List<CanvasGroup> lord1CG = new List<CanvasGroup> { };
    public List<int> lord1P = new List<int> { };
    public List<GameObject> lord1ArmyObjects = new List<GameObject> { };
    public List<Mesh> lord2ArmyMesh = new List<Mesh>{ };
    public List<Button> lord2B = new List<Button> { };
    public List<CanvasGroup> lord2CG = new List<CanvasGroup> { };
    public List<int> lord2P = new List<int> { };
    public List<GameObject> lord2ArmyObjects = new List<GameObject> { };

    public List<int> indexMatcher = new List<int> { };

    public static List<string> kingArmyAlias = new List<string> {"king","commander_king","knight_king_7","knight_king_6", "knight_king_5","soldier_king_4", "soldier_king_3_1", "soldier_king_3_2", "soldier_king_2_1", "soldier_king_2_2", "soldier_king_1_1", "soldier_king_1_2" };
    public static List<string> lord1ArmyAlias = new List<string> { "lord1", "commander_lord1", "knight_lord1_7", "knight_lord1_6", "knight_lord1_5", "soldier_lord1_4", "soldier_lord1_3", "soldier_lord1_2", "soldier_lord1_1" };
    public static List<string> lord2ArmyAlias = new List<string> { "lord2", "commander_lord2", "knight_lord2_7", "knight_lord2_6", "knight_lord2_5", "soldier_lord2_4", "soldier_lord2_3", "soldier_lord2_2", "soldier_lord2_1" };

    public bool loading = false;

    public int strategyPosition = -1;

    public int piecePos = -1;
    public int boardPos = -1;

    public GameObject boardPosObject;
    public GameObject piece;

    public Material baseMat;
    public Material bronzeMat;
    public Material silverMat;
    public Material goldMat;

    public Dictionary<string, int> currentboardPiecePos = new Dictionary<string, int> { };
    public List<bool> occupiedBoardPos = new List<bool> { };
    public int allotedPos = 0;

    public delegate void SaveStrategyCallBack(bool x);
    public delegate void LoadStrategyCallBack(List<Dictionary<string,int>> x);

    public delegate void CallBackDelegate(int x);
    public List<CallBack> kingCallbacks = new List<CallBack> { };
    public List<CallBack> lord1Callbacks = new List<CallBack> { };
    public List<CallBack> lord2Callbacks = new List<CallBack> { };

    private LoginClient loginClient;

    public List<Dictionary<string, int>> strategiesList=new List<Dictionary<string, int>> { };

    // Start is called before the first frame update
    void Start()
    {
        TestLoadingButton.onClick.AddListener(TestingLoad);
        loading = true;
        loginClient = GameObject.Find("LoginClient").GetComponent<LoginClient>();
        showPanel(kingArmyCG);
        hidePanel(lord1ArmyCG);
        hidePanel(lord2ArmyCG);
        hidePanel(firstStrategyCG);
        hidePanel(secondStrategyCG);
        hidePanel(thirdStrategyCG);
        Debug.Log("Panels Managed");
        armyType.onValueChanged.AddListener(delegate { armyTypeChangeListener(); });
        cancelButton.onClick.AddListener(cancelAction);
        confirmButton.onClick.AddListener(confirmAction);
        saveButton.onClick.AddListener(saveStrategyAction);
        backButton.onClick.AddListener(delegate { SceneManager.LoadScene("Home"); });
        Debug.Log("Listeners Managed");
        for(int i = 0; i < kingB.Count; i++)
        {
            CallBack callBack = new CallBack();
            callBack.callBackDelegate = kingArmyListener;
            callBack.pos = i;
            kingCallbacks.Add(callBack);
            kingB[i].onClick.AddListener(callBack.callBack);
        }

        for (int i = 0; i < lord1B.Count; i++)
        {
            CallBack callBack = new CallBack();
            callBack.callBackDelegate = lord1ArmyListener;
            callBack.pos = i;
            lord1Callbacks.Add(callBack);
            lord1B[i].onClick.AddListener(callBack.callBack);
        }

        for (int i = 0; i < lord2B.Count; i++)
        {
            CallBack callBack = new CallBack();
            callBack.callBackDelegate = lord2ArmyListener;
            callBack.pos = i;
            lord2Callbacks.Add(callBack);
            lord2B[i].onClick.AddListener(callBack.callBack);
        }
        Debug.Log("CallBacks managed");
        LoadStrategyCallBack lscb = getStrategiesCB;
        loginClient.getUserStrategies(lscb);
        AddNewButton.onClick.AddListener(CreateNewStrategy);
        firstStrategyButton.onClick.AddListener(delegate { LoadStrategy(0);});
        secondStrategyButton.onClick.AddListener(delegate { LoadStrategy(1);});
        thirdStrategyButton.onClick.AddListener(delegate { LoadStrategy(2);});
        Debug.Log("Second Listeners attached");
        //loading = false;
    }

    void TestingLoad()
    {
        Dictionary<string,int> dict=randomStrategyGenerator();
        strategiesList = new List<Dictionary<string, int>> { };
        strategiesList.Add(dict);
        LoadStrategy(0);
    }

    void getStrategiesCB(List<Dictionary<string,int>> x)
    {
        if (x != null)
        {
            this.strategiesList = x;

            if (strategiesList.Count == 1)
            {
                showPanel(firstStrategyCG);
            }
            if (strategiesList.Count == 2)
            {
                showPanel(firstStrategyCG);
                showPanel(secondStrategyCG);
            }
            if (strategiesList.Count == 3)
            {
                showPanel(firstStrategyCG);
                showPanel(secondStrategyCG);
                showPanel(thirdStrategyCG);
                hidePanel(addNewStrategyCG);
            }
        }
        loading = false;
        Debug.Log("Strategies Managed");
    }

    void cancelAction()
    {
        hidePanel(LoadingPanel);
        showPanel(StrategyListPanel);
        hidePanel(EditStrategyPanel);

        kingP = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        lord1P = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        lord2P = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        currentboardPiecePos = new Dictionary<string, int> { };
        indexMatcher = new List<int> { };
        for(int i = 0; i < 30; i++)
        {
            indexMatcher.Add(-1);
        }
        occupiedBoardPos = new List<bool> { };
        for (int i = 0; i < 30; i++)
        {
            occupiedBoardPos.Add(false);
        }
        for (int i = 0; i < 12; i++)
        {
            if (kingArmyObjects[i] != null)
            {
                Destroy(kingArmyObjects[i]);
            }
            kingArmyObjects[i]=null;
        }
        for (int i = 0; i < 9; i++)
        {
            if (lord1ArmyObjects[i] != null)
            {
                Destroy(lord1ArmyObjects[i]);
            }
            lord1ArmyObjects[i]=null;
        }
        for (int i = 0; i < 9; i++)
        {
            if (lord2ArmyObjects[i] != null)
            {
                Destroy(lord2ArmyObjects[i]);
            }
            lord2ArmyObjects[i] = null;
        }
        allotedPos = 0;
        boardPosObject.SetActive(false);
        for (int i = 0; i < kingB.Count; i++)
        {
            kingB[i].interactable = true;
        }
        for (int i = 0; i < lord1B.Count; i++)
        {
            lord1B[i].interactable = true;
        }
        for (int i = 0; i < lord2B.Count; i++)
        {
            lord2B[i].interactable = true;
        }
        strategyPosition = -1;
    }

    void saveStrategyAction()
    {
        if (allotedPos == 30)
        {
            SaveStrategyCallBack svcb = saveStrategyCB;
            loading = true;
            loginClient.SaveStrategy(currentboardPiecePos,strategyPosition,svcb);
        }
    }

    void saveStrategyCB(bool x)
    {
        if (x) {
            Debug.Log("Save Complete");
            if (strategiesList.Count > strategyPosition)
            {
                strategiesList[strategyPosition] = currentboardPiecePos;
                foreach (KeyValuePair<string, int> tempItem in currentboardPiecePos)
                {
                    Debug.Log(tempItem.Key + ":" + tempItem.Value);
                }
            }
            else
            {
                strategiesList.Add(currentboardPiecePos);
            }
                
                if (strategiesList.Count == 1)
                {
                    showPanel(firstStrategyCG);
                }
                if (strategiesList.Count == 2)
                {
                    showPanel(firstStrategyCG);
                    showPanel(secondStrategyCG);
                }
                if (strategiesList.Count == 3)
                {
                    showPanel(firstStrategyCG);
                    showPanel(secondStrategyCG);
                    showPanel(thirdStrategyCG);
                    hidePanel(addNewStrategyCG);
                }
                cancelAction();
            }
        else
        {
            Debug.Log("Save Failed");
        }
        loading = false;
    }
    
    void confirmAction()
    {
        if(boardPos!=-1 && piecePos !=-1)
        {
            int i = (boardPos+70) / 10;
            int j = boardPos % 10;
            float tempx = (j - 5) * (2.5f) + (1.25f);
            float tempy = (5 - i) * (2.5f) - (1.25f);
            if (armyType.value == 0)
            {
                kingB[piecePos].interactable = false;
                currentboardPiecePos.Add(kingArmyAlias[piecePos], boardPos);
                GameObject tempObject = Instantiate(piece, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
                tempObject.GetComponent<MeshFilter>().mesh = kingArmyMesh[piecePos];
                if (kingArmyAlias[piecePos].Split('_')[0] == "king")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = goldMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                else if (kingArmyAlias[piecePos].Split('_')[0] == "commander" || kingArmyAlias[piecePos].Split('_')[0] =="knight")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = silverMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                else if (kingArmyAlias[piecePos].Split('_')[0] == "soldier")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = bronzeMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                kingArmyObjects[piecePos] = tempObject;
                kingP[piecePos] = boardPos;
                indexMatcher[boardPos] = piecePos;
            }
            else if (armyType.value == 1)
            {
                lord1B[piecePos].interactable = false;
                currentboardPiecePos.Add(lord1ArmyAlias[piecePos],boardPos);
                GameObject tempObject = Instantiate(piece, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
                tempObject.GetComponent<MeshFilter>().mesh = lord1ArmyMesh[piecePos];
                if (lord1ArmyAlias[piecePos].Split('_')[0] == "lord1")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = goldMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                else if (lord1ArmyAlias[piecePos].Split('_')[0] == "commander" || lord1ArmyAlias[piecePos].Split('_')[0] == "knight")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = silverMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                else if (lord1ArmyAlias[piecePos].Split('_')[0] == "soldier")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = bronzeMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                lord1ArmyObjects[piecePos] = tempObject;
                lord1P[piecePos] = boardPos;
                indexMatcher[boardPos] = 12 + piecePos;
            }
            else if (armyType.value == 2)
            {
                lord2B[piecePos].interactable = false;
                currentboardPiecePos.Add(lord2ArmyAlias[piecePos], boardPos);
                GameObject tempObject = Instantiate(piece, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
                tempObject.GetComponent<MeshFilter>().mesh = lord2ArmyMesh[piecePos];
                if (lord2ArmyAlias[piecePos].Split('_')[0] == "lord2")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = goldMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                else if (lord2ArmyAlias[piecePos].Split('_')[0] == "commander" || lord2ArmyAlias[piecePos].Split('_')[0] == "knight")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = silverMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                else if (lord2ArmyAlias[piecePos].Split('_')[0] == "soldier")
                {
                    Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                    tempMaterials[1] = bronzeMat;
                    tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
                }
                lord2ArmyObjects[piecePos] = tempObject;
                lord2P[piecePos] = boardPos;
                indexMatcher[boardPos] = 21 + piecePos;
            }
            occupiedBoardPos[boardPos] = true;
            allotedPos++;
            ResetPositions();
            ResetPositions();
        }
    }

    void CreateNewStrategy()
    {
        if (strategiesList.Count < 3)
        {
            Debug.Log("Create New Strategy Activated");
            //strategyPosition = 0;
            strategyPosition = strategiesList.Count;
            hidePanel(LoadingPanel);
            hidePanel(StrategyListPanel);
            showPanel(EditStrategyPanel);
            kingP = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            lord1P=new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            lord2P = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            currentboardPiecePos = new Dictionary<string, int> { };
            indexMatcher = new List<int> { };
            for (int i = 0; i < 30; i++)
            {
                indexMatcher.Add(-1);
            }
            occupiedBoardPos = new List<bool> { };
            for(int i = 0; i < 30; i++)
            {
                occupiedBoardPos.Add(false);
            }
            kingArmyObjects = new List<GameObject> { };
            for(int i = 0; i < 12; i++)
            {
                kingArmyObjects.Add(null);
            }
            lord1ArmyObjects = new List<GameObject> { };
            for(int i = 0; i < 9; i++)
            {
                lord1ArmyObjects.Add(null);
            }
            lord2ArmyObjects = new List<GameObject> { };
            for (int i = 0; i < 9; i++)
            {
                lord2ArmyObjects.Add(null);
            }
            allotedPos = 0;
            boardPosObject.SetActive(false);
            for(int i = 0; i < kingB.Count; i++)
            {
                kingB[i].interactable = true;
            }
            for (int i = 0; i < lord1B.Count; i++)
            {
                lord1B[i].interactable = true;
            }
            for (int i = 0; i < lord2B.Count; i++)
            {
                lord2B[i].interactable = true;
            }
        }
    }

    void LoadStrategy(int x)
    {
        loading = true;
        strategyPosition = x;
        kingP = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        lord1P = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        lord2P = new List<int> { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        currentboardPiecePos = new Dictionary<string, int> { };
        indexMatcher = new List<int> { };
        for (int i = 0; i < 30; i++)
        {
            indexMatcher.Add(-1);
        }
        occupiedBoardPos = new List<bool> { };
        for (int i = 0; i < 30; i++)
        {
            occupiedBoardPos.Add(false);
        }
        kingArmyObjects = new List<GameObject> { };
        for (int i = 0; i < 12; i++)
        {
            kingArmyObjects.Add(null);
        }
        lord1ArmyObjects = new List<GameObject> { };
        for (int i = 0; i < 9; i++)
        {
            lord1ArmyObjects.Add(null);
        }
        lord2ArmyObjects = new List<GameObject> { };
        for (int i = 0; i < 9; i++)
        {
            lord2ArmyObjects.Add(null);
        }
        allotedPos = 0;
        for (int i = 0; i < kingB.Count; i++)
        {
            kingB[i].interactable = true;
        }
        for (int i = 0; i < lord1B.Count; i++)
        {
            lord1B[i].interactable = true;
        }
        for (int i = 0; i < lord2B.Count; i++)
        {
            lord2B[i].interactable = true;
        }
        boardPosObject.SetActive(false);
        List<string> keys = new List<string>(strategiesList[x].Keys);
        List<int> values = new List<int>(strategiesList[x].Values);
        int loop = 0;
        for(int i = 0; i < kingArmyAlias.Count; i++)
        {
            int tempPos = keys.IndexOf(kingArmyAlias[i]);
            kingP[i] = values[tempPos];
            int ip = (kingP[i]+70)/10;
            int jp = kingP[i] % 10;
            float tempx = (jp - 5) * (2.5f) + (1.25f);
            float tempy = (5 - ip) * (2.5f) - (1.25f);
            occupiedBoardPos[(values[tempPos])] = true;
            currentboardPiecePos.Add(kingArmyAlias[i], kingP[i]);
            allotedPos++;
            kingB[i].interactable = false;
            GameObject tempObject = Instantiate(piece, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
            tempObject.GetComponent<MeshFilter>().mesh = kingArmyMesh[i];
            if (kingArmyAlias[i].Split('_')[0] == "king")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            else if (kingArmyAlias[i].Split('_')[0] == "commander" || kingArmyAlias[i].Split('_')[0] == "knight")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = silverMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            else if (kingArmyAlias[i].Split('_')[0] == "soldier")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = bronzeMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            kingArmyObjects[i] = tempObject;
            indexMatcher[kingP[i]] = i;
        }

        for(int i = 0; i < lord1ArmyAlias.Count; i++)
        {
            int tempPos = keys.IndexOf(lord1ArmyAlias[i]);
            lord1P[i] = values[tempPos];
            int ip = (lord1P[i]+70) / 10;
            int jp = lord1P[i] % 10;
            float tempx = (jp - 5) * (2.5f) + (1.25f);
            float tempy = (5 - ip) * (2.5f) - (1.25f);
            occupiedBoardPos[(values[tempPos])] = true;
            currentboardPiecePos.Add(lord1ArmyAlias[i], lord1P[i]);
            allotedPos++;
            lord1B[i].interactable = false;
            GameObject tempObject = Instantiate(piece, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
            tempObject.GetComponent<MeshFilter>().mesh = lord1ArmyMesh[i];
            if (lord1ArmyAlias[i].Split('_')[0] == "lord1")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            else if (lord1ArmyAlias[i].Split('_')[0] == "commander" || lord1ArmyAlias[i].Split('_')[0] == "knight")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = silverMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            else if (lord1ArmyAlias[i].Split('_')[0] == "soldier")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = bronzeMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            lord1ArmyObjects[i] = tempObject;
            indexMatcher[lord1P[i]] = 12+i;
        }
        for(int i = 0; i < lord2ArmyAlias.Count; i++)
        {
            int tempPos = keys.IndexOf(lord2ArmyAlias[i]);
            lord2P[i] = values[tempPos];
            int ip = (lord2P[i]+70) / 10;
            int jp = lord2P[i] % 10;
            float tempx = (jp - 5) * (2.5f) + (1.25f);
            float tempy = (5 - ip) * (2.5f) - (1.25f);
            occupiedBoardPos[(values[tempPos])] = true;
            currentboardPiecePos.Add(lord2ArmyAlias[i], lord2P[i]);
            allotedPos++;
            lord2B[i].interactable = false;
            GameObject tempObject = Instantiate(piece, new Vector3(tempx, 0.3f, tempy), Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
            tempObject.GetComponent<MeshFilter>().mesh = lord2ArmyMesh[i];
            if (lord2ArmyAlias[i].Split('_')[0] == "lord2")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = goldMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            else if (lord2ArmyAlias[i].Split('_')[0] == "commander" || lord2ArmyAlias[i].Split('_')[0] == "knight")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = silverMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            else if (lord2ArmyAlias[i].Split('_')[0] == "soldier")
            {
                Material[] tempMaterials = tempObject.GetComponent<MeshRenderer>().materials;
                tempMaterials[1] = bronzeMat;
                tempObject.GetComponent<MeshRenderer>().materials = tempMaterials;
            }
            lord2ArmyObjects[i] = tempObject;
            indexMatcher[lord2P[i]] = 21 + i;
        }
        hidePanel(LoadingPanel);
        hidePanel(StrategyListPanel);
        showPanel(EditStrategyPanel);
        loading = false;
    }

    // Update is called once per frame
    void Update()
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
                    checkBoardIndex(hit.point.x, hit.point.z);
                }
            }
        }

        if (loading == true)
        {
            showPanel(LoadingPanel);
            hidePanel(StrategyListPanel);
            hidePanel(EditStrategyPanel);
        }
        else
        {
            hidePanel(LoadingPanel);
            if (strategyPosition > -1)
            {
                showPanel(EditStrategyPanel);
                hidePanel(StrategyListPanel);
            }
            else
            {
                showPanel(StrategyListPanel);
                hidePanel(EditStrategyPanel);
            }
        }

        if (allotedPos == 30)
        {
            saveButton.interactable = true;
        }
        else
        {
            saveButton.interactable = false;
        }
    }

    void kingArmyListener(int x)
    {
        Debug.Log("King Army:"+x);
        if (kingP[x] == -1)
        {
            if (piecePos != x)
            {
                if (piecePos != -1)
                {
                    
                    ColorBlock colors1 = kingB[piecePos].colors;
                    colors1.normalColor = new Color32(50, 190, 230, 255);
                    colors1.pressedColor= new Color32(50, 190, 230, 255);
                    colors1.selectedColor = new Color32(50, 190, 230, 255);
                    colors1.highlightedColor = new Color32(50, 190, 230, 255);
                    kingB[piecePos].colors = colors1;
                }
                piecePos = x;
                ColorBlock colors = kingB[piecePos].colors;
                colors.normalColor = new Color32(50, 110, 230, 255);
                colors.pressedColor = new Color32(50, 110, 230, 255);
                colors.selectedColor = new Color32(50, 110, 230, 255);
                colors.highlightedColor = new Color32(50, 110, 230, 255);
                kingB[piecePos].colors = colors;
            }
            else
            {
                ColorBlock colors = kingB[piecePos].colors;
                colors.normalColor = new Color32(50, 110, 230, 255);
                colors.pressedColor = new Color32(50, 190, 230, 255);
                colors.selectedColor = new Color32(50, 190, 230, 255);
                colors.highlightedColor = new Color32(50, 190, 230, 255);
                kingB[piecePos].colors = colors;
                piecePos = -1;
            }
            if (boardPos != -1)
            {
                boardPosObject.SetActive(false);
                boardPos = -1;
            }
        }
    }

    void lord1ArmyListener(int x)
    {
        Debug.Log("Lord1 Army:"+x);
        if (lord1P[x] == -1)
        {
            if (piecePos != x)
            {
                if (piecePos != -1)
                {

                    ColorBlock colors1 = lord1B[piecePos].colors;
                    colors1.normalColor = new Color32(50, 190, 230, 255);
                    colors1.pressedColor = new Color32(50, 190, 230, 255);
                    colors1.highlightedColor = new Color32(50, 190, 230, 255);
                    colors1.selectedColor = new Color32(50, 190, 230, 255);
                    lord1B[piecePos].colors = colors1;
                }
                piecePos = x;
                ColorBlock colors = lord1B[piecePos].colors;
                colors.normalColor = new Color32(50, 110, 230, 255);
                colors.pressedColor = new Color32(50, 110, 230, 255);
                colors.selectedColor = new Color32(50, 110, 230, 255);
                colors.highlightedColor = new Color32(50, 110, 230, 255);
                lord1B[piecePos].colors = colors;
            }
            else
            {
                ColorBlock colors = lord1B[piecePos].colors;
                colors.normalColor = new Color32(50, 190, 230, 255);
                colors.pressedColor = new Color32(50, 190, 230, 255);
                colors.selectedColor = new Color32(50, 190, 230, 255);
                colors.highlightedColor = new Color32(50, 190, 230, 255);
                lord1B[piecePos].colors = colors;
                piecePos = -1;
            }
            if (boardPos != -1)
            {
                boardPosObject.SetActive(false);
                boardPos = -1;
            }
        }
    }

    void lord2ArmyListener(int x)
    {
        Debug.Log("Lord2 Army:" + x);
        if (lord2P[x] == -1)
        {
            if (piecePos != x)
            {
                if (piecePos != -1)
                {

                    ColorBlock colors1 = lord2B[piecePos].colors;
                    colors1.normalColor = new Color32(50, 190, 230, 255);
                    colors1.pressedColor = new Color32(50, 190, 230, 255);
                    colors1.selectedColor = new Color32(50, 190, 230, 255);
                    colors1.highlightedColor = new Color32(50, 190, 230, 255);
                    lord2B[piecePos].colors = colors1;
                }
                piecePos = x;
                ColorBlock colors = lord2B[piecePos].colors;
                colors.normalColor = new Color32(50, 110, 230, 255);
                colors.pressedColor = new Color32(50, 110, 230, 255);
                colors.selectedColor = new Color32(50, 110, 230, 255);
                colors.highlightedColor = new Color32(50, 110, 230, 255);
                lord2B[piecePos].colors = colors;
            }
            else
            {
                ColorBlock colors = lord2B[piecePos].colors;
                colors.normalColor = new Color32(50, 190, 230, 255);
                colors.pressedColor = new Color32(50, 190, 230, 255);
                colors.selectedColor = new Color32(50, 190, 230, 255);
                colors.highlightedColor = new Color32(50, 190, 230, 255);
                lord2B[piecePos].colors = colors;
                piecePos = -1;
            }
            if (boardPos != -1)
            {
                boardPosObject.SetActive(false);
                boardPos = -1;
            }
        }
    }
    void armyTypeChangeListener()
    {
        if (armyType.value == 0)
        {
            showPanel(kingArmyCG);
            hidePanel(lord1ArmyCG);
            hidePanel(lord2ArmyCG);
        }
        else if (armyType.value == 1)
        {
            hidePanel(kingArmyCG);
            showPanel(lord1ArmyCG);
            hidePanel(lord2ArmyCG);
        }
        else if (armyType.value == 2)
        {
            hidePanel(kingArmyCG);
            hidePanel(lord1ArmyCG);
            showPanel(lord2ArmyCG);
        }
        ResetPositions();
    }

    void checkBoardIndex(float x, float y)
    {
        /*int i=0;
        if (y < 0)
        {
            y = (-1)*y;
            i = Convert.ToInt32((12.5f + y) / 2.5f);
        }
        else
        {
            i = Convert.ToInt32((12.5f - y) / 2.5f);
        }*/
        int i = Convert.ToInt32(Math.Floor((12.5f-y)/2.5f));
        int j = Convert.ToInt32(Math.Floor((12.5f+x)/(2.5f)));
        Debug.Log(i);
        Debug.Log(j);
        int tempPos = 10 * i + j-70;
        Debug.Log(tempPos);
        if (!occupiedBoardPos[tempPos])
        {
            Debug.Log("Not Occupied");
            if (boardPos != tempPos)
            {
                if (boardPos == -1)
                {
                    boardPosObject.SetActive(true);
                }
                float tempx = (j - 5) * (2.5f) + (1.25f);
                float tempy = (5 - i) * (2.5f) - (1.25f);
                boardPosObject.transform.position = new Vector3(tempx, 0.22f, tempy);
                boardPos = tempPos;
            }
            else
            {
                boardPos = -1;
                boardPosObject.SetActive(false);
            }
        }
        else
        {
            int k = indexMatcher[tempPos];
            if (k != -1)
            {
                if (k < 12)
                {
                    occupiedBoardPos[tempPos] = false;
                    kingP[k] = -1;
                    kingB[k].interactable = true;
                    Destroy(kingArmyObjects[k]);
                    kingArmyObjects[k] = null;
                    currentboardPiecePos.Remove(kingArmyAlias[k]);
                }
                else if (k < 21)
                {
                    occupiedBoardPos[tempPos] = false;
                    lord1P[k-12] = -1;
                    lord1B[k-12].interactable = true;
                    Destroy(lord1ArmyObjects[k-12]);
                    lord1ArmyObjects[k-12] = null;
                    currentboardPiecePos.Remove(lord1ArmyAlias[k-12]);
                }
                else if (k < 30)
                {
                    occupiedBoardPos[tempPos] = false;
                    lord2P[k-21] = -1;
                    lord2B[k-21].interactable = true;
                    Destroy(lord2ArmyObjects[k-21]);
                    lord2ArmyObjects[k-21] = null;
                    currentboardPiecePos.Remove(lord2ArmyAlias[k-21]);
                }
                allotedPos--;
                indexMatcher[tempPos] = -1;
                ResetPositions();
            }
        }
    }

    void ResetPositions()
    {
        if (piecePos != -1)
        {
            if (armyType.value == 0)
            {
                ColorBlock colors = kingB[piecePos].colors;
                colors.normalColor = new Color32(50, 190, 230, 255);
                colors.highlightedColor = new Color32(50, 190, 230, 255);
                colors.pressedColor = new Color32(50, 190, 230, 255);
                colors.selectedColor = new Color32(50, 190, 230, 255);
                kingB[piecePos].colors = colors;
            }
            else if (armyType.value == 1)
            {
                ColorBlock colors = lord1B[piecePos].colors;
                colors.normalColor = new Color32(50, 190, 230, 255);
                colors.highlightedColor = new Color32(50, 190, 230, 255);
                colors.pressedColor = new Color32(50, 190, 230, 255);
                colors.selectedColor = new Color32(50, 190, 230, 255);
                lord1B[piecePos].colors = colors;
            }
            else
            {
                ColorBlock colors = lord2B[piecePos].colors;
                colors.normalColor = new Color32(50, 190, 230, 255);
                colors.highlightedColor = new Color32(50, 190, 230, 255);
                colors.pressedColor = new Color32(50, 190, 230, 255);
                colors.selectedColor = new Color32(50, 190, 230, 255);
                lord2B[piecePos].colors = colors;
            }
        }
        boardPos = -1;
        piecePos = -1;
        boardPosObject.SetActive(false);
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

    public static Dictionary<string,int> randomStrategyGenerator()
    {
        Dictionary<string, int> temp = new Dictionary<string, int> { };
        List<int> occupiedSlots = new List<int> { };
        
        for(int i = 0; i < kingArmyAlias.Count; i++)
        {
            bool assigned = false;
            while (!assigned)
            {
                int tempRand=random.Next(0, 30);
                if (!occupiedSlots.Contains(tempRand))
                {
                    temp.Add(kingArmyAlias[i], tempRand);
                    occupiedSlots.Add(tempRand);
                    assigned = true;
                }
            }
        }
        for (int i = 0; i < lord1ArmyAlias.Count; i++)
        {
            bool assigned = false;
            while (!assigned)
            {
                int tempRand = random.Next(0, 30);
                if (!occupiedSlots.Contains(tempRand))
                {
                    temp.Add(lord1ArmyAlias[i], tempRand);
                    occupiedSlots.Add(tempRand);
                    assigned = true;
                }
            }
        }
        for (int i = 0; i < lord2ArmyAlias.Count; i++)
        {
            bool assigned = false;
            while (!assigned)
            {
                int tempRand = random.Next(0, 30);
                if (!occupiedSlots.Contains(tempRand))
                {
                    temp.Add(lord2ArmyAlias[i], tempRand);
                    occupiedSlots.Add(tempRand);
                    assigned = true;
                }
            }
        }
        return temp;
    }
        
    public static string RandomString(int size)
    {
        var builder = new StringBuilder(size);

        char offset = 'A';
        const int lettersOffset = 26; // A...Z or a..z: length=26  

        for (var i = 0; i < size; i++)
        {
            var @char = (char)random.Next(offset, offset + lettersOffset);
            builder.Append(@char);
        }

        return builder.ToString();
    }
}

public class CallBack{
    public int pos;
    public StrategyManagerScript.CallBackDelegate callBackDelegate;
   
    public void callBack()
    {
        callBackDelegate(pos);
    }
    
}
