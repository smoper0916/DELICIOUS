using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSceneTester : MonoBehaviour
{
    public EventHandler eventHandler;
    private ServerManager serverManager = new ServerManager();

    public bool flagWakeUp = false;

    private IEnumerator waitThenCallback(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        SceneLoader.Instance.LoadScene("Login");
    }

    public void EventHandlerTest()
    {
        flagWakeUp = false;
        StartCoroutine(waitThenCallback(
            1,
            () =>
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();

                dic.Add("url", "restaurant/37275398/photo");
                dic.Add("method", "GET");

                eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Photo);
            }
            )
           );
    }

    public void WakeUp()
    {
        flagWakeUp = true;
    }
}
