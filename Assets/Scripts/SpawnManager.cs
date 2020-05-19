using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

public class SpawnManager : MonoBehaviour
{
    public GameObject anchoredPrefab;
    float latitude;
    float longitude;
    Vector3 vector3;

    float degreesLatitudeInMeters = 111132;
    float degreesLongitudeInMetersAtEquator = 111319.9f;

    // Update is called once per frame
    private void Start()
    {
        latitude = 36.13657f;
        longitude = 128.3962f;

    }

    void Update()
    {
        // Real GPS Position - This will be the world origin.
        float gpsLat = GPSManager.Instance.latitude;
        float gpsLon = GPSManager.Instance.longitude;
        // GPS position converted into unity coordinates
        if (latitude != gpsLat || longitude != gpsLon)
        {
            latitude = gpsLat;
            longitude = gpsLon;
            float latOffset = (latitude - gpsLat) * degreesLatitudeInMeters;
            float lonOffset = (longitude - gpsLon) * GetLongitudeDegreeDistance(latitude);

            vector3 = new Vector3(latOffset, 0, latOffset);

            GameObject.Instantiate(anchoredPrefab, vector3, anchoredPrefab.transform.rotation);

            Debug.Log(anchoredPrefab.transform.position);
        }
    }
    private float GetLongitudeDegreeDistance(float latitude)
    {
        return degreesLongitudeInMetersAtEquator * Mathf.Cos(latitude * (Mathf.PI / 180));
    }
}
