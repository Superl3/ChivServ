using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChivServ
{
    partial class MainWindow
    {
        Dictionary<long, Player> Players = new Dictionary<long, Player>();

        private static bool isbot(long guid)
        {
            if (guid == 0)
                return true;
            return false;
        }

        private bool add_player(long guid, string name)
        {
            if (!Players.ContainsKey(guid))
            {// 이미 들어온 유저에게 conenct 패킷이 발생
                Player player = new Player(guid, name); // 새로운 유저
                Players.Add(guid, player);
                return true;
            }
            return false;
        }

        private void player_dead(long guid)
        {
            if (!isbot(guid))
            {
                add_player(guid, "error");
                Players[guid].D += 1;
                Players[guid].streak = 0;
            }
        }
        private void player_kill(long guid)
        {
            if (!isbot(guid))
            {
                add_player(guid, "error");
                Players[guid].K += 1;
                Players[guid].streak += 1;
            }
        }

        //   class procpacket {
        private void player_connect(Packet p)
        {
            long guid = p.popGUID();
            string name = p.popString();

            if (isbot(guid))
                return;

            if (!add_player(guid, name))
                Players[guid].Name = name;
        }
        private void player_disconnect(Packet p)
        {
            long guid = p.popGUID();

            if (isbot(guid))
                return;

            Players.Remove(guid); // 해당 유저 삭제
        }
        private void player_chat(Packet p)
        {
            long guid = p.popGUID();
            string chat = p.popString();

            add_player(guid, "error");
        }
        private void name_changed(Packet p)
        {
            player_connect(p);
        }
        private void team_changed(Packet p)
        {
            long guid = p.popGUID();
            int team = p.popInt();

            if (isbot(guid))
                return;

            add_player(guid, "error");
            Players[guid].team = (Player.Team)team;
        }
        private void map_changed(Packet p)
        {
            int idx = p.popInt();
            string map = p.popString();
        }
        private void round_end(Packet p)
        {
            int win = p.popInt();
        }
        private void map_list(Packet p)
        {
            string map = p.popString();
        }
        private void kill(Packet p)
        {
            long killer_guid = p.popGUID();
            long victim_guid = p.popGUID();

            player_kill(killer_guid);
            player_dead(victim_guid);
        }
        private void suicide(Packet p)
        {
            long victim_guid = p.popGUID();

            player_dead(victim_guid);
        }

        private void ping(Packet p)
        {
            long guid = p.popGUID();
            int ping = p.popInt();

            if (isbot(guid))
                return;

            add_player(guid, "error");

            if (Server.ping_limit < ping)
            {
                Players[guid].Ping += 1;
                if (Server.ping_threshold < Players[guid].Ping)
                {
                    //KICK_PLAYER(guid);
                }
            }
            else
                Players[guid].Ping = 0;
        }
        //  }
    }
}
