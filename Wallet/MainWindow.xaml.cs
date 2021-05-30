using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
using PublicClasses;
using RestSharp;

namespace Wallet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void BlockChainDelegate();
        public delegate void GUIDelegate();
        private ServiceHost host;
        private bool initialized = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoBut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BlockChainDelegate blockchain = this.RunServer;
                AsyncCallback callback = this.OnServerClose;

                
                blockchain.BeginInvoke(callback, null);

            }
            catch (Exception ex)
            {
                ErrorLogger.WriteError(ex.Message);
                Dispatcher.Invoke(() =>
                {
                    ErrorLabel.Text = ex.Message;
                });
            }
        }

        // Shows the gui
        private void RunGUI()
        {
            Dispatcher.Invoke(() =>
            {
                int.TryParse(WalletIDText.Text, out int walletID);
                GuiWindow gui = new GuiWindow(walletID);
                gui.Show();
                this.Hide();
            });
        }
        
        private void OnGuiCompletion(IAsyncResult asyncResult)
        {
            try
            {
                GUIDelegate guidel;
                AsyncResult asyncObj = (AsyncResult)asyncResult;

                if (asyncObj.EndInvokeCalled == false)
                {
                    guidel = (GUIDelegate)asyncObj.AsyncDelegate;
                    guidel.EndInvoke(asyncObj);
                }
                asyncObj.AsyncWaitHandle.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void RunServer()
        {
            try
            {
                string IP = "";
                uint Port = 0;

                Dispatcher.Invoke(() =>
                {
                    IP = "net.tcp://" + IPText.Text;
                    uint.TryParse(PortText.Text, out Port);
                });

                string peer_ip = IP + ":" + Port + "/PeerServer";

                // Opens the server 
                var tcp = new NetTcpBinding();
                host = new ServiceHost(typeof(PeerServer));
                host.AddServiceEndpoint(typeof(PeerServerInterface), tcp, peer_ip);
                host.Open();

                string URL = "https://localhost:44333/";
                RestClient client = new RestClient(URL);
                RestRequest request = new RestRequest("api/addpeer");
                request.AddJsonBody(peer_ip);
                IRestResponse response = client.Post(request);

                PeerServer.peer_ip = peer_ip;

                // Runs the gui
                GUIDelegate guidel = this.RunGUI;
                AsyncCallback guicallback = this.OnGuiCompletion;

                guidel.BeginInvoke(guicallback, null);

                // Keep the thread alive while host is open
                while (host.State.Equals(CommunicationState.Opened) || host.State.Equals(CommunicationState.Opening))
                {
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.WriteError(e.Message);
                Dispatcher.Invoke(() =>
                {
                    ErrorLabel.Text = e.Message;
                });
                throw new Exception("Please enter a valid IP address and port (for testing use IP: \"localhost\" and port is any number less than 65535)");
            }
        }

        public void On_WindowClose(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            host.Close();
            // Ensure enough time for host to close 
            Thread.Sleep(6000);
        }

        //Close and clean up the thread
        public void OnServerClose(IAsyncResult asyncResult)
        {
            try
            {
                BlockChainDelegate blockdel;
                AsyncResult asyncObj = (AsyncResult)asyncResult;

                if (asyncObj.EndInvokeCalled == false)
                {
                    blockdel = (BlockChainDelegate)asyncObj.AsyncDelegate;
                    blockdel.EndInvoke(asyncObj);
                }
                asyncObj.AsyncWaitHandle.Close();
            }
            catch (Exception e)
            {
                ErrorLogger.WriteError(e.Message);
                Dispatcher.Invoke(() =>
                {
                    ErrorLabel.Text = e.Message;
                });
            }
        }
    }
}
