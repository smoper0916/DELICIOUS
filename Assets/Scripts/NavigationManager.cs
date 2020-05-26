using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public EventHandler eventHandler;
    public ServerManager serverManager;
    List<WayPoint> wayPoints = new List<WayPoint>();
    List<Vector3> vectors = new List<Vector3>();
    List<GameObject> gameObjects = new List<GameObject>();
    Dictionary<string, string> dic = new Dictionary<string, string>();
    public GameObject NavPrefab;

    public static GameObject target;
    float degreesLongitudeInMetersAtEquator;
    private bool flagWakeUp = false;
    private bool flagCreate = false;

    int index = 0;
    float speed = 5.0f;

    float heading;

    //public GameObject canvas;
    //public GameObject loadingBar;
    private void Start()
    {
        StartCoroutine(loadRestaurants());
    }

    void Update()
    {
        if (flagWakeUp)
        { 
            var curPos = NavPrefab.transform.position;

            NavPrefab = Instantiate(NavPrefab, curPos, NavPrefab.transform.rotation, transform);

            NavPrefab.transform.rotation = Looking(vectors[index], NavPrefab.transform.position);

            if (index < vectors.Count)
            {
                float step = speed * Time.deltaTime;

                NavPrefab.transform.position = Vector3.MoveTowards(curPos, vectors[index], step);

                if (Vector3.Distance(curPos, vectors[index]) == 0f)
                {
                    index++;
                }
                
                Debug.Log(NavPrefab.transform.position);
            }

            //for (int i = 0; i < vectors.Count; i++)
            //{
            //    Pose pose = new Pose(vectors[i], transform.rotation);

            //    poses.Add(pose);

            //    anchors.Add(Session.CreateAnchor(pose));

            //    var gameObject = Instantiate(anchoredPrefab, anchors[i].transform.position, anchoredPrefab.transform.rotation, anchors[i].transform);
            //    textMeshs = gameObject.GetComponentsInChildren<TextMesh>();


            //    gameObject.transform.localScale = new Vector3(10, 7, 0);
            //    // Debug.Log("Added : " + restaurants[i].name);
            //}
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
        //var gpsLat = 36.1377368f;
        //var gpsLon = 128.4195133f;
        var gpsLat = GPSManager.Instance.latitude;
        var gpsLon = GPSManager.Instance.longitude;

        vectors.Insert(0, new Vector3(0, -1, 0));

        dic.Add("url", "routes/ped");
        dic.Add("method", "GET");
        dic.Add("startX", gpsLon.ToString());
        dic.Add("startY", gpsLat.ToString());
        dic.Add("endX", "128.397286");
        dic.Add("endY", "36.137711");

        //IEnumerator sender = serverManager.SendRequest(dic);
        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Route);

        while (!flagWakeUp)
            yield return new WaitForSeconds(5.0f);


        wayPoints = eventHandler.result as List<WayPoint>;

        // GPS position converted into unity coordinates
        foreach (WayPoint wayPoint in wayPoints)
        {
            var latOffset = (float.Parse(wayPoint.lat) - gpsLat) * degreesLatitudeInMeters;
            var lonOffset = (float.Parse(wayPoint.lon) - gpsLon) * GetLongitudeDegreeDistance(float.Parse(wayPoint.lat));

            Vector3 vector3 = new Vector3(latOffset, -1, lonOffset);

            Debug.Log(GPSManager.Instance.heading);

            heading = Quaternion.LookRotation(Camera.main.transform.TransformDirection(GPSManager.Instance.headingVector)).eulerAngles.y;

            vector3 = Quaternion.AngleAxis(heading, Vector3.up) * vector3;

            Debug.Log(vector3);

            vectors.Add(vector3);

        }
        //loadingBar.SetActive(false);
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
    //public IEnumerator waitForGPSReady()
    //{
    //    while (!GPSManager.Instance.isReady)
    //    {
    //        Debug.Log("Waiting...");
    //        yield return new WaitForSeconds(1.0f);
    //    }
    //}
    Quaternion Looking(Vector3 tartget, Vector3 current)
    {
        Quaternion lookAt = Quaternion.identity;
        Vector3 lookAtVec = (tartget - current).normalized;

        lookAt.SetLookRotation(lookAtVec);

        return lookAt;
    }
}

