using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
    Dictionary<string, string> dic = new Dictionary<string, string>();
    //public ViewPort g;
    public GameObject ReviewData;
    public GameObject photoScrollRect;
    public GameObject mainCamera;
    public string id;

    public GameObject target;


    void Start()
    {
        //arcoreDevice.SetActive(false);
        //Input.compass.enabled = true;
        //cameraObj.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
        //cameraObj.transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading, 0);
        //StartCoroutine(DoLoop());

        //StartCoroutine(loadPhoto());
    }

    // Update is called once per frame
    void Update()
    {
        //target.transform.Rotate()
        float dist = Vector3.Distance(mainCamera.transform.position, target.transform.position);
        Debug.Log(dist);
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

    private IEnumerator loadPhoto()
    {
        flagWakeUp = false;
        dic.Clear();
        dic.Add("url", "restaurant/" + id + "/photo");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Photo);
        // ADD: 로딩 중 표시
        while (!flagWakeUp)
            yield return new WaitForSeconds(0.1f);
        if (eventHandler.result == null) Debug.Log("eventHandler.result가 null이에요.");
        List<string> urls = eventHandler.result as List<string>;
        ScrollRect scrollRect = photoScrollRect.GetComponent<ScrollRect>();
        Debug.Log(urls);
        if (urls == null || urls.Count == 0)
        {
            yield break;
        }
        float y = 0;

        var fixedWidth = 1800; var fixedHeight = 300; float anchorsMinMax = 0.1507898f;//1.3, 1
        for (int i = 0; i < urls.Count; i+=2)
        {
            GameObject photoPanel = new GameObject("PhotoPanel" + i);
            photoPanel.AddComponent<CanvasRenderer>();
            RectTransform photoPanelRect = photoPanel.AddComponent<RectTransform>();
            photoPanelRect.sizeDelta = new Vector2(fixedWidth, fixedHeight);
            //photoPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(fixedWidth, fixedHeight);
            photoPanel.transform.SetParent(scrollRect.content);
            //
            GameObject insidePhoto1 = new GameObject("InsidePhoto1");
            insidePhoto1.AddComponent<CanvasRenderer>();
            RectTransform insidePhotoRect1 = insidePhoto1.AddComponent<RectTransform>();
            //insidePhotoRect1.anchorMin = new Vector2(anchorsMinMax, 1);
            //insidePhotoRect1.anchorMax = new Vector2(anchorsMinMax, 1);
            insidePhotoRect1.pivot = new Vector2(1.3f, 1);
            insidePhotoRect1.sizeDelta = new Vector2(350, 350);
            
            insidePhoto1.transform.SetParent(photoPanel.transform);
            insidePhotoRect1.localPosition = new Vector3(0, 0, 0);

            RawImage rawPhoto1 = insidePhoto1.AddComponent<RawImage>();

            IEnumerator sender = downloadPhoto(urls[i], rawPhoto1);
            while (sender.MoveNext())
            {
                object result = sender.Current;

                if (result is UnityWebRequestAsyncOperation)
                {
                    var r = (UnityWebRequestAsyncOperation)result;
                    while (!r.webRequest.isDone)
                        yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    Debug.Log(result);
                }
            }
            if (i != urls.Count - 1)
            {
                GameObject insidePhoto2 = new GameObject("InsidePhoto2");
                insidePhoto2.AddComponent<CanvasRenderer>();
                RectTransform insidePhotoRect2 = insidePhoto2.AddComponent<RectTransform>();
                insidePhotoRect2.pivot = new Vector2(1.3f, 1);
                insidePhotoRect2.sizeDelta = new Vector2(350, 350);
                
                insidePhoto2.transform.SetParent(photoPanel.transform);
                insidePhotoRect2.localPosition = new Vector3(350 + 75, 0, 0);
                RawImage rawPhoto2 = insidePhoto2.AddComponent<RawImage>();


                sender = downloadPhoto(urls[i + 1], rawPhoto2);
                while (sender.MoveNext())
                {
                    object result = sender.Current;

                    if (result is UnityWebRequestAsyncOperation)
                    {
                        var r = (UnityWebRequestAsyncOperation)result;
                        while (!r.webRequest.isDone)
                            yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        Debug.Log(result);
                    }
                }
            }
            /*
            var datas = Instantiate(PhotoData, new Vector3(0, y, 0), Quaternion.identity, scrollRect.content);
            RawImage[] photos = scrollRect.content.GetComponentsInChildren<RawImage>();

            StartCoroutine(downloadPhoto(urls[i], photos[0]));
            Debug.Log("Start Photo!");
            if(i != urls.Count - 1)
                StartCoroutine(downloadPhoto(urls[i + 1], photos[1]));

            y -= 400;
            */

        }
    }

    private IEnumerator downloadPhoto(string url, RawImage image)
    {
        Debug.Log("StartDownloadingPhoto => " + url);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            image.texture = myTexture;
            Debug.Log("Texture 생성 완료");
        }
    }
}
