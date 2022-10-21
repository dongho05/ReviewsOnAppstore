using System;

namespace ReviewOnAppstoreData.Requests
{
    public class CreateReviewResquest
    {
        public CreateReviewModelRequest data { get; set; }
        public string app_id { get; set; }
    }
}
