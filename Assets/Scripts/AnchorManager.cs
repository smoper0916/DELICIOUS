using System;
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
    public GameObject compassMoverData;

    float degreesLongitudeInMetersAtEquator;
    private bool flagWakeUp = false;
    private bool flagCreate = false;

    public GameObject canvas;
    public GameObject loadingBar;
    public GameObject zzimData;

    public List<GameObject> zzimObjList = new List<GameObject>();

    public enum ZzimEventType { DELETE, GO, DETAIL}
    public enum State { Browse, Detail, Navigation, Zzim, History, MyInfo }
    public static State currentState;
    public State previousState;

    LoadingManager loadingManager;
    List<KeyValuePair<string, Restaurant>> zzim = new List<KeyValuePair<string, Restaurant>>();

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
        for (int i = 0; i < resList.Count; i++)
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

            gameObject.transform.localScale = new Vector3(18, 13, 0);
            // Debug.Log("Added : " + restaurants[i].name);
            // gameObjects.Add(gameObject);
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
            else if (previousState == State.Browse && currentState == State.Zzim)
            {
                foreach (var i in gameObjects)
                    i.SetActive(false);
            }
            else if (previousState == State.Browse && currentState == State.Detail)
            {
                DetailedRestaurantManager.previous = State.Browse;
                showCheck = true;
                detailedRestaurantManager.SetActive(true);
                
                canvas.SetActive(false);

                foreach (var i in gameObjects)
                    i.SetActive(false);
            }
            else if (previousState == State.Zzim && currentState == State.Detail)
            {
                DetailedRestaurantManager.previous = State.Zzim;
                detailedRestaurantManager.SetActive(true);
                showCheck = true;
                canvas.SetActive(false);
                foreach (GameObject i in zzimObjList)
                    Destroy(i);
                zzimObjList.Clear();

            }
            else if (previousState == State.Detail && currentState == State.Zzim)
            {
                
                StartCoroutine(HandlerZzim(false));
            }
            else
            {
                // 아직 잡지 못한 예외
                ToastMaker.instance.ShowToast("Update: 상태 변화 불일치 중 예외 발생! = " + previousState + " => " + currentState);
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
        loadingManager.step = 60f;
        loadingManager.ToggleUpdateFlag();

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        degreesLongitudeInMetersAtEquator = 111319.9f;

        //Real GPS Position - This will be the world origin.
        //var gpsLat = "36.1380077";
        //var gpsLon = "128.4166394";

        var gpsLat = GPSManager.Instance.latitude;
        var gpsLon = GPSManager.Instance.longitude;

        dic.Add("url", "restaurants/near");
        dic.Add("method", "GET");
        dic.Add("lat", gpsLat);
        dic.Add("lon", gpsLon);
        dic.Add("email", Login.userId);
        dic.Add("radius", "500");

        //IEnumerator sender = serverManager.SendRequest(dic);
        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        restaurants = eventHandler.result as Dictionary<string, Restaurant>;

        Debug.Log("Count : " + restaurants.Count);
        // GPS position converted into unity coordinates

        GameObject compassMover = Instantiate(compassMoverData, null);
        CompassMover mover = compassMover.GetComponent<CompassMover>();


        loadingBar.SetActive(false);
        // 좋은 각도가 나올때까지 대기.
        while (!mover.isGoodDegree)
            yield return new WaitForSeconds(0.1f);
        // mover 삭제
        mover.isDone = true;
        loadingBar.SetActive(true);

        ToastMaker.instance.ShowToast("Heading : " + GPSManager.Instance.heading);
        foreach (string k in restaurants.Keys)
        {
            Restaurant restaurant = restaurants[k];
            var latOffset = (float)(restaurant.y - double.Parse(gpsLat)) * degreesLatitudeInMeters;
            var lonOffset = (float)(restaurant.x - double.Parse(gpsLon)) * GetLongitudeDegreeDistance(restaurant.x);

            Debug.Log("latOffset : " + latOffset);
            Debug.Log("lonOffset : " + lonOffset);
            Debug.Log("=============");

            Vector3 vector3 = new Vector3(latOffset, 0, lonOffset);

            Debug.Log(vector3.magnitude);

            Debug.Log(-GPSManager.Instance.heading);

            //heading = Quaternion.LookRotation(Camera.main.transform.TransformDirection(GPSManager.Instance.headingVector)).eulerAngles.y;

            
            //vector3 = Quaternion.AngleAxis((-GPSManager.Instance., Vector3.up) * vector3;

            if (vector3.magnitude < 100.0f)
            {
                vector3.y = -20.0f;
            }
            else if (vector3.magnitude < 150.0f)
            {
                vector3.y = 10.0f;
            }
            else if (vector3.magnitude < 200.0f)
            {
                vector3.y = 40.0f;
            }
            else if (vector3.magnitude < 250.0f)
            {
                vector3.y = 90.0f;
            }
            else if (vector3.magnitude < 300.0f)
            {
                vector3.y = 150.0f;
            }
            else if (vector3.magnitude < 350.0f)
            {
                vector3.y = 220.0f;
            }
            else if (vector3.magnitude < 400.0f)
            {
                vector3.y = 310.0f;
            }
            else if (vector3.magnitude < 450.0f)
            {
                vector3.y = 380.0f;
            }
            else
            {
                vector3.y = 460.0f;
            }

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
        StartCoroutine(HandlerZzim());
        
    }

    public void OnClickZzimPanelButtons(string id, string name, string rating, ZzimEventType eventType)
    {
        switch (eventType)
        {
            case ZzimEventType.DELETE:
                ToastMaker.instance.ShowToast(name + "이 찜 목록에서 삭제되었습니다. ");
                DetailedRestaurantManager.zzim.Remove(id);
                foreach (GameObject i in zzimObjList)
                    Destroy(i);
                zzimObjList.Clear();
                StartCoroutine(HandlerZzim(false));
                break;

            case ZzimEventType.GO:
                break;

            case ZzimEventType.DETAIL:
                DetailedRestaurantManager.instance.restaurantName.text = name;
                DetailedRestaurantManager.instance.id = id;
                DetailedRestaurantManager.instance.score.text = rating;
                currentState = State.Detail;
                
                break;

            default:
                break;
        }
    }

    public IEnumerator HandlerZzim(bool isToggle = true)
    {
        if ((isToggle && currentState == State.Browse) || (currentState == State.Zzim && !isToggle))
        {
            Debug.Log(DetailedRestaurantManager.zzim.Count);


            currentState = State.Zzim;
            zzim.Clear();
            /*
            var r = new Restaurant();
            r.name = "고베규카츠 상수점";
            r.rating = 3.57f;
            DetailedRestaurantManager.zzim.Add("37275398", r);
            */
            foreach (var i in DetailedRestaurantManager.zzim)
                zzim.Add(new KeyValuePair<string, Restaurant>(i.Key, i.Value));

            // 1. 현재 찜의 개수를 확인해서 개수별 처리를 한다.
            // -는 좌측, +는 우측
            var PrefixDistance = 1681.0f;
            var vCamera = Camera.main.transform.position;
            var vDistance = Vector3.forward * PrefixDistance;
            Quaternion qRotate; Vector3 vTargetPoint; Vector3 vDest;
            Transform zzimPanel; Text[] txtComponents; Button[] btnComponents;

            Dictionary<int, float[]> angleArray = new Dictionary<int, float[]>();
            angleArray.Add(1, new float[] { 0f });
            angleArray.Add(2, new float[] { 22.5f, -22.5f });// 내 카메라 우측 22.5도, 좌측 22.5도 생성
            angleArray.Add(3, new float[] { 45f, 0, -45f });// 내 카메라 우측 45도, 중앙, 좌측 45도 생성
            angleArray.Add(4, new float[] { 67.5f, 22.5f, -22.5f, -67.5f });// 내 카메라 우측 45 + 22.5, 22.5 , 좌측 22.5, 45 + 22.5

            if (DetailedRestaurantManager.zzim.Count > 0)
            {
                // 내 카메라 앞에 생성한다.
                for (var i = 0; i < DetailedRestaurantManager.zzim.Count; i++)
                {
                    //ToastMaker.instance.ShowToast(angleArray[DetailedRestaurantManager.zzim.Count][i].ToString());
                    qRotate = Quaternion.Euler(0f, angleArray[DetailedRestaurantManager.zzim.Count][i], 0f);
                    vTargetPoint = (Camera.main.transform.rotation * qRotate) * vDistance;
                    vDest = vCamera + vTargetPoint;

                    zzimObjList.Add(Instantiate(zzimData, vDest, Quaternion.identity, null));
                    zzimObjList[i].transform.LookAt(Camera.main.transform);

                    zzimPanel = zzimObjList[i].transform.Find("ZzimPanel");
                    var resId = zzim[i].Key;
                    var resName = zzim[i].Value.name;
                    var resRating = zzim[i].Value.rating.ToString();


                    txtComponents = zzimPanel.GetComponentsInChildren<Text>();
                    btnComponents = zzimPanel.GetComponentsInChildren<Button>();
                    btnComponents[0].onClick.AddListener(delegate { OnClickZzimPanelButtons(resId, resName, resRating, ZzimEventType.DETAIL); });
                    Debug.Log("컴포넌트 + " + btnComponents.Length);

                    // 해당 찜의 정보 표출
                    txtComponents[1].text = zzim[i].Value.rating.ToString();
                    txtComponents[2].text = zzim[i].Value.name;
                    txtComponents[3].text = "대표메뉴가\n없습니다."; // 대표메뉴인데.. 모르겠다.

                    // 찜에 맞는 OnClickListener 등록
                    btnComponents[1].onClick.AddListener(delegate { OnClickZzimPanelButtons(resId, resName, resRating, ZzimEventType.DELETE); });
                    btnComponents[2].onClick.AddListener(delegate { OnClickZzimPanelButtons(resId, resName, resRating, ZzimEventType.GO); });
                }
            }
            else
            {
                // 찜 목록이 없다고 안내한다.
                ToastMaker.instance.ShowToast("찜 목록이 비어있습니다.");
                yield return new WaitForEndOfFrame();
                currentState = State.Browse;
            }
        }
        else
        {
            foreach (GameObject i in zzimObjList)
                Destroy(i);
            zzimObjList.Clear();
            currentState = State.Browse;
        }
    }
}
