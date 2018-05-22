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
        List<string> Maps = new List<string>();

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

            if (guid == 76561198007625358) // Superl3
                Players[guid].perm = 99;
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
            if (Players[guid].block != 0)
                KICK_PLAYER(guid, "채팅 금지를 어기셨습니다.");
            if (chat[0] == '/')
                chat_command(guid, chat);
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
            if (Maps.Count <= idx)
                Console.WriteLine("잘못된 인덱스 : " + idx);
            else
                Console.WriteLine(Maps[idx] + " : " + idx);
        }
        private void round_end(Packet p)
        {
            int win = p.popInt();
        }
        private void map_list(Packet p)
        {
            string map = p.popString();
            Maps.Add(map);
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
                    KICK_PLAYER(guid, "User over" + Server.ping_limit + " cannot play on this server.");
                }
            }
            else
                Players[guid].Ping = 0;
        }

        Dictionary<string, int> command_level = new Dictionary<string, int>();

        private void chat_command(long guid, string command)
        {
            int perm = Players[guid].perm;
            string[] vars = command.Split(' ');
            switch (vars[0])
            {
                case "/kick":
                case "/k":
                    if (checkInfo(guid, "kick", perm, vars.Length >= 2))
                        command_kick(vars[1], String.Join(" ", vars.Skip<string>(2)), guid, perm);
                    break;
                case "/ban":
                case "/b":
                    if (checkInfo(guid, "ban", perm, vars.Length >= 3))
                        command_ban(vars[1], vars[2], String.Join(" ", vars.Skip<string>(3)), guid, perm);
                    break;
                case "/block":
                case "/cb":
                    if (checkInfo(guid, "block", perm, vars.Length >= 2))
                        command_block(vars[1], String.Join(" ", vars.Skip<string>(2)), guid, perm);
                    break;
            }
        }

        private bool checkInfo(long guid, string key, int perm, bool lenchk)
        {
            if (command_level[key] <= perm)
            {
                if (lenchk)
                    return true;
                else
                {
                    print_error(guid, "kick");
                    return false;
                }
            }
            print_error(guid, "perm");
            return false;
        }
        private void print_error(long guid, string key)
        {
            if(key!="success")
                SAY(guid, "[오류]" + command_err[key]);
            else
                SAY(guid, "[안내]" + command_err[key]);
        }

        private bool getGUID(string name, out long guid)
        {
            List<long> GUIDs = Players.Values.Where(val => val.Name.Contains("name")).ToList().ConvertAll(e => e.GUID);
            if (GUIDs.Count != 1)
            {
                guid = 0;
                return false;
            }
            guid = GUIDs[0];
            return true;
        }

        private void command_kick(string name, string reason, long owner_guid, int perm)
        {
            long target_guid;
            if (!getGUID(name, out target_guid))
            {
                print_error(owner_guid, "name");
                return;
            }

            if (Players[target_guid].perm < perm)
            {
                KICK_PLAYER(owner_guid, reason);
                print_error(owner_guid, "success");
            }
            else
                print_error(owner_guid, "perm");
        }

        private void command_ban(string name, string raw_duration, string reason, long owner_guid, int perm)
        {
            long target_guid = 0;
            if (!getGUID(name, out target_guid))
            {
                print_error(owner_guid, "name");
                return;
            }

            if (Players[target_guid].perm >= perm)
            {
                print_error(owner_guid, "perm");
                return;
            }

            int duration;
            if (!Int32.TryParse(raw_duration, out duration))
            {
                print_error(owner_guid, "ban");
                return;
            }

            if (duration <= 0)
                BAN_PLAYER(owner_guid, reason);
            else
                TEMP_BAN_PLAYER(owner_guid, reason, duration);

            print_error(owner_guid, "success");
        }

        private void command_block(string name, string raw_duration, long owner_guid, int perm)
        {
            long target_guid;
            if (!getGUID(name, out target_guid))
            {
                print_error(owner_guid, "name");
                return;
            }

            if (Players[target_guid].perm >= perm)
            {
                print_error(owner_guid, "perm");
                return;
            }

            int duration;
            if (!Int32.TryParse(raw_duration, out duration))
            {
                print_error(owner_guid, "ban");
                return;
            }

            if (duration <= 0)
                Players[target_guid].block = -1;
            else
                Players[target_guid].block = duration;

            print_error(owner_guid, "success");
        }
    }
}
