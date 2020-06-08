using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance { set; get; }
    public float latitude;
    public float longitude;

    public float heading;
    public float headingAccuracy;

    public Vector3 headingVector;

    public bool isReady = false;

    [HideInInspector]
    public bool isRunning = true;

    [HideInInspector]
    public LocationServiceStatus ServiceStatus = LocationServiceStatus.Stopped;

    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());
    }

    private IEnumerator StartLocationService()
    {
        isReady = false;
        ServiceStatus = LocationServiceStatus.Initializing;

        Input.compass.enabled = true;

        yield return new WaitForSeconds(1);

        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("user has not enabled gps");
            yield break;
        }

        // Wait for the GPS to start up so there's time to connect
        Input.location.Start();

        yield return new WaitForSeconds(1);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("Timed Out");
            yield break;
        }

        // If gps hasn't started by now, just give up
        ServiceStatus = Input.location.status;
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        //Loop forever to get GPS updates
        while (isRunning)
        {
            yield return new WaitForSeconds(0.1f);
            UpdateGPS();
        }
    }

    private void UpdateGPS()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;

            heading = -Input.compass.magneticHeading;
            headingAccuracy = Input.compass.headingAccuracy;
            ServiceStatus = Input.location.status;

            headingVector = Input.compass.rawVector;

            //Debug.Log(string.Format("Lat: {0} Long: {1} Heading: {2} HeadingAccuracy : {3} HeadingVector3 : {4}" , latitude, longitude, heading, headingAccuracy, headingVector));
            if (!isReady) isReady = true;
        }
        else
        {
            Debug.Log("GPS is " + Input.location.status);
        }
    }
}
