using PublicClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet
{
    class BlockChain
    {
        // This represents the local blockchain
        public static List<Block> blockchain = new List<Block>();
        public static HashSet<uint> wallet_ids = new HashSet<uint>();

        // Returns the balance of the wallet
        public static float GetWalletBalance(uint id)
        {
            try
            {
                // The wallet with ID 0 has infinite money so always return max value
                if (id == 0)
                {
                    return float.MaxValue;
                }

                float bal = 0;
                //Itereate through each transaction and find total wallet balance
                if (wallet_ids.Contains(id))
                {
                    foreach (Block tran in blockchain)
                    {
                        if (tran.WalletIDTo == id)
                        {
                            bal += tran.Amount;
                        }
                        if (tran.WalletIDFrom == id)
                        {
                            bal -= tran.Amount;
                        }
                    }
                }
                else
                {
                    return 0;
                }
                return bal;
            }
            catch(Exception e)
            {
                throw e;
            }

        }

        // Get the latest block's hash
        public static string GetLastHash()
        {
            try
            {
                if (blockchain.Count == 0)
                {
                    return "";
                }
                return blockchain.Last().Hash;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        //Inserts a block to the block chain
        public static void InsertBlock(Block block)
        {
            try
            {
                blockchain.Add(block);
                wallet_ids.Add(block.WalletIDFrom);
                wallet_ids.Add(block.WalletIDTo);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
