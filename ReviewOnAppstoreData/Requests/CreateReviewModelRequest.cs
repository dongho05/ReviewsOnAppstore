namespace ReviewOnAppstoreData.Requests
{
    public class CreateReviewModelRequest
    {
        public RelationReviewRequest relationships { get; set; }
        public string type { get; set; }
        public AttributeReviewRequest attributes { get; set; }
    }
}
