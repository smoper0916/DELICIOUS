using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastMaker : MonoBehaviour
{
#if UNITY_ANDROID

    static public ToastMaker instance;

    AndroidJavaObject currentActivity;
    AndroidJavaClass UnityPlayer;
    AndroidJavaObject context;
    AndroidJavaObject toast;

    public enum Gravity { DEFAULT, VERTICAL_CENTER }

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        UnityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        currentActivity = UnityPlayer
            .GetStatic<AndroidJavaObject>("currentActivity");


        context = currentActivity
            .Call<AndroidJavaObject>("getApplicationContext");
    }

    public void ShowToast(string message, Gravity gravity = Gravity.VERTICAL_CENTER)
    {
        currentActivity.Call
        (
            "runOnUiThread",
            new AndroidJavaRunnable(() =>
            {
                AndroidJavaClass Toast
                = new AndroidJavaClass("android.widget.Toast");

                AndroidJavaClass Gravity
                = new AndroidJavaClass("android.view.Gravity");

                AndroidJavaObject javaString
                = new AndroidJavaObject("java.lang.String", message);

                toast = Toast.CallStatic<AndroidJavaObject>
                (
                    "makeText",
                    context,
                    javaString,
                    Toast.GetStatic<int>("LENGTH_SHORT")
                );

                if (gravity == ToastMaker.Gravity.VERTICAL_CENTER)
                {
                    var center = Gravity.GetStatic<int>("CENTER_VERTICAL");
                    toast.Call("setGravity", center, 0, 0);
                }
                toast.Call("show");
            })
         );
    }

    public void CancelToast()
    {
        currentActivity.Call("runOnUiThread",
            new AndroidJavaRunnable(() =>
            {
                if (toast != null) toast.Call("cancel");
            }));
    }


#else
    void Awake()
    {
        Destroy(gameObject);
    }
#endif
}
