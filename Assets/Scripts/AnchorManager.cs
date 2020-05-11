using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    public GameObject eventHandler;
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

    float degreesLongitudeInMetersAtEquator;

    bool flagWakeUp = false;

    private void Start()
    {
        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        degreesLongitudeInMetersAtEquator = 111319.9f;

        // Real GPS Position - This will be the world origin.

        var gpsLat = 35.870619f; //GPSManager_NoCompass.Instance.latitude;
        var gpsLon = 128.5958397f; //GPSManager_NoCompass.Instance.longitude;
        dic.Add("url", "restaurants/near");
        dic.Add("lat", gpsLat.ToString());
        dic.Add("lon", gpsLon.ToString());
        dic.Add("radius", "200");
        //GameObject tmp = GameObject.Find("EventHandler");
        EventHandler EV = GameObject.Find("EventHandler").GetComponent<EventHandler>();
        serverManager = GameObject.Find("ServerManager").GetComponent<ServerManager>();

        EV.onClick(this, serverManager.SendRequest(dic), 0);

        while (!flagWakeUp)
        {

        }
        restaurants = EV.result as List<Restaurant>;
        // GPS position converted into unity coordinates
        foreach (Restaurant restaurant in restaurants)
        {
            var latOffset = (restaurant.y - gpsLat) * degreesLatitudeInMeters;
            var lonOffset = (restaurant.x - gpsLon) * GetLongitudeDegreeDistance(restaurant.y);

            Vector3 vector3 = new Vector3(latOffset, 0, latOffset);

            vectors.Add(vector3);
        }

    }

    void Update()
    {       // Real world position of object. Need to update with something near your own location.
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            for (int i = 0; i < vectors.Count; i++)
            {
                Pose pose = new Pose(vectors[i], transform.rotation);

                poses.Add(pose);

                anchors.Add(Session.CreateAnchor(pose));

                var gameObject = Instantiate(anchoredPrefab, anchors[i].transform.position, anchoredPrefab.transform.rotation, anchors[i].transform);

                textMeshs = gameObject.GetComponentsInChildren<TextMesh>();


                textMeshs[0].text = restaurants[i].rating.ToString();
                textMeshs[1].text = restaurants[i].name;
                textMeshs[2].text = restaurants[i].brief;

            }

            foreach (Anchor anchor in anchors)
            {
                gameObjects.Add(gameObject);
            }




        }
        if (Input.touchCount > 1 && gameObjects.Count != 0)
        {
            Vector2 pos = Input.GetTouch(Input.touchCount).position;
            Vector3 theTouch = new Vector3(pos.x, pos.y, 0.0f);    // 변환 안하고 바로 Vector3로 받아도 되겠지.

            Ray ray = Camera.main.ScreenPointToRay(theTouch);    // 터치한 좌표 레이로 바꾸엉

            RaycastHit hit;    // 정보 저장할 구조체 만들고

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))    // 레이저를 끝까지 쏴블자. 충돌 한넘이 있으면 return true다.

            {

                if (Input.GetTouch(0).phase == TouchPhase.Began)    // 딱 처음 터치 할때 발생한다

                {



                }

            }
        }

        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
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
        flagWakeUp = true;
    }
}
