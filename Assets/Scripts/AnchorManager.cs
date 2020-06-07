﻿using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.UI;

public class AnchorManager : MonoBehaviour
{
    public EventHandler eventHandler;
    public ServerManager serverManager;
    static public Dictionary<string, Restaurant> restaurants = new Dictionary<string, Restaurant>();
    List<Vector3> vectors = new List<Vector3>();
    List<Pose> poses = new List<Pose>();
    List<Anchor> anchors = new List<Anchor>();
    List<GameObject> gameObjects = new List<GameObject>();
    Dictionary<string, string> dic = new Dictionary<string, string>();
    public GameObject anchoredPrefab;

    TextMesh[] textMeshs;

    Vector3 lastAnchoredPosition;
    Quaternion lastAnchoredRotation;

    public static GameObject target;
    public static bool showCheck = false;
    public GameObject detailedRestaurantManager;

    float degreesLongitudeInMetersAtEquator;
    private bool flagWakeUp = false;
    private bool flagCreate = false;

    public GameObject canvas;
    public GameObject loadingBar;
    public GameObject likedBtn;
    public GameObject zzimData;

    public List<GameObject> zzimObjList = new List<GameObject>();

    public enum State { Browse, Detail, Navigation, Zzim }
    public static State currentState;
    public State previousState;

    LoadingManager loadingManager;

