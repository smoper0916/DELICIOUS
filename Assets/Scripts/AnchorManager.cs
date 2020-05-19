using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    public EventHandler eventHandler;
    public ServerManager serverManager;
    List<Restaurant> restaurants = new List<Restaurant>();
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
    public GameObject detailedRestaurantManager;

    float degreesLongitudeInMetersAtEquator;
    private bool flagWakeUp = false;
    private bool flagCreate = false;

    public GameObject canvas;

    private void Start()
    {
        StartCoroutine(loadRestaurants());
    }

    void Update()
    {
        if (flagCreate == false && Session.Status == SessionStatus.Tracking)
        {
            // Real world position of object. Need to update with something near your own location.
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                for (int i = 0; i < vectors.Count; i++)
                {
                    Pose pose = new Pose(vectors[i], transform.rotation);

                    poses.Add(pose);

                    anchors.Add(Session.CreateAnchor(pose));

                    var gameObject = Instantiate(anchoredPrefab, anchors[i].transform.position, anchoredPrefab.transform.rotation, anchors[i].transform);
                    gameObject.transform.rotation = Looking(gameObject.transform.position, transform.position);
                    textMeshs = gameObject.GetComponentsInChildren<TextMesh>();

                    textMeshs[0].text = restaurants[i].id;
                    textMeshs[1].text = restaurants[i].rating.ToString();
                    textMeshs[2].text = restaurants[i].name;
                    textMeshs[3].text = restaurants[i].brief;

                    gameObject.transform.localScale = new Vector3(10, 7, 0);
                    // Debug.Log("Added : " + restaurants[i].name);
                }

                foreach (Anchor anchor in anchors)
                {
                    gameObjects.Add(gameObject);
                }
                flagCreate = true;
            }
        }
        if (Input.touchCount > 0 && flagCreate == true)
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
                    detailedRestaurantManager.SetActive(true);
                    canvas.SetActive(false);
                }

            }
        }

        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
    }
    private IEnumerator loadRestaurants()
    {
        while (!GPSManager.Instance.isReady)
        {
            yield return new WaitForSeconds(1.0f);
        }

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        degreesLongitudeInMetersAtEquator = 111319.9f;

        //Real GPS Position - This will be the world origin.
        var gpsLat = 36.1377368f; //GPSManager.Instance.latitude;
        var gpsLon = 128.4195133f; //GPSManager.Instance.longitude;
        //var gpsLat = GPSManager.Instance.latitude;
        //var gpsLon = GPSManager.Instance.longitude;

        dic.Add("url", "restaurants/near");
        dic.Add("method", "GET");
        dic.Add("lat", gpsLat.ToString());
        dic.Add("lon", gpsLon.ToString());
        dic.Add("radius", "200");

        //IEnumerator sender = serverManager.SendRequest(dic);
        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(5.0f);


        restaurants = eventHandler.result as List<Restaurant>;

        // GPS position converted into unity coordinates
        foreach (Restaurant restaurant in restaurants)
        {
            var latOffset = (restaurant.y - gpsLat) * degreesLatitudeInMeters;
            var lonOffset = (restaurant.x - gpsLon) * GetLongitudeDegreeDistance(restaurant.y);

            Vector3 vector3 = new Vector3(latOffset, 0, lonOffset);

            Debug.Log(GPSManager.Instance.heading);

            vector3 = Quaternion.AngleAxis(-GPSManager.Instance.heading, Vector3.up) * vector3;

            Debug.Log(vector3);

            vectors.Add(vector3);
        }
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
}
