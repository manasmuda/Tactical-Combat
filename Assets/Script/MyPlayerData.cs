using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerData
{
    public string playerId;

    public Dictionary<string, object> king;
    public GameObject kingObject;
    public Dictionary<string, object> lord1;
    public GameObject lord1Object;
    public Dictionary<string, object> lord2;
    public GameObject lord2Object;
    public Dictionary<string, object> commanderk;
    public GameObject commanderkObject;
    public Dictionary<string, object> commanderl1;
    public GameObject commanderl1Object;
    public Dictionary<string, object> commanderl2;
    public GameObject commanderl2Object;
    public Dictionary<string,Dictionary<string, object>> knights;
    public Dictionary<string,GameObject> knightObjects;
    public Dictionary<string,Dictionary<string, object>> soldiers;
    public Dictionary<string,GameObject> soldierObjects;

    public Dictionary<int, string> positionIdMatcher;

    public List<string> fort1;
    public List<GameObject> fort1Objects;
    public List<string> fort2;
    public List<GameObject> fort2Objects;

    public int posMultiplier;
    public int offset;
    public int fDir;
    public float angleYOffSet;

    public MyPlayerData(string id)
    {
        this.playerId = id;
        this.knights = new Dictionary<string, Dictionary<string, object>> { };
        this.soldiers = new Dictionary<string, Dictionary<string, object>> { };
        this.knightObjects = new Dictionary<string, GameObject> { };
        this.soldierObjects = new Dictionary<string, GameObject> { };
        this.positionIdMatcher = new Dictionary<int, string> { };
        this.fort1 = new List<string> { };
        this.fort2 = new List<string> { };
        this.fort1Objects = new List<GameObject> { };
        this.fort2Objects = new List<GameObject> { };
    }

    public void InitializeKing(int x, int y,string id)
    {
        king = new Dictionary<string, object> { };
        king.Add("id", id);
        king.Add("color", "gold");
        king.Add("teamId", playerId);
        king.Add("posI", x);
        king.Add("posJ", y);
        king.Add("state", "alive");
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void InitializeLord1(int x, int y,string id)
    {
        lord1 = new Dictionary<string, object> { };
        lord1.Add("id", id);
        lord1.Add("color", "gold");
        lord1.Add("teamId", playerId);
        lord1.Add("posI", x);
        lord1.Add("posJ", y);
        lord1.Add("state", "alive");
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void InitializeLord2(int x, int y,string id)
    {
        lord2 = new Dictionary<string, object> { };
        lord2.Add("id", id);
        lord2.Add("color", "gold");
        lord2.Add("teamId", playerId);
        lord2.Add("posI", x);
        lord2.Add("posJ", y);
        lord2.Add("state", "alive");
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void InitializeCommanderK(int x, int y,string id)
    {
        commanderk = new Dictionary<string, object> { };
        commanderk.Add("id", id);
        commanderk.Add("color", "silver");
        commanderk.Add("teamId", playerId);
        commanderk.Add("posI", x);
        commanderk.Add("posJ", y);
        commanderk.Add("state", "alive");
        commanderk.Add("serves", "king");
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void InitializeCommanderL1(int x, int y,string id)
    {
        commanderl1 = new Dictionary<string, object> { };
        commanderl1.Add("id", id);
        commanderl1.Add("color", "silver");
        commanderl1.Add("teamId", playerId);
        commanderl1.Add("posI", x);
        commanderl1.Add("posJ", y);
        commanderl1.Add("state", "alive");
        commanderl1.Add("serves","lord1");
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void InitializeCommanderL2(int x, int y,string id)
    {
        commanderl2 = new Dictionary<string, object> { };
        commanderl2.Add("id", id);
        commanderl2.Add("color", "silver");
        commanderl2.Add("teamId", playerId);
        commanderl2.Add("posI", x);
        commanderl2.Add("posJ", y);
        commanderl2.Add("state", "alive");
        commanderl2.Add("serves", "lord2");
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void addSoldier(int x, int y, int power, string serves,string id)
    {
        Dictionary<string, object> dict = new Dictionary<string,object> { };
        dict.Add("id", id);
        dict.Add("posI", x);
        dict.Add("posJ", y);
        dict.Add("power", power);
        dict.Add("serves", serves);
        dict.Add("color", "bronze");
        dict.Add("state", "alive");
        soldiers.Add(id,dict);
        positionIdMatcher.Add(x * 10 + y, id);
    }

    public void addKnight(int x, int y, int power, string serves,string id)
    {
        Dictionary<string, object> dict = new Dictionary<string, object> { };
        dict.Add("id", id);
        dict.Add("posI", x);
        dict.Add("posJ", y);
        dict.Add("power", power);
        dict.Add("serves", serves);
        dict.Add("color", "silver");
        dict.Add("state", "alive");
        knights.Add(id,dict);
        positionIdMatcher.Add(x * 10 + y, id);
    }


    /*public void updatePos(int pos, string type, string x, string y)
    {
        if (type == "king")
        {
            king["posX"] = x;
            king["posY"] = y;
            return king["id"];
        }
        else if (type == "lord1")
        {
            lord1["posX"] = x;
            lord2["posY"] = y;
            return lord1["id"];
        }
        else if (type == "lord2")
        {
            lord2["posX"] = x;
            lord2["posY"] = y;
            return lord2["id"];
        }
        else if (type == "commanderk")
        {
            commanderk["posX"] = x;
            commanderk["posY"] = y;
            return commanderk["id"];
        }
        else if (type == "commanderl1")
        {
            commanderl1["posX"] = x;
            commanderl1["posY"] = y;
            return commanderl1["id"];
        }
        else if (type == "commanderl2")
        {
            commanderl2["posX"] = x;
            commanderl2["posY"] = y;
            return commanderl2["id"];
        }
        else if (type == "knight")
        {
            knights[pos]["posX"] = x;
            knights[pos]["posY"] = y;
            return knights[pos]["id"];
        }
        else if (type == "soldier")
        {
            soldiers[pos]["posX"] = x;
            soldiers[pos]["posY"] = y;
            return soldiers[pos]["id"];
        }
        else return "0";
    }

    public string updateState(string type, int pos)
    {
        if (type == "commanderl1")
        {
            commanderl1["state"] = "injure";
            return commanderl1["id"];
        }
        else if (type == "commanderl2")
        {
            commanderl2["state"] = "injure";
            return commanderl2["id"];
        }
        else if (type == "knight")
        {
            if (knights[pos]["serves"] != "king")
            {
                knights[pos]["state"] = "injure";
                return knights[pos]["id"];
            }
            else
            {
                knights[pos]["state"] = "dead";
                knights.RemoveAt(pos);
                return knights[pos]["id"];
            }

        }
        else if (type == "soldier")
        {
            if (soldiers[pos]["serves"] != "king")
            {
                soldiers[pos]["state"] = "injure";
                return soldiers[pos]["id"];
            }
            else
            {
                soldiers[pos]["state"] = "dead";
                soldiers.RemoveAt(pos);
                return soldiers[pos]["id"];
            }
        }
        else return "0";
    }*/
}
