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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDone)
        {
            var heading = (int)(GPSManager.Instance.heading);
            // 나침반에 대해 실시간 변경
            image.transform.rotation = Quaternion.Euler(0, 0, heading);
            angleText.text = heading.ToString();

            if (heading < 3 || heading > 357)
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
