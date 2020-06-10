using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassMover : MonoBehaviour
{
    public Image image;
    public Text angleText;
    public Text guideText;

    public bool isDone = false;
    public bool isGoodDegree = false;


    public float reading;
    public Queue<float> data;
    private float[] dataList;
    public int maxData = 5;
    public int count;
    public float sum, average;

    public float dampening = 0.1f;
    public float rotationToNorth;

    // Start is called before the first frame update
    void Start()
    {
        data = new Queue<float>();
        dataList = new float[maxData];
        rotationToNorth = GPSManager.Instance.heading;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDone)
        {
            count = data.Count;

            if (data.Count > 0)
            {
                data.CopyTo(dataList, 0);
            }

            if (data.Count == maxData)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sum += dataList[i];
                }
                if (Mathf.Abs(dataList[maxData - 1]) > 0)
                {
                    average = sum / maxData;
                    sum = 0;
                    data.Clear();
                    dataList = new float[maxData];

                }
            }

            if (data.Count >= maxData)
            {
                data.Dequeue();
            }

            reading = Mathf.Round(GPSManager.Instance.heading);
            data.Enqueue(reading);

                /*
                rotationToNorth = average / 180;
                rotationToNorth = (Mathf.Round(rotationToNorth * 10)) / 10;
                rotationToNorth = rotationToNorth - 1;
                */

            rotationToNorth = Mathf.LerpAngle(rotationToNorth, GPSManager.Instance.heading, 0.1f);
            // 나침반에 대해 실시간 변경
            //var heading = (int)(rotationToNorth);
            var heading = reading;
            image.transform.rotation = Quaternion.Euler(0, 0, heading);
            angleText.text = heading.ToString();

            if (heading <= 2 && heading >= 0)
            {
                guideText.text = "조정 중...";
                isGoodDegree = true;
            }
            else if (heading >= 3 && heading < 180)
            {
                guideText.text = "0도까지 왼쪽 방향으로 회전해주세요.";
            }
            else if (heading >= 180 && heading <= 357)
            {
                guideText.text = "0도까지 오른쪽 방향으로 회전해주세요.";
            }
            else
            {
                guideText.text = "0도까지 회전해주세요.";
            }
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
