using System;

namespace ReviewOnAppstoreData.Requests
{
    public class UpdateStatusRequest
    {
        public Guid ResponseID { get; set; }
        public string State_response { get; set; }
    }
}
