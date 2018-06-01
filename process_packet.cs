using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ChivServ
{
    partial class MainWindow
    {
        Dictionary<long, Player> Players = new Dictionary<long, Player>();
        Dictionary<long, Account> Accounts = new Dictionary<long, Account>();
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
                if(!Accounts.ContainsKey(guid)) // 새로운 유저임
                {
                    Players.Add(guid, new Player(guid, name));
                    Accounts.Add(guid, new Account(guid, name));
                }
                else // 게임은 새로 왔으나 기존 유저임
                    Players.Add(guid, Accounts[guid].ToPlayer());
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
                Accounts[guid].D += 1;
                Players[guid].streak = 0;
            }
        }
        private void player_kill(long guid, long vic_guid)
        {
            if (!isbot(guid))
            {
                add_player(guid, "error");
                if (!isbot(vic_guid))
                {
                    if (Players[guid].team == Players[vic_guid].team && Players[guid].team != Player.Team.None)
                    {
                        SAY(guid, "[주의] 방금 당신은 팀킬을 하셨습니다.");
                        SAY(guid, "[주의] 방금 당신은 팀킬을 하셨습니다.");
                        SAY(guid, "[주의] 방금 당신은 팀킬을 하셨습니다.");
                        Players[guid].TK += 1;
                        Accounts[guid].TK += 1;
                        Players[guid].streak = 0;
                    }
                    else
                    {
                        Players[guid].K += 1;
                        Accounts[guid].K += 1;
                        Players[guid].streak += 1;

                    }
                }
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
                Players[guid].Name = Accounts[guid].Name = name;

            if (guid == 76561198007625358) // Superl3
                Players[guid].perm = Accounts[guid].perm = 99;
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
            string chat = p.popString().Trim();

            add_player(guid, "error");
            if (Players[guid].block != 0)
                KICK_PLAYER(guid, "채팅 금지를 어기셨습니다.");
            if (chat[0] == '/')
                chat_command(guid, chat);
            writeChat(guid, chat);
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
            string map = p.popString().Trim();
            Server.map = map;
            foreach(Player player in Players.Values)
            {
                player.team = Player.Team.None;
                player.streak = 0;
            }
            writeChat(0, map + "맵으로 변경되었습니다.");
        }
        private void round_end(Packet p)
        {
            int win = p.popInt();
            writeChat(0, (Server.map + "맵에서 " + (Player.Team)win).ToString() + "팀이 승리했습니다.");
        }

        private void map_list(Packet p)
        {
            if (!Server.status)
            {
                Server.autosave.Start();
                Server.status = true;
            }
            
            string map = p.popString().Trim().ToLower();
            if(!Maps.Contains(map))
                Maps.Add(map);
        }
        private void kill(Packet p)
        {
            long killer_guid = p.popGUID();
            long victim_guid = p.popGUID();

            player_kill(killer_guid, victim_guid);
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
                    KICK_PLAYER(guid, "Over" + Server.ping_limit + " ping is not allowed to play in here.");
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
                case "/kd":
                case "/status":
                    if (checkInfo(guid, "status", perm, vars.Length >= 1))
                        command_status(String.Join(" ", vars.Skip<string>(1)), guid);
                    break;
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
                case "/restart":
                case "/reset":
                    if (checkInfo(guid, "restart", perm, vars.Length == 1))
                        command_restart(guid);
                    break;
                case "/rotate":
                case "/next":
                    if (checkInfo(guid, "rotate", perm, vars.Length == 1))
                        command_rotate(guid);
                    break;
                case "/change":
                case "/map":
                case "/m":
                    if (checkInfo(guid, "change", perm, vars.Length >= 2))
                        command_change(String.Join(" ", vars.Skip<string>(1)), guid);
                    break;
                case "/cancel":
                    if (checkInfo(guid, "cancel", perm, vars.Length == 1))
                        command_cancel(guid);
                    break;
                case "/maplist":
                    if (checkInfo(guid, "maplist", perm, vars.Length == 2))
                        command_maplist(vars[1], guid);
                    break;
                case "/promote":
                    if (checkInfo(guid, "promote", perm, vars.Length == 3))
                        command_promote(vars[1], vars[2], guid, perm);
                    break;
                case "/debug":
                      command_debug();
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
                    print_error(guid, key);
                    return false;
                }
            }
            print_error(guid, "perm");
            return false;
        }
        private void print_error(long guid, string key)
        {
            if (key != "success")
                SAY(guid, "[오류]" + command_err[key]);
            else
            {
                SAY(guid, "[안내]" + command_err[key]);
                writeChat(0, "[명령어] " + Players[guid].Name + "님이 " + key + "명령어를 사용하셨습니다.");
            }
        }

        private bool chkMapScale(string map, int key) // 0 all 1 small 2 large
        {
            switch(key)
            {
                case 1:
                    return map.Contains("mines") || map.Contains("shipyard") || map.Contains("frostpeak") ||
                           map.Contains("dininghall") || map.Contains("courtyard") || map.Contains("cistern") ||
                           map.Contains("bridge") || map.Contains("nomercy") || map.Contains("arena");
                case 2:
                    return map.Contains("battleground") || map.Contains("belmez") || map.Contains("colosseum") ||
                           map.Contains("darkforest_xl") || map.Contains("forest-cm") || map.Contains("coldfront") ||
                           map.Contains("drunkenbazaar") || map.Contains("ruins_large");
                default:
                    return !map.Contains("tavern");
            }
        }

        private bool getGUID(string name, out long guid)
        {
            List<long> GUIDs = Players.Values.Where(val => val.Name.ToLower().Contains(name.ToLower())).ToList().ConvertAll(e => e.GUID);
            if (GUIDs.Count != 1)
            {
                guid = 0;
                return false;
            }
            guid = GUIDs[0];
            return true;
        }

        static Random r = new Random();

        private bool getMAP(string map, out string res, string arg = "")
        {
            List<string> raw =  Maps.Where(val => val.ToLower().Contains(map.ToLower())).ToList();
            if (raw.Count >= 2)
            {
                if (arg == "")
                {
                    res = "";
                    return false;
                }
                int key = 0;
                if (arg == "s" || arg == "small")
                    key = 1;
                else if (arg == "l" || arg == "large")
                    key = 2;
                raw = raw.Where(val => chkMapScale(val, key)).ToList();
                res = raw[r.Next(raw.Count)];
            }
            else
                res = raw[0];
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
                KICK_PLAYER(target_guid, reason);
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
                BAN_PLAYER(target_guid, reason);
            else
                TEMP_BAN_PLAYER(target_guid, reason, duration);

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

        private string next_map = "";
        private void autosave(object sender, ElapsedEventArgs e)
        {
            this.writeDB(true);
        }
        private void map_change(object sender, ElapsedEventArgs e)
        {
            if (next_map == "nextmap")
                ROTATE_MAP();
            else
                CHANGE_MAP(next_map);
        }
        private void command_restart(long owner_guid)
        {
            SAY_ALL("[안내] 현재 맵으로 3초 뒤 재시작됩니다.");
            Server.map_change.Enabled = false;
            Server.map_change.Enabled = true;
            Server.map_change.Start();
            next_map = Server.map;
            print_error(owner_guid, "success");
        }
        private void command_rotate(long owner_guid)
        {
            SAY_ALL("[안내] 다음 맵으로 3초 뒤 변경됩니다.");
            Server.map_change.Enabled = false;
            Server.map_change.Enabled = true;
            Server.map_change.Start();
            next_map = "nextmap";
            print_error(owner_guid, "success");
        }
        private void command_change(string map, long owner_guid)
        {
            string[] raw = map.Split(' ');
            if (raw.Length >= 2)
            {
                if (!getMAP(raw[0], out raw[0], raw[1]) || raw[0] == "")
                {
                    print_error(owner_guid, "map");
                    return;
                }
            }
            else
            {
                if (!getMAP(raw[0], out raw[0]) || raw[0] == "")
                {
                    print_error(owner_guid, "map");
                    return;
                }
            }

            SAY_ALL("[안내] " + raw[0] + " 맵으로 3초 뒤 변경됩니다.");
            Server.map_change.Enabled = false;
            Server.map_change.Enabled = true;
            Server.map_change.Start();
            next_map = raw[0];
            print_error(owner_guid, "success");
        }
        private void command_cancel(long owner_guid)
        {
            if (Server.map_change.Enabled)
            {
                Server.map_change.Stop();
                print_error(owner_guid, "success");
            }
            else
            {
                print_error(owner_guid, "cancel");
            }
        }
        private void command_maplist(string map, long owner_guid)
        {

            if (getMAP(map, out map))
            {
                print_error(owner_guid, "maplist");
                return;
            }
            string[] raw = map.Split(' ');
            foreach (string line in raw)
                SAY(owner_guid, line);
            print_error(owner_guid, "success");
        }

        private void command_promote(string name, string raw_perm, long owner_guid, int perm)
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

            int target_perm;
            if (!Int32.TryParse(raw_perm, out target_perm) || target_perm >= perm || target_perm < 0)
            {
                print_error(owner_guid, "promote");
                return;
            }
            
            Players[target_guid].perm = target_perm;
            SAY(target_guid, "[안내] 권한이 변경되었습니다.");
            print_error(owner_guid, "success");
        }
        private void command_status(string name, long owner_guid)
        {
            long target_guid = owner_guid;
            if(name != "")
            {
                if (!getGUID(name, out target_guid))
                {
                    print_error(owner_guid, "name");
                    return;
                }
            }
            Account p = Accounts[target_guid];
            SAY(owner_guid, "[" + p.Name + "님의 전적]");
            int ratio = 0;
            if (p.D != 0)
                ratio = p.K / p.D * 100;
            SAY(owner_guid, "킬 : " + p.K + " 데스 : " + p.D + "(" + ratio + "%)");
        }
        private void command_debug()
        {
            foreach (string Map in Maps)
                Console.WriteLine(Map);
            Console.WriteLine("현재 맵 : " + Server.map);
        }
    }
}
