using System;

namespace ReviewOnAppstoreData.Entity
{
    public class ResponseDetail
    {
        public string type { get; set; }
        public Guid id { get; set; }
        public AttributeResponse attributes { get; set; }
    }
}
