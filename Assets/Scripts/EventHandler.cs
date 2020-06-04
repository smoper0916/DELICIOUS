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

    public enum HandlingType { Restaurants, DetailedRestaurant, MyInfo, Default, Menus, Photo, Reviews, Route }
    private IEnumerator target;
    private Queue<(MonoBehaviour owner, IEnumerator target, HandlingType type)> events = new Queue<(MonoBehaviour, IEnumerator, HandlingType)>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        while (events.Count > 0 && isDone)
        {
            var item = events.Dequeue();

            if (result is JsonData)
            {
                JsonData jsonResult = result as JsonData;
                IDictionary dict = jsonResult;
                switch (item.type)
                {
                    case HandlingType.Restaurants:
                        if (dict.Contains("restaurants"))
                        {
                            // 정상처리된 경우
                            Dictionary<string, Restaurant> restaurants = new Dictionary<string, Restaurant>();
                            //List<Restaurant> restaurants = new List<Restaurant>();
                            foreach (JsonData i in jsonResult["restaurants"])
                            {
                                restaurants.Add(i["id"].ToString(), new Restaurant(i));
                            }
                            result = restaurants;
                        }
                        else if (dict.Contains("code"))
                        {
                            // 실패된 경우
                            result = jsonResult;
                        }
                        break;
                    case HandlingType.Menus:
                        if (dict.Contains("info"))
                        {
                            List<Menu> menuList = new List<Menu>();
                            foreach (JsonData i in jsonResult["menu"])
                            {
                                menuList.Add(new Menu(i["name"].ToString(), i["price"].ToString()));
                                // 썸네일까지 불러오게 해야함.
                            }
                            result = new MenuTabResult(menuList, jsonResult["info"]);
                        }
                        else if (dict.Contains("code"))
                        {
                            // 실패된 경우
                            result = jsonResult;
                        }
                        break;
                    case HandlingType.Reviews:
                        if (dict.Contains("records"))
                        {
                            List<Review> reviewList = new List<Review>();
                            foreach (JsonData i in jsonResult["records"])
                            {
                                reviewList.Add(new Review(i["name"].ToString(), i["rating"].ToString(), i["date"].ToString(), i["text"].ToString()));
                            }
                            float avg = float.Parse(jsonResult["avg"].ToString());
                            int pages = 1;
                            if (dict.Contains("pages"))
                                pages = int.Parse(jsonResult["pages"].ToString());

                            result = new ReviewTabResult(reviewList, avg, pages);
                        }
                        else if (dict.Contains("code"))
                        {
                            // 실패된 경우
                            result = jsonResult;
                        }
                        break;
                    case HandlingType.Photo:
                        if (dict.Contains("urls"))
                        {
                            // 정상처리된 경우
                            List<string> urls = new List<string>();
                            foreach (JsonData i in jsonResult["urls"])
                            {
                                urls.Add(i.ToString());
                            }
                            result = urls;
                            Debug.Log("이건 URL입니다.");
                        }
                        else if (dict.Contains("code"))
                        {
                            // 실패된 경우
                            result = jsonResult;
                            Debug.Log("이건 에러입니다.");
                        }
                        break;
                    case HandlingType.Route:
                        if (dict.Contains("features"))
                        {
                            // 정상처리된 경우
                            List<WayPoint> points = new List<WayPoint>();
                            foreach (JsonData i in jsonResult["features"])
                            {
                                points.Add(new WayPoint(i["lon"].ToString(), i["lat"].ToString()));
                            }
                            result = points;
                        }
                        else if (dict.Contains("code"))
                        {
                            // 실패된 경우
                            result = jsonResult;
                        }
                        break;
                    case HandlingType.Default:
                        if (dict.Contains("code"))
                        {
                            result = jsonResult;
                        }
                        break;
                    default:

                        break;
                }
            }
            item.owner.Invoke("WakeUp", 0);
            Debug.Log("Invoked");
            // 만약 이벤트가 2개 이상 쌓여있으면 100% 터짐.
        }
    }

    public void onClick(MonoBehaviour owner, IEnumerator target, HandlingType type)
    {
        isDone = false;
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
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
        Debug.Log("Finished Request");
    }
}
