using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventHandler : MonoBehaviour
{
    public Coroutine coroutine { get; private set; }
    public object result = null;
    public bool isDone = false;

    public enum HandlingType { Restaurants, DetailedRestaurant, MyInfo }
    private IEnumerator target;
    private Queue<(MonoBehaviour owner, IEnumerator target, HandlingType type)> events = new Queue<(MonoBehaviour, IEnumerator, HandlingType)>();
    
    GameObject MemberManager;
    GameObject SpawnManger;
    GameObject DetailedRestaurantManager;
    GameObject LoginManager;
    GameObject LikedManager;
    GameObject Navigation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        while(events.Count > 0 && isDone)
        {
            var item = events.Dequeue();

            // 처리할 것들을 처리하자
            JsonData jsonResult = (JsonData)(result);
            IDictionary dict = jsonResult;
            switch (item.type)
            {
                case HandlingType.Restaurants:
                    if (dict.Contains("restaurants"))
                    {
                        // 정상처리된 경우
                        List<Restaurant> restaurants = new List<Restaurant>();
                        foreach (JsonData i in jsonResult["restaurants"])
                        {
                            restaurants.Add(new Restaurant(i));
                        }
                        result = restaurants;
                    }
                    else if(dict.Contains("code"))
                    {
                        // 실패된 경우
                        result = jsonResult;
                    }
                    break;
                case HandlingType.DetailedRestaurant:
                    if (dict.Contains("info"))
                    {
                        List<(string menu, string price)> menu = new List<(string menu, string price)>();
                        foreach (JsonData i in jsonResult["menu"])
                        {
                            menu.Add((i["name"].ToString(), i["price"].ToString()));
                            // 썸네일까지 불러오게 해야함.
                        }
                        //jsonResult["info"]["desc"]
                        result = menu;
                    }
                    else if (dict.Contains("code"))
                    {
                        // 실패된 경우

                    }
                    break;
                case HandlingType.MyInfo:
                    break;
                default:

                    break;
            }
            item.owner.Invoke("WakeUp", 0);
            // 만약 이벤트가 2개 이상 쌓여있으면 100% 터짐.
        }
    }

    public void onClick(MonoBehaviour owner, IEnumerator target, HandlingType type)
    {
        this.target = target;
        this.coroutine = StartCoroutine(Run());
        events.Enqueue((owner, target, type));
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        isDone = true;
    }
}
