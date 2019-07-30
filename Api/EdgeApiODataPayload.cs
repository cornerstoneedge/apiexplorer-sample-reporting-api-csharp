using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Api
{
    [DataContract]
    public class EdgeApiODataPayload
    {
        [DataMember(Name = "@odata.context")]
        public string Context { get; set; }

        [DataMember(Name = "@odata.nextLink")]
        public string NextLink { get; set; }

        [DataMember(Name = "@odata.count")]
        public long? Count { get; set; }

        [DataMember(Name = "value")]
        public IReadOnlyList<IReadOnlyDictionary<string, string>> Value { get; set; }

        [IgnoreDataMember]
        public EdgeApiErrorValue ErrorValue { get; set; }
    }
}