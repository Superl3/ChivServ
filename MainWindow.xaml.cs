using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Net.Sockets;
using System.Timers;

namespace ChivServ
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        static int BUFFER_SIZE = 4096;
        public struct ServerInformation
        {
            public IPEndPoint ipep;
            public Socket Sock;
            public Socket cbSock;
            public string password;
            public bool status;
            public Timer buffer_check;
            public Timer map_change;
            public Timer autosave;
            public string map;

            public int ping_limit;
            public int ping_threshold;
        }
      
        public byte[] packet = new byte[BUFFER_SIZE];

        ServerInformation Server;

        public void BeginConnect()
        {
            try
            {
                Console.WriteLine("연결시도");
                Server.Sock.BeginConnect(Server.ipep, new AsyncCallback(ConnectCallback), Server.Sock);
            } catch( SocketException se)
            {
                Console.WriteLine(se.NativeErrorCode);
                this.initSocket();// 보통 서버가 죽은경우, 다시 할당해줘야함 소켓에
            }
        }
        public List<byte> recvbuf = new List<byte>();
        public byte[] pkt = new byte[BUFFER_SIZE];
        private void ConnectCallback(IAsyncResult iar)
        {
            try
            {
                Socket sock = (Socket)iar.AsyncState;
                IPEndPoint ipep = (IPEndPoint)sock.RemoteEndPoint;

                Console.WriteLine("서버 접속 성공");

                sock.EndConnect(iar);
                Server.cbSock = sock;
                this.recv(new AsyncCallback(loginCallback));
            } catch (SocketException se)
            {
                if(se.SocketErrorCode == SocketError.NotConnected)
                {
                    Console.WriteLine("소켓 연결 실패");
                }
                Console.WriteLine("에러 + " + se.NativeErrorCode);
                initSocket();
            }
        }

        public void Send(byte[] packet)
        {
            try
            {
                Server.Sock.Send(packet, 0, packet.Length, SocketFlags.None);
            } catch (SocketException se)
            {
                Console.WriteLine("전송실패 + " + se.NativeErrorCode);
            }
        }
        
        public void recv(AsyncCallback cb)
        {
            Server.cbSock.BeginReceive(pkt, 0, pkt.Length, SocketFlags.None,cb, Server.cbSock);
        }

        private void loginCallback(IAsyncResult iar)
        {
            try
            {
                Socket sock = (Socket)iar.AsyncState;
                int nReadSize = sock.EndReceive(iar);
                Packet loginPkt = new Packet(pkt);
                if (loginPkt.type == Packet.Type.SERVER_CONNECT) // 유효한 패킷인지 체크 해야함
                {
                    loginPkt = new Packet(Packet.Type.PASSWORD,
                        SHA1Util.SHA1HashStringForUTF8String(string.Concat(
                        Server.password, new Packet(pkt.Take<byte>((int)pkt.Length).ToArray()).popString())));
                    this.Send(loginPkt.encode());
                    this.recv(new AsyncCallback(loginCallback)); // TODO 비밀번호 실패 처리 해야함
                }
                else if (pkt.Length > 0 && loginPkt.type == Packet.Type.SERVER_CONNECT_SUCCESS) { 
                    Server.status = true;
                    Server.buffer_check.Start();
                    Server.cbSock.Blocking = false;
                    this.recv(new AsyncCallback(recvCallback)); // TODO 비밀번호 실패 처리 해야함
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ErrorCode.ToString());
                this.initSocket();
            }
        }

        private void recvCallback(IAsyncResult iar)
        {
            try
            {
                Socket sock = (Socket)iar.AsyncState;
                int nReadSize = sock.EndReceive(iar);
                if(nReadSize!=0)
                { // TODO : 한번에 여러 패킷이 한줄로 들어올 경우가 있음 + 예외처리 << 
                    recvbuf.AddRange(pkt.Take<byte>(nReadSize));
                    this.recv(new AsyncCallback(recvCallback));
                }
                else
                {
                    Console.WriteLine("비정상적인 신호, 종료합니다.");
                    this.initSocket();
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ErrorCode.ToString());
                this.initSocket();
            }
        }

        private static object buffer_lock = new object();
        private byte[] raw = new byte[BUFFER_SIZE];
        // TODO timeflow를 만들어서 X
        private void buffer_check(object sender, ElapsedEventArgs e)
        {
            while (recvbuf.Count > 2)
            {
                Packet p;
                if (Packet.valid(recvbuf.Take<byte>(2).ToArray()))
                {
                    p = new Packet(recvbuf.ToArray());
                    lock (buffer_lock)
                    {
                        recvbuf.RemoveRange(0, p.getPacketSize());
                    }
                    process_packet(p);
                }
                else
                    break;
                
            }

            // 버퍼에서 첫 패킷 추출 < lock
            // 유효한 패킷인지 검사

            // 해당 패킷 처리
        }

        private void process_packet(Packet p)
        {
            switch(p.type)
            {
                case Packet.Type.PLAYER_CONNECT:
                    player_connect(p);
                    break;
                case Packet.Type.PLAYER_DISCONNECT:
                    player_disconnect(p);
                    break;
                case Packet.Type.NAME_CHANGED:
                    name_changed(p);
                    break;
                case Packet.Type.TEAM_CHANGED:
                    team_changed(p);
                    break;
                case Packet.Type.PLAYER_CHAT:
                    player_chat(p);
                    break;
                case Packet.Type.PING:
                    ping(p);
                    break;
                case Packet.Type.KILL:
                    kill(p);
                    break;
                case Packet.Type.SUICIDE:
                    suicide(p);
                    break;
                case Packet.Type.MAP_LIST:
                    map_list(p);
                    break;
                case Packet.Type.MAP_CHANGED:
                    map_changed(p);
                    break;
                case Packet.Type.ROUND_END:
                    round_end(p);
                    break;
                default:
                    break;
            }
        }

        private void initServer()
        {
            Server = new ServerInformation();

            Server.ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"),27960);
            Server.password = "l3repus";

            Server.ping_limit = 150;
            Server.ping_threshold = 5;

            Server.map = "";
            Server.status = false;
            Server.buffer_check = new Timer(500);
            Server.buffer_check.Elapsed += new ElapsedEventHandler(buffer_check);

            Server.autosave = new Timer(1800000);
            Server.autosave.AutoReset = true;
            Server.autosave.Elapsed += new ElapsedEventHandler(autosave);

            Server.map_change = new Timer(3000);
            Server.map_change.Elapsed += new ElapsedEventHandler(map_change);

            this.readDB();
            Server.autosave.Start();

            this.initSocket();
            this.initCommandLv();
            this.initCommandErr();
        }

        private void initCommandLv()
        {
            command_level.Add("kick", 2);
            command_level.Add("ban", 3);
            command_level.Add("block", 2);
            command_level.Add("unban", 2);
            command_level.Add("restart", 3);
            command_level.Add("rotate", 3);
            command_level.Add("change", 2);
            command_level.Add("maplist", 2);
            command_level.Add("cancel", 3);
            command_level.Add("perm", 4);
        }

        Dictionary<string, string> command_err = new Dictionary<string, string>();

        private void initCommandErr()
        {
            command_err.Add("success", "해당 명령이 성공적으로 수행되었습니다.");
            command_err.Add("perm", "해당 명령을 실행 할 권한이 없습니다.");
            command_err.Add("name", "키워드를 가진 유저가 없거나 둘 이상입니다.");
            command_err.Add("map", "키워드를 가진 맵이 없거나 둘 이상입니다.");
            command_err.Add("kick", "/kick \"유저아이디\" \"사유\" ");
            command_err.Add("ban", "/ban \"유저아이디\" \"초\" \"사유\" -> 0초 입력시 영구밴");
            command_err.Add("block", "/block \"유저아이디\" \"게임 수\" \"사유\" -> 0 게임 입력시 영구 채팅 금지");
            command_err.Add("restart", "/restart");
            command_err.Add("rotate", "/rotate");
            command_err.Add("change", "/change \"맵이름\"");
            command_err.Add("cancel", "/cancel -> 맵 변경이 진행되고 있지 않습니다.");
            command_err.Add("promote", "/promote \"유저아이디\" \"레벨\" -> 자신의 레벨보다 낮아야 합니다.");
            command_err.Add("maplist", "/maplist \"맵이름\" -> 키워드를 가진 맵이 없습니다.");
        }

        private void initSocket()
        {
            Server.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.BeginConnect();
        }

        public MainWindow()
        {
            InitializeComponent();
            initServer();
        }
    }
}
 