using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager: MonoBehaviour {

    private RectTransform rectComponent;
    private Image imageComp;

    public float speed = 400f;
    public Text text;
    public Text textNormal;
    public string state = "";
    public float step = 0.0f;
    private bool flagUpdateNeed = false;

    // Use this for initialization
    void Start () {
        rectComponent = GetComponent<RectTransform>();
        imageComp = rectComponent.GetComponent<Image>();
        imageComp.fillAmount = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
        if (flagUpdateNeed && imageComp.fillAmount < step)
        {
            imageComp.fillAmount = imageComp.fillAmount + Time.deltaTime * speed;
            text.text = (int)(imageComp.fillAmount * 100) + "%";
            textNormal.text = state;
        }
        else
        {
            flagUpdateNeed = false;
        }

    }

    public void ToggleUpdateFlag()
    {
        flagUpdateNeed = true;
    }

    public void tt()
    {


        int a = 0;
        if (imageComp.fillAmount != 1f)
        {
            imageComp.fillAmount = imageComp.fillAmount + Time.deltaTime * speed;
            a = (int)(imageComp.fillAmount * 100);
            if (a > 0 && a <= 33)
            {
                textNormal.text = "Loading...";
            }
            else if (a > 33 && a <= 67)
            {
                textNormal.text = "Downloading...";
            }
            else if (a > 67 && a <= 100)
            {
                textNormal.text = "Please wait...";
            }
            else
            {

            }
            text.text = a + "%";
        }
        else
        {
            imageComp.fillAmount = 0.0f;
            text.text = "0%";
        }
    }
}
