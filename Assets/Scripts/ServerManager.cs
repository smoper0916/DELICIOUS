using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
    private Text res;

   
    public IEnumerator SendMsg(string x, string y, int radius) // Post 방식 서버 연동
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("x=128.622397&y=34.889456&radius=500"));
        ////formData.Add(new MultipartFormDataSection("my file data, myfile.txt")); // 파일도 보낼 수 있나봄
        WWWForm formData = new WWWForm();
        //formData.AddField("x", x);
        //formData.AddField("y", y);
        //formData.AddField("radius", radius);
        formData.AddField("x", "128.622397");
        formData.AddField("y", "34.889456");
        formData.AddField("radius", 500);

        UnityWebRequest www = UnityWebRequest.Post("https://api1.codns.com:80/get_surroundings", formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            res.text = www.downloadHandler.text;
            JsonData obj = JsonMapper.ToObject(res.text);

            for (int i = 0; i < obj["contents"].Count; i++)
            {

                Debug.Log(obj["contents"][i]["name"].ToString());
            }

        }
    }
}
