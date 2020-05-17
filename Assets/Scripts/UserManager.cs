using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using LitJson;
using System;

public class UserManager : MonoBehaviour
{
    private ServerManager serverManager = new ServerManager();
    public EventHandler eventHandler;
    private bool flagWakeUp = false;

    public Text mailContent;
    public Text nameContent;
    public Text birthContent;
    public Text sexContent;
    // Start is called before the first frame update
    void Start()
    {
        var pairs = new Dictionary<string, string>();
        //pairs["url"] = Login.userId + "/profile";
        //pairs["password"] = Login.userPw;
        pairs["url"] = "nsh722/profile";
        pairs["password"] = "1234";
        pairs["method"] = "GET";

        Debug.Log(pairs["url"] + pairs["password"]);

        IEnumerator enumerator = handleUnserInfo(pairs);

        StartCoroutine(enumerator);
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private IEnumerator handleUnserInfo(Dictionary<string, string> pairs)
    {
        Debug.Log("이벤트 핸들 진입");
        flagWakeUp = false;
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);
        Debug.Log("핸들러 온클릭");
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        var check = eventHandler.result as JsonData;

        Debug.Log("핸들러 종료");

    }
    public void WakeUp()
    {
        flagWakeUp = true;
    }
}
