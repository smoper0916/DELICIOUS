using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;

public class ServerManager
{
    public string res;
    private CertificateHandler cert;

    public IEnumerator SendRequest(Dictionary<string, string> msg) // Post 방식 서버 연동
    {
        WWWForm formData = new WWWForm();
        UnityWebRequest www;
        cert = new ForceAcceptAll();

        string method = msg["method"];
        string requestUrl = msg["url"];

        msg.Remove("method");
        msg.Remove("url");

        string url = "https://api3.jaehyeok.kr:80/deli/v1/" + requestUrl;

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
            www.certificateHandler = cert;

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

                Debug.Log(www.error);
            }

            else
            {
                res = www.downloadHandler.text;
                JsonData obj = JsonMapper.ToObject(res);

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
            www.certificateHandler = cert;

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

                Debug.Log(www.error);
            }

            else
            {
                res = www.downloadHandler.text;
                JsonData obj = JsonMapper.ToObject(res);
                Debug.Log(res);
                yield return obj;
            }
        }
        else if (method.Equals("PUT"))
        {
            var strData = "{";
            var cnt = 0;
            foreach (KeyValuePair<string, string> valuePair in msg)
            {
                strData += "\"" + valuePair.Key + "\":\"" + valuePair.Value + "\"";
                if(cnt != msg.Count - 1)
                {
                    strData += ",";
                }
            }
            strData += "}";

            
            www = UnityWebRequest.Put(url, strData);
            www.certificateHandler = cert;

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

                Debug.Log(www.error);
            }

            else
            {
                res = www.downloadHandler.text;
                Debug.Log(res);
                JsonData obj = JsonMapper.ToObject(res);

                yield return obj;
            }
        }
        else if (method.Equals("DELETE"))
        {
            cert = new ForceAcceptAll();
            www = UnityWebRequest.Delete(url);
            www.certificateHandler = cert;

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {

                Debug.Log(www.error);
            }

            else
            {
                Debug.Log(www.responseCode);
                JsonData obj = JsonMapper.ToObject("{\"code\":\"success\"}");
                Debug.Log(obj.ToString());
                yield return obj;
            }
        }
    }
}
