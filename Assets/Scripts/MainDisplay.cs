﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using LitJson;
using System;

public class MainDisplay : MonoBehaviour
{

    public Button optionBT;
    public Button RecycleBT;

    public GameObject mainDisplay;

    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void clickOptionBT()
    {
        AnchorManager.currentState = AnchorManager.State.MyInfo;
        SceneManager.LoadScene("UserInfo", LoadSceneMode.Additive);
    }
}
