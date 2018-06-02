using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace ChivServ
{
    public class Network
    {
        const int BUFFER_SIZE = 4096;

        public Timer chkBuf = new Timer(500);
        Socket Sock;

        IPEndPoint IP;
        string Port, Password;

        public Network(string IP, string Port, string Password)
        {
            IPAddress ip;
            int port;
            if (!Int32.TryParse(Port, out port))
                return;
            if (!IPAddress.TryParse(IP, out ip))
                return;
            this.IP = new IPEndPoint(ip, port);
            this.Password = Password;
        }

        public void StartConnect()
        {
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Sock.Blocking = true;
            this.BeginConnect();
        }

        public void BeginConnect()
        {
            try
            {
                Console.WriteLine("연결시도");
                Sock.BeginConnect(IP, new AsyncCallback(ConnectCallback), Sock);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.NativeErrorCode);
                ConnectionLost(this);
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
                Sock = sock;
                this.recv(new AsyncCallback(loginCallback));
            }
            catch (SocketException se)
            {
                Console.WriteLine("서버에 연결 실패");
                ConnectionLost(this);
            }
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
                        Password, new Packet(pkt.Take<byte>((int)pkt.Length).ToArray()).popString())));
                    this.Send(loginPkt.encode());
                    this.recv(new AsyncCallback(loginCallback)); // TODO 비밀번호 실패 처리 해야함
                }
                else if (pkt.Length > 0 && loginPkt.type == Packet.Type.SERVER_CONNECT_SUCCESS)
                {
                    chkBuf.Start();
                    Sock.Blocking = false;
                    this.recv(new AsyncCallback(recvCallback)); // TODO 비밀번호 실패 처리 해야함
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("logincallback" + se.ErrorCode.ToString());
                ConnectionLost(this);
            }
        }

        public void Send(byte[] packet)
        {
            try
            {
                Sock.Send(packet, 0, packet.Length, SocketFlags.None);
            }
            catch (SocketException se)
            {
                Console.WriteLine("전송실패 + " + se.NativeErrorCode);
            }
        }

        public void recv(AsyncCallback cb)
        {
            Sock.BeginReceive(pkt, 0, pkt.Length, SocketFlags.None, cb, Sock);
        }

        private void recvCallback(IAsyncResult iar)
        {
            try
            {
                Socket sock = (Socket)iar.AsyncState;
                int nReadSize = sock.EndReceive(iar);
                if (nReadSize != 0)
                { // TODO : 한번에 여러 패킷이 한줄로 들어올 경우가 있음 + 예외처리 << 
                    DataReceived(this, pkt, nReadSize);
                    this.recv(new AsyncCallback(recvCallback));
                }
                else
                {
                    Console.WriteLine("비정상적인 패킷 혹은 연결이 끊겼습니다.");
                    ConnectionLost(this);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("비정상적인 패킷 혹은 연결이 끊겼습니다.");
                ConnectionLost(this);
            }
        }

        public event ConnectionLostHandler ConnectionLost;
        public event DataReceivedHandler DataReceived;
        
        public delegate void ConnectionLostHandler(object sender);
        public delegate void DataReceivedHandler(object sender, byte[] Data, int dataLength);
    }
}
