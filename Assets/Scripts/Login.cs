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


    public GameObject LoginPanel;
    public GameObject LoginFailed;

    public Text failedText;

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

                GetUserInfo();

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

    public void GetUserInfo()
    {
        var pairs = new Dictionary<string, string>();
        pairs["url"] = userId + "/profile";
        pairs["password"] = userPw;
        //pairs["url"] = "nsh722/profile"; //실제 id넣어야함
        pairs["method"] = "GET";

        Debug.Log(pairs["url"] + pairs["password"]);

        IEnumerator enumerator = HandleUserInfo(pairs);

        StartCoroutine(enumerator);
    }


    private IEnumerator HandleUserInfo(Dictionary<string, string> pairs)
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
    

    private IEnumerator HandleLogin(Dictionary<string, string> pairs, bool isKakaoAccount=false, string id = "")
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
            userId = isKakaoAccount ? id : IDfield.text;
            userPw = isKakaoAccount ? "default" : PWfield.text;

            if (auto.isOn)
            {


                PlayerPrefs.SetString("ID", IDfield.text);
                PlayerPrefs.SetString("PW", PWfield.text);

                userId = PlayerPrefs.GetString("ID");
                userPw = PlayerPrefs.GetString("PW");

                Debug.Log("실행완료");

                GetUserInfo();
                
             
            }
            //자동로그인 체크안된경우 그냥 메인화면으로 넘김
            else
            {
                GetUserInfo();
               
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

    public void OnClickLoginBtn()
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

        IEnumerator enumerator = HandleLogin(pairs);

        this.StartCoroutine(enumerator);
    }

    public void OnClickRegisterBtn()
    {
        SceneManager.LoadScene("RegisterUser");
    }
    
    public void OnClickConfirmBtnOnFailed()
    {
        IDfield.text = "";
        PWfield.text = "";
        failedText.text = "등록되지않은 사용자 이거나,\n비밀번호가 일치하지 않습니다.";
        LoginFailed.SetActive(false);
    }

    public void OnClickKakaoLoginBtn()
    {
        StartCoroutine(HandleKakaoLogin());
        
    }

    public void GetKakaoInfo()
    {
        
        kotlin.Call("UnLink");
        //kotlin.Call("GetMe");
    }

    public void WakeUp()
    {
        flagWakeUp = true;
    }

    private JsonData RequestToKakao()
    {
        string key = kotlin.Call<string>("Login");
        if (key != null)
        {
            return JsonMapper.ToObject(key);
        }
        else
            return null;
    }
    private IEnumerator HandleKakaoLogin()
    {
        JsonData kakaoResponse = RequestToKakao();
        if (kakaoResponse != null)
        {
            IDictionary kakaoDictionary = kakaoResponse["kakao_account"] as IDictionary;

            // 받은 정보를 토대로 추가 정보 요청 혹은 재요청 검토
            if (kakaoResponse["kakao_account"]["profile_needs_agreement"].ToString() == "False" || kakaoResponse["kakao_account"]["email_needs_agreement"].ToString() == "False")
            {
                // 프로필 제공 동의 혹은 이메일 제공 동의를 받지 못한 경우
                kotlin.Call("RequestMoreInfo", new string[]{ "account_email", "gender", "age_range" });
                kakaoResponse = RequestToKakao();
            }
            else if (kakaoResponse["kakao_account"]["has_email"].ToString() == "True" && !kakaoDictionary.Contains("email"))
            {
                // 이메일이 있는데도 이메일을 가져올 수 없는 경우
                kotlin.Call("RequestMoreInfo", new string[] { "account_email", "gender", "age_range" });
                kakaoResponse = RequestToKakao();
            }

            var pairs = new Dictionary<string, string>();
            var kakaoID = "k" + kakaoResponse["id"].ToString();
            pairs["url"] = kakaoID + "/check"; // 이메일로 체크할까..
            pairs["method"] = "GET";

            flagWakeUp = false;
            eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Default);
            Debug.Log("이벤트 핸들러 실행");
            while (!flagWakeUp)
                yield return new WaitForSeconds(1.0f);

            var check = eventHandler.result as JsonData;

            Debug.Log(check["code"].ToString());

            if (check["code"].ToString() == "success")
            {
                // 서버에 회원가입 요청
                pairs.Clear();
                pairs["url"] = kakaoID + "/profile";
                pairs["method"] = "POST";
                pairs["password"] = "default";
                pairs["name"] = kakaoResponse["kakao_account"]["profile"]["nickname"].ToString();
                pairs["sex"] = "None";
                pairs["age"] = "-1";
                if (kakaoResponse["kakao_account"]["has_gender"].ToString() == "True" && kakaoDictionary.Contains("gender"))
                {
                    // 성별 정보가 있다면
                    pairs["sex"] = kakaoResponse["kakao_account"]["gender"].ToString();
                }
                if (kakaoResponse["kakao_account"]["has_age_range"].ToString() == "True" && kakaoDictionary.Contains("age_range"))
                {
                    // 연령대 정보가 있다면
                    pairs["age"] = kakaoResponse["kakao_account"]["age_range"].ToString().Split('~')[0];
                }

                foreach (var i in pairs.Keys)
                {
                    Debug.Log(i + ": " + pairs[i]);
                }

                IEnumerator enumerator = HandleRegister(pairs, kakaoID);

                this.StartCoroutine(enumerator);
            }
            else if (check["code"].ToString() == "duplicated")
            {
                // 이미 가입한 계정으로 로그인 진행
                LoginByKakao(kakaoID);
            }
            else
            {
                failedText.text = "잠시 문제가 발생했습니다. 다시 한번 시도해주세요.";
                LoginFailed.SetActive(true);
            }

            
        }
        else
        {
            // 카카오 로그인 실패
            failedText.text = "카카오 계정과의 연결에 실패했습니다. 다시 시도해주세요.";
            LoginFailed.SetActive(true);
            Debug.LogError("카카오 로그인 실패");
        }
    }

    private IEnumerator HandleRegister(Dictionary<string, string> pairs, string id)
    {
        flagWakeUp = false;
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        var check = eventHandler.result as JsonData;

        if (check["code"].ToString() == "success")
        {
            Debug.Log("카카오 로그인을 통한 회원가입 완료");

            // 바로 로그인 진행
            LoginByKakao(id);
        }
        else
        {
            failedText.text = "카카오 계정을 통한 회원가입에 실패했습니다. 다시 시도해주세요.";
            LoginFailed.SetActive(true);
            Debug.Log("카카오 로그인을 통한 회원가입 실패");
        }

    }

    private void LoginByKakao(string id)
    {
        var pairs = new Dictionary<string, string>();
        pairs["url"] = "user/auth";
        pairs["method"] = "POST";
        pairs["id"] = id;
        pairs["password"] = "default";

        IEnumerator enumerator = HandleLogin(pairs, true, id);

        this.StartCoroutine(enumerator);
    }
}
