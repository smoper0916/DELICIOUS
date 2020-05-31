using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

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
    public GameObject PhotoData;

    public Button closeBtn;

    GameObject target;
    TextMesh[] textMeshs;

    MenuTabResult menuTabResult = new MenuTabResult();

    public GameObject canvas;

    string id;
    string txt;
    private bool flagWakeUp = false;
    private bool flagSelect = false;

    Dictionary<string, string> dic = new Dictionary<string, string>();
    // Start is called before the first frame update
    void Start()
    {
     
    }

    private void Update()
    {
        if (AnchorManager.showCheck)
        {
            txt = "";
            this.target = AnchorManager.target;
            textMeshs = target.GetComponentsInChildren<TextMesh>();
            id = textMeshs[0].text;
            restaurantName.text = textMeshs[2].text;
            score.text = textMeshs[1].text;

            AnchorManager.showCheck = false;
        }
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

        reviewScrollRect.SetActive(true);

        ReviewTabResult reviewTabResult = eventHandler.result as ReviewTabResult;

        ScrollRect scrollRect = reviewScrollRect.GetComponent<ScrollRect>();
        Debug.Log(scrollRect.content.transform.parent);
        float y = 0;

        if (reviewTabResult.reviewList.Count == 0)
        {
            var datas = Instantiate(ReviewData, new Vector3(0, y, 0), Quaternion.identity,scrollRect.content);
            Text[] texts = datas.GetComponentsInChildren<Text>();

            texts[0].text = "";
            texts[1].text = "리뷰가 없습니다.";

        }

        foreach (Review review in reviewTabResult.reviewList)
        {
            var datas = Instantiate(ReviewData, new Vector3(0, y, 0), Quaternion.identity, scrollRect.content);
            Text[] texts = datas.GetComponentsInChildren<Text>();

            texts[0].text = review.rating + "    " + review.date;
            texts[1].text = review.text;

            y -= datas.GetComponent<RectTransform>().rect.height;

        }

    }
    private IEnumerator loadPhoto()
    {
        dic.Add("url", "restaurant/" + id + "/photo");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Photo);

        while (!flagWakeUp)
            yield return new WaitForSeconds(1.0f);

        List<string> urls = eventHandler.result as List<string>;

        ScrollRect scrollRect = photoScrollRect.GetComponent<ScrollRect>();

        float y = 0;
        for (int i = 0; i < urls.Count / 2 + 1; i++)
        {
            var datas = Instantiate(PhotoData, new Vector3(0, y, 0), Quaternion.identity,scrollRect.content);
            RawImage[] photos = scrollRect.content.GetComponentsInChildren<RawImage>();

            StartCoroutine(downloadPhoto(urls[i], photos[0]));

            StartCoroutine(downloadPhoto(urls[i + 1], photos[1]));

            y -= 400;

        }

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

    private IEnumerator downloadPhoto(string url, RawImage image)
    {
        url = url;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            image.texture = myTexture;
        }

    }

    public void WakeUp()
    {
        Debug.Log("Wake Up!");
        flagWakeUp = true;

        dic.Clear();
    }
    public void Exit()
    {
        this.gameObject.SetActive(false);
        canvas.SetActive(true);
        //Destroy(this.gameObject);
    }
}
