using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DetailedRestaurantManager : MonoBehaviour
{
    public EventHandler eventHandler;
    public ServerManager serverManager;

    public Toggle heartToggle;

    public Button menuBtn;
    public Button reviewBtn;
    public Button photoBtn;
    private bool[] btnPressed = { false, false, false };

    public Text restaurantName;
    public Text score;

    public GameObject menuScrollRect;
    public GameObject reviewScrollRect;
    public GameObject photoScrollRect;

    public GameObject ReviewData;
    public GameObject MenuPanelData;
    public GameObject MenuItemData1;
    public GameObject MenuItemData1WithoutPhoto;
    public GameObject MenuItemData2;
    public GameObject MenuItemData2WithoutPhoto;
    public GameObject DespPanelData;

    public GameObject NothingPanel;
    public Text NothingText;
    public GameObject BizHourText;

    public GameObject LoadingPoints;

    public Button closeBtn;

    bool isEmptyMenu = false;
    bool isEmptyReview = false;
    bool isEmptyPhoto = false;


    GameObject target;
    TextMesh[] textMeshs;
    List<GameObject> panels = new List<GameObject>();

    MenuTabResult menuTabResult = new MenuTabResult();



    public GameObject canvas;

    string id;
    string txt;
    private bool flagWakeUp = false;
    private bool flagSelect = false;
    private bool isInitLikedToggle = false;

    private bool menuCheck = false; bool onMenuCoroutine = false;
    private bool reviewCheck = false; bool onReviewCoroutine = false;
    private bool photoCheck = false; bool onPhotoCoroutine = false;

    public static Dictionary<string, Restaurant> zzim = new Dictionary<string, Restaurant>();
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

            isInitLikedToggle = true;
            if (zzim.ContainsKey(id))
            {
                heartToggle.isOn = true;
            }
            else
            {
                heartToggle.isOn = false;
            }

            AnchorManager.showCheck = false;

            selectMenuTap();
        }
    }

    private IEnumerator loadMenu()
    {
        flagWakeUp = false;
        dic.Clear();
        dic.Add("url", "restaurant/" + id + "/menu");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Menus);

        LoadingPoints.SetActive(true);
        while (!flagWakeUp)
            yield return new WaitForSeconds(0.1f);

        LoadingPoints.SetActive(false);

        ScrollRect scrollRect = menuScrollRect.GetComponent<ScrollRect>();

        
        if (eventHandler.result is MenuTabResult)
        {
            menuTabResult = eventHandler.result as MenuTabResult;
            var bizHourObject = Instantiate(BizHourText, new Vector3(0, 0, 0), Quaternion.identity, scrollRect.content);
            if (menuTabResult.biztime != "")
            {
                bizHourObject.GetComponent<Text>().text = menuTabResult.biztime;
            }
            else
            {
                bizHourObject.GetComponent<Text>().text = "영업시간 정보가 없네요.";
            }

            if (menuTabResult.menuList.Count != 0)
            {
                for (int i = 0; i < menuTabResult.menuList.Count; i += 2)
                {
                    var menuPanelObject = Instantiate(MenuPanelData, new Vector3(0, 0, 0), Quaternion.identity, scrollRect.content);

                    Menu menu1 = menuTabResult.menuList[i];

                    GameObject menuItemObject1 = Instantiate((menu1.img != null) ? MenuItemData1 : MenuItemData1WithoutPhoto, menuPanelObject.transform);
                    Text[] txtComponents = menuItemObject1.GetComponentsInChildren<Text>();
                    
                    txtComponents[0].text = menu1.name;
                    txtComponents[1].text = menu1.price;

                    if (menu1.img != null)
                    {

                        RawImage thumnail = menuItemObject1.GetComponentInChildren<RawImage>();
                        IEnumerator sender = downloadPhoto(menu1.img, thumnail);
                        while (sender.MoveNext())
                        {
                            object result = sender.Current;

                            if (result is UnityWebRequestAsyncOperation)
                            {
                                var r = (UnityWebRequestAsyncOperation)result;
                                while (!r.webRequest.isDone)
                                    yield return new WaitForSeconds(0.1f);
                            }
                            else
                            {
                                Debug.Log(result);
                            }
                        }
                    }

                    if (i != menuTabResult.menuList.Count - 1)
                    {
                        Menu menu2 = menuTabResult.menuList[i + 1];
                        GameObject menuItemObject2 = Instantiate((menu2.img != null) ? MenuItemData2 : MenuItemData2WithoutPhoto, menuPanelObject.transform);
                        Text[] txtComponents2 = menuItemObject2.GetComponentsInChildren<Text>();

                        txtComponents2[0].text = menu2.name;
                        txtComponents2[1].text = menu2.price;

                        if (menu2.img != null)
                        {
                            RawImage thumnail = menuItemObject2.GetComponentInChildren<RawImage>();
                            IEnumerator sender = downloadPhoto(menu2.img, thumnail);
                            while (sender.MoveNext())
                            {
                                object result = sender.Current;

                                if (result is UnityWebRequestAsyncOperation)
                                {
                                    var r = (UnityWebRequestAsyncOperation)result;
                                    while (!r.webRequest.isDone)
                                        yield return new WaitForSeconds(0.1f);
                                }
                                else
                                {
                                    Debug.Log(result);
                                }
                            }
                        }


                        RectTransform menuRect1 = menuItemObject1.GetComponent<RectTransform>();
                        RectTransform menuRect2 = menuItemObject2.GetComponent<RectTransform>();
                        if (menu1.img == null && menu2.img == null)
                            menuPanelObject.GetComponent<RectTransform>().sizeDelta = new Vector2(menuPanelObject.GetComponent<RectTransform>().sizeDelta[0], 500);
                        else if (menu1.img == null)
                            menuRect1.offsetMin = new Vector2(menuRect1.offsetMin.x, 600);
                        else if (menu2.img == null)
                            menuRect2.offsetMin = new Vector2(menuRect2.offsetMin.x, 600);
                    }
                    else
                    {
                        RectTransform menuRect1 = menuItemObject1.GetComponent<RectTransform>();
                        if (menu1.img == null)
                        {
                            menuPanelObject.GetComponent<RectTransform>().sizeDelta = new Vector2(menuPanelObject.GetComponent<RectTransform>().sizeDelta[0], 500);
                            menuRect1.offsetMin = new Vector2(menuRect1.offsetMin.x, 600);
                        }
                    }
                }
                reviewScrollRect.SetActive(false);
                photoScrollRect.SetActive(false);

                menuScrollRect.SetActive(true);

                //ScrollRect scrollRect = menuScrollRect.GetComponent<ScrollRect>();
                Text contents = scrollRect.content.GetComponentInChildren<Text>();
                contents.text = txt;
                isEmptyMenu = false;
                reviewScrollRect.SetActive(false);
                photoScrollRect.SetActive(false);
                menuScrollRect.SetActive(true);
            }

            if (menuTabResult.desc != "")
            {
                var despPanelObject = Instantiate(DespPanelData, new Vector3(0, 0, 0), Quaternion.identity, scrollRect.content);
                despPanelObject.transform.Find("DespText").GetComponent<Text>().text = menuTabResult.desc;
            }
        }
        else
        {
            NothingText.text = "이 식당은 메뉴 정보가 없습니다.";
            NothingPanel.SetActive(true);
            isEmptyMenu = true;
        }
        menuCheck = true;
        onMenuCoroutine = false;
    }
    private IEnumerator loadReviews()
    {
        
        flagWakeUp = false;
        dic.Clear();
        dic.Add("url", "restaurant/" + id + "/reviews");
        dic.Add("method", "GET");
        dic.Add("page", "0");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Reviews);
        LoadingPoints.SetActive(true);
        while (!flagWakeUp)
            yield return new WaitForSeconds(0.1f);

        LoadingPoints.SetActive(false);

        ReviewTabResult reviewTabResult = eventHandler.result != null ? eventHandler.result as ReviewTabResult : null;

        ScrollRect scrollRect = reviewScrollRect.GetComponent<ScrollRect>();
        Debug.Log(scrollRect.content.transform.parent);
        float y = 0;

        if (reviewTabResult == null || reviewTabResult.reviewList.Count == 0)
        {
            var datas = Instantiate(ReviewData, new Vector3(0, y, 0), Quaternion.identity, scrollRect.content);
            Text[] texts = datas.GetComponentsInChildren<Text>();

            //texts[0].text = "";
            //texts[1].text = "리뷰가 없습니다.";
            NothingText.text = "이 식당에 남겨진 리뷰가 없습니다.";
            NothingPanel.SetActive(true);
            isEmptyReview = true;
            reviewCheck = true;
            onReviewCoroutine = false;
            yield break;
        }

        isEmptyReview = false;
        foreach (Review review in reviewTabResult.reviewList)
        {
            var datas = Instantiate(ReviewData, new Vector3(0, y, 0), Quaternion.identity, scrollRect.content);
            Text[] texts = datas.GetComponentsInChildren<Text>();

            texts[0].text = review.rating + "    " + review.date;
            texts[1].text = review.text;

            y -= datas.GetComponent<RectTransform>().rect.height;

        }
        reviewScrollRect.SetActive(true);
        photoScrollRect.SetActive(false);
        menuScrollRect.SetActive(false);
        reviewCheck = true;
        onReviewCoroutine = false;

    }
    private IEnumerator loadPhoto()
    {
        flagWakeUp = false;
        dic.Clear();
        dic.Add("url", "restaurant/" + id + "/photo");
        dic.Add("method", "GET");

        eventHandler.onClick(this, serverManager.SendRequest(dic), EventHandler.HandlingType.Photo);
        LoadingPoints.SetActive(true);
        while (!flagWakeUp)
            yield return new WaitForSeconds(0.1f);
        LoadingPoints.SetActive(false);
        if (eventHandler.result == null) Debug.Log("eventHandler.result가 null이에요.");
        List<string> urls = eventHandler.result as List<string>;
        ScrollRect scrollRect = photoScrollRect.GetComponent<ScrollRect>();
        Debug.Log(urls);
        if (urls == null || urls.Count == 0)
        {
            NothingText.text = "사진이 없습니다.";
            NothingPanel.SetActive(true);
            isEmptyPhoto = true;
            photoCheck = true;
            onPhotoCoroutine = false;
            yield break;
        }
        isEmptyPhoto = false;
        float y = 0;

        reviewScrollRect.SetActive(false);
        photoScrollRect.SetActive(true);
        menuScrollRect.SetActive(false);

        var fixedWidth = 1800; var fixedHeight = 300;
        var photoSize = 350; var pivotX = 1.3f; var pivotY = 1.0f;
        for (int i = 0; i < urls.Count; i += 2)
        {
            GameObject photoPanel = new GameObject("PhotoPanel" + i);
            photoPanel.AddComponent<CanvasRenderer>();
            RectTransform photoPanelRect = photoPanel.AddComponent<RectTransform>();
            photoPanelRect.sizeDelta = new Vector2(fixedWidth, fixedHeight);
            //photoPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(fixedWidth, fixedHeight);
            photoPanel.transform.SetParent(scrollRect.content);
            //
            GameObject insidePhoto1 = new GameObject("InsidePhoto1");
            insidePhoto1.AddComponent<CanvasRenderer>();
            RectTransform insidePhotoRect1 = insidePhoto1.AddComponent<RectTransform>();
            insidePhotoRect1.pivot = new Vector2(pivotX, pivotY);
            insidePhotoRect1.sizeDelta = new Vector2(photoSize, photoSize);

            insidePhoto1.transform.SetParent(photoPanel.transform);
            insidePhotoRect1.localPosition = new Vector3(0, 0, 0);

            RawImage rawPhoto1 = insidePhoto1.AddComponent<RawImage>();

            IEnumerator sender = downloadPhoto(urls[i], rawPhoto1);
            while (sender.MoveNext())
            {
                object result = sender.Current;

                if (result is UnityWebRequestAsyncOperation)
                {
                    var r = (UnityWebRequestAsyncOperation)result;
                    while (!r.webRequest.isDone)
                        yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    Debug.Log(result);
                }
            }
            if (i != urls.Count - 1)
            {
                GameObject insidePhoto2 = new GameObject("InsidePhoto2");
                insidePhoto2.AddComponent<CanvasRenderer>();
                RectTransform insidePhotoRect2 = insidePhoto2.AddComponent<RectTransform>();
                insidePhotoRect2.pivot = new Vector2(pivotX, pivotY);
                insidePhotoRect2.sizeDelta = new Vector2(photoSize, photoSize);

                insidePhoto2.transform.SetParent(photoPanel.transform);
                insidePhotoRect2.localPosition = new Vector3(photoSize + 75, 0, 0);
                RawImage rawPhoto2 = insidePhoto2.AddComponent<RawImage>();
                

                sender = downloadPhoto(urls[i + 1], rawPhoto2);
                while (sender.MoveNext())
                {
                    object result = sender.Current;

                    if (result is UnityWebRequestAsyncOperation)
                    {
                        var r = (UnityWebRequestAsyncOperation)result;
                        while (!r.webRequest.isDone)
                            yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        Debug.Log(result);
                    }
                }
            }
        }
        GameObject blankPanel = new GameObject("BlankPanel");
        blankPanel.AddComponent<CanvasRenderer>();
        RectTransform blankPanelRect = blankPanel.AddComponent<RectTransform>();
        blankPanelRect.sizeDelta = new Vector2(fixedWidth, fixedHeight);
        blankPanel.transform.SetParent(scrollRect.content);


        photoCheck = true;
        onPhotoCoroutine = false;
    }
    public void selectMenuTap()
    {
        if (!btnPressed[0] &&(!onMenuCoroutine && !onReviewCoroutine && !onPhotoCoroutine))
        {
            btnPressed = new bool[] { true, false, false };
            menuBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_menu_on");
            reviewBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_review_off");
            photoBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_photo_off");

            NothingPanel.SetActive(false);
            if (!menuCheck)
            {
                StartCoroutine(loadMenu());
            }
            else
            {
                reviewScrollRect.SetActive(false);
                photoScrollRect.SetActive(false);


                if (isEmptyMenu)
                {
                    NothingText.text = "이 식당은 메뉴 정보가 없습니다.";
                    NothingPanel.SetActive(true);
                    menuScrollRect.SetActive(false);
                }
                else
                {
                    menuScrollRect.SetActive(true);
                }
            }
        }
    }
    public void selectReviewTap()
    {
        if (!btnPressed[1] && (!onMenuCoroutine && !onReviewCoroutine && !onPhotoCoroutine))
        {
            btnPressed = new bool[] { false, true, false };
            menuBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_menu_off");
            reviewBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_review_on");
            photoBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_photo_off");

            NothingPanel.SetActive(false);
            if (!reviewCheck)
            {
                StartCoroutine(loadReviews());

            }
            else
            {
                menuScrollRect.SetActive(false);
                photoScrollRect.SetActive(false);

                if (isEmptyReview)
                {
                    NothingText.text = "이 식당에 남겨진 리뷰가 없습니다.";
                    NothingPanel.SetActive(true);
                    reviewScrollRect.SetActive(false);
                }
                else
                {
                    reviewScrollRect.SetActive(true);
                }
            }
        }
    }
    public void selectPhotoTap()
    {
        if (!btnPressed[2] && (!onMenuCoroutine && !onReviewCoroutine && !onPhotoCoroutine))
        {
            btnPressed = new bool[] { false, false, true };
            menuBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_menu_off");
            reviewBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_review_off");
            photoBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_photo_on");
            NothingPanel.SetActive(false);
            if (!photoCheck)
            {
                StartCoroutine(loadPhoto());

                return;
            }
            else
            {
                menuScrollRect.SetActive(false);
                reviewScrollRect.SetActive(false);
                if (isEmptyPhoto)
                {
                    NothingText.text = "사진이 없습니다.";
                    NothingPanel.SetActive(true);
                    photoScrollRect.SetActive(false);
                }
                else
                {
                    photoScrollRect.SetActive(true);
                }
            }
        }
    }

    private IEnumerator downloadPhoto(string url, RawImage image)
    {
        Debug.Log("StartDownloadingPhoto => " + url);
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
            Debug.Log("Texture 생성 완료");
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
        ScrollRect menuRect = menuScrollRect.GetComponent<ScrollRect>();
        ScrollRect reviewRect = reviewScrollRect.GetComponent<ScrollRect>();
        ScrollRect photoRect = photoScrollRect.GetComponent<ScrollRect>();

        Text contents = menuRect.content.GetComponentInChildren<Text>();
        RawImage[] photos = photoRect.content.GetComponentsInChildren<RawImage>();

        AnchorManager.currentState = AnchorManager.State.Browse;
        menuCheck = false; isEmptyMenu = false; onMenuCoroutine = false;
        reviewCheck = false; isEmptyReview = false; onReviewCoroutine = false;
        photoCheck = false; isEmptyPhoto = false; onPhotoCoroutine = false;
        contents.text = "";

        foreach (Transform child in menuRect.content)
            Destroy(child.gameObject);
        foreach (Transform child in reviewRect.content)
            Destroy(child.gameObject);
        foreach (Transform child in photoRect.content)
            Destroy(child.gameObject);


        btnPressed = new bool[] { false, false, false };
        menuBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_menu_off");
        reviewBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_review_off");
        photoBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("btnDetailed_photo_off");
        this.gameObject.SetActive(false);
        canvas.SetActive(true);
        //Destroy(this.gameObject);
        Debug.Log("Exit이 잘 되었습니다.");
    }

    public void ClickHeart()
    {
        if (isInitLikedToggle) isInitLikedToggle = false;
        else
        {
            if (zzim.ContainsKey(id))
            {
                //AnchorManager.restaurants[id].zzimCheck = false;
                heartToggle.isOn = false;
                zzim.Remove(id);
                Debug.Log(id + "삭제");
                ToastMaker.instance.ShowToast("찜이 취소되었습니다.");
            }
            else
            {
                if (zzim.Count < 4)
                {
                    //AnchorManager.restaurants[id].zzimCheck = true;
                    heartToggle.isOn = true;
                    zzim[id] = AnchorManager.restaurants[id];
                    ToastMaker.instance.ShowToast("찜이 등록되었습니다.");
                }
                else
                {
                    heartToggle.isOn = false;
                    Debug.Log("찜목록은 최대 4개까지 가능");
                    ToastMaker.instance.ShowToast("찜 인벤토리가 꽉 찼습니다. 찜은 4개까지만 가능합니다.");
                }
            }
        }
    }

}
