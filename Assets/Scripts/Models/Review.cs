﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Review
{
    string name { get; set; }
    string rating { get; set; }
    string date { get; set; }
    string text { get; set; }

    public Review(string name, string rating, string date, string text)
    {
        this.name = name;
        this.rating = rating;
        this.date = date;
        this.text = text;
    }
}
