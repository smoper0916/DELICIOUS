using LitJson;
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
        Input.gyro.enabled = true;
        StartCoroutine(DoLoop());
    }

    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator DoLoop()
    {
        while (true)
        {
            var heading = Math.Atan2(Input.gyro.attitude.x, Input.gyro.attitude.z);
            var declination = 0.145735;
            heading += declination;

            if (heading < 0) heading += 2 * Math.PI;
            if (heading > 2 * Math.PI) heading -= 2 * Math.PI;

            var headingDegress = heading * 180 / Math.PI;

            //headingFiltered = headingF
            Debug.Log(headingDegress);
            yield return new WaitForSeconds(1);
        }
    }
    public IEnumerator DoLoop2()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();

        dic.Add("url", "restaurant/1476730341/reviews");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Reviews);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        if (eventHandler.result != null)
        {
            if (eventHandler.result is ReviewTabResult)
            {
                ReviewTabResult jsonResult = (ReviewTabResult)(eventHandler.result);
                Debug.Log("AA -> " + jsonResult.reviewList);
                foreach (Review i in jsonResult.reviewList)
                {
                    Debug.Log("" + i.name);
                    Debug.Log("" + i.rating);
                    Debug.Log("" + i.text);
                }
            }
            else
            {
                Debug.Log("BB : " + eventHandler.result);
            }
        }
    }

    public void OnClick()
    {
        SceneLoader.Instance.LoadScene("Login");
    }

    public void EventHandlerTest()
    {
        flagWakeUp = false;
        StartCoroutine(DoLoop2());


    }

    public void WakeUp()
    {
        flagWakeUp = true;
    }
}
