using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using LitJson;

public class PostBehaviour : MonoBehaviour
{
    private Text res;
    private Button btn;
    public ServerManager serverManager;

    private CertificateHandler cert;

    public CoroutineManager coroutineManager = null;

    public bool isFirst = true;

    void Start()
    {
        this.serverManager = new ServerManager();

        //btn = GameObject.Find("ConnectBtn").GetComponent<Button>();
        //res = GameObject.Find("ConnectTxt").GetComponent<Text>();
        //btn.onClick.AddListener(OnClick);

        var pairs = new Dictionary<string, string>();
        var serverManager = new ServerManager();
        pairs["url"] = "smoper123459@dfjfdk.com/profile";
        pairs["method"] = "GET";
        pairs["password"] = "rkskkgkd1564";
        pairs["age"] = "12";
        pairs["sex"] = "f";
        pairs["name"] = "박갑수dkdk";

        IEnumerator sender = this.serverManager.SendRequest(pairs);

        coroutineManager = new CoroutineManager(this, sender);

        /*
        while (sender.MoveNext())
        {
            object result = sender.Current;

            if (result is UnityWebRequestAsyncOperation)
            {
                var r = (UnityWebRequestAsyncOperation)result;
                while (!r.webRequest.isDone)
                {
                    Debug.Log(r.isDone);
                }
                var res = r.webRequest.downloadHandler.text;
                JsonData obj = JsonMapper.ToObject(res);
                Debug.Log(res);
            }
            else if (result is JsonData)
            {
                Debug.Log((JsonData)result);
            }
            else
            {
                Debug.Log(result);
            }
        }
        */

    }

    void OnClick()
    {
        res.text = "헤헤헤";
        //StartCoroutine(SendMsg());
    }

    

    private void Update()
    {
        if (isFirst && coroutineManager.isDone)
        {
            if (coroutineManager.result != null)
            {
                if (coroutineManager.result is JsonData)
                {
                    JsonData jsonResult = (JsonData)(coroutineManager.result);
                    Debug.Log("In Update : " + jsonResult["code"]);
                    
                }
                else
                {
                    Debug.Log("In Update : " + coroutineManager.result);
                }
            }
            isFirst = false;
        }

    }

    

    IEnumerator SendMsg() // Post 방식 서버 연동
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("x=128.622397&y=34.889456&radius=500"));
        ////formData.Add(new MultipartFormDataSection("my file data, myfile.txt")); // 파일도 보낼 수 있나봄
        WWWForm formData = new WWWForm();
        formData.AddField("x", "128.595835");
        formData.AddField("y", "35.869171");
        formData.AddField("radius", 100);
         
        cert = new ForceAcceptAll();

        UnityWebRequest www = UnityWebRequest.Post("https://api1.codns.com:80/get_surroundings", formData);

        www.certificateHandler = cert;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            res.text = www.error;
            Debug.Log(www.error);
        }
        else
        {
            res.text = www.downloadHandler.text;
            Debug.Log(res.text);
            JsonData obj = JsonMapper.ToObject(res.text);
                for (int i = 0; i < obj["contents"].Count; i++)
            {

                //Debug.Log(obj["contents"][i]);
            }

            //cert.Dispose();
        }
    }
}
