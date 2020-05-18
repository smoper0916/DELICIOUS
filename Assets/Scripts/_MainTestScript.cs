using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _MainTestScript : MonoBehaviour
{
    public GameObject arcoreDevice;
    public GameObject cameraObj;
    // Start is called before the first frame update

    Quaternion currentHeading;

    void Start()
    {
        //arcoreDevice.SetActive(false);
        //Input.compass.enabled = true;
        //cameraObj.transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
        //cameraObj.transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading, 0);
        //StartCoroutine(DoLoop());
    }

    // Update is called once per frame
    void Update()
    {
        //currentHeading = Quaternion.Euler(0, -Input.compass.trueHeading, 0);

        //cameraObj.transform.rotation = Quaternion.Slerp(cameraObj.transform.rotation,
           // currentHeading, Time.deltaTime * 3f);
    }

    public IEnumerator DoLoop()
    {
        arcoreDevice.SetActive(true);
        while (true)
        {
            cameraObj.transform.rotation = Quaternion.Euler(0, Input.compass.magneticHeading, 0);
            Debug.Log("R: " + cameraObj.transform.rotation + ", T: " + cameraObj.transform.position);
            yield return new WaitForSeconds(1);
        }
    }
}
