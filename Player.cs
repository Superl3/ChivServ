using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChivServ
{
    class Player
    {
        public Team team = Team.None;
        public int streak = 0;
        public int perm = 0;
        public int block = 0;
        public int onK = 0;
        public int onD = 0;
        public int totalK = 0;
        public int totalD = 0;
        public int TK = 0;
        public string Name = "temp";
        public int Ping = 0;
        public long GUID = 0;
        
        public Player(long GUID, string Name)
        {
            this.GUID = GUID;
            this.Name = Name;
        }
        public Player(string raw)
        {   
            string[] vars = raw.Split('\t');
            if(vars.Length == 7) // ver 05-23
            {
                long.TryParse(vars[0], out GUID);
                Name = vars[1];
                int.TryParse(vars[2], out perm);
                int.TryParse(vars[3], out totalK);
                int.TryParse(vars[4], out totalD);
                int.TryParse(vars[5], out TK);
                int.TryParse(vars[6], out block);
            }
            // GUID / Name / pm / totalK / totalD / TK / block
        }

        public override string ToString()
        {
            char s = '\t';
            return GUID + s + Name + s + perm + s + totalK + s + totalD + s + TK + s + block ;
        }

        public enum Team
        {
            Agatha,
            Mason,
            None
        }
    }
}
