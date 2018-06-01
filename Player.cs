using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChivServ
{
    class User
    {
        public int perm = 0;
        public int block = 0;
        public int TK = 0;
        public int K = 0;
        public int D = 0;
        public string Name = "temp";
        public long GUID = 0;

        public User(long GUID, string Name)
        {
            this.GUID = GUID;
            this.Name = Name;
        }
        public User(string raw)
        {

        }
    }
    class Player : User
    {
        public Team team = Team.None;
        public int streak = 0;
        public int Ping = 0;
        public Player(long GUID, string Name) : base(GUID,Name)
        {
            this.GUID = GUID;
            this.Name = Name;
        }
        public Player(string raw) : base(raw)
        {   
            string[] vars = raw.Split('\t');
            if(vars.Length == 7) // ver 05-23
            {
                long.TryParse(vars[0], out GUID);
                Name = vars[1];
                int.TryParse(vars[2], out perm);
                int.TryParse(vars[5], out TK);
                int.TryParse(vars[6], out block);
            }
            // GUID / Name / pm / totalK / totalD / TK / block
        }

        public enum Team
        {
            Agatha,
            Mason,
            None
        }
    }
    class Account : User
    {
        public Account(long GUID, string Name) : base(GUID, Name)
        {
            this.GUID = GUID;
            this.Name = Name;
        }
        public Account(string raw) : base(raw)
        {
            string[] vars = raw.Split('\t');
            if (vars.Length == 7) // ver 05-23
            {
                long.TryParse(vars[0], out GUID);
                Name = vars[1];
                int.TryParse(vars[2], out perm);
                int.TryParse(vars[3], out K);
                int.TryParse(vars[4], out D);
                int.TryParse(vars[5], out TK);
                int.TryParse(vars[6], out block);
            }
            // GUID / Name / pm / totalK / totalD / TK / block
        }
        public override string ToString()
        {
            string s = "\t";
            return GUID + s + Name + s + perm + s + K + s + D + s + TK + s + block;
        }
        public Player ToPlayer()
        {
            return new Player(ToString());
        }
    }
}
