using System;

namespace ReviewOnAppstoreData.Entity
{
    public class ReviewInfomation
    {
        public Guid IdCustomer { get; set; }
        public string TypeReview { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string BodyDescription { get; set; }
        public string NameReviewer { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Territory { get; set; }
        public int App_ID { get; set; }
    }
}
