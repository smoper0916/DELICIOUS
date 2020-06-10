﻿using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using GPSLogger;

public class NavigationManager : MonoBehaviour
{
    EventHandler eventHandler;
    ServerManager serverManager;
    List<WayPoint> wayPoints = new List<WayPoint>();
    List<Vector3> vectors = new List<Vector3>();
    List<Anchor> anchors = new List<Anchor>();
    List<GameObject> wayPointsGameObject = new List<GameObject>();
    Dictionary<string, string> dic = new Dictionary<string, string>();
    List<double> dis = new List<double>();

    public GameObject WayPrefab;
    public GameObject Destination;

    GpsCalc GpsCalc = new GpsCalc();

    public GameObject compassMoverData;
    public static GameObject target;
    GameObject arrow;
    GameObject wayPoint;

    float degreesLongitudeInMetersAtEquator;
    private bool flagWakeUp = false;
    private bool flagCreate = false;

    float heading;
    float degree;
    public static int idx = 0;
    public static bool checkWayPoint = false;

    public static string lon = "";
    public static string lat = "";

    //public GameObject canvas;
    //public GameObject loadingBar;
    private void Start()
    {
        StartCoroutine(initProcess());
    }

    void Update()
    {
        if (flagCreate)
        {
            if (checkWayPoint == false || wayPointsGameObject[idx] == null)
            {
                checkWayPoint = true;
                wayPointsGameObject[idx].SetActive(true);
                //ToastMaker.instance.ShowToast("그려지나?");

                //Debug.Log(idx);
                //Debug.Log(wayPointsGameObject[idx].gameObject.tag);

                //wayPoint.transform.localScale = new Vector3(7, 7, 0);
            }


            //Debug.Log(Vector3.SignedAngle(transform.up, arrow.transform.forward - Camera.main.transform.forward, -transform.forward));

            //Debug.Log(arrow.transform.position);
            //arrow.transform.rotation = Quaternion.AngleAxis(270, Vector3.up);
            //arrow.transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y - 0.2f, transform.parent.position.z + 0.5f);
            arrow.transform.LookAt(wayPointsGameObject[idx].transform.position);
        }


    }

    private IEnumerator initProcess()
    {
        GameObject compassMover = Instantiate(compassMoverData, null);
        CompassMover mover = compassMover.GetComponent<CompassMover>();

        // 좋은 각도가 나올때까지 대기.
        while (!mover.isGoodDegree)
            yield return new WaitForSeconds(0.1f);
        // mover 삭제
        mover.isDone = true;

        foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (g.name == "EventHandler")
                eventHandler = g.GetComponent<EventHandler>();
            else if (g.name == "ServerManager")
                serverManager = g.GetComponent<ServerManager>();
        }

        StartCoroutine(loadWayPoints());

        //Invoke("test", 3.0f);

        Debug.Log(Camera.main.transform.position);

        arrow = Camera.main.transform.Find("arrows").gameObject;
        arrow.SetActive(true);

