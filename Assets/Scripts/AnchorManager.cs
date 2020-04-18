using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
    public GameObject anchoredPrefab;
    Anchor anchor;
    Vector3 vector3;

    Vector3 lastAnchoredPosition;
    float degreesLongitudeInMetersAtEquator;

    // Update is called once per frame
    private void Start()
    {
        float latitude = 36.1451108f;
        float longitude = 128.3933821f;

        // Conversion factors
        float degreesLatitudeInMeters = 111132;
        degreesLongitudeInMetersAtEquator = 111319.9f;

        // Real GPS Position - This will be the world origin.
        var gpsLat = GPSManager_NoCompass.Instance.latitude;
        var gpsLon = GPSManager_NoCompass.Instance.longitude;
        // GPS position converted into unity coordinates
        var latOffset = (latitude - gpsLat) * degreesLatitudeInMeters;
        var lonOffset = (longitude - gpsLon) * GetLongitudeDegreeDistance(latitude);

        vector3 = new Vector3(latOffset, 0, latOffset);

    }

    void Update()
    {       // Real world position of object. Need to update with something near your own location.
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Pose pose = new Pose(vector3, transform.rotation);
            anchor = Session.CreateAnchor(pose);
            Debug.Log(vector3);
            GameObject.Instantiate(anchoredPrefab, anchor.transform.position, anchor.transform.rotation, anchor.transform);
            lastAnchoredPosition = anchor.transform.position;

            Debug.Log(anchoredPrefab.transform.position);
        }
        if (anchor == null) return;

        if (anchor.transform.position != lastAnchoredPosition)
        {
            Debug.Log(Vector3.Distance(anchor.transform.position, lastAnchoredPosition));
            lastAnchoredPosition = anchor.transform.position;
        }
    }
    private float GetLongitudeDegreeDistance(float latitude)
    {
        return degreesLongitudeInMetersAtEquator * Mathf.Cos(latitude * (Mathf.PI / 180));
    }
}
