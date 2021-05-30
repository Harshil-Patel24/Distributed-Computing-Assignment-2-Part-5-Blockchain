using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PublicClasses
{
    [DataContract]
    public class Block
    {
        [DataMember]
        public uint BlockID { get; set; }
        [DataMember]
        public uint WalletIDTo { get; set; }
        [DataMember]
        public uint WalletIDFrom { get; set; }
        [DataMember]
        public float Amount { get; set; }
        [DataMember]
        public uint Offset { get; set; }
        [DataMember]
        public string PrevHash { get; set; }
        [DataMember]
        public string Hash { get; set; }
    }
}
