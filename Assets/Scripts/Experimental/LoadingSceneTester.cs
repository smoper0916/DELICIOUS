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

        dic.Add("url", "routes/ped");
        dic.Add("startX", "128.422101");
        dic.Add("startY", "36.138313");
        dic.Add("endX", "128.418121");
        dic.Add("endY", "36.137834");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Route);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        if (eventHandler.result != null)
        {
            if (eventHandler.result is List<WayPoint>)
            {
                List<WayPoint> jsonResult = eventHandler.result as List<WayPoint>;
                foreach (WayPoint i in jsonResult)
                {
                    Debug.Log("" + i.lat);
                    Debug.Log("" + i.lon);
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
