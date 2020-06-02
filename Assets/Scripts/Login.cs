using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using LitJson;
using System;

public class Login : MonoBehaviour
{
    TouchScreenKeyboard keyboard;
    AndroidJavaObject kotlin;
    AndroidJavaObject kotlin2;

    public Toggle autoLogin;
    public InputField IDfield;
    public InputField PWfield;
    public Toggle auto;

    //public string checkid = "aaa";
    //public string checkpw = "1234";

    public static string userId;
    public static string userPw;
    public static string name;
    public static string age;
    public static string sex;


    public GameObject LoginPanel = null;
    public GameObject LoginFailed = null;

    public EventHandler eventHandler;
    private ServerManager serverManager = new ServerManager();
    private bool flagWakeUp = false;

    private void Awake()
    {
        kotlin = new AndroidJavaObject("com.delicious.ohtasty.KakaoPlugin");
    }

    public void Start()
    {
        Debug.Log(PlayerPrefs.GetString("PW"));
        //자동로그인이 체크되어있는경우
        if(auto != null && auto.isOn)
        {   //로컬에서 id값이 존재하면
            
            if(PlayerPrefs.HasKey("ID"))
            {
        
                //id,pw를 불러와서 로그인 실행
                userId = PlayerPrefs.GetString("ID");
                userPw = PlayerPrefs.GetString("PW");
                //서버로 로그인 요청을 보내는 코드 작성
                //response를 확인후 

                getUserInfo();

            }
        }




        TouchScreenKeyboard.hideInput = false;
        //if (keyboard == null || !TouchScreenKeyboard.visible)
            //keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "xyz");
        //LoginPanel = GameObject.Find("Login");
        //LoginFailed = GameObject.Find("failed");

        LoginFailed.SetActive(false);

        Debug.Log("[[[[[[[[[[[[ : "+kotlin.Get<string>("tt"));
    }

    public void getUserInfo()
    {
        var pairs = new Dictionary<string, string>();
        pairs["url"] = userId + "/profile";
        pairs["password"] = userPw;
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

        Debug.Log("사용자 정보요청" + check["user"]["name"].ToString() + check["user"]["age"].ToString());

        name = check["user"]["name"].ToString();
        age = check["user"]["age"].ToString();
        if (check["user"]["sex"].ToString() == "f")
        {
            sex = "여성";
        }
        else if(check["user"]["sex"].ToString() == "m")
        {
            sex = "남성";
        }
        SceneManager.LoadScene("Main");
    }

    public void Update()
    {
        //eventHandler = new EventHandler();

        //뒤로 가기 누르면 어플이 종료됨
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

    }
    

    private IEnumerator handleLogin(Dictionary<string, string> pairs)
    {
        flagWakeUp = false;
        Debug.Log("로그인 핸들 진입");

        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        var check = eventHandler.result as JsonData;

        //자동로그인이 체크되어있으면
        if (check["code"].ToString() == "success")
        {
            //로그인 성공시 세션을 위해 스태틱변수에 유저 정보저장
            userId = IDfield.text;
            userPw = PWfield.text;

            if (auto.isOn)
            {


                PlayerPrefs.SetString("ID", IDfield.text);
                PlayerPrefs.SetString("PW", PWfield.text);

                userId = PlayerPrefs.GetString("ID");
                userPw = PlayerPrefs.GetString("PW");

                Debug.Log("실행완료");

                getUserInfo();
                
             
            }
            //자동로그인 체크안된경우 그냥 메인화면으로 넘김
            else
            {
                getUserInfo();
               
                //메인화면 씬으로 전환
            }
        }
        else if(check["code"].ToString() == "unexpected")
        {
            Debug.Log("500 Error");
            LoginFailed.SetActive(true);
        }
        else
        {
            Debug.Log("비밀번호 오류");
            LoginFailed.SetActive(true);

        }
        //this.StopCoroutine(enumerator);
    }

    public void clickLogin()
    {
        //flagWakeUp = false;
        //eventHandler = new EventHandler();
        //Debug.Log(IDfield.text + PWfield.text);



        //if (IDfield.text == checkid && PWfield.text == checkpw)
        //{
        //    userID = IDfield.text;
        //    userPW = PWfield.text;
        //    SceneManager.LoadScene("complete");
        //}
        //else
        //{
        //    LoginFailed.SetActive(true);

        //}
        //서버로 로그인 요청을 보내는 부분
        var pairs = new Dictionary<string, string>();
        pairs["url"] = "user/auth";
        pairs["method"] = "POST";
        pairs["id"] = IDfield.text;
        pairs["password"] = PWfield.text;

        IEnumerator enumerator = handleLogin(pairs);

        this.StartCoroutine(enumerator);
    }

    public void clickRegister()
    {
        SceneManager.LoadScene("RegisterUser");
    }
    
    public void failed()
    {
        IDfield.text = "";
        PWfield.text = "";
        LoginFailed.SetActive(false);
    }

    public void KakaoLogin()
    {
        try
        {
            //kotlin.Call("Login");
            string key = kotlin.Call<string>("Login");
            Debug.Log("로그인 지남.");
            Debug.Log("Key : ==================" + key);
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }
        
    }

    public void GetKakaoInfo()
    {
        kotlin.Call("GetMe");
    }

    public void WakeUp()
    {
        flagWakeUp = true;
    }
}
