using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Delegates;

namespace Websocket_Client_Server
{
    public class Websocket_Client
    {
        // Delegate to display log in UI
        public LogDelegate logDelegate;

        public Websocket_Client()
        {
        }


        private ClientWebSocket websocketClient;
        private Uri websocketServerUIR;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken cancellationToken;

        private bool bUserDisconnected;

        // ex. ws://192.168.1.64:8006
        public void start(string serverURI)
        {
            this.websocketClient = new ClientWebSocket();
            this.websocketClient.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            this.websocketServerUIR = new Uri(serverURI);
            cancellationToken = cancellationTokenSource.Token;

            Connect();
        }
        public void stop()
        {
            Disconnect();
        }


        public void Connect()
        {
            bUserDisconnected = false;
            ConnectAsync();
        }

        public void Disconnect()
        {
            bUserDisconnected = true;
            DisconnectAsync();
        }


        // Send message to websocket server
        public void SendMessage(string message)
        {
            logDelegate("client sending message : " + message);
            SendMessageAsync(message);
        }

        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        // Send message to websocket server
        private async void SendMessageAsync(string message)
        {
            try
            {
                if (websocketClient.State != WebSocketState.Open)
                {
                    //throw new Exception("Connection is not open.");
                    logDelegate("Connnection to websocket server is not opened");
                }

                var messageBuffer = Encoding.UTF8.GetBytes(message);
                var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

                for (var i = 0; i < messagesCount; i++)
                {
                    var offset = (SendChunkSize * i);
                    var count = SendChunkSize;
                    var lastMessage = ((i + 1) == messagesCount);

                    if ((count * (i + 1)) > messageBuffer.Length)
                    {
                        count = messageBuffer.Length - offset;
                    }

                    await websocketClient.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logDelegate("Send message to websocekt server failed : " + ex.Message.ToString());
            }
        }

        private async void ConnectAsync()
        {
            // dummy tests
            //int i = 0;
            ////for (int index = 0; index < 255; index++)
            ////{
            //string str = "ws://192.168.1.64"; // + index.ToString();
            //var uri = new Uri(str + ":9500");
            //try
            //{
            //    Debug.WriteLine("IP : " + uri.ToString());
            //    await websocketClient.ConnectAsync(uri, cancellationToken);

            //    Debug.WriteLine(websocketClient.State);
            //    // i = index;
            //    // break;
            //}
            //catch
            //{

            //}
            ////}

            //Debug.WriteLine("Server found : " + "192.168.1." + i.ToString());

            //Ping p = new Ping();
            //for (int index = 0; index < 255; index++)
            //{
            //    PingReply reply = p.Send("192.168.1." + index.ToString(), 9500);
            //    if (reply.Status == IPStatus.Success)
            //    {
            //        Debug.WriteLine("Server found : " + "192.168.1." + index.ToString());
            //    }

            //}


            try
            {
                await websocketClient.ConnectAsync(websocketServerUIR, cancellationToken);
                CallOnConnected();
                StartListen();
            }
            catch (Exception ex)
            {
                logDelegate("Connnection to websocket server failed : " + ex.Message.ToString());
            }
        }

        private async void DisconnectAsync()
        {
            try
            {
                await websocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logDelegate("DisConnnection from websocket server failed : " + ex.Message.ToString());
            }
        }

        private async void StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (websocketClient.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await websocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            if (!bUserDisconnected)
                            {
                                await websocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            }
                            bUserDisconnected = false;
                            CallOnDisconnected();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    CallOnMessage(stringResult);

                }
            }
            catch (Exception)
            {
                CallOnDisconnected();
            }
            finally
            {
                websocketClient.Dispose();
            }
        }

        private void CallOnMessage(StringBuilder stringResult)
        {
            logDelegate("message received : " + stringResult);
        }

        private void CallOnDisconnected()
        {
            logDelegate("disconnected");
        }

        private void CallOnConnected()
        {
            logDelegate("connected");
        }



    }
}

