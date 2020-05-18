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
    public GameObject MyHistory;
    

    public GameObject historyData;
    public GameObject scrollView;
    Text[] historyArr;
    //Image navi;

    int num = 10;
    // Start is called before the first frame update
    void Start()
    {

        ScrollRect scroll = scrollView.GetComponent<ScrollRect>();
        historyArr = historyData.GetComponentsInChildren<Text>();
        //navi = historyData.transform.Find("Image").GetComponent<Image>();
        float f = 0;
        
        for(int i = 0;i<num;i++)
        {
            var Object = Instantiate(historyData,new Vector3(0,f,0),Quaternion.identity);
            Object.transform.SetParent(scroll.content);

            historyArr[0].text = i.ToString();
            historyArr[1].text = i.ToString();
            historyArr[2].text = i.ToString();
            historyArr[3].text = i.ToString();
            historyArr[4].text = i.ToString();
            //navi.sprite = Resources.Load("btnNavigating", typeof(Sprite)) as Sprite;

            Debug.Log(historyArr);
            f -= 203.8f;
        }

        mailContent.text = Login.userId;

        nameContent.text = Login.name;
        ageContent.text = Login.age;
      
        sexContent.text = Login.sex;



        UpdateInfo.SetActive(false);
        UnMatchCurrentError.SetActive(false);
        UnMatchChangeError.SetActive(false);
        MyHistory.SetActive(false);

        Debug.Log("세션"+PlayerPrefs.GetString("ID"));

    }

    // Update is called once per frame
    void Update()
    {
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    if (Input.GetKey(KeyCode.Escape))
        //    {
        //        SceneManager.LoadScene("Main");
        //    }
        //}
    }
    public void clickXbtAthis()
    {
        MyHistory.SetActive(false);
    }
    public void clickHistory()
    {

        MyHistory.SetActive(true);

    }

    public void goBackMain()
    {
        SceneManager.LoadScene("Main");
    }
    public void goBackInfo()
    {
        UpdateInfo.SetActive(false);
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
