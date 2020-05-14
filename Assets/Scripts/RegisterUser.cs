using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LitJson;
public class RegisterUser : MonoBehaviour
{
    public InputField id;
    public InputField name;
    public InputField pw;
    public InputField pwCheck;

    public ServerManager serverManager = new ServerManager();
    public EventHandler eventHandler;
    private bool flagWakeUp = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private IEnumerator handleRegister(Dictionary<string, string> pairs)
    {
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
    }
    public void clickCancle()
    {
        SceneManager.LoadScene("Login");
    }

    public void clickCheck()
    {
        var pairs = new Dictionary<string, string>();
        pairs["url"] = id.text;
        pairs["method"] = "GET";
        StartCoroutine(serverManager.SendRequest(pairs));

        var check = eventHandler.result as JsonData;
    }
    public void clickConfirm()
    {
       
        var pairs = new Dictionary<string, string>();
        pairs["url"] = id.text + "/profile";
        pairs["method"] = "POST";
        pairs["password"] = pw.text;
        pairs["name"] = name.text;
        pairs["sex"] = "f";
        pairs["age"] = "25";
        
        var check = eventHandler.result as JsonData;
        if (pw.text == pwCheck.text)
        {
            StartCoroutine(serverManager.SendRequest(pairs));
            Debug.Log("실행완료");
        }
        else
        {

        }
        
    }

}
