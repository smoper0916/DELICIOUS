using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WayPoint
{
    
    public string lon { get; set; }
    public string lat { get; set; }

    public WayPoint(string lon, string lat)
    {
        this.lon = lon;
        this.lat = lat;
    }
}

