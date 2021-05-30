using Newtonsoft.Json;
using PublicClasses;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wallet
{
    class Miner
    {
        public static Queue<Block> transactions = new Queue<Block>();
        private delegate void MiningDelegate();
        private static bool executing = false;
        
        public static void AddTransaction(Block intran)
        {
            try
            {
                int count = BlockChain.blockchain.Count;

                if (count == 0)
                {
                    Block initial = new Block();
                    initial.Amount = 0;
                    initial.BlockID = 0;
                    initial.WalletIDFrom = 0;
                    initial.WalletIDTo = 0;

                    transactions.Enqueue(initial);
                }

                // Validate before adding
                // Execute if not already executing one
                intran.BlockID = (uint)count;
                transactions.Enqueue(intran);

                if (!executing)
                {
                    MiningDelegate mining = ExecuteAll;
                    AsyncCallback callback = OnMiningCompletion;

                    mining.BeginInvoke(callback, null);
                }
            }
            catch(Exception e) { ErrorLogger.WriteError(e.Message); }
        }

        // Checks and updates the blockchain to the most popular blockchain
        public static void UpdateBlockChain()
        {
            try
            {
                string URL = "https://localhost:44333/";
                RestClient client = new RestClient(URL);
                RestRequest request = new RestRequest("api/getpeers");
                IRestResponse response = client.Get(request);
                List<string> peers = JsonConvert.DeserializeObject<List<string>>(response.Content);

                // This contains <hash, frequency>
                Dictionary<string, int> frequencies = new Dictionary<string, int>();

                // Only proceed if there are peers
                if(peers == null)
                {
                    return;
                }

                // Checks each peers latest hash and adds to a dictionary of <hash, frequenct>
                foreach(string peer in peers)
                {
                    var tcp = new NetTcpBinding();
                    var peerfactory = new ChannelFactory<PeerServerInterface>(tcp, peer);
                    var pr = peerfactory.CreateChannel();

                    // If the Peer's blockchain is empty, then move on
                    if(pr.GetBlockChain().Count == 0)
                    {
                        continue;
                    }

                    string curr_hash = pr.GetCurrentBlock().Hash;

                    if(frequencies.TryGetValue(curr_hash, out int count))
                    {
                        frequencies[curr_hash] = count + 1;
                    }
                    else
                    {
                        frequencies.Add(curr_hash, 1);
                    }
                }

                // Sort it so most frequent is at the front
                var sorted = from entry in frequencies orderby entry.Value ascending select entry;
                // Take the first element, which is the most frequent hash
                string hash = sorted.First().Key;

                //If out chain is the same, then no need to do anything
                if(BlockChain.GetLastHash().Equals(hash))
                {
                    return;
                }

                //Otherwise find the blockchain and replace our blockchain with it
                foreach (string peer in peers)
                {
                    if (!peer.Equals(PeerServer.peer_ip))
                    {
                        var tcp = new NetTcpBinding();
                        var peerfactory = new ChannelFactory<PeerServerInterface>(tcp, peer);
                        var pr = peerfactory.CreateChannel();

                        string curr_hash = pr.GetCurrentBlock().Hash;
                    
                        if(curr_hash.Equals(hash))
                        {
                            BlockChain.blockchain = pr.GetBlockChain();
                            return;
                        }
                    }
                }
            }
            catch(Exception e) { ErrorLogger.WriteError(e.Message); }
        }

        // Executes all transactions in the queue
        public static void ExecuteAll()
        {
            try
            {
                executing = true;

                while (transactions.Count > 0)
                {
                    Block tran = transactions.Dequeue();
                    ExecuteBlock(tran);
                }
            }
            catch(Exception e) { ErrorLogger.WriteError(e.Message); }
        }

        //Executes the transcation
        public static void ExecuteBlock(Block tran)
        {
            try
            {
                int count = BlockChain.blockchain.Count;

                // Checks if the block is valid
                if (ValidateInBlock(tran) || count == 0)
                {
                    tran.PrevHash = BlockChain.GetLastHash();
                    tran.Offset = 0;
                    string hash = "";
                    // Brute force a valid hash
                    do
                    {
                        tran.Offset += 5;
                        hash = ComputeSha256Hash(tran);
                    }
                    while (!hash.StartsWith("12345"));
                    tran.Hash = hash;

                    BlockChain.InsertBlock(tran);
                    Thread.Sleep(1000);
                    UpdateBlockChain();
                }
            }
            catch(Exception e) { ErrorLogger.WriteError(e.Message); }
        }

        // Taken from https://www.c-sharpcorner.com/article/compute-sha256-hash-in-c-sharp/
        private static string ComputeSha256Hash(Block block)
        {
            try
            {
                string rawData = block.PrevHash + block.WalletIDFrom.ToString() + block.WalletIDTo.ToString() + block.BlockID.ToString() + block.Amount.ToString() + block.Offset.ToString();
                // Create a SHA256   
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // ComputeHash - returns byte array  
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                    // Convert byte array to a string  
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        // Checks if the block is valid
        private static bool ValidateInBlock(Block tran)
        {
            try
            {
                float balance = BlockChain.GetWalletBalance(tran.WalletIDFrom);

                if (tran.Amount <= 0)
                {
                    return false;
                }

                if (balance < tran.Amount)
                {
                    return false;
                }

                if (tran.WalletIDFrom < 0)
                {
                    return false;
                }

                if (tran.WalletIDTo < 0)
                {
                    return false;
                }

                return true;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static void OnMiningCompletion(IAsyncResult asyncResult)
        {
            try
            {
                MiningDelegate mindel;
                AsyncResult asyncObj = (AsyncResult)asyncResult;

                if (asyncObj.EndInvokeCalled == false)
                {
                    mindel = (MiningDelegate)asyncObj.AsyncDelegate;
                    executing = false;
                    mindel.EndInvoke(asyncObj);
                }
                asyncObj.AsyncWaitHandle.Close();
            }
            catch(Exception e)
            {
                ErrorLogger.WriteError(e.Message);
            }
        }
     }
}
