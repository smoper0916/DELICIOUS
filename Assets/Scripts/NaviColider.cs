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
        if (other.gameObject.tag == "wayPoint")
        {
            Debug.Log("충돌충돌충돌충돌충돌충돌충돌충돌");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("destination"))
        {
            Destroy(other.gameObject);
           // arrow.SetActive(false);

            //이력 등록


        }
    }
}
