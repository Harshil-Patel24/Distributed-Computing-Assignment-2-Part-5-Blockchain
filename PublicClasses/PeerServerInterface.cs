using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PublicClasses
{
    [ServiceContract]
    public interface PeerServerInterface
    {
        [OperationContract]
        void ProcessBlock(Block block);

        [OperationContract]
        List<Block> GetBlockChain();

        [OperationContract]
        Block GetCurrentBlock();
    }
}
