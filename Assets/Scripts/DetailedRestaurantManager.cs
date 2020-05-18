using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailedRestaurantManager : MonoBehaviour
{
    public EventHandler eventHandler;
    public ServerManager serverManager;

    public ToggleGroup toggles;
    public Toggle menuToggle;
    public Toggle reviewToggle;
    public Toggle photoToggle;

    public Text restaurantName;
    public Text score;

    public GameObject menuScrollRect;
    public GameObject reviewScrollRect;
    public GameObject photoScrollRect;

    public GameObject ReviewData;

    GameObject target;
    TextMesh[] textMeshs;

    MenuTabResult menuTabResult = new MenuTabResult();

    string id;
    string txt;
    private bool flagWakeUp = false;
    private bool flagSelect = false;

    Dictionary<string, string> dic = new Dictionary<string, string>();
    // Start is called before the first frame update
    void Start()
    {
        txt = "";
        this.target = AnchorManager.target;
        textMeshs = target.GetComponentsInChildren<TextMesh>();
        id = textMeshs[0].text;
        restaurantName.text = textMeshs[2].text;
        score.text = textMeshs[1].text;
    }

    private IEnumerator loadMenu()
    {
        flagWakeUp = false;
        dic.Add("url", "restaurant/" + id + "/menu");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Menus);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        menuTabResult = eventHandler.result as MenuTabResult;
        
        foreach (Menu menu in menuTabResult.menuList)
        {
            txt = txt + menu.name + "------------" + menu.price + "\n";
        }
        Debug.Log(txt);
        reviewScrollRect.SetActive(false);
        photoScrollRect.SetActive(false);

        menuScrollRect.SetActive(true);

        ScrollRect scrollRect = menuScrollRect.GetComponent<ScrollRect>();
        Text contents = scrollRect.content.GetComponentInChildren<Text>();
        contents.text = txt;

    }
    private IEnumerator loadReviews()
    {
        flagWakeUp = false;
        dic.Add("url", "restaurant/" + id + "/reviews");
        dic.Add("method", "GET");
        dic.Add("page", "0");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Reviews);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        menuScrollRect.SetActive(false);
        photoScrollRect.SetActive(false);

        ReviewTabResult reviewTabResult = eventHandler.result as ReviewTabResult;

        ScrollRect scrollRect = reviewScrollRect.GetComponent<ScrollRect>();
        float y = 0;
        foreach(Review review in reviewTabResult.reviewList)
        {
            var datas = Instantiate(ReviewData, new Vector3(0, y, 0), Quaternion.identity);
            Text[] texts = datas.GetComponentsInChildren<Text>();

            texts[0].text = review.rating + "    " + review.date;
            texts[1].text = review.text;

            datas.transform.SetParent(scrollRect.content);

            y -= datas.GetComponent<RectTransform>().rect.height;

        }
        
        reviewScrollRect.SetActive(true);

    }
    private IEnumerator loadPhoto()
    {
        dic.Add("url", "restaurant/" + id + "/photo");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Photo);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        menuScrollRect.SetActive(false);
        reviewScrollRect.SetActive(false);

        photoScrollRect.SetActive(true);

    }
    public void selectMenuTap()
    {
        StartCoroutine(loadMenu());
    }
    public void selectReviewTap()
    {
        StartCoroutine(loadReviews());
    }
    public void selectPhotoTap()
    {
        StartCoroutine(loadPhoto());
    }

    public void WakeUp()
    {
        Debug.Log("Wake Up!");
        flagWakeUp = true;
    }
}
