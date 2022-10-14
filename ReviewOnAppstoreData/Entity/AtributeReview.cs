using System;

namespace ReviewOnAppstoreData.Entity
{
    public class AtributeReview
    {
        public int rating { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string reviewerNickname { get; set; }
        public DateTime createdDate { get; set; }
        public string territory { get; set; }
    }
}
