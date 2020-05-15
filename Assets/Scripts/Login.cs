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


    public Toggle autoLogin;
    public InputField IDfield;
    public InputField PWfield;
    public Toggle auto;

    //public string checkid = "aaa";
    //public string checkpw = "1234";

    public static string userId;
    public static string userPw;

    public GameObject LoginPanel = null;
    public GameObject LoginFailed = null;

    public EventHandler eventHandler;
    private ServerManager serverManager = new ServerManager();
    private bool flagWakeUp = false;

    public void Start()
    {
        //자동로그인이 체크되어있는경우
        if(auto.isOn)
        {   //로컬에서 id값이 존재하면
            if(PlayerPrefs.HasKey("ID"))
            {
                //id,pw를 불러와서 로그인 실행
                userId = PlayerPrefs.GetString("ID");
                userPw = PlayerPrefs.GetString("PW");
                //서버로 로그인 요청을 보내는 코드 작성
                //response를 확인후 
            }
        }


        TouchScreenKeyboard.hideInput = false;
        //if (keyboard == null || !TouchScreenKeyboard.visible)
            //keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "xyz");
        LoginPanel = GameObject.Find("Login");
        LoginFailed = GameObject.Find("failed");

        LoginFailed.SetActive(false);
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
    IEnumerator enumerator;

    private IEnumerator handleLogin(Dictionary<string, string> pairs)
    {
        Debug.Log("로그인 핸들 진입");

        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        var check = eventHandler.result as JsonData;

        //자동로그인이 체크되어있으면
        if (check["code"].ToString() == "success")
        {
            if (auto.isOn)
            {
                //서버의 응답을 확인해서 success일 경우 로컬에 id, pw저장
                PlayerPrefs.SetString("ID", IDfield.text);
                PlayerPrefs.SetString("PW", PWfield.text);
                Debug.Log("실행완료");
                SceneManager.LoadScene("Main");
                //response에 따라 씬 결정
            }
            //자동로그인 체크안된경우 그냥 메인화면으로 넘김
            else
            {
                SceneManager.LoadScene("Main");
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
        this.StopCoroutine(enumerator);
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

        enumerator = handleLogin(pairs);

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

    public void kakaoLogin()
    {
        //Application.OpenURL("https://kauth.kakao.com/oauth/authorize?client_id=6c33293e24aace367218848ba3e60573&redirect_uri=https://api2.jaehyeok.kr:80/deli/v1/oauth&response_type=code");
    }

    public void WakeUp()
    {
        flagWakeUp = true;
    }
}
