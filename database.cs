using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChivServ
{
    partial class MainWindow
    {
        private string path = "data";
        
        private void readDB()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            getAcc("accounts.db");
        }

        private void writeDB(bool autosave)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            setAcc("accounts.db");
            if (autosave)
                return;
            Players.Clear();
        }

        private bool chkFile(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
                return true;
            }
            return false;
        }

        private void getAcc(string filename)
        {
            string path = Path.Combine(this.path, filename);

            if (chkFile(path))
                return;


            Players.Clear();

            string[] raw = File.ReadAllLines(path);
            foreach(string line in raw)
            {
                Player p = new Player(line);
                Players.Add(p.GUID, p);
            }
        }

        private void setAcc(string filename)
        {
            string path = Path.Combine(this.path, filename);

            chkFile(path);

            List<string> data = new List<string>();
            foreach (Player p in Players.Values)
            {
                data.Add(p.ToString());
            }
            File.WriteAllLines(path, data);
        }
    }
}
