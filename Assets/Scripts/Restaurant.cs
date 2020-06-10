using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restaurant
{
    public string id { get; private set; }
    public string name { get; set; }
    public float x { get; private set; }
    public float y { get; private set; }
    public float rating { get; set; }
    public string mood { get; private set; }
    public string brief { get; private set; }
    public string category { get; private set; }
    public bool zzimCheck { get; set; }
    public int rank { get; private set; }


    public Restaurant()
    {
        
    }

    

    public Restaurant(JsonData i)
    {
        PutData(i);
    }

    public Restaurant(string id, string name, string rating)
    {
        this.id = id;
        this.name = name;
        this.rating = float.Parse(rating);
    }

    public void PutData(JsonData i)
    {
        IDictionary dict = i;
        this.id = i["id"].ToString();
        this.name = i["name"].ToString();
        this.x = float.Parse(i["lon"].ToString());
        this.y = float.Parse(i["lat"].ToString());
        this.category = i["category"].ToString();
        try
        {
            this.rating = float.Parse(dict.Contains("rating") ? i["rating"].ToString() : "-1");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            this.rating = -1f;
        }
        this.rank = int.Parse(dict.Contains("rank") ? i["rank"].ToString() : "-1");
        this.mood = dict.Contains("mood") ? i["mood"].ToString() : null;
        this.brief = dict.Contains("brief") ? i["brief"].ToString() : null;
        this.zzimCheck = false;
    }
}
