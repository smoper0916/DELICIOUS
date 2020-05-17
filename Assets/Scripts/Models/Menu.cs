using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Menu
{
    
    string name { get; set; }
    string price { get; set; }
    string img { get; set; }

    public Menu(string name, string price)
    {
        this.name = name;
        this.price = price;
    }
}

