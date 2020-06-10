using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaviColider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        ToastMaker.instance.ShowToast("충돌충돌!!!!");
        Debug.Log("충돌충돌충돌충돌충돌충돌충돌충돌");
        if (other.gameObject.tag == "wayPoint")
        {
            Debug.Log("충돌충돌충돌충돌충돌충돌충돌충돌");
            //NavigationManager.idx++;
            NavigationManager.checkWayPoint = false;
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("destination"))
        {
            Destroy(other.gameObject);
            NavigationManager.checkWayPoint = false;
            //arrow.SetActive(false);

            ToastMaker.instance.ShowToast("도착했습니다");
            //이력 등록


        }
        ToastMaker.instance.ShowToast("충돌충돌2!!!!");
    }
}
