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

        string method = msg["method"];
        string requestUrl = msg["url"];

        msg.Remove("method");
        msg.Remove("url");

        string url = "https://api2.jaehyeok.kr:80/deli/v1/" + requestUrl;

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
        }
        else if (method.Equals("POST"))
        {
            foreach (KeyValuePair<string, string> valuePair in msg)
            {
                formData.AddField(valuePair.Key, valuePair.Value);          
            }

            www = UnityWebRequest.Post(url, formData);
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
        }
        else if (method.Equals("DELETE"))
        {
            www = UnityWebRequest.Delete(url);
        }
        else
        {
            www = null;
            yield break;
        }

        cert = new ForceAcceptAll();
        www.certificateHandler = cert;
        yield return www.SendWebRequest();

        if (method.Equals("DELETE"))
        {
            var response = "";
            if (www.responseCode == 200)
                response = "{\"code\":\"success\"}";
            else if (www.responseCode == 400)
                response = "{\"code\":\"notMatch\"}";
            else
                response = "{\"code\":\"unexpected\"}";

            JsonData obj = JsonMapper.ToObject(response);
            yield return obj;
        }
        else
        {
            if (www.downloadHandler != null)
            {
                res = www.downloadHandler.text;
                JsonData obj = JsonMapper.ToObject(res);
                yield return obj;
            }
            else
            {
                if (www.isNetworkError || www.isHttpError)
                    yield return www.error;
            }
        }
    }
}
