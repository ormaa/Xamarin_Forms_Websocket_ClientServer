using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

using Xamarin.Forms;

namespace Websocket_Client_Server
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // fill the current IP as server IP, by default
            string ip = new Websocket_Server().getCurrentIP();
            EntryServerIP.Text = ip;
        }


        Websocket_Server server;
        Websocket_Client client;


        // start web socket server
        void StartServerClick(object sender, System.EventArgs e)
        {
            server = new Websocket_Server();
            server.logDelegate = doLog;
            server.start(EntryServerIP.Text, 9500);
        }
        // Stop server
        void StopServerClick(object sender, System.EventArgs e)
        {
            server.stop();
        }


        // Start web socket client
        void StartClientClick(object sender, System.EventArgs e)
        {
            client = new Websocket_Client();
            client.logDelegate = doLog;
            client.start("ws://" + EntryServerIP.Text + ":9500");
        }
        // disconnect client from websocket server
        void DisconnectClientClick(object sender, System.EventArgs e)
        {
            if (client != null)
            {
                client.stop();
            }
        }


        // send a message : or to server, or to clients
        void SendMsgClick(object sender, System.EventArgs e)
        {
            if (client != null)
            {
                client.SendMessage(EntryMsg.Text);
            }
            else
            {
                if (server != null)
                {
                    server.SendMessageAsync(EntryMsg.Text);
                }
                else
                {
                    doLog("Start the server or connect the client, at first");
                }
            }
        }


        // write message in a Label + scrollView when label is too long

        String logStr = "";
        public void doLog(String text)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                logStr = logStr + text + "\n";
                Log.Text = logStr;
            });
        }





    }
}

