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
            chatPath = "";
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
        }
        private void writeChat(long guid, string str)
        {
            const string s = "\t";
            string text = DateTime.Now.ToString("H:mm:ss");
            if(chatPath == "" || chatPath.Substring(0, chatPath.IndexOf('.')) !=  DateTime.Now.Date.ToString("MM-dd-yyyy"))
                getChat();

            text += s;
            if (guid != 0)
            {
                Player p = Players[guid];
                text += p.team.ToString() + s + p.Name + s + str;
            }
            else
                text += str;

            text += "\r\n"; 

            lock (chat_file)
            {
                File.AppendAllText(chatPath, text);
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
                Account acc = new Account(line);
                Accounts.Add(acc.GUID, acc);
            }
        }

        private void setAcc(string filename)
        {
            string path = Path.Combine(this.path, filename);

            chkFile(path);

            List<string> data = new List<string>();
            foreach (Account acc in Accounts.Values)
            {
                data.Add(acc.ToString());
            }

            lock (account_file)
            {
                File.WriteAllLines(path, data);
            }
        }
    }
}
