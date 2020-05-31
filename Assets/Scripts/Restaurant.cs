using LitJson;
using System.Collections;
using System.Collections.Generic;

public class Restaurant
{
    public string id { get; private set; }
    public string name { get; private set; }
    public float x { get; private set; }
    public float y { get; private set; }
    public float rating { get; private set; }
    public string mood { get; private set; }
    public string brief { get; private set; }
    public string category { get; private set; }
    public bool zzimCheck { get; set; }
    public Restaurant()
    {
        
    }

    public Restaurant(JsonData i)
    {
        PutData(i);
    }

    public void PutData(JsonData i)
    {
        IDictionary dict = i;
        this.id = i["id"].ToString();
        this.name = i["name"].ToString();
        this.x = float.Parse(i["lon"].ToString());
        this.y = float.Parse(i["lat"].ToString());
        this.category = i["category"].ToString();
        this.rating = float.Parse(dict.Contains("rating") ? i["rating"].ToString() : "-1");
        this.mood = dict.Contains("mood") ? i["mood"].ToString() : null;
        this.brief = dict.Contains("brief") ? i["brief"].ToString() : null;
        this.zzimCheck = false;
    }
}
