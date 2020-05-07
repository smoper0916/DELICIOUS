using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


public class ServerManager : MonoBehaviour
{
    public Text res;

    public IEnumerator SendRequest(Dictionary<string, string> msg) // Post 방식 서버 연동
    {
        WWWForm formData = new WWWForm();
        UnityWebRequest www;

        string method = msg["method"];
        string requestUrl = msg["url"];

        msg.Remove("method");
        msg.Remove("url");

        string url = "https://api2.jaehyeok.kr/deli/v1/" + requestUrl;

        if (method.Equals("GET"))
        {
            url = url + "?";
            foreach (KeyValuePair<string, string> valuePair in msg)
            {
                string k = valuePair.Key;
                string v = valuePair.Value;
                url = url + k + "=" + v + "&";
            }
            url = url.Substring(0, url.Length - 1);

            www = UnityWebRequest.Get(url);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

                Debug.Log(www.error);
            }

            else
            {
                res.text = www.downloadHandler.text;
                JsonData obj = JsonMapper.ToObject(res.text);

                yield return obj;
            }
        }
        else if (method.Equals("POST"))
        {
            foreach (KeyValuePair<string, string> valuePair in msg)
            {
                formData.AddField(valuePair.Key, valuePair.Value);          
            }

            www = UnityWebRequest.Post(url, formData);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

                Debug.Log(www.error);
            }

            else
            {
                res.text = www.downloadHandler.text;
                JsonData obj = JsonMapper.ToObject(res.text);

                yield return obj;
            }
        }
        //else if (method.Equals("PUT"))
        //{

        //}
        //else if (method.Equals("DELETE"))
        //{

        //}
    }
}
