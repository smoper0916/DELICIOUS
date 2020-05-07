public class Restaurant
{
    public static Restaurant instance { get; set; }

    private string id;
    private string name;
    private float x;
    private float y;
    private int mood;
    private string menu;
    private string photo;

    Restaurant(string id, string name, float x, float y, int mood, string menu, string photo)
    {
        instance = this;
    }

}
