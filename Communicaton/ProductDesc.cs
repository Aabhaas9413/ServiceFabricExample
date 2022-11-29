using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Communicaton
{
    [DataContract]
    public class ProductDesc
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        
    }
}
