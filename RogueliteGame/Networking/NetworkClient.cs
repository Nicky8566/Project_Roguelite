using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;

namespace RogueliteGame.Networking
{
    public class NetworkClient
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private bool connected;
        
        public StateMessage LastState { get; private set; }
        public bool HasNewState { get; private set; }
        public uint PlayerId { get; private set; }
        
        // NEW: Event when server assigns our player ID
        public event Action<uint> OnPlayerIdAssigned;

        public NetworkClient()
        {
            udpClient = null;
            connected = false;
            PlayerId = 0; // 0 = not assigned yet
        }

        // Connect to server
        public void Connect(string serverIp, int serverPort, string playerName)
        {
            try
            {
                // Create UDP client
                udpClient = new UdpClient();
                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                
                // Send CONNECT message
                byte[] connectPacket = Protocol.SerializeConnect(playerName);
                udpClient.Send(connectPacket, connectPacket.Length, serverEndPoint);
                
                connected = true;
                Console.WriteLine($"Connected to server at {serverIp}:{serverPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                connected = false;
            }
        }

        // Send input to server
        public void SendInput(InputKeys keys, float mouseX, float mouseY)
        {
            if (!connected || udpClient == null) return;

            try
            {
                byte[] inputPacket = Protocol.SerializeInput(PlayerId, keys, mouseX, mouseY);
                udpClient.Send(inputPacket, inputPacket.Length, serverEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send input: {ex.Message}");
            }
        }

        // Receive state from server (non-blocking)
        public void Update()
        {
            if (!connected || udpClient == null) return;

            HasNewState = false;

            try
            {
                // Non-blocking receive
                if (udpClient.Available > 0)
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpClient.Receive(ref remoteEndPoint);

                    // Check message type
                    if (receivedData.Length > 0)
                    {
                        MessageType msgType = (MessageType)receivedData[0];
                        
                        if (msgType == MessageType.Welcome && receivedData.Length >= 5)
                        {
                            // Server told us our player ID!
                            uint assignedId = ((uint)receivedData[1] << 24) |
                                            ((uint)receivedData[2] << 16) |
                                            ((uint)receivedData[3] << 8) |
                                            ((uint)receivedData[4]);
                            
                            PlayerId = assignedId;
                            Console.WriteLine($"[NetworkClient] Server assigned us Player ID: {PlayerId}");
                            
                            // Trigger event
                            OnPlayerIdAssigned?.Invoke(PlayerId);
                        }
                        else if (msgType == MessageType.State)
                        {
                            LastState = Protocol.DeserializeState(receivedData, receivedData.Length);
                            HasNewState = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to receive: {ex.Message}");
            }
        }

        // Disconnect from server
        public void Disconnect()
        {
            if (!connected || udpClient == null) return;

            try
            {
                // Send DISCONNECT message
                byte[] disconnectPacket = new byte[] { (byte)MessageType.Disconnect };
                udpClient.Send(disconnectPacket, disconnectPacket.Length, serverEndPoint);
                
                udpClient.Close();
                connected = false;
                Console.WriteLine("Disconnected from server");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting: {ex.Message}");
            }
        }

        public bool IsConnected => connected;
    }
}