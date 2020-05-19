using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
public class Test : MonoBehaviour
{
    public ServerManager server = new ServerManager();
    // Start is called before the first frame update

    void Start()
    {
       
    }

    public void Onclick()
    { 

        JsonData jsonData = new JsonData();
        Dictionary<string, string> msg = new Dictionary<string, string>();

        msg.Add("lat", "35.870649");
        msg.Add("lon", "128.593874");
        msg.Add("radius", "200");
        msg.Add("method", "GET");
        msg.Add("url", "restaurants/near");
        StartCoroutine(this.server.SendRequest(msg));

        Debug.Log(jsonData["restaurants"].ToString());
    }
}
