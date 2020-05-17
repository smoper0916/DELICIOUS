using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using LitJson;
using System;

public class UserManager : MonoBehaviour
{
    private ServerManager serverManager = new ServerManager();
    public EventHandler eventHandler;
    private bool flagWakeUp = false;

    public Text mailContent;
    public Text nameContent;
    public Text ageContent;
    public Text sexContent;

    public Text UmailContent;
    public InputField CurrentPWField;
    public InputField ChangePWField;
    public InputField CheckChangePWField;

    public GameObject UpdateInfo;
    public GameObject UnMatchCurrentError;
    public GameObject UnMatchChangeError;

    // Start is called before the first frame update
    void Start()
    {
        getUserInfo();
        UpdateInfo.SetActive(false);
        UnMatchCurrentError.SetActive(false);
        UnMatchChangeError.SetActive(false);

        Debug.Log("세션"+PlayerPrefs.GetString("ID"));

    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    public void getUserInfo()
    {
        var pairs = new Dictionary<string, string>();
        pairs["url"] = Login.userId + "/profile";
        pairs["password"] = Login.userPw;
        //pairs["url"] = "nsh722/profile"; //실제 id넣어야함
        pairs["method"] = "GET";

        Debug.Log(pairs["url"] + pairs["password"]);

        IEnumerator enumerator = handleUserInfo(pairs);

        StartCoroutine(enumerator);
    }


    private IEnumerator handleUserInfo(Dictionary<string, string> pairs)
    {
        Debug.Log("이벤트 핸들 진입");
        flagWakeUp = false;
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);
        Debug.Log("핸들러 온클릭");
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        var check = eventHandler.result as JsonData;

        Debug.Log("핸들러 종료");

        mailContent.text = Login.userId; // 실제 id를 넣어야함
        nameContent.text = check["user"]["name"].ToString();
        ageContent.text = check["user"]["age"].ToString();
        if(check["user"]["sex"].ToString() == "f")
        {
            sexContent.text = "여성";
        }
        else
        {
            sexContent.text = "남성";
        }
        
    }

    public void clickUpdateInfo()
    {
        UmailContent.text = Login.userId; // 실제 id와 비교 해야함
        UpdateInfo.SetActive(true);
    }

    public void clickCompleteUpdate()
    {
        if (CurrentPWField.text == Login.userPw) // 실제 비밀번호와 비교 해야함
        {
            if (ChangePWField.text == CheckChangePWField.text)
            {

                Login.userPw = ChangePWField.text;
                if(PlayerPrefs.HasKey("PW"))
                {
                    PlayerPrefs.SetString("PW", ChangePWField.text);
                }

                var pairs = new Dictionary<string, string>();
                //pairs["url"] = Login.userId + "/profile";
                //pairs["password"] = Login.userPw;
                pairs["url"] = UmailContent.text + "/profile";
                pairs["new_password"] = ChangePWField.text;
                //pairs["password"] = CurrentPWField.text;
                pairs["method"] = "PUT";


                IEnumerator enumerator = handleUpdateInfo(pairs);

                StartCoroutine(enumerator);

            }
            else
            {
                UnMatchChangeError.SetActive(true);
                ChangePWField.text = "";
                CheckChangePWField.text = "";
            }
        }
        else
        {
            Debug.Log("현재 비밀번호 틀림");
            UnMatchCurrentError.SetActive(true);
            CurrentPWField.text = "";
        }
    }

    private IEnumerator handleUpdateInfo(Dictionary<string, string> pairs)
    {

        Debug.Log("서버로 변경내용 전송");
        flagWakeUp = false;
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);
        Debug.Log("핸들러 온클릭");
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        var check = eventHandler.result as JsonData;

        Debug.Log(check["code"].ToString());
        if (check["code"].ToString() == "success")
        {
            UpdateInfo.SetActive(false);
        }
        else
        {
            Debug.Log("뭔가 오류가 났음");
        }


    }

    public void CurrentOk()
    {
        UnMatchCurrentError.SetActive(false);
    }
    public void ChangeOk()
    {
        UnMatchChangeError.SetActive(false);
    }

    public void Logout()
    {
        if(PlayerPrefs.HasKey("ID"))
        {
            PlayerPrefs.DeleteAll();
        }
        Login.userId = "";
        Login.userPw = "";
        SceneManager.LoadScene("Login");
    }


    public void WakeUp()
    {
        flagWakeUp = true;
    }
}
