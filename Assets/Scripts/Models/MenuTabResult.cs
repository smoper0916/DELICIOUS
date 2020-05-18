using LitJson;
using System.Collections;
using System.Collections.Generic;


public class MenuTabResult
{
    public List<Menu> menuList { get; set; }

    public string desc { get; set; }
    public string tvHistory { get; set; }
    public string biztime { get; set; }
    public string addr { get; set; }
    public string telephone { get; set; }
    public string convenience { get; set; }

    public MenuTabResult()
    {

    }

    public MenuTabResult(List<Menu> menuList, JsonData i = null)
    {
        PutData(menuList, i);
    }

    public void PutData(List<Menu> menuList, JsonData i)
    {
        if (i != null)
        {
            IDictionary dict = i;
            this.desc = dict.Contains("desc") ? i["desc"].ToString() : "";
            this.tvHistory = dict.Contains("tv_history") ? i["tv_history"].ToString() : "";
            this.biztime = dict.Contains("biztime") ? i["biztime"].ToString() : "";
            this.addr = dict.Contains("addr") ? i["addr"].ToString() : "";
            this.telephone = dict.Contains("telephone") ? i["telephone"].ToString() : "";
            this.convenience = dict.Contains("convenience") ? i["convenience"].ToString() : "";
        }

        this.menuList = menuList;
    }

    public void PutExtra(JsonData i)
    {
        IDictionary dict = i;
        this.desc = dict.Contains("desc") ? i["desc"].ToString() : "";
        this.tvHistory = dict.Contains("tv_history") ? i["tv_history"].ToString() : "";
        this.biztime = dict.Contains("biztime") ? i["biztime"].ToString() : "";
        this.addr = dict.Contains("addr") ? i["addr"].ToString() : "";
        this.telephone = dict.Contains("telephone") ? i["telephone"].ToString() : "";
        this.convenience = dict.Contains("convenience") ? i["convenience"].ToString() : "";
    }

    public void PutMenu(JsonData i)
    {
        menuList = new List<Menu>();
        foreach (JsonData item in i)
        {
            menuList.Add(new Menu(item["name"].ToString(), item["price"].ToString()));
            // 썸네일까지 불러오게 해야함.
        }
    }
}
