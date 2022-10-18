using IronPython.Runtime;
using ReviewOnAppstoreData.Entity;
using System.Collections.Generic;

namespace ReviewOnAppstoreData.Responses
{
    public class ReviewInfomationResponse
    {
        public int Total { get; set; }
        public List<ReviewInfomation> Data { get; set; }
    }
}
