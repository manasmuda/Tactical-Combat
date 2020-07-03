using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OppPlayerData
{
    public Dictionary<string,Dictionary<string, object>> gold;
    public Dictionary<string,GameObject> goldObjects;
    public Dictionary<string,Dictionary<string, object>> silver;
    public Dictionary<string,GameObject> silverObjects;
    public Dictionary<string,Dictionary<string, object>> bronze;
    public Dictionary<string,GameObject> bronzeObjects;

    public List<string> fort1;
    public List<GameObject> fort1Objects;
    public List<string> fort2;
    public List<GameObject> fort2Objects;

    public int posMultiplier;
    public int offset;
    public int fDir;
    public float angleYOffSet;

    public OppPlayerData()
    {
        this.bronze = new Dictionary<string, Dictionary<string, object>> { };
        this.gold = new Dictionary<string, Dictionary<string, object>> { };
        this.silver = new Dictionary<string, Dictionary<string, object>> { };
        this.goldObjects = new Dictionary<string, GameObject> { };
        this.silverObjects = new Dictionary<string, GameObject> { };
        this.bronzeObjects = new Dictionary<string, GameObject> { };
        this.fort1 = new List<string> { };
        this.fort2 = new List<string> { };
        this.fort1Objects = new List<GameObject> { };
        this.fort2Objects = new List<GameObject> { };
    }

    public void AddGold(int x, int y,string id)
    {
        Dictionary<string,object> temp = new Dictionary<string, object> { };
        temp.Add("id", id);
        temp.Add("color", "gold");
        temp.Add("posI", x);
        temp.Add("posJ", y);
        temp.Add("state", "alive");
        gold.Add(id,temp);
    }

    public void AddSilver(int x, int y, string id)
    {
        Dictionary<string, object> temp = new Dictionary<string, object> { };
        temp.Add("id", id);
        temp.Add("color", "silver");
        temp.Add("posI", x);
        temp.Add("posJ", y);
        temp.Add("state", "alive");
        silver.Add(id,temp);
    }

    public void AddBronze(int x, int y, string id)
    {
        Dictionary<string,object> temp = new Dictionary<string,object> { };
        temp.Add("id", id);
        temp.Add("color", "bronze");
        temp.Add("posI", x);
        temp.Add("posJ", y);
        temp.Add("state", "alive");
        bronze.Add(id,temp);
    }

    /*public void updatePos(string id, string type, string x, string y)
    {
        if (type == "gold")
        {
            for(int i = 0; i < gold.Count; i++)
            {
                if (gold[i]["id"] == id)
                {
                    gold[i]["posX"] = x;
                    gold[i]["posY"] = y;
                }
            }
        }
        else if (type == "silver")
        {
            for (int i = 0; i < silver.Count; i++)
            {
                if (silver[i]["id"] == id)
                {
                    silver[i]["posX"] = x;
                    silver[i]["posY"] = y;
                }
            }
        }
        else if (type == "bronze")
        {
            for (int i = 0; i < bronze.Count; i++)
            {
                if (bronze[i]["id"] == id)
                {
                    bronze[i]["posX"] = x;
                    bronze[i]["posY"] = y;
                }
            }
        }
    }

    public void updateState(string type, string id,string state,string fi)
    {
        if (type == "gold")
        {
            for (int i = 0; i < gold.Count; i++)
            {
                if (gold[i]["id"] == id)
                {
                    gold[i]["state"] = state;
                }
            }
        }
        else if (type == "silver")
        {
            for (int i = 0; i < silver.Count; i++)
            {
                if (silver[i]["id"] == id)
                {
                    silver[i]["state"] = state;
                }
            }
        }
        else if (type == "bronze")
        {
            for (int i = 0; i < bronze.Count; i++)
            {
                if (bronze[i]["id"] == id)
                {
                    bronze[i]["state"] = state;
                }
            }
        }
    }*/
}
