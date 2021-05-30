using PublicClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet
{
    class PeerServer : PeerServerInterface
    {
        public static string peer_ip;
        public static int wallet_id = -1;

        public List<Block> GetBlockChain()
        {
            try
            {
                return BlockChain.blockchain;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public Block GetCurrentBlock()
        {
            try
            {
                return BlockChain.blockchain.Last();
            }
            catch(Exception)
            {
                throw new Exception("No elements in blockchain");
            }
        }

        // Give to miner
        public void ProcessBlock(Block block)
        {
            try
            {
                Miner.AddTransaction(block);
            }
            catch
            {
                throw;
            }
        }
    }
}
