// <copyright file=Networkmanager.cs
// <copyright>
//  Copyright (c) 2016, University of Stuttgart
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the Software),
//  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
//  OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <license>MIT License</license>
// <main contributors>
//  Markus Funk, Thomas Kosch, Sven Mayer
// </main contributors>
// <co-contributors>
//  Paul Brombosch, Mai El-Komy, Juana Heusler, 
//  Matthias Hoppe, Robert Konrad, Alexander Martin
// </co-contributors>
// <patent information>
//  We are aware that this software implements patterns and ideas,
//  which might be protected by patents in your country.
//  Example patents in Germany are:
//      Patent reference number: DE 103 20 557.8
//      Patent reference number: DE 10 2013 220 107.9
//  Please make sure when using this software not to violate any existing patents in your country.
// </patent information>
// <date> 11/2/2016 12:25:59 PM</date>

using System.Net;
using System.Net.Sockets;
using System.Text;
using KoBeLUAdmin.ContentProviders;
using System;
using Newtonsoft.Json.Linq;
using KoBeLUAdmin.Backend;
using Newtonsoft.Json;
using System.Windows;

namespace KoBeLUAdmin.Network
{
    class NetworkManager
    {

        // Singleton
        private static NetworkManager m_Instance;
        private Socket m_Socket;

        public static NetworkManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new NetworkManager();
                }
                return m_Instance;
            }
        }


        public NetworkManager()
        {

        }


        // Sends one data packet over UDP to the corresponding IPAddress and port
        public void SendDataOverUDP(string ipAddress, int port, string message)
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (ipAddress != null)
            {
                IPAddress serverAddr = IPAddress.Parse(ipAddress);
                IPEndPoint endPoint = new IPEndPoint(serverAddr, port);
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                m_Socket.SendTo(buffer, endPoint);
            }
        }

        public void StartAsyncUDPServer(int port)
        {
            // create socket and start async udp server
            UdpClient socket = new UdpClient(port);
            socket.BeginReceive(new AsyncCallback(OnUDPData), socket);
        }


        public void OnUDPData(IAsyncResult result)
        {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);
            string encoded_message = Encoding.UTF8.GetString(message, 0, message.Length);
            ParseMessage(encoded_message);
            socket.BeginReceive(new AsyncCallback(OnUDPData), socket);
        }

        private void ParseMessage(string encoded_message)
        {
            // deserialize incoming JSON
            dynamic jsonResponse = JObject.Parse(encoded_message);
            string call = jsonResponse.call.ToString();

            Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    () =>
                    {

                        switch (call)
                        {
                            case "load_workflow":
                                string workflowpath = jsonResponse.path.ToString();
                                WorkflowManager.Instance.loadWorkflow(workflowpath);
                                break;
                            case "get_workflow_data":
                                string message = JsonConvert.SerializeObject(WorkflowManager.Instance.CurrentWorkingStepSerialization);
                                NetworkManager.Instance.SendDataOverUDP(SettingsManager.Instance.Settings.UDPIPTarget, 20000, message);
                                break;
                            case "start_workflow":
                                WorkflowManager.Instance.startWorkflow();
                                break;
                            case "stop_workflow":
                                WorkflowManager.Instance.stopWorkflow();
                                break;
                            case "previous_step":
                                WorkflowManager.Instance.PreviousWorkingStep();
                                break;
                            case "next_step":
                                WorkflowManager.Instance.NextWorkingStep(HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.WorkingStepEndConditionTrigger.WORKFLOWPANEL_BUTTON);
                                break;
                            default:
                                break;
                        }
                    }
                )
            );
        }

        public Socket Socket
        {
            get { return m_Socket; }
            set { m_Socket = value; }
        }

    }
}
