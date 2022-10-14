using System;

namespace ReviewOnAppstoreData.Entity
{
    public class ReviewDetail
    {
        public string type { get; set; }
        public Guid id { get; set; }
        public AtributeReview attributes { get; set; }
    }
}
