namespace ReviewOnAppstoreData.Requests
{
    public class CustomerReviewRequest
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
        public string Query { get; set; }
    }
}
