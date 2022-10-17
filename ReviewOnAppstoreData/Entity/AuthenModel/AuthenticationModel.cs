using System;

namespace ReviewOnAppstoreData.Entity.AuthenModel
{
    public class AuthenticationModel
    {
        public int Id { get; set; }
        public string Key_ID { get; set; }
        public string Issuer_ID { get; set; }
        public string Audience { get; set; }
        public string Private_Key { get; set; }
        public string Algorithm { get; set; }
        public string Type_Algorithm { get; set; }
    }
}
