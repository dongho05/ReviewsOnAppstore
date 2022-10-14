using IronPython.Runtime;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReviewOnAppstoreData.Entity;
using System;
using System.Collections.Generic;

namespace ReviewOnAppstoreData.Responses
{
    public class ReviewsRespone
    {
        public List<ReviewDetail> Data { get; set; }
    }
}
