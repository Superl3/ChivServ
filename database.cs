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
        private string chatPath = "";
        private object account_file = new object();
        private object chat_file = new object();
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
            bool success = false;
            lock (account_file)
            {
                if (!File.Exists(path))
                {
                    File.Create(path);
                    success = true;
                }
            }
            return success;
        }
        private void getChat()
        {
            chatPath = DateTime.Now.Date.ToString("MM-dd-yyyy") + ".log";
            if (chkFile(chatPath))
                return;
        }
        private void writeChat(long guid, string str)
        {
            const string s = "\t";
            string text = DateTime.Now.Date.ToString("MM-dd-yyyy");
            if(chatPath == "" || chatPath.Substring(0, chatPath.IndexOf('.')) != text)
                getChat();

            text += s;
            if (guid != 0)
            {
                Player p = Players[guid];
                text += p.team.ToString() + s + p.Name + s + str; text += str;
            }
            else
                text += str;

            lock (chat_file)
            {
                File.AppendText(chatPath).WriteLine(text);
            }
        }
        private void getAcc(string filename)
        {
            string path = Path.Combine(this.path, filename);
            string[] raw;

            if (chkFile(path))
                return;
            Players.Clear();

            lock (account_file)
            {
                raw = File.ReadAllLines(path);
            }

            foreach (string line in raw)
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

            lock (account_file)
            {
                File.WriteAllLines(path, data);
            }
        }
    }
}