    float heading;
    int backKeyCnt = 0;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        currentState = State.Browse;
        previousState = State.Browse;
        loadingManager = loadingBar.transform.Find("vica").gameObject.GetComponent<LoadingManager>();
        StartCoroutine(loadRestaurants());
    }

    void Draw()
    {
        List<Restaurant> resList = new List<Restaurant>(restaurants.Values);
        for (int i = 0; i < vectors.Count; i++)
        {
            Pose pose = new Pose(vectors[i], transform.rotation);

            poses.Add(pose);

            anchors.Add(Session.CreateAnchor(pose));

            var gameObject = Instantiate(anchoredPrefab, anchors[i].transform.position, anchoredPrefab.transform.rotation, anchors[i].transform);
            gameObject.transform.rotation = Looking(gameObject.transform.position, transform.position);
            textMeshs = gameObject.GetComponentsInChildren<TextMesh>();

            textMeshs[0].text = resList[i].id;
            textMeshs[1].text = resList[i].rating.ToString();
            textMeshs[2].text = resList[i].name;
            textMeshs[3].text = resList[i].brief;

            gameObject.transform.localScale = new Vector3(7, 4, 0);
            // Debug.Log("Added : " + restaurants[i].name);
            gameObjects.Add(gameObject);
        }

        foreach (Anchor anchor in anchors)
        {
            gameObjects.Add(anchor.gameObject);
        }

        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
    }

    void Update()
    {
        // 상태 변경 감지
        if(currentState != previousState)
        {
            if (previousState == State.Detail && currentState == State.Browse)
            {
                foreach (var i in gameObjects)
                    i.SetActive(true);
            }
            else if (previousState == State.Zzim && currentState == State.Browse)
            {
                foreach (var i in gameObjects)
                    i.SetActive(true);
            }
        }
        previousState = currentState;

        // 상태별 행동 정의
        if (currentState == State.Browse && Input.touchCount > 0)
        {
            Vector2 pos = Input.GetTouch(0).position;
            Vector3 theTouch = new Vector3(pos.x, pos.y, 0.0f);    // 변환 안하고 바로 Vector3로 받아도 되겠지.

            Ray ray = Camera.main.ScreenPointToRay(theTouch);    // 터치한 좌표 레이로 바꾸엉

            RaycastHit hit;    // 정보 저장할 구조체 만들고

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))    // 레이저를 끝까지 쏴블자. 충돌 한넘이 있으면 return true다.
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)    // 딱 처음 터치 할때 발생한다
                {
                    target = hit.collider.gameObject;
                    Debug.Log(target.transform.position);
                    detailedRestaurantManager.SetActive(true);
                    showCheck = true;
                    canvas.SetActive(false);

                    foreach(var i in gameObjects)
                        i.SetActive(false);
                    
                    currentState = State.Detail;
                }
            }
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            switch (currentState)
            {
                case State.Browse:
                    backKeyCnt++;
                    if (backKeyCnt == 1)
                    {
                        ToastMaker.instance.ShowToast("뒤로가기를 한번 더 누르시면 종료됩니다.");
                    }
                    else if(backKeyCnt == 2)
                    {
                        Application.Quit();
                    }
                    else
                    {
                        backKeyCnt -= 2;
                    }
                    break;
                case State.Detail:
                    break;

                case State.Zzim:
                    ClickZzimBtn();
                    break;

                case State.Navigation:
                    break;
                default:
                    break;
            }
        }
        
    }
    private IEnumerator loadRestaurants()
    {
        loadingManager.state = "GPS 신호 수신 중...";
        loadingManager.step = 30f;
        loadingManager.ToggleUpdateFlag();

        while (!GPSManager.Instance.isReady)
        {
            yield return new WaitForSeconds(1.0f);
        }

        loadingManager.state = "서버로부터 주변 식당 정보 받는 중...";
        loadingManager.step = 90f;
        loadingManager.ToggleUpdateFlag();

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        degreesLongitudeInMetersAtEquator = 111319.9f;

        //Real GPS Position - This will be the world origin.
        var gpsLat = 36.1380077f;
        var gpsLon = 128.4166394f;

        //var gpsLat = GPSManager.Instance.latitude;
        //var gpsLon = GPSManager.Instance.longitude;

        dic.Add("url", "restaurants/near");
        dic.Add("method", "GET");
        dic.Add("lat", gpsLat.ToString());
        dic.Add("lon", gpsLon.ToString());
        dic.Add("radius", "1000");

        //IEnumerator sender = serverManager.SendRequest(dic);
        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        restaurants = eventHandler.result as Dictionary<string, Restaurant>;

        Debug.Log("Count : " + restaurants.Count);
        // GPS position converted into unity coordinates
        foreach (string k in restaurants.Keys)
        {
            Restaurant restaurant = restaurants[k];
            var latOffset = (restaurant.y - gpsLat) * degreesLatitudeInMeters;
            var lonOffset = (restaurant.x - gpsLon) * GetLongitudeDegreeDistance(restaurant.y);

            Vector3 vector3 = new Vector3(latOffset, 0, lonOffset);

            Debug.Log(vector3.magnitude);

            Debug.Log(GPSManager.Instance.heading);

            //heading = Quaternion.LookRotation(Camera.main.transform.TransformDirection(GPSManager.Instance.headingVector)).eulerAngles.y;

            vector3 = Quaternion.AngleAxis(GPSManager.Instance.heading, Vector3.up) * vector3;

            //if (vector3.magnitude > 50.0f)
            //{
            //    vector3 = new Vector3(latOffset, -5.0f, lonOffset);
            //}
            //else if (vector3.magnitude > 150.0f)
            //{
            //    vector3 = new Vector3(latOffset, -1.0f, lonOffset);
            //}
            //else
            //{
            //    vector3 = new Vector3(latOffset, 1.0f, lonOffset);
            //}

            Debug.Log(vector3);

            vectors.Add(vector3);
        }
        Destroy(loadingBar.gameObject);
        Draw();
        //yield return new WaitUntil(() => flagWakeUp == true);
    }
    private float GetLongitudeDegreeDistance(float latitude)
    {
        return degreesLongitudeInMetersAtEquator * Mathf.Cos(latitude * (Mathf.PI / 180));
    }
    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }
    public void WakeUp()
    {
        Debug.Log("Wake Up!");
        flagWakeUp = true;
    }
    Quaternion Looking(Vector3 tartget, Vector3 current)
    {
        Quaternion lookAt = Quaternion.identity;
        Vector3 lookAtVec = (tartget - current).normalized;

        lookAt.SetLookRotation(lookAtVec);

        return lookAt;
    }
    public IEnumerator waitForGPSReady()
    {
        while (!GPSManager.Instance.isReady)
        {
            Debug.Log("Waiting...");
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void ClickZzimBtn()
    {
        if (currentState == State.Browse)
        {
            Debug.Log(DetailedRestaurantManager.zzim.Count);

            currentState = State.Zzim;
            foreach (var i in gameObjects)
                i.SetActive(false);

            // 1. 현재 찜의 개수를 확인해서 개수별 처리를 한다.
            // -는 좌측, +는 우측
            var PrefixDistance = 1681.0f;
            var vCamera = Camera.main.transform.position;
            var vDistance = Vector3.forward * PrefixDistance;
            Quaternion qRotate; Vector3 vTargetPoint; Vector3 vDest;
            GameObject zzimObj1; GameObject zzimObj2; GameObject zzimObj3; GameObject zzimObj4; GameObject zzimPanel;
            switch (DetailedRestaurantManager.zzim.Count)
            {
                case 0:
                    // 찜 목록이 없다고 안내한다.
                    ToastMaker.instance.ShowToast("찜 목록이 비어있습니다.");
                    currentState = State.Browse;
                    break;
                case 1:
                    // 내 카메라 앞에 생성한다.

                    vTargetPoint = Camera.main.transform.rotation * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj1 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj1.transform.LookAt(Camera.main.transform);
                    //zzimPanel = zzimObj.transform.Find("ZzimPanel");

                    zzimObjList.Add(zzimObj1);
                    break;
                case 2:
                    // 내 카메라 우측 22.5도, 좌측 22.5도 생성
                    qRotate = Quaternion.Euler(0f, 22.5f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj1 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj1.transform.LookAt(Camera.main.transform);

                    qRotate = Quaternion.Euler(0f, -22.5f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj2 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj2.transform.LookAt(Camera.main.transform);

                    zzimObjList.AddRange(new GameObject[] { zzimObj1, zzimObj2 });
                    break;
                case 3:
                    // 내 카메라 우측 45도, 중앙, 좌측 45도 생성
                    qRotate = Quaternion.Euler(0f, 45f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj1 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj1.transform.LookAt(Camera.main.transform);

                    qRotate = Quaternion.Euler(0f, 0f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj2 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj2.transform.LookAt(Camera.main.transform);

                    qRotate = Quaternion.Euler(0f, -45f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj3 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj3.transform.LookAt(Camera.main.transform);

                    zzimObjList.AddRange(new GameObject[] { zzimObj1, zzimObj2, zzimObj3});
                    break;
                case 4:
                    // 내 카메라 우측 45 + 22.5, 22.5 , 좌측 22.5, 45 + 22.5
                    qRotate = Quaternion.Euler(0f, 67.5f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj1 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj1.transform.LookAt(Camera.main.transform);

                    qRotate = Quaternion.Euler(0f, 22.5f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj2 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj2.transform.LookAt(Camera.main.transform);

                    qRotate = Quaternion.Euler(0f, -22.5f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj3 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj3.transform.LookAt(Camera.main.transform);

                    qRotate = Quaternion.Euler(0f, -67.5f, 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObj4 = Instantiate(zzimData, vDest, Quaternion.identity, transform.root);
                    zzimObj4.transform.LookAt(Camera.main.transform);

                    zzimObjList.AddRange(new GameObject[] { zzimObj1, zzimObj2, zzimObj3, zzimObj4 });
                    break;
                default:
                    break;
            }
        }
        else
        {
            foreach(GameObject i in zzimObjList)
                Destroy(i);
            currentState = State.Browse;
        }
    }

    public void OnClickLikedBtn()
    {
        likedBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_menu_on");

        
        transform.Rotate(Vector3.up, 60f, Space.World);
        var cameraVector = Camera.main.transform.position;
        Vector3 finalDirection = cameraVector + cameraVector.normalized * 1681;
    }

}
