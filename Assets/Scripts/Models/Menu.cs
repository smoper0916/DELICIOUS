using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Menu
{
    
    public string name { get; set; }
    public string price { get; set; }
    public string img { get; set; }

    public Menu(string name, string price, string img = null)
    {
        this.name = name;
        this.price = price;
        this.img = img;
    }
}