        //arrow = Instantiate(NavPrefab, new Vector3(0, - 0.55f, 0), NavPrefab.transform.rotation, Camera.main.transform);
        InvokeRepeating("CheckDegree", 5, 5);
        InvokeRepeating("CheckDistance", 7.0f, 0.5f);
    }

    private void CheckDistance()
    {
        double distance = GpsCalc.distance(double.Parse(GPSManager.Instance.latitude), double.Parse(GPSManager.Instance.longitude), double.Parse(wayPoints[idx].lat), double.Parse(wayPoints[idx].lon));
        ToastMaker.instance.ShowToast(distance.ToString());
        if (dis[idx] * 0.2f >= distance)
        {
            if (idx != vectors.Count - 2)
                idx++;
            checkWayPoint = false;
            wayPointsGameObject[idx].SetActive(false);
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
        //var gpsLat = 36.1377368f;
        //var gpsLon = 128.4195133f;
        var gpsLat = GPSManager.Instance.latitude;
        var gpsLon = GPSManager.Instance.longitude;

        dic.Add("url", "routes/ped");
        dic.Add("method", "GET");
        dic.Add("startX", gpsLon);
        dic.Add("startY", gpsLat);
        dic.Add("endX", lon.ToString());
        dic.Add("endY", lat.ToString());

        //IEnumerator sender = serverManager.SendRequest(dic);
        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Route);

        while (!flagWakeUp)
            yield return new WaitForSeconds(2.0f);

        wayPoints = eventHandler.result as List<WayPoint>;

        //Debug.Log(GPSManager.Instance.heading);

        ToastMaker.instance.ShowToast("WayP 길이 : " + wayPoints.Count);
        Debug.Log("WayP 길이 : " + wayPoints.Count);

        foreach (WayPoint wayPoint in wayPoints)
        {
            var distance = GpsCalc.distance(double.Parse(gpsLat), double.Parse(gpsLon), double.Parse(wayPoint.lat), double.Parse(wayPoint.lon));

            dis.Add(distance);

            Debug.Log(string.Format("{0},{1}", wayPoint.lat, wayPoint.lon));
            //ToastMaker.instance.ShowToast(string.Format("{0},{1}", wayPoint.lat, wayPoint.lon));

            var latOffset = ((double.Parse(wayPoint.lat) - double.Parse(gpsLat)) * degreesLatitudeInMeters);
            var lonOffset = (double.Parse(wayPoint.lon) - double.Parse(gpsLon)) * GetLongitudeDegreeDistance(float.Parse(wayPoint.lon));

            Vector3 vector3 = new Vector3((float)latOffset, 0, (float)lonOffset);

            //heading = Quaternion.LookRotation(Camera.main.transform.TransformDirection(GPSManager.Instance.headingVector)).eulerAngles.y;

            //vector3 = Quaternion.AngleAxis(GPSManager.Instance.heading, Vector3.up) * vector3;

            Debug.Log(vector3);

            vectors.Add(vector3);

            Pose pose = new Pose(vector3, Quaternion.Euler(0, 0, 0));
            Anchor anchor = Session.CreateAnchor(pose);

            anchors.Add(anchor);

        }

        Debug.Log(anchors.Count);

        DrawCheckPoint();
    }

    public void test()
    {
        vectors.Add(new Vector3(0, -0.2f, 3.0f));
        vectors.Add(new Vector3(0.5f, -0.2f, 6.0f));
        vectors.Add(new Vector3(0.5f, -0.2f, 9.0f));
        vectors.Add(new Vector3(1.0f, -0.2f, 9.0f));
        vectors.Add(new Vector3(1.5f, -0.2f, 10.0f));
        vectors.Add(new Vector3(2.0f, -0.2f, 11.0f));
        vectors.Add(new Vector3(2.5f, -0.2f, 12.0f));
        vectors.Add(new Vector3(3.0f, -0.2f, 13.0f));
        vectors.Add(new Vector3(3.5f, -0.2f, 14.0f));
        vectors.Add(new Vector3(4.0f, -0.2f, 15.0f));

        for (int i = 0; i < vectors.Count; i++)
        {
            Pose poseTmp = new Pose(vectors[i], Quaternion.AngleAxis(GPSManager.Instance.heading, Vector3.up));
            Anchor anchorTmp = Session.CreateAnchor(poseTmp);

            anchors.Add(anchorTmp);
        }

        DrawCheckPoint();
    }

    public void DrawCheckPoint()
    {
        for (int i = 0; i < anchors.Count - 2; i++)
        {
            wayPoint = Instantiate(WayPrefab, anchors[i].transform.position, WayPrefab.transform.rotation, anchors[i].transform);
            wayPoint.transform.LookAt(vectors[i + 1]);
            wayPointsGameObject.Add(wayPoint);

            wayPoint.SetActive(false);
        }
        Vector3 point = new Vector3(anchors[anchors.Count - 1].transform.position.x, anchors[anchors.Count - 1].transform.position.y + 0.25f, anchors[anchors.Count - 1].transform.position.z);
        wayPoint = Instantiate(Destination, point, Destination.transform.rotation, anchors[anchors.Count - 1].transform);
        wayPointsGameObject.Add(wayPoint);
        wayPoint.SetActive(false);

        flagCreate = true;
        checkWayPoint = false;

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

    // 둔각 탐지 알고리즘
    private void CheckDegree()
    {
        degree = Vector3.SignedAngle(transform.up, arrow.transform.forward - Camera.main.transform.forward, -transform.forward);
        Debug.Log(degree);
        if (degree > 80.0f || degree < -80.0f)
        {
            Debug.Log(idx);

            wayPointsGameObject[idx].SetActive(false);

            idx++;

            checkWayPoint = false;


            ToastMaker.instance.ShowToast("Next CheckPoint");

        }
    }
}

