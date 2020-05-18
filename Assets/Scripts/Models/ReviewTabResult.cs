using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ReviewTabResult
{
    public List<Review> reviewList { get; set; }

    public int pages { get; set; }
    public float avg { get; set; }

    public ReviewTabResult()
    {

    }

    public ReviewTabResult(List<Review> reviewList, float avg, int pages)
    {
        PutData(reviewList, avg, pages);
    }

    public void PutData(List<Review> reviewList, float avg, int pages)
    {
        this.reviewList = reviewList;
        this.avg = avg;
        this.pages = pages;
    }
}

