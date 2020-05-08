using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class AutoLogin : MonoBehaviour
{


    public InputField id;
    public InputField pw;

    public Toggle auto;
    public static string userId;
    public static string userPw;

    // Start is called before the first frame update
    void Start()
    {
        if (auto.isOn)
        {
            if(PlayerPrefs.HasKey("ID"))
            {
                userId = PlayerPrefs.GetString("ID");
                userPw = PlayerPrefs.GetString("PW");
                SceneManager.LoadScene("complete");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void Login()
    {
        userId = id.text;
        userPw = pw.text;
        
        if(userId == "aaa" && userPw =="1234")
        {
            if (auto.isOn)
            {
                PlayerPrefs.SetString("ID", userId);
                PlayerPrefs.SetString("PW", userPw);
                SceneManager.LoadScene("complete");
            }
            else
            {
                SceneManager.LoadScene("complete");
            }
        }
    }
}
