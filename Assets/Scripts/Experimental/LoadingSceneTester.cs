using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSceneTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;
        StartCoroutine(OnGyro);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator OnGyro()
    {
        StartCoroutine(waitThenCallback(1, () => { Debug.Log(Input.gyro.attitude+","+Input.gyro.gravity+","+Input.gyro.) }));
    }

    public void OnClick()
    {
        SceneLoader.Instance.LoadScene("Login");
    }
}
