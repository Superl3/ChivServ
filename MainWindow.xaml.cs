using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            public Timer buffer_check_timer;

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
                    Server.buffer_check_timer.Start();
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
                    recvbuf.AddRange(pkt);
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

        private void buffer_check(object sender, ElapsedEventArgs e)
        {            
            while (true)
            {
                Packet p;
                lock (buffer_lock)
                {
                    if (Packet.valid(recvbuf.Take<byte>(2).ToArray()))
                    {
                        p = new Packet(recvbuf.ToArray());
                        recvbuf.RemoveRange(0, p.getPacketSize());
                    }
                    else
                        return;
                }
                process_packet(p);
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
                    break;
                case Packet.Type.NAME_CHANGED:
                    break;
                case Packet.Type.TEAM_CHANGED:
                    break;
                case Packet.Type.PLAYER_CHAT:
                    break;
                case Packet.Type.PING:
                    break;
                case Packet.Type.KILL:
                    break;
                case Packet.Type.SUICIDE:
                    break;
                case Packet.Type.MAP_LIST:
                    break;
                case Packet.Type.MAP_CHANGED:
                    break;
                case Packet.Type.ROUND_END:
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
            Server.status = false;

            Server.buffer_check_timer = new Timer(500);
            Server.buffer_check_timer.Elapsed += new ElapsedEventHandler(buffer_check);

            Server.ping_limit = 100;
            Server.ping_threshold = 5;
            this.initSocket();
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
