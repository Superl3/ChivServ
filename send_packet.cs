using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChivServ
{
    partial class MainWindow
    {
        public void SAY_ALL_BIG(string msg)
        {
            Send(new Packet(Packet.Type.SAY_ALL_BIG, msg).encode());
        }
        public void SAY_ALL(string msg)
        {
            Send(new Packet(Packet.Type.SAY_ALL, msg).encode());
        }
        public void SAY(long guid, string msg)
        {
            Send(new Packet(Packet.Type.SAY, guid, msg).encode());
        }
        public void CHANGE_MAP(string map)
        {
            Send(new Packet(Packet.Type.CHANGE_MAP, map).encode());
        }
        public void ROTATE_MAP()
        {
            Send(new Packet(Packet.Type.ROTATE_MAP).encode());
        }
        public void KICK_PLAYER(long guid, string reason)
        {
            Send(new Packet(Packet.Type.KICK_PLAYER, guid, reason).encode());
        }
        public void BAN_PLAYER(long guid, string reason)
        {
            Send(new Packet(Packet.Type.BAN_PLAYER, guid, reason).encode());
        }
        public void TEMP_BAN_PLAYER(long guid, string reason, int duration)
        {
            Send(new Packet(Packet.Type.TEMP_BAN_PLAYER, guid, reason, duration).encode());
        }
        public void UNBAN_PLAYER(long guid)
        {
            Send(new Packet(Packet.Type.UNBAN_PLAYER, guid).encode());
        }
    }
}
