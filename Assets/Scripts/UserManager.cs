﻿using System.Collections;
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
    public GameObject detailedRestaurant;
    public GameObject showInfo;

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

    public List<GameObject> list = new List<GameObject>();

    public GameObject historyData;
    public GameObject scrollView;

    public AnchorManager.State previousState = AnchorManager.State.MyInfo;

    //Image navi;

    int num = 10;
    // Start is called before the first frame update
    void Start()
    {
        
        //Login.userId = "1"; Login.userPw = "1";

        mailContent.text = Login.userId;

        nameContent.text = Login.nickname;
        ageContent.text = Login.age;
      
        sexContent.text = Login.sex;
        

        UpdateInfo.SetActive(false);
        UnMatchCurrentError.SetActive(false);
        UnMatchChangeError.SetActive(false);
        MyHistory.SetActive(false);
        detailedRestaurant.SetActive(false);


        var pairs = new Dictionary<string, string>();
        pairs["url"] = Login.userId + "/history"; //나중에 유저 id로 바꾸기
        pairs["method"] = "GET";

        IEnumerator enumerator = handleZzimInfo(pairs);

        StartCoroutine(enumerator);

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

        // 상태 변경 감지
        if (AnchorManager.currentState != previousState)
        {
            if (previousState == AnchorManager.State.Detail && AnchorManager.currentState == AnchorManager.State.MyInfo)
            {
                showInfo.SetActive(true);
                MyHistory.SetActive(true);
            }
            else if (previousState == AnchorManager.State.MyInfo && AnchorManager.currentState == AnchorManager.State.Detail)
            {
                AnchorManager.showCheck = true;
                detailedRestaurant.SetActive(true);
                

                MyHistory.SetActive(false);
                showInfo.SetActive(false);
            }
            else
            {
                // 아직 잡지 못한 예외
                ToastMaker.instance.ShowToast("Update: 상태 변화 불일치 중 예외 발생! = " + previousState + " => " + AnchorManager.currentState);
            }
        }
        previousState = AnchorManager.currentState;
    }
    public void clickXbtAthis()
    {
        MyHistory.SetActive(false);
    }
    public void clickHistory()
    {

        MyHistory.SetActive(true);



        //var pairs = new Dictionary<string, string>();
        //pairs["url"] = "1" + "/history";
        //pairs["method"] = "GET";

        //IEnumerator enumerator = handleZzimInfo(pairs);

        //StartCoroutine(enumerator);


    }

    private IEnumerator handleZzimInfo(Dictionary<string, string> pairs)
    {

        Debug.Log("서버로 변경내용 전송");
        flagWakeUp = false;
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Default);
        Debug.Log("핸들러 온클릭");
        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);
        var check = eventHandler.result as JsonData;
        ScrollRect scroll = scrollView.GetComponent<ScrollRect>();
        float f = 200;
        int num = 1;

        foreach (JsonData pair in check["zzim_list"])
        {
            //IDictionary i = pair;
            //Debug.Log(pair["res_name"]);

            //foreach (var k in i.Keys)
            //{
            //    Debug.Log(k);
            //}

            GameObject historyObj = Instantiate(historyData, new Vector3(0, f, 0), Quaternion.identity, scroll.content);

            // 상세조회 버튼
            historyObj.GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(clickList(pair["res_code"].ToString(), pair["res_name"].ToString())); });
            historyObj.GetComponentsInChildren<Button>()[1].onClick.AddListener(delegate { clickNavi(pair["res_lon"].ToString(), pair["res_lat"].ToString()); });
            list.Add(historyObj);

            Text[] historyArr = historyObj.GetComponentsInChildren<Text>();

            historyArr[0].text = num.ToString();
            historyArr[1].text = pair["res_name"].ToString();
            historyArr[2].text = pair["pup_regdate"].ToString();
            historyArr[3].text = pair["pup_regtime"].ToString();
            if(pair["pup_zzim"].ToString() == "1")
                historyArr[4].text = "O";
            else
                historyArr[4].text = "X";
            f -= 203.8f;
            num += 1;
            
        }

        




    }

    public Button btn;

    public IEnumerator clickList(string id, string name)
    {
        Debug.Log(id);

        flagWakeUp = false;
        var pairs = new Dictionary<string, string>();
        pairs["url"] =  id + "/info";
        pairs["method"] = "GET";
        eventHandler.onClick(this, serverManager.SendRequest(pairs), EventHandler.HandlingType.Restaurant);
        while (!flagWakeUp)
            yield return new WaitForSeconds(0.1f);

        if (eventHandler.result is Restaurant)
        {
            Restaurant r = eventHandler.result as Restaurant;
            // 상세조회

            //ToastMaker.instance.ShowToast("1 전");
            var desObj = detailedRestaurant.GetComponent<DetailedRestaurantManager>();
            desObj.restaurantName.text = name;
            desObj.id = id;
            desObj.score.text = r.rating.ToString();
            //ToastMaker.instance.ShowToast("1 후");
            DetailedRestaurantManager.previous = AnchorManager.State.MyInfo;
            AnchorManager.currentState = AnchorManager.State.Detail;
            
        }
    }

    public void clickNavi(string lon, string lat)
    {
        Debug.Log(lon + lat);

        // 길 안내 시작


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
