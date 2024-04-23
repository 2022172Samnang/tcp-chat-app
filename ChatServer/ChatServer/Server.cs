using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    public class Server
    {
        public static Hashtable htUsuarios = new Hashtable(30);

        public static Hashtable htConexoes = new Hashtable(30);

        private IPAddress enderecoIP;
        private int portaHost;
        private TcpClient tcpCliente;

        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        public Server(IPAddress endereco, int porta)
        {
            enderecoIP = endereco;
            portaHost = porta;
        }

        private Thread thrListener;
        
        private TcpListener tlsCliente;

        bool ServRodando = false;

 
        public static void IncluiUsuario(TcpClient tcpUsuario, string strUsername)
        {

            Server.htUsuarios.Add(strUsername, tcpUsuario);
            Server.htConexoes.Add(tcpUsuario, strUsername);

            EnviaMensagemAdmin(htConexoes[tcpUsuario] + " came in.");
        }

        public static void RemoveUser(TcpClient tcpUsuario)
        {

            if (htConexoes[tcpUsuario] != null)
            {
                EnviaMensagemAdmin(htConexoes[tcpUsuario] + " saiu...");

                Server.htUsuarios.Remove(Server.htConexoes[tcpUsuario]);
                Server.htConexoes.Remove(tcpUsuario);
            }
        }

        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                statusHandler(null, e);
            }
        }

        public static void EnviaMensagemAdmin(string Mensagem)
        {
            StreamWriter swSenderSender;
            e = new StatusChangedEventArgs("Administrador: " + Mensagem);
            OnStatusChanged(e);

     
            TcpClient[] tcpClientes = new TcpClient[Server.htUsuarios.Count];
          
            Server.htUsuarios.Values.CopyTo(tcpClientes, 0);

            for (int i = 0; i < tcpClientes.Length; i++)
            {

                try
                {

                    if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine("Administrador: " + Mensagem);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch 
                {
                    RemoveUser(tcpClientes[i]);
                }
            }
        }


        public static void EnviaMensagem(string Origem, string Mensagem)
        {
            StreamWriter swSenderSender;


            e = new StatusChangedEventArgs(Origem + " said : " + Mensagem);
            OnStatusChanged(e);

      
            TcpClient[] tcpClientes = new TcpClient[Server.htUsuarios.Count];
            
            Server.htUsuarios.Values.CopyTo(tcpClientes, 0);
           
            for (int i = 0; i < tcpClientes.Length; i++)
            {
                
                try
                {
                    if (Mensagem.Trim() == "" || tcpClientes[i] == null)
                    {
                        continue;
                    }
                 
                    swSenderSender = new StreamWriter(tcpClientes[i].GetStream());
                    swSenderSender.WriteLine(Origem + " said: " + Mensagem);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch 
                {
                    RemoveUser(tcpClientes[i]);
                }
            }
        }

        public void Start()
        {
            try
            {

                IPAddress ipaLocal = enderecoIP;
                int portaLocal = portaHost;

                tlsCliente = new TcpListener(ipaLocal, portaLocal);

                tlsCliente.Start();

                ServRodando = true;

                thrListener = new Thread(MantemAtendimento);
                thrListener.IsBackground = true;
                thrListener.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void MantemAtendimento()
        {
            
            while (ServRodando == true)
            {
          
                tcpCliente = tlsCliente.AcceptTcpClient();
          
                Connection newConnection = new Connection(tcpCliente);
            }
        }
    }
}
