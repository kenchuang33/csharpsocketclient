using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
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
using System.Threading;
using Newtonsoft.Json.Linq;

namespace socketclient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket server;
        private bool isConnected = false;
        private Thread recThread = null;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress serverIP = IPAddress.Parse(clientSocketAddress.Text);
                int serverPort = int.Parse(clientPort.Text);
                clientSocket.Connect(new IPEndPoint(serverIP, serverPort));
                server = clientSocket;
                Hello();
                
                recThread = new Thread(() => rec());
                recThread.Start();
                // 更改按鈕的文字為 "Disconnect"
                connect.Content = "Disconnect";
                isConnected = true;
                statuetextBlock.Text = "Online";
            }
            else
            {

                //Thread thread= new Thread(()=> server.Disconnect(true));

               
                string jsonData = JsonConvert.SerializeObject(new { Type = "Disconnect", Data = clientUsername.Text });
                byte[] data = Encoding.ASCII.GetBytes(jsonData);
                server.Send(data);

                // 更新UI和連接狀態
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    clientchatbox.Text += "Disconnected from server." + Environment.NewLine;
                    connect.Content = "Connect";
                });
                isConnected = false;
                statuetextBlock.Text = "Offline";

            }
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected) { 
            string send_msg = clientUsername.Text + ":" + clientMessage.Text;
            string jsonData = JsonConvert.SerializeObject(new { Data = send_msg });

            byte[] data = Encoding.ASCII.GetBytes(jsonData);
            server.Send(data);
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    clientchatbox.Text += "Send fail." + Environment.NewLine;
                });
            }
        }
        private void Hello()
        {
            Console.WriteLine("Hello method is called.");
            string hello = "Welcome " + clientUsername.Text +"!";
            string jsonData = JsonConvert.SerializeObject(new { Data = hello });
            byte[] data = Encoding.ASCII.GetBytes(jsonData);
            server.Send(data);
        }
        private void rec()
        {
            while (server.Connected)
            {
                byte[] result = new byte[server.Available];
                //取得byte array長度
                int receive_num = server.Receive(result);
                //byte array轉回json string
                string receive_str = Encoding.ASCII.GetString(result, 0, receive_num);
                if (receive_num > 0)
                {
                    // 將 JSON 字串轉換為 JObject
                    JObject obj = JObject.Parse(receive_str);
                    if (obj["Data"].ToString() == "Server close !") {

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            statuetextBlock.Text = "Offline";
                            // 更新UI的程式碼
                        }); 
                    }
                    // 讀取 "Data" 屬性的值
                    string dataValue = obj["Data"].ToString();
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        clientchatbox.Text += dataValue + Environment.NewLine;
                        // 更新UI的程式碼
                    });
                }
            }
        }
    }
}
