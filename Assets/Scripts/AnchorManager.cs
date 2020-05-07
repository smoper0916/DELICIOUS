using System;
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
    Quaternion lastAnchoredRotation;

    float degreesLongitudeInMetersAtEquator;
  
    private void Start()
    {
        StartCoroutine(DelayMethod(3.5f, () =>
        {
            float latitude = 36.1370f;
            float longitude = 128.3962f;
            // Conversion factors
            float degreesLatitudeInMeters = 111132;
            degreesLongitudeInMetersAtEquator = 111319.9f;



            // Real GPS Position - This will be the world origin.
            var gpsLat = GPSManager_NoCompass.Instance.latitude;
            var gpsLon = GPSManager_NoCompass.Instance.longitude;

            Debug.Log(gpsLat);
            Debug.Log(gpsLon);

            // GPS position converted into unity coordinates
            var latOffset = (latitude - gpsLat) * degreesLatitudeInMeters;
            var lonOffset = (longitude - gpsLon) * GetLongitudeDegreeDistance(latitude);

            vector3 = new Vector3(latOffset, 0, latOffset);
            Debug.Log(vector3);
            Debug.Log(transform.position);
        }));
 
    }

    void Update()
    {       // Real world position of object. Need to update with something near your own location.
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Pose pose = new Pose(vector3, transform.rotation);

            anchor = Session.CreateAnchor(pose);

            var gameObject = Instantiate(anchoredPrefab, anchor.transform.position, anchoredPrefab.transform.rotation, anchor.transform);
            lastAnchoredPosition = anchor.transform.position;
        }
        if (anchor == null) return;
        if (lastAnchoredPosition != anchor.transform.position)
        {
            Debug.Log(Vector3.Distance(anchor.transform.position, lastAnchoredPosition));
            lastAnchoredPosition = anchor.transform.position;
        }
        if(Session.Status != SessionStatus.Tracking)
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
}
