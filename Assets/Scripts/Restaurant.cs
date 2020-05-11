public class Restaurant
{
    public  Restaurant restaurant { get; set; }

    public string score;
    public string id;
    public string name;
    public string x;
    public string y;
    public string mood;
    public string menu;
    public string brief;
    public string photo;

    Restaurant(string score, string id, string name, string x, string y, string mood, string menu,string brief, string photo)
    {
        this.score = score;
        this.id = id;
        this.name = name;
        this.x = x;
        this.y = y;
        this.mood = mood;
        this.menu = menu;
        this.brief = brief;
        this.photo = photo;
    }

}
