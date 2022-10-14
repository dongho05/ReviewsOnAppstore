using System;

namespace ReviewOnAppstoreData.Entity
{
    public class ResponseInformation
    {
        public string Type_Response { get; set; }
        public Guid ResponseID { get; set; }
        public string ResponseBody { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string State_response { get; set; }
    }
}
