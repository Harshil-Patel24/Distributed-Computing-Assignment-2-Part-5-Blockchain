using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using PublicClasses;
using RestSharp;

namespace Wallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GuiWindow : Window
    {
        public delegate void MiningDelegate();

        public GuiWindow(int walletID)
        {
            InitializeComponent();
            PeerServer.wallet_id = walletID;
            //Get the most popular blockchain before proceeding
            Miner.UpdateBlockChain();
            MiningDelegate miningdel = this.MiningThread;
            AsyncCallback callback = this.OnMineCompletion;
            WalletIDText.Content = walletID;
            miningdel.BeginInvoke(callback, null);
        }



        private void MiningThread()
        {
            try
            {
                // Update the blockchain display
                do
                {
                    DisplayBlockChain();
                    Dispatcher.Invoke(() =>
                    {
                        WalletBalanceText.Content = BlockChain.GetWalletBalance((uint)PeerServer.wallet_id);
                    });
                    Thread.Sleep(1000);
                }
                while (true);
            }
            catch(Exception e)
            {
                ErrorLogger.WriteError(e.Message);
            }
        }

        private void OnMineCompletion(IAsyncResult asyncResult)
        {
            try
            {
                MiningDelegate minedel;
                AsyncResult asyncObj = (AsyncResult)asyncResult;

                if (asyncObj.EndInvokeCalled == false)
                {
                    minedel = (MiningDelegate)asyncObj.AsyncDelegate;
                    minedel.EndInvoke(asyncObj);
                }
                asyncObj.AsyncWaitHandle.Close();
            }
            catch(Exception e)
            {
                ErrorLogger.WriteError(e.Message);
            }
        }

        // Displays the blockchain in a text block
        private void DisplayBlockChain()
        {
            Dispatcher.Invoke(() =>
            {
                string blockchaintext = "";

                foreach(PublicClasses.Block blck in BlockChain.blockchain)
                {
                    blockchaintext += "Block ID: " + blck.BlockID + " Amount: " + blck.Amount + " From: " + blck.WalletIDFrom + " To: " + blck.WalletIDTo + "\n";
                }
                TransactionDisplay.Text = blockchaintext; 
            });
        }

        // Gets and sends transaction
        private void SendBut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uint.TryParse(AddressText.Text, out uint address);
                uint.TryParse(AmountText.Text, out uint amount);

                PublicClasses.Block tran = new PublicClasses.Block();
                tran.Amount = amount;
                tran.WalletIDTo = address;
            
                tran.WalletIDFrom = (uint)PeerServer.wallet_id;

                Broadcast(tran);
            }
            catch (Exception ex)
            {
                ErrorLogger.WriteError(ex.Message);
            }
        }

        // Broadcasts a given transaction to all miners 
        private void Broadcast(PublicClasses.Block tran)
        {
            string URL = "https://localhost:44333/";
            RestClient client = new RestClient(URL);
            RestRequest request = new RestRequest("api/getpeers/");
            IRestResponse response = client.Get(request);

            List<string> peers = JsonConvert.DeserializeObject<List<string>>(response.Content);

            if(peers == null)
            {
                throw new Exception("Could not find any peers!");
            }

            foreach(string peer in peers)
            {
                var tcp = new NetTcpBinding();
                var prfactory = new ChannelFactory<PeerServerInterface>(tcp, peer);
                var pr = prfactory.CreateChannel();

                pr.ProcessBlock(tran);
            }
        }
    }
}
