using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using LitJson;
using System;
public class RegisterUser : MonoBehaviour
{
    public InputField id;
    public InputField name;
    public InputField pw;
    public InputField pwCheck;
    public InputField age;

    public Toggle male;
    public Toggle female;

    public Text warningField;
    public Text checkPW;


    private ServerManager serverManager = new ServerManager();
    public EventHandler eventHandler;
    private bool flagWakeUp = false;
    public Button registerBT;

    public GameObject RegisterPanel = null;
    public GameObject SuccessPanel = null;
    public GameObject EmptyNotiPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        TouchScreenKeyboard.hideInput = false;

        RegisterPanel = GameObject.Find("Register");
        SuccessPanel = GameObject.Find("Success");
        EmptyNotiPanel = GameObject.Find("EmptyFieldNoti");
        SuccessPanel.SetActive(false);
        EmptyNotiPanel.SetActive(false);
        registerBT.interactable = false;
    }


    // Update is called once per frame
    void Update()
    {
        //뒤로 가기 누르면 어플이 종료됨
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    private IEnumerator handleRegister(Dictionary<string, string> pairs)
    {
        flagWakeUp = false;
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        var check = eventHandler.result as JsonData;

        if (check["code"].ToString() == "success")
        {
            Debug.Log("회원가입 성공");
            SuccessPanel.SetActive(true);
        }
        else
        {
            Debug.Log("회원가입 실패");
        }

    }
    //뭔지 모를 에러가뜸
    private IEnumerator handleCheckId(Dictionary<string, string> pairs)
    {
        flagWakeUp = false;
        Debug.Log("중복확인 클릭");
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Default);
        Debug.Log("이벤트 핸들러 실행");
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        Debug.Log("대기 종료");
        var check = eventHandler.result as JsonData;

        Debug.Log(check["code"].ToString());

        if(check["code"].ToString() == "success")
        {
            warningField.text = "사용할 수 있는 ID입니다.";

            //성공했을때만 버튼을 활성화시켜야함
            registerBT.interactable = true;
        }
        else if(check["code"].ToString() == "duplicated")
        {
             warningField.text = "이미 존재하는 ID입니다.";
             id.text = "";
        }
        else
        {
            warningField.text = "알 수 없는 에러";
            id.text = "";
        }
    }

    public void clickCancle()
    {
        SceneManager.LoadScene("Login");
    }

    public void clickCheck()
    {
        var pairs = new Dictionary<string, string>();
        pairs["url"] = id.text + "/check";
        pairs["method"] = "GET";
        //StartCoroutine(serverManager.SendRequest(pairs));

        IEnumerator enumerator = handleCheckId(pairs);

        this.StartCoroutine(enumerator);

        
    }
    public void clickConfirm()
    {
       
        var pairs = new Dictionary<string, string>();
        pairs["url"] = id.text + "/profile";
        pairs["method"] = "POST";
        pairs["password"] = pw.text;
        pairs["name"] = name.text;

        if (male.isOn)
        {
            pairs["sex"] = "m";
        }
        if(female.isOn)
        {
            pairs["sex"] = "f";
        }
        
        pairs["age"] = age.text;

        Debug.Log(id.text + pw.text + pwCheck.text + name.text + pairs["sex"] + pairs["age"]);

        if (id.text != "" && name.text != "" && pw.text != "" && pwCheck.text != "" && age.text != "")
        {
            if (pw.text == pwCheck.text)
            {

                IEnumerator enumerator = handleRegister(pairs);

                this.StartCoroutine(enumerator);
                Debug.Log("전송완료");
            }
            else
            {
                checkPW.text = "비밀번호가 다릅니다.";
                Debug.Log("비밀번호 불일치");
            }
        }
        else
        {
            EmptyNotiPanel.SetActive(true);
        }
     

    }
    public void clickGoLogin()
    {
        SceneManager.LoadScene("Login");
    }

    public void clickGoRegister()
    {
        EmptyNotiPanel.SetActive(false);
    }
    public void WakeUp()
    {
        flagWakeUp = true;
    }

}
