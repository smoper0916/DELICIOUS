using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _MainTestScript : MonoBehaviour
{
    public GameObject arcoreDevice;
    public GameObject cameraObj;
    // Start is called before the first frame update

    Quaternion currentHeading;
    private bool flagWakeUp = false;

    public EventHandler eventHandler;
    private ServerManager serverManager = new ServerManager();
    public GameObject reviewScrollRect;

    //public ViewPort g;
    public GameObject ReviewData;

    void Start()
    {
        //arcoreDevice.SetActive(false);
        //Input.compass.enabled = true;
        //cameraObj.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
        //cameraObj.transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading, 0);
        //StartCoroutine(DoLoop());

        StartCoroutine(DoLoop2());
    }

    // Update is called once per frame
    void Update()
    {
        //currentHeading = Quaternion.Euler(0, -Input.compass.trueHeading, 0);

        //cameraObj.transform.rotation = Quaternion.Slerp(cameraObj.transform.rotation,
           // currentHeading, Time.deltaTime * 3f);
    }

    public IEnumerator DoLoop()
    {
        arcoreDevice.SetActive(true);
        while (true)
        {
            cameraObj.transform.rotation = Quaternion.Euler(0, Input.compass.magneticHeading, 0);
            Debug.Log("R: " + cameraObj.transform.rotation + ", T: " + cameraObj.transform.position);
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator DoLoop2()
    {
        flagWakeUp = false;
        Dictionary<string, string> dic = new Dictionary<string, string>();

        dic.Add("url", "restaurant/1467708077/reviews");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Reviews);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        if (eventHandler.result != null)
        {
            ReviewTabResult reviewTabResult = eventHandler.result as ReviewTabResult;

            ScrollRect scrollRect = reviewScrollRect.GetComponent<ScrollRect>();
            Debug.Log(scrollRect.content.transform.parent);
            float y = 0;

            if (reviewTabResult.reviewList.Count == 0)
            {
                var datas = Instantiate(ReviewData, new Vector3(0, y, 0), Quaternion.identity);
                Text[] texts = datas.GetComponentsInChildren<Text>();

                texts[0].text = "";
                texts[1].text = "리뷰가 없습니다.";

                datas.transform.SetParent(scrollRect.content);
            }

            foreach (Review review in reviewTabResult.reviewList)
            {
                var datas = Instantiate(ReviewData, new Vector3(0, 0, 0), Quaternion.identity, scrollRect.content);
                Text[] texts = datas.GetComponentsInChildren<Text>();

                
                texts[0].text = review.rating + "    " + review.date;
                texts[1].text = review.text;

                //datas.transform.SetParent(scrollRect.content);
                //datas.transform.localScale = new Vector3(1, 1, 1);
                //RectTransform rt = datas.GetComponent<RectTransform>();
                //rt.sizeDelta = new Vector2(datas.GetComponent<RectTransform>().rect.width, 300);
                y -= datas.GetComponent<RectTransform>().rect.height;
                
            }
        }
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
