using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedRestaurantManager : MonoBehaviour
{
    public EventHandler eventHandler;
    public ServerManager serverManager;
   
    GameObject target;
    TextMesh[] textMeshs;
    string id;

    private bool flagWakeUp = false;

    Dictionary<string, string> dic = new Dictionary<string, string>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.target = AnchorManager.target;
        textMeshs = target.GetComponentsInChildren<TextMesh>();
        id = textMeshs[0].text;
    }

    private IEnumerator loadMenu()
    {
        dic.Add("url", "restaurants/" + id + "menu");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(5.0f);
    }
    private IEnumerator loadReviews()
    {
        dic.Add("url", "restaurants/" + id + "reviews");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(5.0f);
    }
    private IEnumerator loadPhoto()
    {
        dic.Add("url", "restaurants/" + id + "photo");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Restaurants);

        while (!flagWakeUp)
            yield return new WaitForSeconds(5.0f);
    }

    public void getTargetInfo(GameObject target)
    {
        this.target = target;

        textMeshs = gameObject.GetComponentsInChildren<TextMesh>();
        id = textMeshs[0].text;
    }

    public void WakeUp()
    {
        Debug.Log("Wake Up!");
        flagWakeUp = true;
    }
}
