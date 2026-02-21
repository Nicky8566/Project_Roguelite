using System;
using System.Text;

namespace RogueliteGame.Networking
{
    // Message types (same as C protocol)
    public enum MessageType : byte
    {
        Connect = 0,
        Disconnect = 1,
        Input = 2,
        State = 3,
        Ping = 4,
        Pong = 5
    }

    // Input keys (same as C protocol)
    [Flags]
    public enum InputKeys : byte
    {
        None = 0,
        W = 0x01,
        A = 0x02,
        S = 0x04,
        D = 0x08,
        Space = 0x10
    }

    // Entity types
    public enum EntityType : byte
    {
        Player = 0,
        Enemy = 1,
        Projectile = 2
    }

    // Entity state
    public struct EntityState
    {
        public uint EntityId;
        public EntityType Type;
        public float X, Y;
        public short Health;
        public bool Active;
    }

    // Game state message
    public struct StateMessage
    {
        public uint Tick;
        public EntityState[] Entities;
    }

    public static class Protocol
    {
        // Serialize CONNECT message
        public static byte[] SerializeConnect(string playerName)
        {
            byte[] buffer = new byte[33];
            buffer[0] = (byte)MessageType.Connect;
            
            // Player name (32 bytes, padded with zeros)
            byte[] nameBytes = Encoding.ASCII.GetBytes(playerName);
            int copyLength = Math.Min(nameBytes.Length, 32);
            Array.Copy(nameBytes, 0, buffer, 1, copyLength);
            
            return buffer;
        }

        // Serialize INPUT message
        public static byte[] SerializeInput(uint playerId, InputKeys keys, float mouseX, float mouseY)
        {
            byte[] buffer = new byte[14];
            int offset = 0;
            
            // Message type
            buffer[offset++] = (byte)MessageType.Input;
            
            // Player ID (4 bytes, network byte order)
            WriteUInt32(buffer, offset, playerId);
            offset += 4;
            
            // Keys (1 byte)
            buffer[offset++] = (byte)keys;
            
            // Mouse X (4 bytes)
            WriteFloat(buffer, offset, mouseX);
            offset += 4;
            
            // Mouse Y (4 bytes)
            WriteFloat(buffer, offset, mouseY);
            offset += 4;
            
            return buffer;
        }

        // Deserialize STATE message
        public static StateMessage DeserializeState(byte[] buffer, int length)
        {
            if (length < 6) throw new Exception("Buffer too small");
            
            StateMessage state = new StateMessage();
            int offset = 0;
            
            // Skip message type
            offset++;
            
            // Tick number (4 bytes)
            state.Tick = ReadUInt32(buffer, offset);
            offset += 4;
            
            // Entity count (1 byte)
            byte entityCount = buffer[offset++];
            state.Entities = new EntityState[entityCount];
            
            // Each entity (16 bytes each)
            for (int i = 0; i < entityCount; i++)
            {
                if (offset + 16 > length) break;
                
                EntityState entity = new EntityState();
                
                // Entity ID (4 bytes)
                entity.EntityId = ReadUInt32(buffer, offset);
                offset += 4;
                
                // Entity type (1 byte)
                entity.Type = (EntityType)buffer[offset++];
                
                // Position (8 bytes)
                entity.X = ReadFloat(buffer, offset);
                offset += 4;
                entity.Y = ReadFloat(buffer, offset);
                offset += 4;
                
                // Health (2 bytes)
                entity.Health = ReadInt16(buffer, offset);
                offset += 2;
                
                // Active (1 byte)
                entity.Active = buffer[offset++] != 0;
                
                state.Entities[i] = entity;
            }
            
            return state;
        }

        // Helper: Write uint32 to buffer (network byte order). Htnol in C# is not available, so we manually convert to big-endian format.
        private static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            // Manaully Convert to network byte order (big-endian)
            // basically we take the 4 bytes of the uint and place them in the buffer in reverse order
            // then oxff = 0x000000ff, so we mask out all but the last byte, then shift right to get the next byte, and so on
            buffer[offset] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }

        // Helper: Read uint32 from buffer (network byte order)
        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return ((uint)buffer[offset] << 24) |
                   ((uint)buffer[offset + 1] << 16) |
                   ((uint)buffer[offset + 2] << 8) |
                   (uint)buffer[offset + 3];
        }

        // Helper: Write float to buffer (as uint32, network byte order)
        private static void WriteFloat(byte[] buffer, int offset, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                // Reverse for network byte order
                buffer[offset] = bytes[3];
                buffer[offset + 1] = bytes[2];
                buffer[offset + 2] = bytes[1];
                buffer[offset + 3] = bytes[0];
            }
            else
            {
                Array.Copy(bytes, 0, buffer, offset, 4);
            }
        }

        // Helper: Read float from buffer (as uint32, network byte order)
        private static float ReadFloat(byte[] buffer, int offset)
        {
            byte[] bytes = new byte[4];
            if (BitConverter.IsLittleEndian)
            {
                // Reverse from network byte order
                bytes[0] = buffer[offset + 3];
                bytes[1] = buffer[offset + 2];
                bytes[2] = buffer[offset + 1];
                bytes[3] = buffer[offset];
            }
            else
            {
                Array.Copy(buffer, offset, bytes, 0, 4);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        // Helper: Read int16 from buffer (network byte order)
        private static short ReadInt16(byte[] buffer, int offset)
        {
            return (short)(((short)buffer[offset] << 8) | (short)buffer[offset + 1]);
        }
    }
}