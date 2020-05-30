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
    Dictionary<string, string> dic = new Dictionary<string, string>();

    public GameObject NavPrefab;
    public GameObject WayPrefab;
    public GameObject Destination;

    public static GameObject target;
    GameObject arrow;
    GameObject wayPoint;

    float degreesLongitudeInMetersAtEquator;
    private bool flagWakeUp = false;
    private bool flagCreate = false;

    float heading;
    int idx = 0;
    bool checkWayPoint = false;

    //public GameObject canvas;
    //public GameObject loadingBar;
    private void Start()
    {
        StartCoroutine(loadWayPoints());
        arrow = Instantiate(NavPrefab, new Vector3(0, Camera.main.transform.position.y - 0.2f, 0.5f), NavPrefab.transform.rotation, Camera.main.transform);
    }

    void Update()
    {
        if (flagCreate)
        {
            if (checkWayPoint == false)
            {
                if (idx != vectors.Count)
                {
                    Pose pose = new Pose(vectors[idx], transform.rotation);
                    Anchor anchor = Session.CreateAnchor(pose);

                    wayPoint = Instantiate(WayPrefab, anchor.transform.position, WayPrefab.transform.rotation, anchor.transform);
                    wayPoint.transform.LookAt(vectors[idx + 1]);
                    wayPoint.GetComponent<Collider>().tag = "wayPoint";
                }
                else
                {
                    Pose pose = new Pose(vectors[idx], transform.rotation);
                    Anchor anchor = Session.CreateAnchor(pose);

                    wayPoint = Instantiate(Destination, anchor.transform.position, Destination.transform.rotation, anchor.transform);

                    wayPoint.GetComponent<Collider>().tag = "destination";
                }

                checkWayPoint = true;

                Debug.Log(wayPoint.transform.position);

                //wayPoint.transform.localScale = new Vector3(7, 7, 0);
            }
            arrow.transform.LookAt(vectors[idx]);
            arrow.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
        }

    }
    private IEnumerator loadWayPoints()
    {

        while (!GPSManager.Instance.isReady)
        {
            yield return new WaitForSeconds(1.0f);
        }

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        degreesLongitudeInMetersAtEquator = 111319.9f;

        //Real GPS Position - This will be the world origin.
        var gpsLat = 36.1377368f;
        var gpsLon = 128.4195133f;
        //var gpsLat = GPSManager.Instance.latitude;
        //var gpsLon = GPSManager.Instance.longitude;

        dic.Add("url", "routes/ped");
        dic.Add("method", "GET");
        dic.Add("startX", gpsLon.ToString());
        dic.Add("startY", gpsLat.ToString());
        dic.Add("endX", "128.397286");
        dic.Add("endY", "36.137711");

        //IEnumerator sender = serverManager.SendRequest(dic);
        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Route);

        while (!flagWakeUp)
            yield return new WaitForSeconds(2.0f);

        wayPoints = eventHandler.result as List<WayPoint>;

        vectors.Add(new Vector3(0, -0.2f, 5.0f));

        Debug.Log(GPSManager.Instance.heading);

        foreach (WayPoint wayPoint in wayPoints)
        {
            var latOffset = (float.Parse(wayPoint.lat) - gpsLat) * degreesLatitudeInMeters;
            var lonOffset = (float.Parse(wayPoint.lon) - gpsLon) * GetLongitudeDegreeDistance(float.Parse(wayPoint.lat));

            Vector3 vector3 = new Vector3(latOffset, 0, lonOffset);

            //heading = Quaternion.LookRotation(Camera.main.transform.TransformDirection(GPSManager.Instance.headingVector)).eulerAngles.y;

            vector3 = Quaternion.AngleAxis(GPSManager.Instance.heading, Vector3.up) * vector3;

            vectors.Add(vector3);
        }
        flagCreate = true;

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
    public IEnumerator waitForGPSReady()
    {
        while (!GPSManager.Instance.isReady)
        {
            Debug.Log("Waiting...");
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("충돌충돌충돌충돌충돌충돌충돌충돌");
        if (other.CompareTag("wayPoint"))
        {
            gameObject.SetActive(false);

            checkWayPoint = false;

            idx++;

        }
        else if (other.CompareTag("destination"))
        {
            gameObject.SetActive(false);
            arrow.SetActive(false);

            //이력 등록


        }
    }
}

