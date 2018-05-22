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
        public int K = 0;
        public int D = 0;
        public int TK = 0;
        public string Name = "temp";
        public int Ping = 0;
        public long GUID = 0;
        
        public Player(long GUID, string Name)
        {
            this.GUID = GUID;
            this.Name = Name;
        }

        public enum Team
        {
            Agatha,
            Mason,
            None
        }
    }
}
