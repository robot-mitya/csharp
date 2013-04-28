// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpCommunicationHelper.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2013
// </copyright>
// <summary>
//   Implementation of UDP-communication between PC and robot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// Implementation of UDP-communication between PC and robot.
    /// </summary>
    public sealed class UdpCommunicationHelper : CommunicationHelper
    {
        /// <summary>
        /// UDP client.
        /// </summary>
        private static UdpClient udpReceiveClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpCommunicationHelper" /> class.
        /// </summary>
        /// <param name="roboHeadAddress">Phone's IP-address string.</param>
        /// <param name="udpSendPort">Send port for commands and messages directed from PC to robot.</param>
        /// <param name="udpReceivePort">Receive port for messages directed from robot to PC.</param>
        /// <param name="nonrecurrentMessageRepetitions">Number of repetitions we send nonrecurrent messages to the robot.</param>
        public UdpCommunicationHelper(string roboHeadAddress, int udpSendPort, int udpReceivePort, int nonrecurrentMessageRepetitions)
            : base(nonrecurrentMessageRepetitions)
        {
            this.RoboHeadAddress = IPAddress.Parse(roboHeadAddress);
            this.UdpSendPort = udpSendPort;
            this.UdpReceivePort = udpReceivePort;

            this.FinalizePort();            
            udpReceiveClient = new UdpClient(udpReceivePort);
            try
            {
                udpReceiveClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (Exception e)
            {
                this.LastErrorMessage = e.Message;
            }
        }

        /// <summary>
        /// Gets phone's IP-address.
        /// </summary>
        public IPAddress RoboHeadAddress { get; private set; }

        /// <summary>
        /// Gets the send port for socket. Commands and messages are sent from PC to robot through this port.
        /// </summary>
        public int UdpSendPort { get; private set; }

        /// <summary>
        /// Gets the receive port for socket. Commands and messages are received from robot to PC through this port.
        /// </summary>
        public int UdpReceivePort { get; private set; }

        /// <summary>
        /// Internal method for message transmition throught UDP socket.
        /// Doesn't correct message. Doesn't handle errors, just generate exceptions.
        /// </summary>
        /// <param name="message">
        /// Message to transmit.
        /// </param>
        protected override void TransmitMessage(string message)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            UdpClient udpClient = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(this.RoboHeadAddress, this.UdpSendPort);
            int bytesSent = udpClient.Send(messageBytes, messageBytes.Length, endPoint);
            if (bytesSent != messageBytes.Length)
            {
                throw new IOException("Нет связи с роботом");
            }
        }

        /// <summary>
        /// Communication finalization.
        /// </summary>
        protected override void FinalizePort()
        {
            if ((udpReceiveClient == null) || (udpReceiveClient.Client == null))
            {
                return;
            }

            // Shutdown the socket.
            try
            {
                udpReceiveClient.Client.Shutdown(SocketShutdown.Receive);
                udpReceiveClient.Client.Close();
            }
            catch (ObjectDisposedException)
            {
            }
            
            // Wait until the socket is released.
            while (true)
            {
                try
                {
                    if ((udpReceiveClient == null) || (udpReceiveClient.Client == null))
                    {
                        break;
                    }

                    int available = udpReceiveClient.Client.Available;
                }
                catch (ObjectDisposedException)
                {
                    // ObjectDisposedException means that socket is released.
                    break;
                }
            }

            udpReceiveClient = null;
        }

        /// <summary>
        /// This method is called automatically when UDP packet is received.
        /// </summary>
        /// <param name="result">Callback result</param>
        private void ReceiveCallback(IAsyncResult result)
        {
            if (udpReceiveClient == null)
            {
                return;
            }

            try
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, this.UdpReceivePort);
                byte[] received = udpReceiveClient.EndReceive(result, ref remoteIpEndPoint);

                this.OnTextReceived(new TextReceivedEventArgs(Encoding.UTF8.GetString(received)));

                udpReceiveClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (Exception)
            {
                // todo: log error to file
            }
        }
    }
}
