using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

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
    public enum State { Browse, Detail, Navigation, Zzim }
    public static State currentState;
    public State previousState;

    LoadingManager loadingManager;

    float heading;

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
        }
        previousState = currentState;

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
        Debug.Log(DetailedRestaurantManager.zzim.Count);
    }

}
