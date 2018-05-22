using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChivServ
{
    class Packet
    {
        public Type type;
        public int len;
        public List<byte> data = new List<byte>();

        public Packet(byte[] bytes)
        {
            decode(bytes);
        }
        public Packet(List<byte> bytes)
        {
            decode(bytes);
        }
        public Packet(Type type, String str)
        {
            this.type = type;
            this.addString(str);
        }

        public void addGUID(string GUID)
        {
            this.data.AddRange(Encoding.UTF8.GetBytes(GUID).Reverse<byte>());
        }

        public void addInt(int num)
        {
            this.data.AddRange(BitConverter.GetBytes(num).Reverse<byte>());
        }

        public void addString(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            this.data.AddRange(BitConverter.GetBytes(bytes.Length).Reverse<byte>());
            this.data.AddRange(bytes);
        }

        public static Type typechk(byte[] bytes)
        {
            return (Type)BitConverter.ToInt16(bytes.Take<byte>(2).Reverse<byte>().ToArray<byte>(), 0);
        }

        public static bool valid(byte[] bytes)
        {
            int t = BitConverter.ToInt16(bytes.Take<byte>(2).Reverse<byte>().ToArray<byte>(), 0);
            if (0 <= t && t <= Enum.GetNames(typeof(Type)).Length)
                return true;
            return false;
        }

        private void decode(byte[] bytes)
        {
            type = (Type)BitConverter.ToInt16(bytes.Take<byte>(2).Reverse<byte>().ToArray<byte>(), 0);
            len = BitConverter.ToInt32(bytes.Skip<byte>(2).Take<byte>(4).Reverse<byte>().ToArray<byte>(), 0);
            data = bytes.Skip<byte>(6).Take<byte>((int)len).ToList();
            //Console.WriteLine(type.ToString());
        }
        private void decode(List<byte> bytes)
        {
            type = (Type)BitConverter.ToInt16(bytes.Take<byte>(2).Reverse<byte>().ToArray<byte>(), 0);
            len = BitConverter.ToInt32(bytes.Skip<byte>(2).Take<byte>(4).Reverse<byte>().ToArray<byte>(), 0);
            data = bytes.Skip<byte>(6).Take<byte>((int)len).ToList();
            //Console.WriteLine(type.ToString());
        }

        public byte[] encode()
        {
            List<byte> packet = new List<byte>();
            packet.AddRange(BitConverter.GetBytes((ushort)this.type).Reverse<byte>());
            packet.AddRange(BitConverter.GetBytes(this.data.Count).Reverse<byte>());
            packet.AddRange(this.data);
            return packet.ToArray();
        }

        public ulong getGUID()
        {
            ulong num = BitConverter.ToUInt64(this.data.Take<byte>(8).Reverse<byte>().ToArray<byte>(), 0);
            this.data.RemoveRange(0, 8);
            return num;
        }

        public static int getHeaderSize = 6;
        
        public int getLength()
        {
            int len = BitConverter.ToInt32(this.data.Take<byte>(4).Reverse<byte>().ToArray<byte>(), 0);
            this.data.RemoveRange(0, 4);
            return len;
        }
        
        public int getPacketSize()
        {
            return this.len + Packet.getHeaderSize;
        }
        public static int getPacketSize(byte[] rawPacket, int offset)
        {
            return BitConverter.ToInt32(rawPacket.Skip<byte>(2 + offset).Take<byte>(4).Reverse<byte>().ToArray<byte>(), 0) + Packet.getHeaderSize;
        }

        public string getString()
        {
            int len = this.getLength();
            string str = Encoding.UTF8.GetString(this.data.Take<byte>(len).ToArray<byte>());
            this.data.RemoveRange(0, len);
            return str;
        }

        public enum Type
        {
            SERVER_CONNECT,
            SERVER_CONNECT_SUCCESS,
            PASSWORD,
            PLAYER_CHAT,
            PLAYER_CONNECT,
            PLAYER_DISCONNECT,
            SAY_ALL,
            SAY_ALL_BIG,
            SAY,
            MAP_CHANGED,
            MAP_LIST,
            CHANGE_MAP,
            ROTATE_MAP,
            TEAM_CHANGED,
            NAME_CHANGED,
            KILL,
            SUICIDE,
            KICK_PLAYER,
            TEMP_BAN_PLAYER,
            BAN_PLAYER,
            UNBAN_PLAYER,
            ROUND_END,
            PING
        }

    }
}
